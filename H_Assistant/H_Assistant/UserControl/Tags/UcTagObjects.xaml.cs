using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Assistant.Views.Category;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace H_Assistant.UserControl.Tags
{
    /// <summary>
    /// TagObjects.xaml 的交互逻辑
    /// </summary>
    public partial class UcTagObjects : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcTagObjects), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UcTagObjects), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DbDataProperty = DependencyProperty.Register(
            "DbData", typeof(Model), typeof(UcTagObjects), new PropertyMetadata(default(Model)));
        /// <summary>
        /// 当前选中连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 基础数据
        /// </summary>
        public Model DbData
        {
            get => (Model)GetValue(DbDataProperty);
            set => SetValue(DbDataProperty, value);
        }
        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty.Register(
            "SelectedTag", typeof(TagInfo), typeof(UcTagObjects), new PropertyMetadata(default(TagInfo)));
        /// <summary>
        /// 当前选中标签
        /// </summary>
        public TagInfo SelectedTag
        {
            get => (TagInfo)GetValue(SelectedTagProperty);
            set => SetValue(SelectedTagProperty, value);
        }

        /// <summary>
        /// 标签对象数据列表
        /// </summary>
        public static readonly DependencyProperty TagObjectListProperty = DependencyProperty.Register(
            "TagObjectList", typeof(List<TagObjects>), typeof(UcTagObjects), new PropertyMetadata(default(List<TagObjects>)));
        public List<TagObjects> TagObjectList
        {
            get => (List<TagObjects>)GetValue(TagObjectListProperty);
            set
            {
                SetValue(TagObjectListProperty, value);
                OnPropertyChanged(nameof(TagObjectList));
            }
        }
        #endregion

        public UcTagObjects()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 初始化加载数据
        /// </summary>
        public void LoadPageData()
        {
            var conn = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var selTag = SelectedTag;

            Task.Run(() =>
            {
                var liteInstance = LiteDBHelper.GetInstance();
                var tagObjectList = liteInstance.ToList<TagObjects>(x =>
                    x.ConnectId == conn.ID &&
                    x.DatabaseName == selDatabase &&
                    x.TagId == selTag.TagId);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (tagObjectList.Any())
                    {
                        MainNoDataText.Visibility = Visibility.Collapsed;
                    }
                    TagObjectItems = tagObjectList;
                    TagObjectList = tagObjectList;
                }));
            });
        }
        private List<TagObjects> TagObjectItems;

        private void SearchObjects_TextChanged(object sender, TextChangedEventArgs e)
        {
            #region MyRegion
            var searchData = TagObjectItems;
            var searchText = SearchObjects.Text.Trim();
            if (!string.IsNullOrEmpty(searchText) && TagObjectItems != null)
            {
                var tagObjs = TagObjectItems.Where(x => x.ObjectName.ToLower().Contains(searchText.ToLower()));
                if (tagObjs.Any())
                {
                    searchData = tagObjs.ToList();
                }
                else
                {
                    searchData = new List<TagObjects>();
                }
            }
            MainNoDataText.Visibility = searchData != null && searchData.Any() ? Visibility.Collapsed : Visibility.Visible;
            TagObjectList = searchData;
            #endregion
        }

        /// <summary>
        /// 行删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRowDelete_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedItem = (TagObjects)TableGrid.CurrentItem;
            if (selectedItem != null)
            {
                var conn = SelectedConnection;
                var selDatabase = SelectedDataBase;
                var selTag = SelectedTag;
                var liteDBInstance = LiteDBHelper.GetInstance();
                liteDBInstance.db.GetCollection<TagObjects>().Delete(selectedItem.Id);
                if (selTag.SubCount > 0)
                {
                    selTag.SubCount -= 1;
                    liteDBInstance.db.GetCollection<TagInfo>().Update(selTag);
                }
                var tagObjectList = liteDBInstance.db.GetCollection<TagObjects>().Find(x =>
                    x.ConnectId == conn.ID &&
                    x.DatabaseName == selDatabase &&
                    x.TagId == selTag.TagId).ToList();
                MainNoDataText.Visibility = tagObjectList.Any() ? Visibility.Collapsed : Visibility.Visible;
                TagObjectItems = tagObjectList;
                TagObjectList = tagObjectList;
                var parentWindow = (TagsView)Window.GetWindow(this);
                parentWindow?.ReloadMenu();
            }
            #endregion
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetTag_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTag == null)
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectLabel"));
                return;
            }
            var parentWindow = (TagsView)Window.GetWindow(this);
            var ucAddObjects = new UcAddTagObject();
            ucAddObjects.SelectedConnection = SelectedConnection;
            ucAddObjects.SelectedDataBase = SelectedDataBase;
            ucAddObjects.DbData = DbData;
            ucAddObjects.SelectedTag = SelectedTag;
            ucAddObjects.LoadPageData();
            parentWindow.MainContent = ucAddObjects;
        }
    }
}
