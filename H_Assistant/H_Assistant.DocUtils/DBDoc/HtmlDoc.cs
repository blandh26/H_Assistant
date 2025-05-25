using H_Assistant.DocUtils.Dtos;
using H_Assistant.DocUtils.Properties;
using System.Text;

namespace H_Assistant.DocUtils.DBDoc
{
    /// <summary>
    /// 生成Html文档
    /// </summary>
    public class HtmlDoc : Doc
    {
        public HtmlDoc(DBDto dto, string filter = "html files (*.html)|*.html") : base(dto, filter)
        {
        }

        public override bool Template(string filePath) { return true; }
        public override bool Build(string filePath)
        {
            int count_total = Dto.Tables.Count + Dto.Views.Count + Dto.Procs.Count;
            // 更新进度
            base.OnProgress(new ChangeRefreshProgressArgs
            {
                Type = DocType.html,
                BuildNum = count_total,
                TotalNum = count_total,
                IsEnd = true
            });
            var htmlTpl = Encoding.UTF8.GetString(Resources.html);
            var htmlContent = htmlTpl.RazorRender(this.Dto);
            WriteLine(filePath, htmlContent, Encoding.UTF8);
            return true;
        }
    }
}
