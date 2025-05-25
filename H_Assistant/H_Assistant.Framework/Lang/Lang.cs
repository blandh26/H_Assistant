using H_Assistant.Framework.PhysicalDataModel;
using System.Collections.Generic;

namespace H_Assistant.Framework.Lang
{
    public abstract class Lang
    {
        public string TableName { get; set; }
        public string TableComment { get; set; }
        public List<Column> Columns { get; set; }

        public Lang(string tbName, string tbComment, List<Column> columns)
        {
            TableName = tbName;
            TableComment = tbComment;
            Columns = columns;
        }

        public abstract string BuildEntity();
    }
}
