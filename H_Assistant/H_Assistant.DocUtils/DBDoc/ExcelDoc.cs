using H_Assistant.DocUtils.Dtos;
using H_Assistant.DocUtils.Excel;
using NPOI;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using H_Assistant.Framework;

namespace H_Assistant.DocUtils.DBDoc
{
    public class ExcelDoc : Doc
    {
        public ExcelDoc(DBDto dto, string filter = "excel files (.xlsx)|*.xlsx") : base(dto, filter)
        {

        }

        public override bool Build(string filePath)
        {
            ExportExcel(filePath, this.Dto);
            return true;
        }
        public override bool Template(string filePath)
        {
            ExportTemplate(filePath, this.Dto);
            return true;
        }

        /// <summary>
        /// 引用NPOI导出excel数据库字典文档
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="databaseName"></param>
        /// <param name="tables"></param>
        public void ExportExcel(string fileName, DBDto dto)
        {
            var tables = dto.Tables;
            FileInfo xlsFileInfo = new FileInfo(fileName);
            if (xlsFileInfo.Exists)
            {
                xlsFileInfo.Delete();//  注意此处：存在Excel文档即删除再创建一个
            }
            try
            {
                XSSFWorkbook workbook = new XSSFWorkbook();//创建excel
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    CreateLogSheet(workbook, AppConst.LOG_CHAPTER_NAME, tables);//  创建overview sheet
                    CreateOverviewSheet(workbook, AppConst.TABLE_CHAPTER_NAME, dto, tables);//  创建overview sheet
                    CreateTableSheet(workbook, AppConst.TABLE_STRUCTURE_CHAPTER_NAME, dto, tables);//  创建tables sheet
                    workbook.Write(fs);//写入到Excel中
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            // 更新进度
            base.OnProgress(new ChangeRefreshProgressArgs
            {
                BuildNum = tables.Count,
                TotalNum = tables.Count,
                IsEnd = true
            });
        }

        /// <summary>
        /// 创建修订日志sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private void CreateLogSheet(IWorkbook workbook, string sheetName, List<TableDto> tables)
        {
            ISheet worksheet = workbook.CreateSheet(sheetName);
            ICellStyle cellStyle = NpoiHelper.GetCellStyle(workbook, 1, true, true, false);// 单元格设定
            #region 前60行设置
            for (int i = 0; i < 62; i++)
            {
                IRow row = worksheet.CreateRow(i);
                row.CreateCell(0).CellStyle = cellStyle;
                row.CreateCell(1).CellStyle = cellStyle;
                row.CreateCell(2).CellStyle = cellStyle;
                row.CreateCell(3).CellStyle = cellStyle;
                row.CreateCell(4).CellStyle = cellStyle;
                row.CreateCell(5).CellStyle = cellStyle;
                row.Height = 340;
                if (i >= 2) { row.GetCell(0).SetCellValue(i - 1); }
            }
            // 设置宽度
            worksheet.SetColumnWidth(0, 1500);
            worksheet.SetColumnWidth(1, 5000);
            worksheet.SetColumnWidth(2, 5000);
            worksheet.SetColumnWidth(3, 14000);
            worksheet.SetColumnWidth(4, 4000);
            worksheet.SetColumnWidth(5, 4000);
            worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));// 合并单元格，首行、末行、首列、末列
            #endregion
            IRow row2 = worksheet.GetRow(1);// 获取当前工作簿的2行
            row2.CreateCell(0).SetCellValue(LanguageHepler.GetLanguage("ExcelDocNumber"));
            row2.CreateCell(1).SetCellValue(LanguageHepler.GetLanguage("ExcelDocVersionNumber"));
            row2.CreateCell(2).SetCellValue(LanguageHepler.GetLanguage("ExcelDocRevisionDate"));
            row2.CreateCell(3).SetCellValue(LanguageHepler.GetLanguage("ExcelDocRevisedContent"));
            row2.CreateCell(4).SetCellValue(LanguageHepler.GetLanguage("ExcelDocRevisedBy"));
            row2.CreateCell(5).SetCellValue(LanguageHepler.GetLanguage("ExcelDocReviewedBy"));
            ICellStyle cellStyle2 = NpoiHelper.GetCellStyle(workbook, 1, true, true, true);// 单元格设定
            row2.GetCell(0).CellStyle = cellStyle2;
            row2.GetCell(1).CellStyle = cellStyle2;
            row2.GetCell(2).CellStyle = cellStyle2;
            row2.GetCell(3).CellStyle = cellStyle2;
            row2.GetCell(4).CellStyle = cellStyle2;
            row2.GetCell(5).CellStyle = cellStyle2;
        }

        /// <summary>
        /// 创建表目录sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private void CreateOverviewSheet(IWorkbook workbook, string sheetName, DBDto dto, List<TableDto> tables)
        {
            ISheet worksheet = workbook.CreateSheet(sheetName);
            ICellStyle cellStyle_title = NpoiHelper.GetCellStyle(workbook, 1, true, true, true, 73);       // 单元格设定  居中     加粗
            ICellStyle cellStyle_center = NpoiHelper.GetCellStyle(workbook, 1, true, true, false);     // 单元格设定  居中     不加粗
            ICellStyle cellStyle_left = NpoiHelper.GetCellStyle(workbook, 1, false, true, false);      // 单元格设定  不居中   不加粗
            worksheet.SetColumnWidth(0, 1500);
            worksheet.SetColumnWidth(1, 14000);
            worksheet.SetColumnWidth(2, 14000);
            worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 2));// 合并单元格，首行、末行、首列、末列

            IRow rowTitle1 = worksheet.CreateRow(0);
            rowTitle1.CreateCell(0).CellStyle = cellStyle_title;
            rowTitle1.CreateCell(1).CellStyle = cellStyle_title;
            rowTitle1.CreateCell(2).CellStyle = cellStyle_title;
            rowTitle1.GetCell(0).SetCellValue(dto.DocTitle);
            IRow rowTitle2 = worksheet.CreateRow(1);
            rowTitle2.CreateCell(0).CellStyle = cellStyle_title;
            rowTitle2.CreateCell(1).CellStyle = cellStyle_title;
            rowTitle2.CreateCell(2).CellStyle = cellStyle_title;
            rowTitle2.GetCell(0).SetCellValue(LanguageHepler.GetLanguage("ExcelDocNumber"));
            rowTitle2.GetCell(1).SetCellValue(LanguageHepler.GetLanguage("ExcelDocTableName"));
            rowTitle2.GetCell(2).SetCellValue(LanguageHepler.GetLanguage("ExcelDocDescription"));
            int row_index = 2; // 行
            int urlRow = 1;
            int rowIndex = 1;
            foreach (var table in tables)
            {
                IRow row = worksheet.CreateRow(row_index);
                row.CreateCell(0).CellStyle = cellStyle_center;
                row.CreateCell(1).CellStyle = cellStyle_left;
                row.CreateCell(2).CellStyle = cellStyle_left;
                IHyperlink link = new XSSFHyperlink(HyperlinkType.Document);
                link.Address = "表!A" + urlRow;
                urlRow += table.Columns.Count + 3;
                row.GetCell(0).SetCellValue(rowIndex);
                row.GetCell(0).Hyperlink = link;
                row.GetCell(1).SetCellValue(table.TableName);
                row.GetCell(2).SetCellValue(!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");
                row_index += 1;
                rowIndex += 1;
            }
        }

        /// <summary>
        /// 创建表结构sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private void CreateTableSheet(IWorkbook workbook, string sheetName, DBDto dto, List<TableDto> tables)
        {
            ISheet worksheet = workbook.CreateSheet(sheetName);
            ICellStyle cellStyle_center_pk = NpoiHelper.GetCellStyle(workbook, 1, true, true, false, 45);    // 单元格设定  居中     不加粗
            ICellStyle cellStyle_left_pk = NpoiHelper.GetCellStyle(workbook, 1, false, true, false, 45);    // 单元格设定  不居中   不加粗
            ICellStyle cellStyle_center_null = NpoiHelper.GetCellStyle(workbook, 1, true, true, false, 43);// 单元格设定  居中     不加粗
            ICellStyle cellStyle_left_null = NpoiHelper.GetCellStyle(workbook, 1, false, true, false, 43); // 单元格设定  不居中   不加粗
            ICellStyle cellStyle_center = NpoiHelper.GetCellStyle(workbook, 1, true, true, false);     // 单元格设定  居中     不加粗
            ICellStyle cellStyle_left = NpoiHelper.GetCellStyle(workbook, 1, false, true, false);      // 单元格设定  不居中   不加粗
            ICellStyle cellStyle_title = NpoiHelper.GetCellStyle(workbook, 1, true, true, true, 73);       // 单元格设定  居中     加粗
            var colTileName = new List<string> {
                    LanguageHepler.GetLanguage("ExcelDocNumber"),// 序号
                    LanguageHepler.GetLanguage("ExcelDocColumnName"),// 列名
                    LanguageHepler.GetLanguage("ExcelDocDataType"),// 数据类型
                    LanguageHepler.GetLanguage("ExcelDocLength"),// 长度
                    LanguageHepler.GetLanguage("ExcelDocPrimaryKey"),// 主键
                    LanguageHepler.GetLanguage("ExcelDocSelfIncreasing"),// 自增
                    LanguageHepler.GetLanguage("ExcelDocAllowNulls"),// 允许空
                    LanguageHepler.GetLanguage("ExcelDocDefaultValue"),// 默认值
                    LanguageHepler.GetLanguage("ExcelDocColumnDescription")// 列说明
                };
            // 设置宽度
            worksheet.SetColumnWidth(1, 1500);
            worksheet.SetColumnWidth(2, 10000);
            worksheet.SetColumnWidth(3, 5000);
            worksheet.SetColumnWidth(4, 2500);
            worksheet.SetColumnWidth(5, 2500);
            worksheet.SetColumnWidth(6, 2500);
            worksheet.SetColumnWidth(7, 2500);
            worksheet.SetColumnWidth(8, 2500);
            worksheet.SetColumnWidth(9, 12000);
            int row_index = 0; // 行
            int tableCount = 1;// 表序号
            foreach (var table in tables)
            {
                #region 创建标题行
                IRow rowTitle = worksheet.CreateRow(row_index);
                rowTitle.CreateCell(0).CellStyle = cellStyle_title;
                rowTitle.CreateCell(1).CellStyle = cellStyle_title;
                rowTitle.CreateCell(2).CellStyle = cellStyle_title;
                rowTitle.CreateCell(3).CellStyle = cellStyle_title;
                rowTitle.CreateCell(4).CellStyle = cellStyle_title;
                rowTitle.CreateCell(5).CellStyle = cellStyle_title;
                rowTitle.CreateCell(6).CellStyle = cellStyle_title;
                rowTitle.CreateCell(7).CellStyle = cellStyle_title;
                rowTitle.CreateCell(8).CellStyle = cellStyle_title;
                rowTitle.CreateCell(9).CellStyle = cellStyle_title;
                rowTitle.GetCell(0).SetCellValue(tableCount);
                rowTitle.GetCell(1).SetCellValue(table.TableName);
                rowTitle.GetCell(5).SetCellValue(!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");
                worksheet.AddMergedRegion(new CellRangeAddress(row_index, row_index, 1, 4));// 合并单元格，首行、末行、首列、末列
                worksheet.AddMergedRegion(new CellRangeAddress(row_index, row_index, 5, 9));// 合并单元格，首行、末行、首列、末列
                row_index += 1;// 行号+1
                tableCount += 1;
                #endregion
                #region 创建表头
                int col_index = 1;// 列号清空
                IRow rowTableTitle = worksheet.CreateRow(row_index);
                foreach (var colTile in colTileName)
                {
                    rowTableTitle.CreateCell(col_index).SetCellValue(colTile);
                    rowTableTitle.GetCell(col_index).CellStyle = cellStyle_title;
                    col_index += 1;// 列号+1
                }
                col_index = 0;// 列号清空
                row_index += 1;// 行号+1
                // 添加数据行,遍历数据库表字段
                foreach (var column in table.Columns)
                {
                    IRow row = worksheet.CreateRow(row_index);
                    if (!string.IsNullOrWhiteSpace(column.IsPK))
                    {
                        row.CreateCell(1).CellStyle = cellStyle_center_pk;
                        row.CreateCell(2).CellStyle = cellStyle_left_pk;
                        row.CreateCell(3).CellStyle = cellStyle_left_pk;
                        row.CreateCell(4).CellStyle = cellStyle_center_pk;
                        row.CreateCell(5).CellStyle = cellStyle_center_pk;
                        row.CreateCell(6).CellStyle = cellStyle_center_pk;
                        row.CreateCell(7).CellStyle = cellStyle_center_pk;
                        row.CreateCell(8).CellStyle = cellStyle_center_pk;
                        row.CreateCell(9).CellStyle = cellStyle_left_pk;
                    }
                    else if (string.IsNullOrWhiteSpace(column.CanNull))
                    {
                        row.CreateCell(1).CellStyle = cellStyle_center_null;
                        row.CreateCell(2).CellStyle = cellStyle_left_null;
                        row.CreateCell(3).CellStyle = cellStyle_left_null;
                        row.CreateCell(4).CellStyle = cellStyle_center_null;
                        row.CreateCell(5).CellStyle = cellStyle_center_null;
                        row.CreateCell(6).CellStyle = cellStyle_center_null;
                        row.CreateCell(7).CellStyle = cellStyle_center_null;
                        row.CreateCell(8).CellStyle = cellStyle_center_null;
                        row.CreateCell(9).CellStyle = cellStyle_left_null;
                    }
                    else
                    {
                        row.CreateCell(1).CellStyle = cellStyle_center;
                        row.CreateCell(2).CellStyle = cellStyle_left;
                        row.CreateCell(3).CellStyle = cellStyle_left;
                        row.CreateCell(4).CellStyle = cellStyle_center;
                        row.CreateCell(5).CellStyle = cellStyle_center;
                        row.CreateCell(6).CellStyle = cellStyle_center;
                        row.CreateCell(7).CellStyle = cellStyle_center;
                        row.CreateCell(8).CellStyle = cellStyle_center;
                        row.CreateCell(9).CellStyle = cellStyle_left;
                    }

                    row.GetCell(1).SetCellValue(column.ColumnOrder);
                    row.GetCell(2).SetCellValue(column.ColumnName);
                    row.GetCell(3).SetCellValue(column.ColumnTypeName);
                    row.GetCell(4).SetCellValue(column.Length);
                    row.GetCell(5).SetCellValue(column.IsPK);
                    row.GetCell(6).SetCellValue(column.IsIdentity);
                    row.GetCell(7).SetCellValue(column.CanNull);
                    row.GetCell(8).SetCellValue(column.DefaultVal);
                    row.GetCell(9).SetCellValue(column.Comment);
                    row_index += 1;// 行号+1
                }
                row_index += 1;// 行号+1
                #endregion
            }
        }

        /// <summary>
        /// 引用NPOI导出excel 表结构模板
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="databaseName"></param>
        /// <param name="tables"></param>
        public void ExportTemplate(string fileName, DBDto dto)
        {
            var tables = dto.Tables;
            FileInfo xlsFileInfo = new FileInfo(fileName);
            if (xlsFileInfo.Exists) { xlsFileInfo.Delete(); }//  注意此处：存在Excel文档即删除再创建一个
            try
            {
                XSSFWorkbook workbook = new XSSFWorkbook();//创建excel
                #region 单元格样式
                ICellStyle cellStyle_blue = NpoiHelper.GetCellStyleRgb(workbook, 1, false, true, false, new byte[] { 0, 176, 240 });        // 单元格设定  不居中   不加粗      蓝色
                ICellStyle cellStyle_yellow = NpoiHelper.GetCellStyleRgb(workbook, 1, false, true, false, new byte[] { 255, 192, 0 });      // 单元格设定  不居中   不加粗      黄色
                ICellStyle cellStyle_green = NpoiHelper.GetCellStyleRgb(workbook, 1, false, true, false, new byte[] { 146, 208, 80 });      // 单元格设定  不居中   不加粗      绿色
                ICellStyle cellStyle_green_pk = NpoiHelper.GetCellStyleRgb(workbook, 1, false, true, false, new byte[] { 146, 208, 80 },2); // 单元格设定  不居中   不加粗      绿色PK
                ICellStyle cellStyle_No = NpoiHelper.GetCellStyle(workbook, 1, true, true, false);                                          // 单元格设定  不居中   不加粗      无色
                #endregion
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    ISheet worksheet = workbook.CreateSheet("Sheet1");// 创建Sheet
                    int row_index = 1; // 起始行  第二行开始
                    foreach (var table in tables)
                    {
                        short rowHeight = 270;
                        int col_index = 3;// 起始列   D列开始
                        //表名
                        IRow row_TalbleName = worksheet.CreateRow(row_index);//表名
                        row_TalbleName.Height = rowHeight;
                        row_TalbleName.CreateCell(col_index).CellStyle = cellStyle_blue;
                        row_TalbleName.GetCell(col_index).SetCellValue(LanguageHepler.GetLanguage("TemplateTalbleName"));//标题
                        row_TalbleName.CreateCell(col_index + 1);
                        row_TalbleName.GetCell(col_index + 1).SetCellValue(table.TableName);//内容
                        //表备注
                        IRow row_TableComment = worksheet.CreateRow(row_index + 1);//表备注
                        row_TableComment.Height = rowHeight;
                        row_TableComment.CreateCell(col_index).CellStyle = cellStyle_blue;
                        row_TableComment.GetCell(col_index).SetCellValue(LanguageHepler.GetLanguage("TemplateTableComment"));//标题
                        row_TableComment.CreateCell(col_index + 1);
                        row_TableComment.GetCell(col_index + 1).SetCellValue(!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");//内容

                        IRow row_ColComment = worksheet.CreateRow(row_index + 2);//表结构-备注
                        IRow row_ColName = worksheet.CreateRow(row_index + 3);//表结构-字段名
                        IRow row_ColType = worksheet.CreateRow(row_index + 4);//表结构-数据类型
                        IRow row_ColPk = worksheet.CreateRow(row_index + 5);//表结构-主键
                        row_ColComment.Height = rowHeight;
                        row_ColName.Height = rowHeight;
                        row_ColType.Height = rowHeight;
                        row_ColPk.Height = rowHeight;

                        //关联序号
                        row_ColComment.CreateCell(col_index).CellStyle = cellStyle_blue;
                        row_ColName.CreateCell(col_index).CellStyle = cellStyle_blue;
                        row_ColType.CreateCell(col_index).CellStyle = cellStyle_blue;
                        row_ColPk.CreateCell(col_index).CellStyle = cellStyle_blue;
                        worksheet.AddMergedRegion(new CellRangeAddress(row_index + 2, row_index + 5, col_index, col_index));// 合并单元格，首行、末行、首列、末列
                        row_ColComment.GetCell(col_index).SetCellValue(LanguageHepler.GetLanguage("TemplateNumber"));//标题

                        //列注释
                        row_ColComment.CreateCell(col_index + 1).CellStyle = cellStyle_yellow;
                        row_ColComment.GetCell(col_index + 1).SetCellValue(LanguageHepler.GetLanguage("TemplateColComment"));//标题
                        //列名称
                        row_ColName.CreateCell(col_index + 1).CellStyle = cellStyle_yellow;
                        row_ColName.GetCell(col_index + 1).SetCellValue(LanguageHepler.GetLanguage("TemplateColName"));//标题
                        //数据类型
                        row_ColType.CreateCell(col_index + 1).CellStyle = cellStyle_yellow;
                        row_ColType.GetCell(col_index + 1).SetCellValue(LanguageHepler.GetLanguage("TemplateColType"));//标题
                        //约束类型  主键PK  不能 Not Null
                        row_ColPk.CreateCell(col_index + 1).CellStyle = cellStyle_yellow;
                        row_ColPk.GetCell(col_index + 1).SetCellValue(LanguageHepler.GetLanguage("TemplateColConstraint"));//标题

                        int ColCount = table.Columns.Count;// 最大字段行数
                        #region 生成结构
                        for (int i = 0; i < ColCount; i++)
                        {
                            int temp_index = col_index + 2 + i;//列坐标
                            //列注释
                            row_ColComment.CreateCell(temp_index).CellStyle = cellStyle_green;
                            row_ColComment.GetCell(temp_index).SetCellValue(!string.IsNullOrWhiteSpace(table.Columns[i].Comment) ? table.Columns[i].Comment : "");
                            //列名称
                            row_ColName.CreateCell(temp_index).CellStyle = cellStyle_green;
                            row_ColName.GetCell(temp_index).SetCellValue(!string.IsNullOrWhiteSpace(table.Columns[i].ColumnName) ? table.Columns[i].ColumnName : "");
                            //数据类型
                            row_ColType.CreateCell(temp_index).CellStyle = cellStyle_green;
                            row_ColType.GetCell(temp_index).
                                SetCellValue(!string.IsNullOrWhiteSpace(table.Columns[i].ColumnTypeName) ? table.Columns[i].ColumnTypeName+ table.Columns[i] .Length: "");
                            //约束类型  主键PK  不能 Not Null
                            row_ColPk.CreateCell(temp_index).CellStyle = cellStyle_green_pk;
                            string isPk = !string.IsNullOrWhiteSpace(table.Columns[i].IsPK) ? table.Columns[i].IsPK : "";
                            string isNull = !string.IsNullOrWhiteSpace(table.Columns[i].CanNull) ? table.Columns[i].CanNull : "";
                            if (isPk == "√")
                            {
                                row_ColPk.GetCell(temp_index).SetCellValue("PK");
                            }
                            else if (isNull == "√")
                            {
                                row_ColPk.GetCell(temp_index).SetCellValue("Not Null");
                            }
                        }
                        #endregion

                        row_index += 6;// 基础6行
                        //生成5个空行
                        for (int j = 0; j < 5; j++)
                        {
                            IRow row_Null = worksheet.CreateRow(row_index);//空白行
                            row_Null.Height = rowHeight;
                            for (int i = 0; i < ColCount + 2; i++)
                            {
                                int tmp_index = col_index;
                                row_Null.CreateCell(i + tmp_index).CellStyle = cellStyle_No;
                                if (i == 1)
                                {
                                    row_Null.GetCell(i + tmp_index).CellStyle = cellStyle_yellow;
                                    row_Null.GetCell(i + tmp_index).CellStyle.Alignment = HorizontalAlignment.Center;
                                    row_Null.GetCell(i + tmp_index).SetCellValue((j + 1).ToString());
                                }
                            }
                            row_index++;
                        }
                        row_index += 2;// 间隔2行
                    }
                    workbook.Write(fs);//写入到Excel中
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            base.OnProgress(new ChangeRefreshProgressArgs { BuildNum = tables.Count, TotalNum = tables.Count, IsEnd = true });// 更新进度
        }
    }
}
