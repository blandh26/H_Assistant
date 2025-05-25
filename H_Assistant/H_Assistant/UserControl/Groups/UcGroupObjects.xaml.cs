using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Assistant.Views;
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
    public partial class UcGroupObjects : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcGroupObjects), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UcGroupObjects), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DbDataProperty = DependencyProperty.Register(
            "DbData", typeof(Model), typeof(UcGroupObjects), new PropertyMetadata(default(Model)));
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

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
            "SelectedGroup", typeof(GroupInfo), typeof(UcGroupObjects), new PropertyMetadata(default(GroupInfo)));
        /// <summary>
        /// 当前选中分组
        /// </summary>
        public GroupInfo SelectedGroup
        {
            get => (GroupInfo)GetValue(SelectedGroupProperty);
            set => SetValue(SelectedGroupProperty, value);
        }

        /// <summary>
        /// 分组对象数据列表
        /// </summary>
        public static readonly DependencyProperty GroupObjectListProperty = DependencyProperty.Register(
            "GroupObjectList", typeof(List<GroupObjects>), typeof(UcGroupObjects), new PropertyMetadata(default(List<GroupObjects>)));
        public List<GroupObjects> GroupObjectList
        {
            get => (List<GroupObjects>)GetValue(GroupObjectListProperty);
            set
            {
                SetValue(GroupObjectListProperty, value);
                OnPropertyChanged(nameof(GroupObjectList));
            }
        }
        #endregion

        public UcGroupObjects()
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
            var selGroup = SelectedGroup;

            Task.Run(() =>
            {
                var liteDbInstance = LiteDBHelper.GetInstance();
                var groupObjectList = liteDbInstance.ToList<GroupObjects>(x =>
                    x.ConnectId == conn.ID &&
                    x.DatabaseName == selDatabase &&
                    x.GroupId == selGroup.Id);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (groupObjectList.Any())
                    {
                        MainNoDataText.Visibility = Visibility.Collapsed;
                    }
                    GroupObjectItems = groupObjectList;
                    GroupObjectList = groupObjectList;
                }));
            });
        }
        private List<GroupObjects> GroupObjectItems;

        private void SearchObjects_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchData = GroupObjectItems;
            var searchText = SearchObjects.Text.Trim();
            if (!string.IsNullOrEmpty(searchText) && GroupObjectItems != null)
            {
                var tagObjs = GroupObjectItems.Where(x => x.ObjectName.ToLower().Contains(searchText.ToLower()));
                if (tagObjs.Any())
                {
                    searchData = tagObjs.ToList();
                }
                else
                {
                    searchData = new List<GroupObjects>();
                }
            }
            MainNoDataText.Visibility = searchData != null && searchData.Any() ? Visibility.Collapsed : Visibility.Visible;
            GroupObjectList = searchData;
        }

        /// <summary>
        /// 行删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRowDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (GroupObjects)TableGrid.CurrentItem;
            if (selectedItem != null)
            {
                var conn = SelectedConnection;
                var selDatabase = SelectedDataBase;
                var selGroup = SelectedGroup;
                var liteDBHelperInstance = LiteDBHelper.GetInstance();
                liteDBHelperInstance.db.GetCollection<GroupObjects>().Delete(selectedItem.Id);
                if (selGroup.SubCount > 0)
                {
                    selGroup.SubCount -= 1;
                    liteDBHelperInstance.db.GetCollection<GroupInfo>().Update(selGroup);
                }
                var groupObjectList = liteDBHelperInstance.db.GetCollection<GroupObjects>().Find(x =>
                    x.ConnectId == conn.ID &&
                    x.DatabaseName == selDatabase &&
                    x.GroupId == selGroup.Id).ToList();
                MainNoDataText.Visibility = groupObjectList.Any() ? Visibility.Collapsed : Visibility.Visible;
                GroupObjectItems = groupObjectList;
                GroupObjectList = groupObjectList;
                var parentWindow = (GroupsView)Window.GetWindow(this);
                parentWindow?.ReloadMenu();
            }
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
        private void BtnSetGroup_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGroup == null)
            {
                Oops.Oh(LanguageHepler.GetLanguage("PleaseSelectGroup"));
                return;
            }
            var parentWindow = (GroupsView)Window.GetWindow(this);
            var ucAddObjects = new UcAddGroupObject();
            ucAddObjects.SelectedConnection = SelectedConnection;
            ucAddObjects.SelectedDataBase = SelectedDataBase;
            ucAddObjects.DbData = DbData;
            ucAddObjects.SelectedGroup = SelectedGroup;
            ucAddObjects.LoadPageData();
            parentWindow.MainContent = ucAddObjects;
        }
    }
}
