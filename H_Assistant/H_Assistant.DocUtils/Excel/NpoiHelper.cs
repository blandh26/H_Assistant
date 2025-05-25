using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Assistant.DocUtils.Excel
{
    public static class NpoiHelper
    {
        /// <summary>
        /// 单元格样式设置
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="borderStyle">边框样式 0-13</param>
        /// <param name="isAlignment">是否水平居中</param>
        /// <param name="isVerticalAlignment">是否垂直居中</param>
        /// <param name="isBold">是否加粗</param>
        /// <param name="groundColor">背景颜色</param>
        /// <param name="color">文字颜色</param>
        /// <returns></returns>
        public static ICellStyle GetCellStyle(
            IWorkbook workbook, int borderStyle, bool isAlignment, bool isVerticalAlignment, bool isBold, short? groundColor = null, short? color = null)
        {
            ICellStyle cellStyle = workbook.CreateCellStyle();
            cellStyle.BorderTop = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.BorderBottom = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.BorderLeft = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.BorderRight = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.FillBackgroundColor = (short)27;
            if (groundColor != null)
            {// 背景颜色
                cellStyle.FillPattern = FillPattern.SolidForeground;
                cellStyle.FillForegroundColor = (short)groundColor;
            }
            IFont font_Bold = workbook.CreateFont();//创建字符样式
            if (isBold) { font_Bold.IsBold = true; }
            if (color != null) { font_Bold.Color = (short)color; }// 文字颜色
            cellStyle.SetFont(font_Bold);
            if (isAlignment) { cellStyle.Alignment = HorizontalAlignment.Center; }// 水平居中
            if (isVerticalAlignment) { cellStyle.VerticalAlignment = VerticalAlignment.Center; }// 垂直居中
            return cellStyle;
        }

        /// <summary>
        /// 单元格样式设置
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="borderStyle">边框样式 0-13</param>
        /// <param name="isAlignment">是否水平居中</param>
        /// <param name="isVerticalAlignment">是否垂直居中</param>
        /// <param name="isBold">是否加粗</param>
        /// <param name="groundColor">背景颜色RGB</param>
        /// <param name="color">文字颜色</param>
        /// <returns></returns>
        public static ICellStyle GetCellStyleRgb(
            IWorkbook workbook, int borderStyle, bool isAlignment, bool isVerticalAlignment, bool isBold, byte[] groundColor = null, short? color = null)
        {
            ICellStyle cellStyle = workbook.CreateCellStyle();
            cellStyle.BorderTop = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.BorderBottom = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.BorderLeft = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.BorderRight = (BorderStyle)Enum.Parse(typeof(BorderStyle), borderStyle.ToString());//边线
            cellStyle.FillBackgroundColor = (short)27;
            if (groundColor != null)
            {// 背景颜色
                cellStyle.FillPattern = FillPattern.SolidForeground;
                cellStyle.FillForegroundColor = 0;
                ((XSSFColor)cellStyle.FillForegroundColorColor).SetRgb(groundColor);
            }
            IFont font_Bold = workbook.CreateFont();//创建字符样式
            if (isBold) { font_Bold.IsBold = true; }
            if (color != null) { font_Bold.Color = (short)color; }// 文字颜色
            cellStyle.SetFont(font_Bold);
            if (isAlignment) { cellStyle.Alignment = HorizontalAlignment.Center; }// 水平居中
            if (isVerticalAlignment) { cellStyle.VerticalAlignment = VerticalAlignment.Center; }// 垂直居中
            return cellStyle;
        }

    }
}
