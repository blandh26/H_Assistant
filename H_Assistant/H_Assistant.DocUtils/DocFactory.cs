using H_Assistant.DocUtils.DBDoc;
using H_Assistant.DocUtils.Dtos;

namespace H_Assistant.DocUtils
{
    public class DocFactory
    {
        public static Doc CreateInstance(DocType type, DBDto dto)
        {
            switch (type)
            {
                case DocType.html:
                    return new HtmlDoc(dto);
                case DocType.excel:
                    return new ExcelDoc(dto);
                case DocType.md:
                    return new MarkDownDoc(dto);
                case DocType.xml:
                    return new XmlDoc(dto);
                case DocType.json:
                    return new JsonDoc(dto);
                case DocType.template:
                    return new ExcelDoc(dto);
                default:
                    return null;
            }
        }
    }
}
