using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JinianNet.JNTemplate;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Framework.Properties;

namespace H_Assistant.Framework.Lang
{
    public class CsharpLang : Lang
    {
        public CsharpLang(string tbName, string tbComment, List<Column> columns) : base(tbName, tbComment, columns)
        {

        }

        public override string BuildEntity()
        {
            var excelTpl = Encoding.UTF8.GetString(Resource.Csharp);
            var template = Engine.CreateTemplate(excelTpl);
            template.Set("ClassName", TableName);
            template.Set("ClassComment", TableComment);
            template.Set("FieldList", Columns);
            return template.Render();
        }
    }
}
