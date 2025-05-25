using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace H_Assistant.Framework.liteDbModel
{
    /// <summary>
    /// 分组信息表
    /// </summary>
    public class GroupInfo
    {
        public int Id { get; set; }
        /// <summary>
        /// 连接ID
        /// </summary>
        public int ConnectId { get; set; }
        /// <summary>
        /// 所属数据库名
        /// </summary>
        public string DataBaseName { get; set; }
        /// <summary>
        /// 分组名
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 展开层级：0.不展开，1.展开当前项，2.展开子项
        /// </summary>
        public int? OpenLevel { get; set; }
        /// <summary>
        /// 排序标记
        /// </summary>
        public DateTime OrderFlag { get; set; } = DateTime.Now;
        /// <summary>
        /// 对象数量
        /// </summary>
        private int _subCount;
        public int SubCount
        {
            get
            {
                var sliteDbHelper = LiteDBHelper.GetInstance();
               var db_GroupObjects = sliteDbHelper.db.GetCollection<GroupObjects>();
                var groupCount = db_GroupObjects.Find(x => x.GroupId == Id).Count();
                return groupCount;
            }
            set
            {
                _subCount = value;
            }
        }
        public bool IsSelected { get; set; }
    }
}
