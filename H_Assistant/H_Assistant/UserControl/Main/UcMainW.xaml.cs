using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Models;
using H_Assistant.Views;
using System.Collections.Generic;
using System.Windows;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// MainW.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainW : BaseUserControl
    {
        #region Fields
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;

        public static readonly DependencyProperty MenuDataProperty = DependencyProperty.Register(
            "MenuData", typeof(Model), typeof(UcMainW), new PropertyMetadata(default(Model)));

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UcMainW), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcMainW), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcMainW), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty MainTitleProperty = DependencyProperty.Register(
            "MainTitle", typeof(string), typeof(UcMainW), new PropertyMetadata(default(string)));

        /// <summary>
        /// 菜单数据
        /// </summary>
        public Model MenuData
        {
            get => (Model)GetValue(MenuDataProperty);
            set => SetValue(MenuDataProperty, value);
        }

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public TreeNodeItem SelectedObject
        {
            get => (TreeNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前数据连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set
            {
                SetValue(SelectedConnectionProperty, value);
                OnPropertyChanged(nameof(SelectedConnection));

            }
        }

        public string MainTitle
        {
            get => (string)GetValue(MainTitleProperty);
            set => SetValue(MainTitleProperty, value);
        }
        #endregion

        public UcMainW()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadPage(List<TreeNodeItem> objectsViewData)
        {
            var liteHelper = LiteDBHelper.GetInstance();
            var isMultipleTab = liteHelper.GetSysBool("IsMultipleTab");
            if (isMultipleTab)
            {
                GridMultiple.Visibility = Visibility.Visible;
                GridSigle.Visibility = Visibility.Collapsed;
                if (SelectedObject.Type == ObjType.Type)
                {
                    MainColumns.Visibility = Visibility.Collapsed;
                    MainObjects.Visibility = Visibility.Visible;
                    MainObjects.MenuData = MenuData;
                    MainObjects.SelectedConnection = SelectedConnection;
                    MainObjects.SelectedDataBase = SelectedDataBase;
                    MainObjects.SelectedObject = SelectedObject;
                    MainObjects.ObjectsViewData = objectsViewData;
                    MainObjects.LoadPageData();
                }
                else
                {
                    MainColumns.Visibility = Visibility.Visible;
                    MainObjects.Visibility = Visibility.Collapsed;
                    MainColumns.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
                    MainColumns.SelectedConnection = SelectedConnection;
                    MainColumns.SelectedDataBase = SelectedDataBase;
                    MainColumns.SelectedObject = SelectedObject;
                    MainColumns.LoadPageData();
                }
            }
            else
            {
                GridMultiple.Visibility = Visibility.Collapsed;
                GridSigle.Visibility = Visibility.Visible;
                MainTitle = SelectedObject.DisplayName;
                if (SelectedObject.Type == ObjType.Type)
                {
                    MainColumn.Visibility = Visibility.Collapsed;
                    MainObject.Visibility = Visibility.Visible;
                    MainObject.MenuData = MenuData;
                    MainObject.SelectedConnection = SelectedConnection;
                    MainObject.SelectedDataBase = SelectedDataBase;
                    MainObject.SelectedObject = SelectedObject;
                    MainObject.ObjectsViewData = objectsViewData;
                    MainObject.LoadPageData();
                }
                else
                {
                    MainColumn.Visibility = Visibility.Visible;
                    MainObject.Visibility = Visibility.Collapsed;
                    MainColumn.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
                    MainColumn.SelectedConnection = SelectedConnection;
                    MainColumn.SelectedDataBase = SelectedDataBase;
                    MainColumn.SelectedObject = SelectedObject;
                    MainColumn.LoadPageData();
                }
            }
        }
    }
}
