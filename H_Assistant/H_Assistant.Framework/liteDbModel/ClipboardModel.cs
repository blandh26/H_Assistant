using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Assistant.Framework.liteDbModel
{
    /// <summary>
    /// 剪贴板实体类
    /// </summary>
    public class ClipboardModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Content { get; set; }
    }
}
