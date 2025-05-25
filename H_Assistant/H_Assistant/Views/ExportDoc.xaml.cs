using H_Assistant.Annotations;
using H_Assistant.DocUtils;
using H_Assistant.DocUtils.Dtos;
using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Framework.Util;
using H_Assistant.Helper;
using H_Assistant.Models;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace H_Assistant.Views
{
    /// <summary>
    /// ExportDoc.xaml 的交互逻辑
    /// </summary>
    public partial class ExportDoc : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string fileExt = ".chm";
        private static readonly string GROUPICON = "pack://application:,,,/Resources/svg/category.svg";
        private static readonly string TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        private static readonly string VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        private static readonly string PROCICON = "pack://application:,,,/Resources/svg/proc.svg";
        #region DependencyProperty
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(ExportDoc), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(ExportDoc), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(ExportDoc), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 树形对象菜单
        /// </summary>
        public List<TreeNodeItem> TreeViewData
        {
            get => (List<TreeNodeItem>)GetValue(TreeViewDataProperty);
            set
            {
                SetValue(TreeViewDataProperty, value);
                OnPropertyChanged(nameof(TreeViewData));
            }
        }

        public static readonly DependencyProperty ExportDataProperty = DependencyProperty.Register(
            "ExportData", typeof(List<TreeNodeItem>), typeof(ExportDoc), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 导出目标数据
        /// </summary>
        public List<TreeNodeItem> ExportData
        {
            get => (List<TreeNodeItem>)GetValue(ExportDataProperty);
            set
            {
                SetValue(ExportDataProperty, value);
                OnPropertyChanged(nameof(ExportData));
            }
        }

        /// <summary>
        /// 菜单数据
        /// </summary>
        public static readonly DependencyProperty MenuDataProperty = DependencyProperty.Register(
            "MenuData", typeof(Model), typeof(ExportDoc), new PropertyMetadata(default(Model)));
        /// <summary>
        /// 菜单数据
        /// </summary>
        public Model MenuData
        {
            get => (Model)GetValue(MenuDataProperty);
            set
            {
                SetValue(MenuDataProperty, value);
                OnPropertyChanged(nameof(MenuData));
            }
        }

        private List<TreeNodeItem> itemList = new List<TreeNodeItem>();
        #endregion

        public ExportDoc()
        {
            InitializeComponent();
            DataContext = this;
            TxtPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void ExportDoc_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            Title = $"{SelectedDataBase.DbName} - {Title}";
            var fName = SelectedDataBase.DbName;
            if (fName.Contains(":"))
            {
                fName = fName.Replace(":", "_");
            }
            TxtFileName.Text = fName + LanguageHepler.GetLanguage("DatabaseDesignDocument");
            var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedConnection.DbMasterConnectString, SelectedDataBase.DbName);
            var list = dbInstance.GetDatabases(SelectedDataBase.DbName);
            SelectDatabase.ItemsSource = list;
            HidSelectDatabase.Text = SelectedDataBase.DbName;
            SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == SelectedDataBase.DbName);
            MenuBind();
            #endregion
        }

        private void BtnLookPath_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                TxtPath.Text = dialog.FileName;
            }
        }


        /// <summary>
        /// 选择数据库发生变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectDatabase_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var selectDatabase = SelectDatabase.SelectedItem;
            if (selectDatabase != null)
            {
                HidSelectDatabase.Text = ((DataBase)selectDatabase).DbName;
                MenuBind(true);
            }
            #endregion
        }

        /// <summary>
        /// 实时搜索表、视图、存储过程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchMenu_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchMenuBind();
        }

        /// <summary>
        /// 菜单绑定
        /// </summary>
        public void MenuBind(bool isSelectDb = false)
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            NoDataText.Visibility = Visibility.Collapsed;
            /////TreeViewTables.ItemsSource = null;
            var selectDataBase = SelectedDataBase;
            var selectDataBaseText = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            var selectData = ExportData;
            var menuData = MenuData;
            Task.Run(() =>
            {
                var liteDBHelper = LiteDBHelper.GetInstance();
                var leftMenuType = 1; // LiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
                var curObjects = new List<SObjectDTO>();
                var curGroups = new List<GroupInfo>();
                var itemParentList = new List<TreeNodeItem>();
                var itemList = new List<TreeNodeItem>();
                var nodeTable = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = LanguageHepler.GetLanguage("Table"),
                    Name = "treeTable",
                    Icon = TABLEICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeTable);
                var nodeView = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = LanguageHepler.GetLanguage("View"),
                    Name = "treeView",
                    Icon = VIEWICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeView);
                var nodeProc = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = LanguageHepler.GetLanguage("Procedure"),
                    Name = "treeProc",
                    Icon = PROCICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeProc);

                #region 分组业务处理
                //是否业务分组
                if (leftMenuType == LeftMenuType.Group.GetHashCode())
                {
                    var db_GroupInfo = liteDBHelper.db.GetCollection<GroupInfo>();
                    curGroups = db_GroupInfo.Query().Where(a =>
                        a.ConnectId == selectConnection.ID &&
                        a.DataBaseName == selectDataBaseText).OrderBy(x => x.OrderFlag).ToList();
                    if (curGroups.Any())
                    {
                        foreach (var group in curGroups)
                        {
                            var itemChildList = new List<TreeNodeItem>();
                            var nodeGroup = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = group.GroupName,
                                Name = "treeGroup",
                                Icon = GROUPICON,
                                FontWeight = "Bold",
                                Type = ObjType.Group,
                                IsExpanded = !(!group.OpenLevel.HasValue || group.OpenLevel == 0)
                            };
                            var nodeTable1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = LanguageHepler.GetLanguage("Table"),
                                Name = "treeTable",
                                Icon = TABLEICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2
                            };
                            itemChildList.Add(nodeTable1);
                            var nodeView1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = LanguageHepler.GetLanguage("View"),
                                Name = "treeView",
                                Icon = VIEWICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2
                            };
                            itemChildList.Add(nodeView1);
                            var nodeProc1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = LanguageHepler.GetLanguage("Procedure"),
                                Name = "treeProc",
                                Icon = PROCICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2
                            };
                            itemChildList.Add(nodeProc1);
                            nodeGroup.Children = itemChildList;
                            itemParentList.Add(nodeGroup);
                        }
                    }
                    var db_GroupObjects = liteDBHelper.db.GetCollection<GroupObjects>();
                    var groupInfoList = db_GroupInfo.Query().ToList();
                    var groupObjectsList = db_GroupObjects.Query().ToList();
                    curObjects = (from a in groupInfoList
                                  join b in groupObjectsList on a.Id equals b.GroupId
                                  where a.ConnectId == selectConnection.ID &&
                                        a.DataBaseName == selectDataBaseText
                                  select new SObjectDTO
                                  {
                                      GroupName = a.GroupName,
                                      ObjectName = b.ObjectName
                                  }).ToList();
                }
                #endregion

                if (isSelectDb)
                {
                    #region 更新左侧菜单
                    var model = new Model();
                    try
                    {
                        var dbInstance = ExporterFactory.CreateInstance(selectConnection.DbType,
                            selectConnection.SelectedDbConnectString(selectDataBaseText), selectDataBase.DbName);
                        model = dbInstance.Init();
                        menuData = model;
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Growl.Warning(new GrowlInfo
                            {
                                Message = $"{LanguageHepler.GetLanguage("ConnectionFailedStr")} {selectConnection.ConnectName}，{LanguageHepler.GetLanguage("Reason")}：" + ex.ToMsg(),
                                ShowDateTime = false,
                                Type = InfoType.Error
                            });
                        }));
                    }
                    #endregion
                }

                var textColor = "#4f5d79";
                #region 表
                foreach (var table in menuData.Tables)
                {
                    var isChecked = selectData != null && selectData.Any(x => x.DisplayName.Equals(table.Value.DisplayName));
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = curObjects.Where(x => x.ObjectName == table.Key).
                            GroupBy(x => x.GroupName).Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == LanguageHepler.GetLanguage("Table"));
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = table.Value.Id,
                                        Parent = ppGroup,
                                        DisplayName = table.Value.DisplayName,
                                        Name = table.Value.Name,
                                        Schema = table.Value.SchemaName,
                                        Comment = table.Value.Comment,
                                        CreateDate = table.Value.CreateDate,
                                        ModifyDate = table.Value.ModifyDate,
                                        TextColor = textColor,
                                        Icon = TABLEICON,
                                        Type = ObjType.Table
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeTable.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = table.Value.Id,
                            Parent = nodeTable,
                            DisplayName = table.Value.DisplayName,
                            Name = table.Value.Name,
                            Schema = table.Value.SchemaName,
                            Comment = table.Value.Comment,
                            CreateDate = table.Value.CreateDate,
                            ModifyDate = table.Value.ModifyDate,
                            TextColor = textColor,
                            Icon = TABLEICON,
                            Type = ObjType.Table,
                            IsChecked = isChecked
                        });
                    }
                }
                nodeTable.IsChecked = ParentIsChecked(nodeTable);
                #endregion

                #region 视图
                foreach (var view in menuData.Views)
                {
                    var isChecked = selectData != null && selectData.Any(x => x.DisplayName.Equals(view.Value.DisplayName));
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = curObjects.Where(x => x.ObjectName == view.Key).
                            GroupBy(x => x.GroupName).Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == LanguageHepler.GetLanguage("View"));
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = view.Value.Id,
                                        Parent = ppGroup,
                                        DisplayName = view.Value.DisplayName,
                                        Name = view.Value.Name,
                                        Schema = view.Value.SchemaName,
                                        Comment = view.Value.Comment,
                                        CreateDate = view.Value.CreateDate,
                                        ModifyDate = view.Value.ModifyDate,
                                        TextColor = textColor,
                                        Icon = VIEWICON,
                                        Type = ObjType.View
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeView.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = view.Value.Id,
                            Parent = nodeView,
                            DisplayName = view.Value.DisplayName,
                            Name = view.Value.Name,
                            Schema = view.Value.SchemaName,
                            Comment = view.Value.Comment,
                            CreateDate = view.Value.CreateDate,
                            ModifyDate = view.Value.ModifyDate,
                            TextColor = textColor,
                            Icon = VIEWICON,
                            Type = ObjType.View,
                            IsChecked = isChecked
                        });
                    }
                }
                nodeView.IsChecked = ParentIsChecked(nodeView);
                #endregion

                #region 存储过程
                foreach (var proc in menuData.Procedures)
                {
                    var isChecked = selectData != null && selectData.Any(x => x.DisplayName.Equals(proc.Value.DisplayName));
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = curObjects.Where(x => x.ObjectName == proc.Key).GroupBy(x => x.GroupName)
                            .Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == LanguageHepler.GetLanguage("Procedure"));
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = proc.Value.Id,
                                        Parent = ppGroup,
                                        DisplayName = proc.Value.DisplayName,
                                        Name = proc.Value.Name,
                                        Schema = proc.Value.SchemaName,
                                        Comment = proc.Value.Comment,
                                        CreateDate = proc.Value.CreateDate,
                                        ModifyDate = proc.Value.ModifyDate,
                                        TextColor = textColor,
                                        Icon = PROCICON,
                                        Type = ObjType.Proc
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeProc.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = proc.Value.Id,
                            Parent = nodeProc,
                            DisplayName = proc.Value.DisplayName,
                            Name = proc.Value.Name,
                            Schema = proc.Value.SchemaName,
                            Comment = proc.Value.Comment,
                            CreateDate = proc.Value.CreateDate,
                            ModifyDate = proc.Value.ModifyDate,
                            TextColor = textColor,
                            Icon = PROCICON,
                            Type = ObjType.Proc,
                            IsChecked = isChecked
                        });
                    }
                }
                nodeProc.IsChecked = ParentIsChecked(nodeProc);
                #endregion

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadingLine.Visibility = Visibility.Hidden;
                    //编写获取数据并显示在界面的代码
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        if (!itemParentList.Any())
                        {
                            NoDataAreaText.TipText = LanguageHepler.GetLanguage("NoDataCreateGroup");
                            NoDataText.Visibility = Visibility.Visible;
                        }
                        itemParentList.ForEach(group =>
                        {
                            group.Children.ForEach(obj =>
                            {
                                if (!obj.Children.Any())
                                {
                                    obj.Visibility = nameof(Visibility.Collapsed);
                                }
                                obj.DisplayName += $"（{obj.Children.Count}）";
                            });
                        });
                        TreeViewData = itemParentList;
                        SearchMenu.Text = string.Empty;
                    }
                    else
                    {
                        if (!itemList.Any(x => x.Children.Count > 0))
                        {
                            NoDataAreaText.TipText = LanguageHepler.GetLanguage("NoData");
                            NoDataText.Visibility = Visibility.Visible;
                        }
                        itemList.ForEach(obj =>
                        {
                            if (!obj.Children.Any())
                            {
                                obj.Visibility = nameof(Visibility.Collapsed);
                            }
                            obj.DisplayName += $"（{obj.Children.Count}）";
                        });
                        TreeViewData = itemList;
                        SearchMenu.Text = string.Empty;
                    }
                }));
            });
            #endregion
        }

        public bool? ParentIsChecked(TreeNodeItem node)
        {
            #region MyRegion
            bool? viewIsCheck = null;
            var viewCount = node.Children.Count;
            var viewCheckCount = node.Children.Count(x => x.IsChecked == true);
            if (viewCount == viewCheckCount)
            {
                viewIsCheck = true;
            }
            if (viewCheckCount == 0)
            {
                viewIsCheck = false;
            }
            return viewIsCheck;
            #endregion
        }

        /// <summary>
        /// 搜索菜单绑定
        /// </summary>
        private void SearchMenuBind()
        {
            #region MyRegion
            NoDataText.Visibility = Visibility.Collapsed;
            itemList = new List<TreeNodeItem>();
            var searchText = SearchMenu.Text.ToLower().Trim();
            var nodeTable = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = LanguageHepler.GetLanguage("Table"),
                Name = "treeTable",
                Icon = TABLEICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeTable);
            var nodeView = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = LanguageHepler.GetLanguage("View"),
                Name = "treeView",
                Icon = VIEWICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeView);
            var nodeProc = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = LanguageHepler.GetLanguage("Procedure"),
                Name = "treeProc",
                Icon = PROCICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeProc);
            var liteDBHelper = LiteDBHelper.GetInstance();
            var leftMenuType = liteDBHelper.GetSysInt(SysConst.Sys_LeftMenuType);
            var isLikeSearch = liteDBHelper.GetSysBool(SysConst.Sys_IsLikeSearch);
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            var menuData = MenuData;
            var currObjects = new List<SObjectDTO>();
            var currGroups = new List<GroupInfo>();
            var itemParentList = new List<TreeNodeItem>();
            leftMenuType = LeftMenuType.All.GetHashCode();
            #region 分组业务处理
            if (leftMenuType == LeftMenuType.Group.GetHashCode())
            {
                currGroups = liteDBHelper.db.GetCollection<GroupInfo>().Query().Where(a =>
                    a.ConnectId == selectConnection.ID &&
                    a.DataBaseName == selectDataBase).OrderBy(x => x.OrderFlag).ToList();
                if (!currGroups.Any())
                {
                    NoDataAreaText.TipText = LanguageHepler.GetLanguage("NoDataCreateGroup");
                    NoDataText.Visibility = Visibility.Visible;
                    return;
                }
                foreach (var group in currGroups)
                {
                    var itemChildList = new List<TreeNodeItem>();
                    var nodeGroup = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = group.GroupName,
                        Name = "treeTable",
                        Icon = GROUPICON,
                        Type = ObjType.Group,
                        IsExpanded = true,
                        FontWeight = "Bold",
                        Children = itemChildList
                    };
                    var nodeTable1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = LanguageHepler.GetLanguage("Table"),
                        Name = "treeTable",
                        Icon = TABLEICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup
                    };
                    itemChildList.Add(nodeTable1);
                    var nodeView1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = LanguageHepler.GetLanguage("View"),
                        Name = "treeView",
                        Icon = VIEWICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup
                    };
                    itemChildList.Add(nodeView1);
                    var nodeProc1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = LanguageHepler.GetLanguage("Procedure"),
                        Name = "treeProc",
                        Icon = PROCICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup
                    };
                    itemChildList.Add(nodeProc1);
                    itemParentList.Add(nodeGroup);
                }
                var db_GroupInfo = liteDBHelper.db.GetCollection<GroupInfo>();
                var db_GroupObjects = liteDBHelper.db.GetCollection<GroupObjects>();
                currObjects = (from a in db_GroupInfo.Query().ToList()
                               join b in db_GroupObjects.Query().ToList() on a.Id equals b.GroupId
                               where a.ConnectId == selectConnection.ID &&
                                     a.DataBaseName == selectDataBase
                               select new SObjectDTO
                               {
                                   GroupName = a.GroupName,
                                   ObjectName = b.ObjectName
                               }).ToList();
            }
            #endregion

            #region 表
            if (menuData.Tables != null)
            {
                foreach (var table in menuData.Tables)
                {
                    var isStartWith = !table.Key.ToLower().StartsWith(searchText, true, null) &&
                                     !table.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !table.Key.ToLower().Contains(searchText) && !table.Key.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = currObjects.Where(x => x.ObjectName == table.Key).
                            GroupBy(x => x.GroupName).Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == LanguageHepler.GetLanguage("Table"));
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = table.Value.Id,
                                        Parent = ppGroup,
                                        DisplayName = table.Value.DisplayName,
                                        Name = table.Value.Name,
                                        Schema = table.Value.SchemaName,
                                        Comment = table.Value.Comment,
                                        CreateDate = table.Value.CreateDate,
                                        ModifyDate = table.Value.ModifyDate,
                                        Icon = TABLEICON,
                                        Type = ObjType.Table
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeTable.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = table.Value.Id,
                            Parent = nodeTable,
                            DisplayName = table.Value.DisplayName,
                            Name = table.Value.Name,
                            Schema = table.Value.SchemaName,
                            Comment = table.Value.Comment,
                            CreateDate = table.Value.CreateDate,
                            ModifyDate = table.Value.ModifyDate,
                            Icon = TABLEICON,
                            Type = ObjType.Table
                        });
                    }
                }
            }
            #endregion

            #region 视图
            if (menuData.Views != null)
            {
                foreach (var view in menuData.Views)
                {
                    var isStartWith = !view.Key.ToLower().StartsWith(searchText, true, null) && !view.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !view.Key.ToLower().Contains(searchText) && !view.Key.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = currObjects.Where(x => x.ObjectName == view.Key).
                            GroupBy(x => x.GroupName).Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == LanguageHepler.GetLanguage("View"));
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = view.Value.Id,
                                        Parent = ppGroup,
                                        DisplayName = view.Value.DisplayName,
                                        Name = view.Value.Name,
                                        Schema = view.Value.SchemaName,
                                        Comment = view.Value.Comment,
                                        CreateDate = view.Value.CreateDate,
                                        ModifyDate = view.Value.ModifyDate,
                                        Icon = VIEWICON,
                                        Type = ObjType.View
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeView.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = view.Value.Id,
                            Parent = nodeView,
                            DisplayName = view.Value.DisplayName,
                            Name = view.Value.Name,
                            Schema = view.Value.SchemaName,
                            Comment = view.Value.Comment,
                            CreateDate = view.Value.CreateDate,
                            ModifyDate = view.Value.ModifyDate,
                            Icon = VIEWICON,
                            Type = ObjType.View
                        });
                    }
                }
            }
            #endregion

            #region 存储过程
            if (menuData.Procedures != null)
            {
                foreach (var proc in menuData.Procedures)
                {
                    var isStartWith = !proc.Key.ToLower().StartsWith(searchText, true, null) && !proc.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !proc.Key.ToLower().Contains(searchText) && !proc.Key.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = currObjects.Where(x => x.ObjectName == proc.Key).GroupBy(x => x.GroupName)
                            .Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == LanguageHepler.GetLanguage("Procedure"));
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = proc.Value.Id,
                                        Parent = ppGroup,
                                        DisplayName = proc.Value.DisplayName,
                                        Name = proc.Value.Name,
                                        Schema = proc.Value.SchemaName,
                                        Comment = proc.Value.Comment,
                                        CreateDate = proc.Value.CreateDate,
                                        ModifyDate = proc.Value.ModifyDate,
                                        Icon = PROCICON,
                                        Type = ObjType.Proc
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeProc.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = proc.Value.Id,
                            Parent = nodeProc,
                            DisplayName = proc.Value.DisplayName,
                            Name = proc.Value.Name,
                            Schema = proc.Value.SchemaName,
                            Comment = proc.Value.Comment,
                            CreateDate = proc.Value.CreateDate,
                            ModifyDate = proc.Value.ModifyDate,
                            Icon = PROCICON,
                            Type = ObjType.Proc
                        });
                    }
                }
            }
            #endregion

            if (leftMenuType == LeftMenuType.Group.GetHashCode())
            {
                itemParentList.ForEach(group =>
                {
                    if (!group.Children.First(x => x.Name.Equals("treeTable")).Children.Any() && !group.Children.First(x => x.Name.Equals("treeView")).Children.Any() && !group.Children.First(x => x.Name.Equals("treeProc")).Children.Any())
                    {
                        group.Visibility = nameof(Visibility.Collapsed);
                    }
                    group.Children.ForEach(obj =>
                    {
                        if (!obj.Children.Any())
                        {
                            obj.Visibility = nameof(Visibility.Collapsed);
                        }
                        obj.DisplayName = $"{obj.DisplayName}({obj.Children.Count})";
                    });
                });
                if (itemParentList.All(x => x.Visibility != nameof(Visibility.Visible)))
                {
                    NoDataAreaText.TipText = LanguageHepler.GetLanguage("NoData");
                    NoDataText.Visibility = Visibility.Visible;
                }
                TreeViewData = itemParentList;
            }
            else
            {
                itemList.ForEach(obj =>
                {
                    if (!obj.Children.Any())
                    {
                        obj.Visibility = nameof(Visibility.Collapsed);
                    }
                    obj.DisplayName = $"{obj.DisplayName}({obj.Children.Count})";
                });
                if (itemList.All(x => x.Visibility != nameof(Visibility.Visible)))
                {
                    NoDataAreaText.TipText = LanguageHepler.GetLanguage("NoData");
                    NoDataText.Visibility = Visibility.Visible;
                }
                TreeViewData = itemList;
            }
            #endregion
        }

        private int curProgressNum = 0;
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedConnection = SelectedConnection;
            var selectedDatabase = SelectedDataBase;
            var exportData = TreeViewData;
            var floderPath = TxtPath.Text;
            var doctype = DocumentType();
            if (string.IsNullOrEmpty(doctype))
            {
                Oops.Oh(LanguageHepler.GetLanguage("PleaseSelectOutputDocumentType"));
                return;
            }
            var docTypeEnum = (DocType)(Enum.Parse(typeof(DocType), doctype));
            var checkAny = exportData.Count(x => x.Type == "Type" && x.IsChecked == false);
            if (checkAny == 3)
            {
                Oops.Oh(LanguageHepler.GetLanguage("PleaseSelectObjectsExport"));
                return;
            }
            if (CharacterList.Any(x => TxtFileName.Text.Contains(x)))
            {
                Oops.Oh($"{LanguageHepler.GetLanguage("NoDocumentSymbols")}：\\ / : * ? \" < > |");
                return;
            }
            if (string.IsNullOrEmpty(TxtFileName.Text))
            {
                var fName = SelectedDataBase.DbName;
                if (fName.Contains(":"))
                {
                    fName = fName.Replace(":", "_");
                }
                TxtFileName.Text = $"{fName}{LanguageHepler.GetLanguage("DatabaseDesignDocument")}";
            }
            //文档标题
            var docTitle = TxtFileName.Text.Trim();
            //文件名
            var fileName = docTitle + fileExt;
            LoadingG.Visibility = Visibility.Visible;
            var dbDto = new DBDto(selectedDatabase.DbName);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    // 导出文档当前进度
                    curProgressNum = 0;
                    // 导出文档总进度
                    var totalProgressNum = ExportTotalProgressNum(docTypeEnum, exportData);
                    //文档标题
                    dbDto.DocTitle = docTitle;
                    //数据库类型
                    dbDto.DBType = selectedConnection.DbType.ToString();
                    //对象列表
                    dbDto.Tables = Trans2Table(exportData, selectedConnection, selectedDatabase, totalProgressNum, docTypeEnum);
                    if (docTypeEnum != DocType.excel && docTypeEnum != DocType.html)
                    {
                        dbDto.Views = Trans2Dictionary(exportData, selectedConnection, selectedDatabase, "View", totalProgressNum, docTypeEnum);
                        dbDto.Procs = Trans2Dictionary(exportData, selectedConnection, selectedDatabase, "Proc", totalProgressNum, docTypeEnum);
                    }

                    //判断文档路径是否存在
                    if (!Directory.Exists(floderPath))
                    {
                        Directory.CreateDirectory(floderPath);
                    }
                    var filePath = Path.Combine(floderPath, fileName);
                    var doc = DocFactory.CreateInstance((DocType)Enum.Parse(typeof(DocType), doctype), dbDto);

                    doc.ChangeRefreshProgressEvent += Doc_ChangeRefreshProgressEvent;
                    var bulResult = doc.Build(filePath);
                    Dispatcher.Invoke(() =>
                    {
                        LoadingG.Visibility = Visibility.Collapsed;
                        if (bulResult)
                            Oops.Success(LanguageHepler.GetLanguage("ExportSuccessful"));
                        else
                            Oops.God(LanguageHepler.GetLanguage("ExportFailed"));
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadingG.Visibility = Visibility.Collapsed;
                        Oops.God($"{LanguageHepler.GetLanguage("ExportFailed")}，{LanguageHepler.GetLanguage("Reason")}：{ex.ToMsg()}");
                    });
                }
            }, TaskCreationOptions.LongRunning);
            #endregion
        }

        /// <summary>
        /// 导出文档进度信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Doc_ChangeRefreshProgressEvent(object sender, DocUtils.DBDoc.Doc.ChangeRefreshProgressArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var d = (e.BuildNum + e.TotalNum) * 1.0f / (e.TotalNum * 2) * 100;
                var pNum = double.Parse(d.ToString());
                LoadingG.ProgressNum = pNum;
                LoadingG.ProgressTitleNum = e.BuildNum.ToString() + " / " + e.TotalNum.ToString();
                LoadingG.ProgressTitle = !e.IsEnd ? $"{LanguageHepler.GetLanguage("GeneratingDocument")}：" + e.BuildName : LanguageHepler.GetLanguage("DocumentGenerationCompleted");
            }));
            Thread.Sleep(50);
        }

        private List<ViewProDto> Trans2Dictionary(List<TreeNodeItem> treeViewData, ConnectConfigs selectedConnection, DataBase selectedDatabase, string type, int totalProgressNum, DocType docType)
        {
            #region MyRegion
            var selectedConnectionString = selectedConnection.SelectedDbConnectString(selectedDatabase.DbName);
            var exporter = ExporterFactory.CreateInstance(selectedConnection.DbType, selectedConnectionString, selectedDatabase.DbName);
            var objectType = type == "View" ? DbObjectType.View : DbObjectType.Proc;
            var viewPro = new List<ViewProDto>();
            foreach (var group in treeViewData)
            {
                if (group.Name.Equals("treeTable"))
                {
                    continue;
                }
                if (group.IsChecked == false)
                {
                    continue;
                }
                if (group.Type == "Type")
                {
                    foreach (var item in group.Children)
                    {
                        if (item.IsChecked == false)
                        {
                            continue;
                        }
                        if (item.Type == type)
                        {
                            var script = exporter.GetScriptInfoById(item.ObejcetId, objectType);
                            viewPro.Add(new ViewProDto
                            {
                                ObjectName = item.Name,
                                Comment = item.Comment,
                                Script = script
                            });
                            curProgressNum++;
                            Dispatcher.Invoke(() =>
                            {
                                var tProgressNum = totalProgressNum * 2;
                                if (docType == DocType.xml || docType == DocType.html || docType == DocType.json)
                                {
                                    tProgressNum = totalProgressNum;
                                }
                                var d = (curProgressNum * 1.0f / tProgressNum) * 100;
                                var pNum = double.Parse(d.ToString());
                                var oName = type == "View" ? LanguageHepler.GetLanguage("View") : LanguageHepler.GetLanguage("Procedure");
                                LoadingG.ProgressNum = pNum;
                                LoadingG.ProgressTitleNum = curProgressNum.ToString() + " / " + totalProgressNum.ToString();
                                LoadingG.ProgressTitle = $"{LanguageHepler.GetLanguage("Getting")}{oName}{LanguageHepler.GetLanguage("Script")}：" + item.Name;
                            });
                        }
                    }
                }
                else
                {
                    if (group.Type == type)
                    {
                        var script = exporter.GetScriptInfoById(group.ObejcetId, objectType);
                        viewPro.Add(new ViewProDto
                        {
                            ObjectName = group.Name,
                            Comment = group.Comment,
                            Script = script
                        });
                    }
                }
            }
            return viewPro;
            #endregion
        }

        private List<TableDto> Trans2Table(List<TreeNodeItem> treeViewData, ConnectConfigs selectedConnection, DataBase selectedDatabase, int totalProgressNum, DocType docType)
        {
            #region MyRegion
            var selectedConnectionString = selectedConnection.SelectedDbConnectString(selectedDatabase.DbName);
            var tables = new List<TableDto>();
            var groupNo = 1;
            var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType,
                selectedConnectionString, selectedDatabase.DbName);
            foreach (var group in treeViewData)
            {
                if (group.Type == "Type" && group.Name.Equals("treeTable"))
                {
                    int orderNo = 1;
                    foreach (var node in group.Children)
                    {
                        if (node.IsChecked == false)
                        {
                            continue;
                        }
                        TableDto tbDto = new TableDto();
                        tbDto.TableOrder = orderNo.ToString();
                        tbDto.TableName = node.Name;
                        tbDto.Comment = node.Comment.FilterIllegalDir();
                        tbDto.DBType = selectedConnection.DbType.ToString();

                        var lst_col_dto = new List<ColumnDto>();
                        var columns = dbInstance.GetColumnInfoById(node.ObejcetId);
                        var columnIndex = 1;
                        foreach (var col in columns)
                        {
                            var colDto = new ColumnDto();
                            colDto.ColumnOrder = columnIndex.ToString();
                            colDto.ColumnName = col.Value.Name;
                            // 数据类型
                            colDto.ColumnTypeName = col.Value.DataType;
                            // 长度
                            colDto.Length = col.Value.LengthName;
                            // 小数位
                            //colDto.Scale = "";//(col.Scale.HasValue ? col.Scale.Value.ToString() : "");
                            // 主键
                            colDto.IsPK = col.Value.IsPrimaryKey ? "√" : "";
                            // 自增
                            colDto.IsIdentity = col.Value.IsIdentity ? "√" : "";
                            // 允许空
                            colDto.CanNull = col.Value.IsNullable ? "√" : "";
                            // 默认值
                            colDto.DefaultVal = !string.IsNullOrWhiteSpace(col.Value.DefaultValue) ? col.Value.DefaultValue : "";
                            // 列注释（说明）
                            colDto.Comment = col.Value.Comment.FilterIllegalDir();

                            lst_col_dto.Add(colDto);
                            columnIndex++;
                        }
                        tbDto.Columns = lst_col_dto;
                        tables.Add(tbDto);
                        orderNo++;
                        curProgressNum++;
                        Dispatcher.Invoke(() =>
                        {
                            var tProgressNum = totalProgressNum * 2;
                            if (docType == DocType.xml || docType == DocType.html || docType == DocType.json)
                            {
                                tProgressNum = totalProgressNum;
                            }
                            var d = (curProgressNum * 1.0f / tProgressNum) * 100;
                            var pNum = double.Parse(d.ToString());
                            LoadingG.ProgressNum = pNum;
                            LoadingG.ProgressTitleNum = curProgressNum.ToString() + " / " + totalProgressNum.ToString();
                            LoadingG.ProgressTitle = $"{LanguageHepler.GetLanguage("GettingTableStructure")}：" + node.Name;
                        });
                    }
                }
                if (group.Type == "Table")
                {
                    TableDto tbDto = new TableDto();
                    tbDto.TableOrder = groupNo.ToString();
                    tbDto.TableName = group.Name;
                    tbDto.Comment = group.Comment.FilterIllegalDir();
                    tbDto.DBType = selectedConnection.DbType.ToString();

                    var lst_col_dto = new List<ColumnDto>();
                    var columns = dbInstance.GetColumnInfoById(group.ObejcetId);
                    var columnIndex = 1;
                    foreach (var col in columns)
                    {
                        ColumnDto colDto = new ColumnDto();
                        colDto.ColumnOrder = columnIndex.ToString();
                        colDto.ColumnName = col.Value.Name;
                        // 数据类型
                        colDto.ColumnTypeName = col.Value.DataType;
                        // 长度
                        colDto.Length = col.Value.LengthName;
                        // 小数位
                        //colDto.Scale = "";//(col.Scale.HasValue ? col.Scale.Value.ToString() : "");
                        // 主键
                        colDto.IsPK = (col.Value.IsPrimaryKey ? "√" : "");
                        // 自增
                        colDto.IsIdentity = (col.Value.IsIdentity ? "√" : "");
                        // 允许空
                        colDto.CanNull = (col.Value.IsNullable ? "√" : "");
                        // 默认值
                        colDto.DefaultVal = (!string.IsNullOrWhiteSpace(col.Value.DefaultValue) ? col.Value.DefaultValue : "");
                        // 列注释（说明）
                        colDto.Comment = col.Value.Comment.FilterIllegalDir();
                        lst_col_dto.Add(colDto);
                        columnIndex++;
                    }
                    tbDto.Columns = lst_col_dto;
                    tables.Add(tbDto);
                    groupNo++;
                }
            }
            return tables;
            #endregion
        }

        private int ExportTotalProgressNum(DocType docType, List<TreeNodeItem> treeNodeItems)
        {
            var tableCount = treeNodeItems.Where(x => x.Type == "Type" && x.Name == "treeTable").Select(x => x.Children).First().Count(x => x.IsChecked == true);
            var viewCount = treeNodeItems.Where(x => x.Type == "Type" && x.Name == "treeView").Select(x => x.Children).First().Count(x => x.IsChecked == true);
            var procCount = treeNodeItems.Where(x => x.Type == "Type" && x.Name == "treeProc").Select(x => x.Children).First().Count(x => x.IsChecked == true);
            var sumCount = tableCount + viewCount + procCount;
            if (docType == DocType.excel || docType == DocType.html)
            {
                sumCount = tableCount;
            }
            return sumCount;
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

        /// <summary>
        /// 对应文档提示信息
        /// </summary>
        private static Dictionary<string, string> _docTypeTipMsg = new Dictionary<string, string>
        {
            {"excel",LanguageHepler.GetLanguage("ExportExcel")},
            {"pdf",LanguageHepler.GetLanguage("ExportPDF")},
            {"html",LanguageHepler.GetLanguage("ExportHtml")},
            {"xml",LanguageHepler.GetLanguage("ExportXML")},
            {"md",LanguageHepler.GetLanguage("ExportMarkDown")},
            {"json",LanguageHepler.GetLanguage("ExportJson")}
        };

        /// <summary>
        /// 文档对应文件扩展名
        /// </summary>
        private static Dictionary<string, string> _docExt = new Dictionary<string, string>
        {
            {"excel",".xlsx"},
            {"html",".html"},
            {"xml",".xml"},
            {"md",".md"},
            {"json",".json"}
        };

        /// <summary>
        /// 导出文档类型单选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle_OnChecked(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            //GridTipMsg.Visibility = Visibility.Collapsed;
            var button = (ToggleButton)sender;
            foreach (ToggleButton toggle in ToggleWarpPanel.Children)
            {
                if (toggle.Name != button.Name)
                {
                    toggle.IsChecked = false;
                }
            }
            var tipMsg = _docTypeTipMsg[button.Name.ToLower()];
            if (!string.IsNullOrEmpty(tipMsg))
            {
                //GridTipMsg.Visibility = Visibility.Visible;
                //TextDocTipMsg.Text = tipMsg;
            }
            fileExt = _docExt[button.Name.ToLower()];
            #endregion
        }

        private string DocumentType()
        {
            var type = string.Empty;
            foreach (ToggleButton button in ToggleWarpPanel.Children)
            {
                if (button.IsChecked == true)
                {
                    type = button.Content.ToString().ToLower();
                }
            }
            return type;
        }

        private void EventSetter_OnHandler(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void TxtFileName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var fileName = TxtFileName.Text;
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            if (CharacterList.Any(x => fileName.Contains(x)))
            {
                Oops.Oh($"{LanguageHepler.GetLanguage("NoDocumentSymbols")}：\\ / : * ? \" < > |");
            }
        }

        /// <summary>
        /// 特殊符号
        /// </summary>
        private static List<string> CharacterList = new List<string>
        {
           "\\","/", ":","*","?","\"","<",">","|"
        };

        private void TreeViewTables_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObject) || selectedObject.Type == ObjType.Group)
            {
                return;
            }
            TreeViewData.ForEach(x =>
            {
                x.Children.ForEach(item =>
                {
                    if (item.DisplayName == selectedObject.DisplayName)
                    {
                        item.IsChecked = !item.IsChecked;
                    }
                });
            });
        }
    }
}
