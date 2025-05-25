using H_Assistant.DocUtils;
using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.Exporter;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Assistant.Models;
using H_Assistant.Views;
using HandyControl.Data;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = HandyControl.Controls.MessageBox;
using PathF = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainColumns : BaseUserControl
    {
        #region Filds
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UcMainColumns), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcMainColumns), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcMainColumns), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SourceColunmDataProperty = DependencyProperty.Register(
            "SourceColunmData", typeof(List<Column>), typeof(UcMainColumns), new PropertyMetadata(default(List<Column>)));

        public static readonly DependencyProperty ObjectColumnsProperty = DependencyProperty.Register(
            "ObjectColumns", typeof(List<Column>), typeof(UcMainColumns), new PropertyMetadata(default(List<Column>)));

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

        public UcMainColumns()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextSqlEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextSqlEditor.TextArea.SelectionCornerRadius = 0;
            TextSqlEditor.TextArea.SelectionBorder = null;
            TextSqlEditor.TextArea.SelectionForeground = null;
            TextSqlEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        /// <summary>
        /// 页面初始化加载数据
        /// </summary>
        public void LoadPageData()
        {
            #region MyRegion
            NoDataText.Visibility = Visibility.Collapsed;
            var selectedObject = SelectedObject;
            var selectedConnection = SelectedConnection;
            var selectedDatabase = SelectedDataBase;
            var dbConnectionString = selectedConnection.SelectedDbConnectString(SelectedDataBase.DbName);
            if (selectedObject.Type == ObjType.Table || selectedObject.Type == ObjType.View)
            {
                SearchColumns.Text = string.Empty;
                var isView = selectedObject.Type == ObjType.View;
                TabTable.SelectedItem = isView ? TabSql : TabStruct;
                LoadingG.Visibility = TabStruct.Visibility = Visibility.Visible;
                TabData.Visibility = Visibility.Collapsed;
                TabSql.Visibility = isView ? Visibility.Visible : Visibility.Collapsed;
                BtnCreateSqlScript.IsEnabled = !isView;
                var objName = isView ? LanguageHepler.GetLanguage("View") : LanguageHepler.GetLanguage("Table");
                TabStruct.Header = objName;
                TabData.Header = objName;
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
                        LoadingG.Visibility = Visibility.Collapsed;
                        if (!list.Any())
                        {
                            NoDataText.Visibility = Visibility.Visible;
                        }
                    }));
                    if (selectedObject.Type == ObjType.View)
                    {
                        var script = dbInstance.GetScriptInfoById(selectedObject.ObejcetId, DbObjectType.View);
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            TextSqlEditor.SelectedText = string.Empty;
                            TextSqlEditor.Text = script;
                        }));
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
                TabStruct.Visibility = Visibility.Collapsed;
                TabData.Visibility = Visibility.Collapsed;
                TabSql.Visibility = Visibility.Visible;
                TabTable.SelectedItem = TabSql;
                var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, dbConnectionString, selectedDatabase.DbName);
                Task.Run(() =>
                {
                    var script = dbInstance.GetScriptInfoById(selectedObject.ObejcetId, DbObjectType.Proc);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextSqlEditor.SelectedText = string.Empty;
                        TextSqlEditor.Text = script;
                    }));
                });
            }
            #endregion
        }


        /// <summary>
        /// 表结构绑定
        /// </summary>
        /// <param name="exporter"></param>
        /// <param name="objects"></param>
        private void BindColumnDataSet(IExporter exporter, TreeNodeItem objects)
        {
            //Task.Run(() =>
            //{
            //    TableColumns = exporter.GetColumnsExt(Convert.ToInt32(objects.ObejcetId), ConnectionString);
            //    var list = TableColumns.Values.ToList();
            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        CbTableColumnData.ItemsSource = list;
            //    }));
            //});
        }

        //表数据选项卡选中时加载数据
        private void TabTable_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
            {
                return;
            }
            var selectedItem = (System.Windows.Controls.TabItem)((System.Windows.Controls.TabControl)sender).SelectedItem;
            if (selectedItem.Name.Equals("TabData"))
            {
                var connectionString = SelectedConnection.DbMasterConnectString;
                var exporter = ExporterFactory.CreateInstance(SelectedConnection.DbType, connectionString, SelectedDataBase.DbName);
                if (TabData.IsSelected)
                {
                    SearchTableExt2.Text = "";
                    BindDataSet(exporter, SelectedObject, string.Empty);
                    BindColumnDataSet(exporter, SelectedObject);
                }
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
            NoDataText.Visibility = Visibility.Collapsed;
            var searchText = SearchColumns.Text.Trim();
            var searchData = ColList;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchData = ColList.Where(x => x.DisplayName.ToLower().Contains(searchText.ToLower()) || (!string.IsNullOrEmpty(x.Comment) && x.Comment.ToLower().Contains(searchText.ToLower()))).ToList();
            }
            NoDataText.Visibility = searchData.Any() ? Visibility.Collapsed : Visibility.Visible;
            ObjectColumns = searchData;
            #endregion
        }

        private string _cellEditValue = string.Empty;
        /// <summary>
        /// 单元格开始编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            #region MyRegion
            if (((DataGrid)sender).SelectedCells.Any())
            {
                //获取选中单元格(仅限单选)
                var selectedCell = ((DataGrid)sender).SelectedCells[0];
                //获取选中单元格数据
                var selectedData = selectedCell.Column.GetCellContent(selectedCell.Item);
                if (selectedData is TextBlock)
                {
                    var selectedText = (TextBlock)selectedData;
                    selectedText.Width = 420;
                    _cellEditValue = selectedText.Text;
                }
            }
            #endregion
        }

        /// <summary>
        /// 单元格编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement != null && e.EditingElement is TextBox)
            {
                var cell = (DataGridCell)e.EditingElement.Parent;
                //设置单元格中的TextBox宽度
                e.EditingElement.Width = cell.ActualWidth - 10;
            }
        }

        /// <summary>
        /// 单元格修改提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region MyRegion
            if (e.EditingElement != null && e.EditingElement is TextBox)
            {
                string newValue = (e.EditingElement as TextBox).Text;
                if (!_cellEditValue.Equals(newValue))
                {
                    if (SelectedObject.Type == ObjType.View)
                    {
                        ((TextBox)e.EditingElement).Text = _cellEditValue;
                        return;
                    }
                    var selectItem = (Column)e.Row.Item;
                    var liteDbInstance = LiteDBHelper.GetInstance();
                    var isShowSaveWin = liteDbInstance.GetSysBool(SysConst.Sys_IsShowSaveWin);
                    if (isShowSaveWin)
                    {
                        var msgResult = MessageBox.Show($"{LanguageHepler.GetLanguage("ConfirmModification")}{SelectedObject.DisplayName}{LanguageHepler.GetLanguage("TheRemarks")}{newValue}？", LanguageHepler.GetLanguage("Tips"), MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (msgResult != MessageBoxResult.OK)
                        {
                            ((TextBox)e.EditingElement).Text = _cellEditValue;
                            return;
                        }
                    }

                    var dbConnectionString = SelectedConnection.SelectedDbConnectString(SelectedDataBase.DbName);
                    try
                    {
                        var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, dbConnectionString, SelectedDataBase.DbName);
                        var objectType = SelectedObject.Type.Equals(ObjType.Table) ? DbObjectType.Table : (SelectedObject.Type.Equals(ObjType.View) ? DbObjectType.View : DbObjectType.Proc);
                        var editResult = dbInstance.UpdateColumnRemark(selectItem, newValue, objectType);
                        if (editResult)
                            Oops.Success(LanguageHepler.GetLanguage("SuccessfullyModified"));
                        else
                            Oops.God(LanguageHepler.GetLanguage("FailedModified"));
                    }
                    catch (NotSupportedException)
                    {
                        ((TextBox)e.EditingElement).Text = _cellEditValue;
                        Oops.Oh($"{SelectedConnection.DbType}{LanguageHepler.GetLanguage("NotOperation")}");
                    }
                    catch (Exception ex)
                    {
                        ((TextBox)e.EditingElement).Text = _cellEditValue;
                        Oops.God(LanguageHepler.GetLanguage("UpdateFailed") + ex.ToMsg());
                    }
                }
            }
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

        private void SearchTableExt2_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            #region MyRegion
            //if (e.Key == Key.Enter)
            //{
            //    var searchText = SearchTableExt2.Text;
            //    if (CbTableColumnData.SelectedItem == null)
            //    {
            //        return;
            //    }
            //    var objects = TreeViewTables.SelectedItem as TreeNodeItem;
            //    if (objects == null || objects.ObejcetId.Equals("0") || objects.TextColor.Equals("Red"))
            //    {
            //        return;
            //    }
            //    var selectColumn = (Column)CbTableColumnData.SelectedItem;
            //    var strWhere = "";
            //    if (!string.IsNullOrEmpty(searchText))
            //    {
            //        strWhere = $" {selectColumn.DisplayName} like '{searchText}%'";
            //    }
            //    IExporter exporter = new SqlServer2008Exporter();
            //    BindDataSet(exporter, objects, strWhere);
            //}
            #endregion
        }

        /// <summary>
        /// 分页页码变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pagination_OnPageUpdated(object sender, FunctionEventArgs<int> e)
        {
            //var pageIndex = PaginationTable.PageIndex;
        }

        /// <summary>
        /// 设置分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetGroup_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            //选中的表对象
            if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectTable"));
                return;
            }
            var liteDBHelper = LiteDBHelper.GetInstance();
            var list = liteDBHelper.db.GetCollection<GroupInfo>().Query().Where(x =>
                x.ConnectId == SelectedConnection.ID && x.DataBaseName == SelectedDataBase.DbName).ToList();
            if (!list.Any())
            {
                Oops.Oh(LanguageHepler.GetLanguage("NoGroup"));
                return;
            }
            var mainWindow = System.Windows.Window.GetWindow(this);
            var group = new SetGroup();
            group.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
            group.SelectedConnection = SelectedConnection;
            group.SelectedDataBase = SelectedDataBase.DbName;
            group.SelectedObjects = new List<TreeNodeItem>() { SelectedObject };
            group.Owner = mainWindow;
            group.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetTag_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            //选中的表对象
            if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectTable"));
                return;
            }
            var liteDBHelper = LiteDBHelper.GetInstance();
            var list = liteDBHelper.db.GetCollection<TagInfo>().Query().Where(x =>
                x.ConnectId == SelectedConnection.ID && x.DataBaseName == SelectedDataBase.DbName).ToList();
            if (!list.Any())
            {
                Oops.Oh(LanguageHepler.GetLanguage("NoLabel"));
                return;
            }
            var mainWindow = System.Windows.Window.GetWindow(this);
            var setTag = new SetTag();
            setTag.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
            setTag.SelectedConnection = SelectedConnection;
            setTag.SelectedDataBase = SelectedDataBase.DbName;
            setTag.SelectedObjects = new List<TreeNodeItem>() { SelectedObject };
            setTag.Owner = mainWindow;
            setTag.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 新建字段脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreateSqlScript_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectRows = GetTableGridSelectRows();
            if (!selectRows.Any())
            {
                if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
                {
                    Oops.Oh(LanguageHepler.GetLanguage("SelectTable"));
                    return;
                }
                selectRows = SourceColunmData;
            }
            var mainWindow = System.Windows.Window.GetWindow(this);
            var scriptW = new ScriptWindow();
            scriptW.SelectDatabase = SelectedDataBase;
            scriptW.SelectedConnection = SelectedConnection;
            scriptW.SelectedObject = SelectedObject;
            scriptW.SelectedColumns = selectRows;
            scriptW.Owner = mainWindow;
            scriptW.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 生成配置---详细页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreateTest_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectRows = GetTableGridSelectRows();
            if (!selectRows.Any())
            {
                if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
                {
                    Oops.Oh(LanguageHepler.GetLanguage("SelectTable"));
                    return;
                }
                selectRows = SourceColunmData;
            }
            var mainWindow = Window.GetWindow(this);
            var testObjectsW = new TestObjects();
            testObjectsW.SelectedConnection = SelectedConnection;
            testObjectsW.SelectedObject = SelectedObject;
            testObjectsW.SelectedDataBase = SelectedDataBase;
            testObjectsW.Owner = mainWindow;
            testObjectsW.ShowDialog();
            #endregion
        }

        /// <summary>
        /// 获取表结构选中行
        /// </summary>
        /// <returns></returns>
        public List<Column> GetTableGridSelectRows()
        {
            var count = TableGrid.SelectedCells.Count;
            var list = new List<Column>(count);
            for (int i = 0; i < count; i++)
            {
                var col = (Column)TableGrid.SelectedCells[i].Item;
                list.Add(col);
            }
            return list.Distinct().ToList();
        }

        /// <summary>BtnSetGroup_OnClick
        /// 生成C#实体类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreateEntity_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            string baseDirectoryPath = PathF.Combine(AppDomain.CurrentDomain.BaseDirectory, "EntityTemp");
            if (!Directory.Exists(baseDirectoryPath))
            {
                Directory.CreateDirectory(baseDirectoryPath);
            }
            if (SelectedObject == null || SelectedObject.ObejcetId.Equals("0") || SelectedObject.TextColor.Equals("Red"))
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectTableCSharp"));
                return;
            }
            var filePath = string.Format($"{baseDirectoryPath}\\{SelectedObject.Name}.cs");
            StrUtil.CreateClass(filePath, SelectedObject.Name, SourceColunmData);
            Oops.Success(LanguageHepler.GetLanguage("EntityGenerationSuccess"));
            Process.Start(baseDirectoryPath);
            #endregion
        }

        /// <summary>
        /// 表数据绑定
        /// </summary>
        /// <param name="exporter"></param>
        /// <param name="objects"></param>
        /// <param name="strWhere"></param>
        private void BindDataSet(IExporter exporter, TreeNodeItem objects, string strWhere)
        {
            #region MyRegion
            //LoadingLineTableData.Visibility = Visibility.Visible;
            //NoDataTextExt.Visibility = Visibility.Collapsed;
            //var connectionString = SelectedConnection.DbMasterConnectString;
            //Task.Run(() =>
            //{
            //    DataSet dataSet = exporter.GetDataSet(connectionString, objects.DisplayName, strWhere);
            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        TableDataGrid.ItemsSource = null;
            //        //编写获取数据并显示在界面的代码
            //        var dataView = dataSet.Tables[0].DefaultView;
            //        TableDataGrid.ItemsSource = dataView;
            //        LoadingLineTableData.Visibility = Visibility.Hidden;
            //        if (dataView.Count < 1)
            //        {
            //            NoDataTextExt.Visibility = Visibility.Visible;
            //        }
            //    }));
            //}); 
            #endregion
        }

        /// <summary>
        /// 复制脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopyScript_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextSqlEditor.Text))
            {
                TextSqlEditor.SelectAll();
                Clipboard.SetDataObject(TextSqlEditor.Text);
                Oops.Success(LanguageHepler.GetLanguage("ScriptCopyClipboard"));
            }
        }

        /// <summary>
        /// 格式化SQL代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormat_OnClick(object sender, RoutedEventArgs e)
        {
            TextSqlEditor.Text = TextSqlEditor.Text.SqlFormat();
        }
    }
}
