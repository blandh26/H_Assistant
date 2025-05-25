using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Framework.Properties;
using JinianNet.JNTemplate;
using System.Collections.Generic;
using System.Text;

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
