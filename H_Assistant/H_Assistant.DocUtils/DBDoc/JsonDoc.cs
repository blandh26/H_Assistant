using H_Assistant.DocUtils.Dtos;
using Newtonsoft.Json;
using System.Text;

namespace H_Assistant.DocUtils.DBDoc
{
    /// <summary>
    /// 生成Json文档
    /// </summary>
    public class JsonDoc : Doc
    {
        public JsonDoc(DBDto dto, string filter = "json files (.json)|*.json") : base(dto, filter)
        {
        }
        public override bool Template(string filePath) { return true; }
        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        /// <summary>
        /// 生成文档
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool BuildDoc(string filePath)
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
            var jsonText = JsonConvert.SerializeObject(this.Dto);
            WriteLine(filePath, jsonText, Encoding.UTF8);
            return true;
        }
    }
}
