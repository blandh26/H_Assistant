using H_Assistant.DocUtils.Dtos;
using H_Assistant.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace H_Assistant.DocUtils.DBDoc
{
    public class MarkDownDoc : Doc
    {
        public MarkDownDoc(DBDto dto, string filter = "markdown files (.md)|*.md") : base(dto, filter)
        {
        }
        public override bool Template(string filePath) { return true; }
        public override bool Build(string filePath)
        {
            return BuildDoc(filePath);
        }

        private bool BuildDoc(string filePath)
        {
            #region MyRegion
            var sb = new StringBuilder();
            sb.AppendLine("### 📚 "+LanguageHepler.GetLanguage("MarkDownDocDatabaseTableCatalog"));
            var regPattern = @"(.+?\|\s+)([a-zA-Z][a-zA-Z0-9_]+)(\s+\|.+\n?)";
            var regPlacement = $"$1[$2](#$2)$3";
            //Extensions.MarkDown();
            var Objects = new List<TableDto>();
            Dto.Tables.ForEach(t =>
            {
                Objects.Add(t);
            });
            Dto.Views.ForEach(v =>
            {
                var oNum = Objects.Count + 1;
                Objects.Add(new TableDto
                {
                    TableOrder = oNum.ToString(),
                    TableName = v.ObjectName,
                    Comment = v.Comment
                });
            });
            Dto.Procs.ForEach(v =>
            {
                var oNum = Objects.Count + 1;
                Objects.Add(new TableDto
                {
                    TableOrder = oNum.ToString(),
                    TableName = v.ObjectName,
                    Comment = v.Comment
                });
            });
            var dirMD = Objects.MarkDown("Columns", "DBType", "Script");
            dirMD = Regex.Replace(dirMD, regPattern, regPlacement, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            sb.Append(dirMD);
            sb.AppendLine();
            int count = 0;
            int count_total = Dto.Tables.Count + Dto.Views.Count + Dto.Procs.Count;
            if (this.Dto.Tables.Any())
            {
                sb.Append("### 📒 "+ LanguageHepler.GetLanguage("MarkDownDocTableStructure"));
                foreach (var dto in this.Dto.Tables)
                {
                    sb.AppendLine();
                    sb.AppendLine($"#### {LanguageHepler.GetLanguage("ExcelDocTableName")}： {dto.TableName}");
                    sb.AppendLine($"{LanguageHepler.GetLanguage("MarkDownDocIllustrate")}： {dto.Comment}");

                    if (dto.DBType.StartsWith("Oracle"))
                    {
                        sb.Append(dto.Columns.MarkDown("IsIdentity"));
                    }
                    else
                    {
                        sb.Append(dto.Columns.MarkDown());
                    }

                    sb.AppendLine();
                    count++;
                    // 更新进度
                    base.OnProgress(new ChangeRefreshProgressArgs
                    {
                        BuildNum = count,
                        TotalNum = count_total,
                        BuildName = dto.TableName
                    });
                }
            }

            if (this.Dto.Views.Any())
            {
                sb.Append("### 📰 "+ LanguageHepler.GetLanguage("View"));
                foreach (var item in this.Dto.Views)
                {
                    sb.AppendLine();
                    sb.AppendLine($"#### {LanguageHepler.GetLanguage("MarkDownDocViewName")}： {item.ObjectName}");
                    sb.AppendLine($"{LanguageHepler.GetLanguage("MarkDownDocIllustrate")}： {item.Comment}");

                    sb.AppendLine("``` sql");
                    var fmtSql = item.Script.SqlFormat();
                    sb.Append(fmtSql);
                    sb.AppendLine("```");
                    sb.AppendLine();
                    count++;
                    // 更新进度
                    base.OnProgress(new ChangeRefreshProgressArgs
                    {
                        BuildNum = count,
                        TotalNum = count_total,
                        BuildName = item.ObjectName
                    });
                }
            }

            if (this.Dto.Procs.Any())
            {
                sb.Append("### 📜 "+ LanguageHepler.GetLanguage("Procedure"));
                foreach (var item in this.Dto.Procs)
                {
                    sb.AppendLine();
                    sb.AppendLine($"#### {LanguageHepler.GetLanguage("MarkDownDocStoredProcedureName")}： {item.ObjectName}");
                    sb.AppendLine($"{LanguageHepler.GetLanguage("MarkDownDocIllustrate")}： {item.Comment}");

                    sb.AppendLine("``` sql");
                    var fmtSql = item.Script.SqlFormat();
                    sb.Append(fmtSql);
                    sb.AppendLine("```");
                    sb.AppendLine();
                    count++;
                    // 更新进度
                    base.OnProgress(new ChangeRefreshProgressArgs
                    {
                        BuildNum = count,
                        TotalNum = count_total,
                        BuildName = item.ObjectName
                    });
                }
            }
            var md = sb.ToString();

            WriteLine(filePath, md, Encoding.UTF8);
            // 更新进度
            base.OnProgress(new ChangeRefreshProgressArgs
            {
                BuildNum = count,
                TotalNum = count_total,
                IsEnd = true
            });
            return true;
            #endregion
        }
    }
}
