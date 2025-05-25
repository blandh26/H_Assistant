using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Assistant.Framework.liteDbModel
{
    /// <summary>
    /// 应用程序实体类
    /// </summary>
    public class ExeModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public int Order { get; set; }
    }
}
