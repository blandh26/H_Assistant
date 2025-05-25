using H_Assistant.DocUtils;
using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.Exporter;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Assistant.Models;
using HandyControl.Data;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using H_Assistant.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H_Assistant.Views
{
    /// <summary>
    /// TestObjects.xaml 的交互逻辑
    /// </summary>
    public partial class TestObjects : INotifyPropertyChanged
    {
        #region Filds

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(TestObjects), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(TestObjects), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(TestObjects), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SourceColunmDataProperty = DependencyProperty.Register(
            "SourceColunmData", typeof(List<Column>), typeof(TestObjects), new PropertyMetadata(default(List<Column>)));

        public static readonly DependencyProperty ObjectColumnsProperty = DependencyProperty.Register(
            "ObjectColumns", typeof(List<Column>), typeof(TestObjects), new PropertyMetadata(default(List<Column>)));

        public static readonly DependencyProperty SelectedColumnsProperty = DependencyProperty.Register(
           "SelectedColumns", typeof(List<Column>), typeof(TestObjects), new PropertyMetadata(default(List<Column>)));
        /// <summary>
        /// 选中字段
        /// </summary>
        public List<Column> SelectedColumns
        {
            get => (List<Column>)GetValue(SelectedColumnsProperty);
            set => SetValue(SelectedColumnsProperty, value);
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
            set => SetValue(SelectedConnectionProperty, value);
        }
        /// <summary>
        /// 当前选中表列数据
        /// </summary>
        public List<Column> SourceColunmData
        {
            get => (List<Column>)GetValue(SourceColunmDataProperty);
            set
            {
                SetValue(SourceColunmDataProperty, value);
                OnPropertyChanged(nameof(SourceColunmData));
            }
        }

        public List<Column> ObjectColumns
        {
            get => (List<Column>)GetValue(ObjectColumnsProperty);
            set
            {
                SetValue(ObjectColumnsProperty, value);
                OnPropertyChanged(nameof(ObjectColumns));
            }
        }

        #endregion

        public TestObjects()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
        }

        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestObjects_OnLoaded(object sender, RoutedEventArgs e)
        {
            var objectName = SelectedObject.DisplayName;
            var colList = SelectedColumns;
            LoadPageData();
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkAll_OnClick(object sender, RoutedEventArgs e)
        {
            var isChecked = ((CheckBox)sender).IsChecked;
            var selectedItem = ObjectColumns;
            selectedItem.ForEach(x =>
            {
                x.IsChecked = isChecked;
            });
            ObjectColumns = selectedItem;
            OnPropertyChanged();
        }

        /// <summary>
        /// 页面初始化加载数据
        /// </summary>
        public void LoadPageData()
        {
            #region MyRegion
            var selectedObject = SelectedObject;
            var selectedConnection = SelectedConnection;
            var selectedDatabase = SelectedDataBase;
            var dbConnectionString = SelectedConnection.SelectedDbConnectString(SelectedDataBase.DbName);
            if (selectedObject.Type == ObjType.Table )
            {
                SearchColumns.Text = string.Empty;
                var isView = selectedObject.Type == ObjType.View;
                var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, dbConnectionString, selectedDatabase.DbName);
                Task.Run(() =>
                {
                    var tableColumns = dbInstance.GetColumnInfoById(selectedObject.ObejcetId);
                    var list = tableColumns.Values.ToList();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SourceColunmData = list;
                        ObjectColumns = list;
                        ColList = list;
                    }));
                    if (selectedObject.Type == ObjType.View)
                    {
                        var script = dbInstance.GetScriptInfoById(selectedObject.ObejcetId, DbObjectType.View);
                    }
                });
                //if (TabData.IsSelected)
                //{
                //    SearchTableExt2.Text = "";
                //    //BindDataSet(exporter, objects, string.Empty);
                //    BindColumnDataSet(exporter, selectedObjct);
                //}
            }
            else
            {
                // 不是表类型
                Oops.Oh(LanguageHepler.GetLanguage("NotTableType"));
            }
            #endregion
        }

        private List<Column> ColList;
        /// <summary>
        /// 表数据检索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchColumns_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            #region MyRegion
            var searchText = SearchColumns.Text.Trim();
            var searchData = ColList;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchData = ColList.Where(x => x.DisplayName.ToLower().Contains(searchText.ToLower()) || (!string.IsNullOrEmpty(x.Comment) && x.Comment.ToLower().Contains(searchText.ToLower()))).ToList();
            }
            ObjectColumns = searchData;
            #endregion
        }

        /// <summary>
        /// 复制选中单元格的文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            #region MyRegion
            if (((DataGrid)sender).SelectedCells.Any())
            {
                //获取选中单元格(仅限单选)
                var selectedCell = ((DataGrid)sender).SelectedCells[0];
                //获取选中单元格数据
                var selectedData = selectedCell.Column.GetCellContent(selectedCell.Item);
                if (selectedData is TextBlock selectedText)
                {
                    //Clipboard.SetDataObject(selectedText.Text);
                    //
                }
            }
            #endregion
        }

        private void Handled_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }


        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = ObjectColumns;
            this.Close();
        }
    }
}
