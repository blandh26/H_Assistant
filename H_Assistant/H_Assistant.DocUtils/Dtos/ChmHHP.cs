using H_Assistant.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace H_Assistant.DocUtils.Dtos
{
    public class ChmHHP
    {
        public ChmHHP() { }

        public ChmHHP(string chmFile, string workTmpDir)
        {
            this.ChmFile = chmFile;
            this.WorkTmpDir = workTmpDir;

            if (!string.IsNullOrWhiteSpace(this.WorkTmpDir))
            {
                this.Files = Directory.GetFiles(this.WorkTmpDir, "*.html", SearchOption.AllDirectories).ToList();
            }
        }

        public string ChmFile { get; set; }

        public string WorkTmpDir { get; set; }

        public string DefaultFile { get; set; } = LanguageHepler.GetLanguage("ChmHHPDatabaseDirectory")+".html";

        public string Title
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.ChmFile))
                {
                    return string.Empty;
                }
                return Path.GetFileName(this.ChmFile);
            }
        }

        public List<string> Files { get; private set; }
    }
}
