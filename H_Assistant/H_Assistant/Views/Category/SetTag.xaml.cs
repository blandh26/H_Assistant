using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Models;
using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace H_Assistant.Views
{
    //定义一个委托去刷新主窗体
    public delegate void TagChangeRefreshHandler();
    /// <summary>
    /// SetObjectGroup.xaml 的交互逻辑
    /// </summary>
    public partial class SetTag : INotifyPropertyChanged
    {
        /// <summary>
        /// 定义委托接收的事件
        /// </summary>
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SetTag()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region MyRegion
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(SetTag), new PropertyMetadata(default(ConnectConfigs)));
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(SetTag), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectsProperty = DependencyProperty.Register(
            "SelectedObjects", typeof(List<TreeNodeItem>), typeof(SetTag), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 分组目标数据
        /// </summary>
        public List<TreeNodeItem> SelectedObjects
        {
            get => (List<TreeNodeItem>)GetValue(SelectedObjectsProperty);
            set
            {
                SetValue(SelectedObjectsProperty, value);
                OnPropertyChanged(nameof(SelectedObjects));
            }
        }

        public static readonly DependencyProperty TagListProperty = DependencyProperty.Register(
            "TagList", typeof(List<TagInfo>), typeof(SetTag), new PropertyMetadata(default(List<GroupInfo>)));
        public List<TagInfo> TagList
        {
            get => (List<TagInfo>)GetValue(TagListProperty);
            set
            {
                SetValue(TagListProperty, value);
                OnPropertyChanged(nameof(TagList));
            }
        }
        #endregion

        public ObservableCollection<TreeNodeItem> LeftObjects { get; set; } =
            new ObservableCollection<TreeNodeItem>();


        private List<GroupInfo> OldGroupList = new List<GroupInfo>();

        private void SetTag_OnLoaded(object sender, RoutedEventArgs e)
        {
            var liteDBHelper = LiteDBHelper.GetInstance();
            SelectedObjects.ForEach(t => LeftObjects.Add(t));
            var list = liteDBHelper.db.GetCollection<TagInfo>().Query().Where(x =>
                  x.ConnectId == SelectedConnection.ID && x.DataBaseName == SelectedDataBase).ToList();
            if (!list.Any())
            {
                return;
            }
            var selectedNames = SelectedObjects.Select(x => x.DisplayName);
            list.ForEach(x =>
            {
                var listObj = liteDBHelper.db.GetCollection<TagObjects>().Count(xx => xx.TagId == x.TagId && selectedNames.Contains(xx.ObjectName));
                x.IsSelected = listObj > 0;// listObj == SelectedObjects.Count;
                //if (listObj == SelectedObjects.Count)
                //{
                //    x.IsSelected = true;
                //    OldGroupList.Add(x);
                //}
            });
            TagList = list;
        }

        /// <summary>
        /// 搜索标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTag_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (TagList == null || !TagList.Any())
            {
                Growl.Warning(new GrowlInfo { Message = LanguageHepler.GetLanguage("PleaseCreateLabel"), WaitTime = 1, ShowDateTime = false });
                return;
            }
            var liteDBHelper = LiteDBHelper.GetInstance();
            //选中的对象列表
            var selectedObjNames = SelectedObjects.Select(x => x.DisplayName).ToList();
            //选中的分组名
            var selectedTag = TagList.Where(x => x.IsSelected).ToList();
            if (!selectedTag.Any())
            {
                return;
            }
            var list = liteDBHelper.db.GetCollection<TagObjects>().Query().Where(x =>
                x.ConnectId == SelectedConnection.ID &&
                x.DatabaseName == SelectedDataBase &&
                selectedObjNames.Contains(x.ObjectName)).ToList();
            foreach (var sobj in list)
            {
                liteDBHelper.db.GetCollection<TagObjects>().Delete(sobj.Id);
            }
            var listNewObject = new List<TagObjects>();
            selectedObjNames.ForEach(selObject =>
            {
                selectedTag.ForEach(selTag =>
                {
                    var tag = new TagObjects()
                    {
                        ConnectId = SelectedConnection.ID,
                        DatabaseName = SelectedDataBase,
                        TagId = selTag.TagId,
                        ObjectName = selObject
                    };
                    listNewObject.Add(tag);
                });
            });
            liteDBHelper.db.GetCollection<TagObjects>().Insert(listNewObject);
            if (ObjChangeRefreshEvent != null)
            {
                ObjChangeRefreshEvent();
            }
            this.Close();
            #endregion
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
