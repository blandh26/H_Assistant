using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Assistant.Models;
using H_Assistant.Views;
using H_Assistant.Views.Category;
using HandyControl.Data;
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
    public partial class UcAddGroupObject : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcAddGroupObject), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UcAddGroupObject), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DbDataProperty = DependencyProperty.Register(
            "DbData", typeof(Model), typeof(UcAddGroupObject), new PropertyMetadata(default(Model)));

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
            "SelectedGroup", typeof(GroupInfo), typeof(UcAddGroupObject), new PropertyMetadata(default(GroupInfo)));
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
        /// 当前选中数据库
        /// </summary>
        public Model DbData
        {
            get => (Model)GetValue(DbDataProperty);
            set => SetValue(DbDataProperty, value);
        }
        /// <summary>
        /// 当前选中分组
        /// </summary>
        public GroupInfo SelectedGroup
        {
            get => (GroupInfo)GetValue(SelectedGroupProperty);
            set => SetValue(SelectedGroupProperty, value);
        }

        /// <summary>
        /// 标签对象数据列表
        /// </summary>
        public static readonly DependencyProperty GroupObjectListProperty = DependencyProperty.Register(
            "GroupObjectList", typeof(List<DbObjectDTO>), typeof(UcAddGroupObject), new PropertyMetadata(default(List<DbObjectDTO>)));
        public List<DbObjectDTO> GroupObjectList
        {
            get => (List<DbObjectDTO>)GetValue(GroupObjectListProperty);
            set
            {
                SetValue(GroupObjectListProperty, value);
                OnPropertyChanged(nameof(GroupObjectList));
            }
        }

        /// <summary>
        /// 标签对象数据分页列表
        /// </summary>
        public static readonly DependencyProperty GroupObjectPageListProperty = DependencyProperty.Register(
            "GroupObjectPageList", typeof(List<DbObjectDTO>), typeof(UcAddGroupObject), new PropertyMetadata(default(List<DbObjectDTO>)));
        public List<DbObjectDTO> GroupObjectPageList
        {
            get => (List<DbObjectDTO>)GetValue(GroupObjectPageListProperty);
            set
            {
                SetValue(GroupObjectPageListProperty, value);
                OnPropertyChanged(nameof(GroupObjectPageList));
            }
        }
        #endregion

        public UcAddGroupObject()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 初始化加载数据
        /// </summary>
        public void LoadPageData()
        {
            LoadingLine.Visibility = Visibility.Visible;
            UcTitle.Content = $"{LanguageHepler.GetLanguage("Set_Table_View_Procedure_Group")}【{SelectedGroup.GroupName}】";
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var model = DbData;
            var selGroup = SelectedGroup;
            var list = new List<DbObjectDTO>();
            Task.Run(() =>
            {
                var liteDbInstance = LiteDBHelper.GetInstance();
                foreach (var table in model.Tables)
                {
                    var isAny = liteDbInstance.IsAny<GroupObjects>(x =>
                        x.ConnectId == selConnection.ID &&
                        x.DatabaseName == selDatabase &&
                        x.GroupId == selGroup.Id &&
                        x.ObjectId == table.Value.Id
                    );
                    var tb = new DbObjectDTO()
                    {
                        ObjectId = table.Value.Id,
                        Name = table.Value.DisplayName,
                        ObjectType = 1,
                        IsEnable = !isAny,
                        IsChecked = isAny,
                        Comment = table.Value.Comment
                    };
                    list.Add(tb);
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                    GroupObjectList = list;
                    PageData();
                }));
            });
        }

        /// <summary>
        /// 页码切换事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageT_OnPageUpdated(object sender, FunctionEventArgs<int> e)
        {
            LoadingLine.Visibility = Visibility.Visible;
            PageData(PageT.PageIndex);
        }

        /// <summary>
        /// 分页加载数据
        /// </summary>
        /// <param name="pageIndex"></param>
        private void PageData(int pageIndex = 0)
        {
            var tagObjects = GroupObjectList;
            var totalCount = GroupObjectList.Count;
            var pageSize = PageT.DataCountPerPage;
            Task.Run(() =>
            {
                var maxPageCount = (int)Math.Ceiling((decimal)totalCount / pageSize);
                var pageList = tagObjects.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    PageT.MaxPageCount = maxPageCount;
                    GroupObjectPageList = pageList;
                    LoadingLine.Visibility = Visibility.Hidden;
                }));
            });
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var selGroup = SelectedGroup;
            var listObjects = new List<GroupObjects>();
            var checkedObjects = GroupObjectList.Where(x => x.IsChecked == true).ToList();
            LoadingLine.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                var liteDBInstance = LiteDBHelper.GetInstance();
                foreach (var obj in checkedObjects)
                {
                    var isAny = liteDBInstance.IsAny<GroupObjects>(x =>
                        x.ConnectId == selConnection.ID &&
                        x.DatabaseName == selDatabase &&
                        x.GroupId == selGroup.Id &&
                        x.ObjectId == obj.ObjectId
                    );
                    if (!isAny)
                    {
                        listObjects.Add(new GroupObjects
                        {
                            ConnectId = selConnection.ID,
                            DatabaseName = selDatabase,
                            ObjectId = obj.ObjectId,
                            ObjectName = obj.Name,
                            GroupId = selGroup.Id
                        });
                    }
                }
                liteDBInstance.db.GetCollection<GroupObjects>().Insert(listObjects);
                selGroup.SubCount += listObjects.Count;
                liteDBInstance.db.GetCollection<GroupInfo>().Update(selGroup);
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadPageData();
                    var parentWindow = (GroupsView)Window.GetWindow(this);
                    parentWindow?.ReloadMenu();
                    Oops.Success($"{LanguageHepler.GetLanguage("SuccessfullySet")}{ listObjects.Count}{LanguageHepler.GetLanguage("DataGroup")}【{SelectedGroup.GroupName}】");
                }));
            });
            #endregion
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (GroupsView)Window.GetWindow(this);
            var ucGroupObjects = new UcGroupObjects();
            ucGroupObjects.SelectedConnection = SelectedConnection;
            ucGroupObjects.SelectedDataBase = SelectedDataBase;
            ucGroupObjects.DbData = DbData;
            ucGroupObjects.SelectedGroup = SelectedGroup;
            ucGroupObjects.LoadPageData();
            parentWindow.MainContent = ucGroupObjects;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in GroupObjectList)
            {
                if (item.IsEnable)
                {
                    item.IsChecked = true;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in GroupObjectList)
            {
                if (item.IsEnable)
                {
                    item.IsChecked = false;
                }
            }
        }

        private void SearchComObjType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            LoadingLine.Visibility = Visibility.Visible;
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var model = DbData;
            var selGroup = SelectedGroup;
            var selItem = ((ComboBoxItem)SearchComObjType.SelectedItem).Tag;
            Task.Run(() =>
            {
                var list = new List<DbObjectDTO>();
                var liteDbInstance = LiteDBHelper.GetInstance();
                if ((string)selItem == "Table")
                {
                    #region Table
                    foreach (var table in model.Tables)
                    {
                        var isAny = liteDbInstance.IsAny<GroupObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.GroupId == selGroup.Id &&
                            x.ObjectId == table.Value.Id
                        );
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = table.Value.Id,
                            Name = table.Value.DisplayName,
                            ObjectType = 1,
                            IsEnable = !isAny,
                            IsChecked = isAny,
                            Comment = table.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else if ((string)selItem == "View")
                {
                    #region View
                    foreach (var view in model.Views)
                    {
                        var isAny = liteDbInstance.IsAny<GroupObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.GroupId == selGroup.Id &&
                            x.ObjectId == view.Value.Id
                        );
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = view.Value.Id,
                            Name = view.Value.DisplayName,
                            ObjectType = 1,
                            IsEnable = !isAny,
                            IsChecked = isAny,
                            Comment = view.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else
                {
                    #region Proc
                    foreach (var proc in model.Procedures)
                    {
                        var isAny = liteDbInstance.IsAny<GroupObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.GroupId == selGroup.Id &&
                            x.ObjectId == proc.Value.Id
                        );
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = proc.Value.Id,
                            Name = proc.Value.DisplayName,
                            ObjectType = 1,
                            IsEnable = !isAny,
                            IsChecked = isAny,
                            Comment = proc.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                    GroupObjectList = list;
                    PageData();
                }));
            });
            #endregion
        }

        /// <summary>
        /// 实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchObjects_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            var searchText = SearchObjects.Text.Trim();
            var selConnection = SelectedConnection;
            var selDatabase = SelectedDataBase;
            var model = DbData;
            var selGroup = SelectedGroup;
            var selItem = ((ComboBoxItem)SearchComObjType.SelectedItem).Tag;
            Task.Run(() =>
            {
                var list = new List<DbObjectDTO>();
                var liteDbInstance = LiteDBHelper.GetInstance();
                if ((string)selItem == "Table")
                {
                    #region Table
                    foreach (var table in model.Tables)
                    {
                        var isAny = liteDbInstance.IsAny<GroupObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.GroupId == selGroup.Id &&
                            x.ObjectId == table.Value.Id
                        );
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            if (!table.Value.Name.ToLower().Contains(searchText.ToLower()) &&
                                !table.Value.Comment.ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = table.Value.Id,
                            Name = table.Value.DisplayName,
                            ObjectType = 1,
                            IsEnable = !isAny,
                            IsChecked = isAny,
                            Comment = table.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else if ((string)selItem == "View")
                {
                    #region View
                    foreach (var view in model.Views)
                    {
                        var isAny = liteDbInstance.IsAny<GroupObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.GroupId == selGroup.Id &&
                            x.ObjectId == view.Value.Id
                        );
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            if (!view.Value.Name.ToLower().Contains(searchText.ToLower()) &&
                                !view.Value.Comment.ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = view.Value.Id,
                            Name = view.Value.DisplayName,
                            ObjectType = 1,
                            IsEnable = !isAny,
                            IsChecked = isAny,
                            Comment = view.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                else
                {
                    #region Proc
                    foreach (var proc in model.Procedures)
                    {
                        var isAny = liteDbInstance.IsAny<GroupObjects>(x =>
                            x.ConnectId == selConnection.ID &&
                            x.DatabaseName == selDatabase &&
                            x.GroupId == selGroup.Id &&
                            x.ObjectId == proc.Value.Id
                        );
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            if (!proc.Value.Name.ToLower().Contains(searchText.ToLower()) &&
                                !proc.Value.Comment.ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }
                        }
                        var tb = new DbObjectDTO()
                        {
                            ObjectId = proc.Value.Id,
                            Name = proc.Value.DisplayName,
                            ObjectType = 1,
                            IsEnable = !isAny,
                            IsChecked = isAny,
                            Comment = proc.Value.Comment
                        };
                        list.Add(tb);
                    }
                    #endregion
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainNoDataText.Visibility = list.Any() ? Visibility.Collapsed : Visibility.Visible;
                    GroupObjectList = list;
                    PageData();
                }));
            });
            #endregion
        }

        private void CheckedRow_Checked(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedItem = (DbObjectDTO)TableGrid.CurrentItem;
            if (selectedItem != null)
            {
                foreach (var item in GroupObjectList)
                {
                    if (item.ObjectId == selectedItem.ObjectId && item.Name == selectedItem.Name)
                    {
                        item.IsChecked = true;
                    }
                }
            }
            #endregion
        }

        private void CheckedRow_Unchecked(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedItem = (DbObjectDTO)TableGrid.CurrentItem;
            if (selectedItem != null)
            {
                foreach (var item in GroupObjectList)
                {
                    if (item.ObjectId == selectedItem.ObjectId && item.Name == selectedItem.Name)
                    {
                        item.IsChecked = false;
                    }
                }
            }
            #endregion
        }
    }
}
