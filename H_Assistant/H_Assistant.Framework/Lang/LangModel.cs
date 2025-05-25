using H_Assistant.Framework.Util;
using System.Collections.Generic;

namespace H_Assistant.Framework.Lang
{
    public class LangModel
    {
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// 类注释
        /// </summary>
        public string ClassComment { get; set; }
        /// <summary>
        /// 字段列表
        /// </summary>
        public List<LangFields> Fields { get; set; }
    }

    public class LangFields
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 字段名驼峰命名
        /// </summary>
        public string FieldPascalName => FieldName.CamelCaseName();

        /// <summary>
        /// 字段名小写命名
        /// </summary>
        public string FieldLowerName => FieldName.ToLower();

        /// <summary>
        /// 字段类型
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 字段说明
        /// </summary>
        public string Comment { get; set; }
    }
}
