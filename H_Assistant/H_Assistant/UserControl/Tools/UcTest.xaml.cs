using H_Assistant.DocUtils;
using H_Assistant.Framework;
using H_Assistant.Helper;
using H_Assistant.UserControl.Controls;
using H_Assistant.Views;
using H_Assistant.Views.Category;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using static H_Assistant.DocUtils.WhoUsePort;
using static H_Assistant.Helper.TestModel;
using MessageBox = HandyControl.Controls.MessageBox;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcTest : BaseUserControl
    {
        XSSFWorkbook wk = new XSSFWorkbook();//excel
        List<TestModel.Sheet> sList = new List<TestModel.Sheet>();//工作簿
        List<TestModel.Acell> aList = new List<TestModel.Acell>();//数据头
        int indexField = 2;//字段名开始行
        int indexType = 3;//字段类型开始行
        int indexData = 7;//数据开始行
        Assembly asm;//加载dll


        public UcTest()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 关键字设定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnKeywords_Click(object sender, RoutedEventArgs e)
        {
            TestKeywordsEditView testKeywordsEditView = new TestKeywordsEditView();
            testKeywordsEditView.ShowDialog();
        }
        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var msResult = MessageBox.Show(LanguageHepler.GetLanguage("IsClear"), LanguageHepler.GetLanguage("Tip"), MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
            if (msResult == MessageBoxResult.OK)
            {
                txtLog.Text = ""; 
            }
        }
        /// <summary>
        /// 选中工作簿
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbSheet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSheet.SelectedIndex == 0) { return; }
            try
            {
                #region 默认值
                if (cmbPlan.Items.Count > 0)
                {
                    cmbPlan.ItemsSource = null;
                    cmbPlan.Items.Clear();
                }
                aList = new List<Acell>();
                Acell aModel = new Acell();
                aModel.rowIndex = 0;
                aModel.Name = "選んでください";
                aList.Add(aModel);
                #endregion
                ISheet sheet = wk.GetSheetAt(cmbSheet.SelectedIndex - 1);   //读取当前表数据
                #region 从C列获取计划下拉框

                #endregion

                //for (int i = 0; i <= sheet.LastRowNum; i++)  //LastRowNum 是当前表的总行数
                //{
                //    IRow row = sheet.GetRow(i);  //读取当前行数据
                //    if (row != null)
                //    {
                //        ICell cell = row.GetCell(0);  //当前表格_第一列 取测试方案名称
                //        if (cell != null)
                //        {
                //            if (cell.ToString() != "")
                //            {
                //                aModel = new Acell();
                //                aModel.rowIndex = i + 1;
                //                aModel.Name = cell.ToString();
                //                aList.Add(aModel);
                //            }
                //        }
                //    }
                //}
                //for (int i = 1; i < aList.Count; i++)//循环方案
                //{
                //    aList[i].tableList = new List<XlxsTable>();
                //    for (int rowIndex = aList[i].rowIndex; rowIndex < sheet.LastRowNum; rowIndex++)
                //    {
                //        IRow row_name = sheet.GetRow(rowIndex);//获取表名行
                //        if (row_name == null)
                //        {
                //            continue;
                //        }
                //        else
                //        {
                //            ICell cell0 = row_name.GetCell(0);//获取方案列
                //            ICell cell1 = row_name.GetCell(1);//获取方テーブル名
                //            if (cell0 == null && cell1 == null)
                //            {
                //                continue;
                //            }
                //            else if (cell0 != null && cell1 == null)
                //            {
                //                break;
                //            }
                //        }
                //        IRow row_field = sheet.GetRow(rowIndex + indexField);//获取字段行
                //        IRow row_type = sheet.GetRow(rowIndex + indexType);//获取数据类行
                //        ICell cell_title = row_name.GetCell(1);//表名行——获取B列
                //        ICell cell_value = row_name.GetCell(2);//表名行——获取C列
                //        if (cell_title != null)
                //        {
                //            if (cell_title.ToString().Contains("テーブル名"))
                //            {
                //                XlxsTable model = new XlxsTable();

                //                #region 表名处理
                //                if (cell_value == null)
                //                {
                //                    txtLog.Text += $"{rowIndex + 1}行にテーブル名はありません。" + Environment.NewLine;
                //                }
                //                else
                //                {
                //                    if (cell_value.ToString().Contains("("))
                //                    {
                //                        model.Title = cell_value.ToString().Split('(')[0].Trim();
                //                        model.Name = cell_value.ToString().Split('(')[1].Replace(")", "").Trim();
                //                    }
                //                    else
                //                    {
                //                        model.Title = cell_value.ToString();
                //                        model.Name = cell_value.ToString();
                //                    }
                //                }
                //                #endregion

                //                #region 装载数据
                //                Type modelType = asm.GetType("Models." + model.Name);//获取指定名称的类型
                //                model.List = new List<object>();
                //                for (int datarow = rowIndex + indexData; datarow < sheet.LastRowNum; datarow++)
                //                {
                //                    IRow row_data = sheet.GetRow(datarow);//获取数据行
                //                    if (row_data == null)
                //                    {
                //                        rowIndex = datarow;//往下寻找表格
                //                        break;
                //                    }
                //                    ICell cell_no = row_data.GetCell(1);//序号列
                //                    if (cell_no == null)
                //                    {
                //                        rowIndex = datarow;//往下寻找表格
                //                        break;
                //                    }
                //                    if (cell_no.ToString() == "")
                //                    {
                //                        rowIndex = datarow;//往下寻找表格
                //                        break;
                //                    }

                //                    //object  aa= Activator.CreateInstance(modelType, null);//创建指定类型实例
                //                    object tModel = Activator.CreateInstance(modelType, null);//创建指定类型实例
                //                    for (int cellIndex = 2; cellIndex < row_field.LastCellNum; cellIndex++)//循环字段名列
                //                    {
                //                        ICell cell_data = row_data.GetCell(cellIndex);//数据列
                //                        ICell cell_field = row_field.GetCell(cellIndex);//字段列
                //                        ICell cell_type = row_type.GetCell(cellIndex);//类型列

                //                        if (cell_field == null && cell_type == null)
                //                        { break; }
                //                        else
                //                        {
                //                            if (cell_field.ToString() == "" && cell_type.ToString() == "")
                //                            { break; }
                //                            else
                //                            {
                //                                object value = GetValue(datarow, model.Name, cell_type.ToString(), cell_field.ToString(), cell_data);
                //                                SetValue(tModel, cell_field.ToString(), value);
                //                            }
                //                        }
                //                    }
                //                    model.List.Add(tModel);//追加数据
                //                }
                //                #endregion

                //                aList[i].tableList.Add(model);//追加数据集合
                //            }
                //            else //结束行
                //            {
                //                continue;
                //            }
                //        }

                //    }
                //}
                //#region sheet绑定                   
                //cmbPlan.SelectedItem = aList;
                //cmbPlan.SelectedValuePath = "rowIndex";
                //cmbPlan.DisplayMemberPath = "Name";
                //cmbPlan.SelectedIndex = 0;
                //#endregion
                ////打印扫描的结果

            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 选中计划
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region 从E列获取表名下拉框

            #endregion
        }
       
        /// <summary>
        /// 还原
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRevert_Click(object sender, RoutedEventArgs e)
        {
            // 根据投入的条件生成 删除语句 打印出来
            // 投入备份数据 还原
        }
        /// <summary>
        /// 投入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            // 1.选中表 解析所有主键条件   生成select 语句 打印出来
            // 2.检索出来的数据 从新生成 insert语句  用于还原用 打印出来
            // 3.根据1. 解析主键条件数据  生成delete语句 打印出来
            // 4.解析excel  + 关键字替换  生成 insert 语句 打印出来
            
            // 5.执行 3.的删除语句
            // 6.执行 4.的新增语句
        }
        /// <summary>
        /// 选择excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                asm = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + "dll\\" + TestHelper.DllName);
                Type modelType = asm.GetType("Models.USER_MENU_BUTTON_RELATION");//获取指定名称的类型
                object tModel = Activator.CreateInstance(modelType, null);//创建指定类型实例
                var dc = new Dictionary<string, object>();
                dc.Add("MENU_ID", "1111");
                dc.Add("BUTTON_ID", "1111");
                var aa1a = TestHelper.DB.Insertable(dc).AS("USER_MENU_BUTTON_RELATION").ExecuteCommand().ToSqlValue();//执行
                var aa11a = TestHelper.DB.Insertable(dc).AS("USER_MENU_BUTTON_RELATION").ToSql();// sql
                var aa21a = TestHelper.DB.Updateable(dc).AS("USER_MENU_BUTTON_RELATION").Where("2=3").ExecuteCommand().ToSqlValue();//执行
                var aa121a = TestHelper.DB.Updateable(dc).AS("USER_MENU_BUTTON_RELATION").Where("1=1").ToSql();// sql
                var aa3a = TestHelper.DB.Deleteable<object>().AS("USER_MENU_BUTTON_RELATION").Where("1=1").ExecuteCommand().ToSqlValue();//执行
                var aa41a = TestHelper.DB.Deleteable<object>().AS("USER_MENU_BUTTON_RELATION").Where("1=1").ToSql();// sql
                var aaaaa = TestHelper.DB.Deleteable(tModel).ToSql();
                var aa31a = TestHelper.DB.Queryable<dynamic>().AS("USER_MENU_BUTTON_RELATION").Where("1=1").ToList();//执行
                var aa411a = TestHelper.DB.Queryable<dynamic>().AS("USER_MENU_BUTTON_RELATION").Where("1=1").ToSql();// sql
            }
            catch (Exception ex)
            {
                Oops.God("加载失败！");
            }
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "処理するファイル";
            dialog.Filter = "処理するファイル(*.xlsx)|*.xlsx";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string file = dialog.FileName;
                try
                {
                    #region 默认值
                    if (cmbSheet.Items.Count > 0)
                    {
                        cmbSheet.ItemsSource = null;
                        cmbSheet.Items.Clear();
                    }
                    sList = new List<TestModel.Sheet>();
                    TestModel.Sheet sModel = new TestModel.Sheet();
                    sModel.Index = 0;
                    sModel.Name = "--";
                    sList.Add(sModel);
                    #endregion

                    using (FileStream fs = File.OpenRead(file))   //打开myxls.xls文件
                    {
                        wk = new XSSFWorkbook(fs);   //把xls文件中的数据写入wk中
                        for (int i = 0; i < wk.NumberOfSheets; i++)  //NumberOfSheets是myxls.xls中总共的表数
                        {
                            sModel = new TestModel.Sheet();
                            sModel.Index = i + 1;
                            sModel.Name = wk.GetSheetName(i);
                            sList.Add(sModel);
                        }
                    }
                    #region sheet绑定                   
                    cmbSheet.ItemsSource = sList;
                    cmbSheet.SelectedValuePath = "Index";
                    cmbSheet.DisplayMemberPath = "Name";
                    cmbSheet.SelectedIndex = 0;
                    #endregion
                    txtLog.Text += $"ファイルの読み取りに成功しました。" + Environment.NewLine;
                }
                catch (Exception ex)
                {
                    Oops.God("ファイルは別のアプリケーションによって開かれています。ファイルへの参照を閉じてください。");
                }
            }
            else
            {
                if (cmbSheet.Items.Count > 0)
                {
                    cmbSheet.SelectedItem = null;
                    cmbSheet.Items.Clear();
                }
                if (cmbPlan.Items.Count > 0)
                {
                    cmbPlan.SelectedItem = null;
                    cmbPlan.Items.Clear();
                }
            }
        }
    }
}
