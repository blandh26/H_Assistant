using H_Assistant.Framework.Lang;
using H_Assistant.Framework.PhysicalDataModel;
using System.Collections.Generic;

namespace H_Assistant.Framework
{
    public class LangFactory
    {
        public static Lang.Lang CreateInstance(LangType type, string tableName, string tableComment, List<Column> columns)
        {
            switch (type)
            {
                case LangType.Csharp: return new CsharpLang(tableName, tableComment, columns);
                case LangType.Java: return new JavaLang(tableName, tableComment, columns);
                default: return new CsharpLang(tableName, tableComment, columns);
            }
        }
    }
}
