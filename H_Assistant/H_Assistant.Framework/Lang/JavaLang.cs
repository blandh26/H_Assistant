using H_Assistant.Framework.PhysicalDataModel;
using System.Collections.Generic;

namespace H_Assistant.Framework.Lang
{
    public class JavaLang : Lang
    {
        public JavaLang(string tbName, string tbComment, List<Column> columns) : base(tbName, tbComment, columns)
        {

        }

        public override string BuildEntity()
        {
            return "";
        }
    }
}
