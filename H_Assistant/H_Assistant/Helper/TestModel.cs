using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Assistant.Helper
{
    public static class TestModel
    {
        /// <summary>
        /// 工作簿
        /// </summary>
        public class Sheet
        {
            /// <summary>
            /// 工作簿
            /// </summary>
            public String Name { get; set; }
            /// <summary>
            /// 工作簿号码
            /// </summary>
            public int Index { get; set; }

        }

        /// <summary>
        /// 方案数据
        /// </summary>
        public class Acell
        {
            /// <summary>
            /// 方案名称
            /// </summary>
            public String Name { get; set; }
            /// <summary>
            /// 方案开始行
            /// </summary>
            public int rowIndex { get; set; }
            /// <summary>
            /// 详细数据
            /// </summary>
            public List<XlxsTable> tableList { get; set; }
        }

        /// <summary>
        /// 表数据
        /// </summary>
        public class XlxsTable
        {
            /// <summary>
            /// 表名
            /// </summary>
            public String Name { get; set; }
            /// <summary>
            /// 表名
            /// </summary>
            public String Title { get; set; }
            /// <summary>
            /// 数据
            /// </summary>
            public List<object> List { get; set; }

        }

        /// <summary>
        /// 工作簿
        /// </summary>
        public class Table
        {
            /// <summary>
            /// 名称
            /// </summary>
            public String Name { get; set; }
            /// <summary>
            /// 对应名称
            /// </summary>
            public String Value { get; set; }

        }
    }
}
