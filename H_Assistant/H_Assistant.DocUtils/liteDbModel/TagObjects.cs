﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Assistant.Framework.liteDbModel
{
    /// <summary>
    /// 标签对象信息
    /// </summary>
    public class TagObjects
    {
        public int Id { get; set; }
        /// <summary>
        /// 所属标签ID
        /// </summary>
        public int TagId { get; set; }
        /// <summary>
        /// 对象ID（数据库ID）
        /// </summary>
        public string ObjectId { get; set; }
        /// <summary>
        /// 对象名称
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// 对象类型（1.表，2.视图，3.存储过程）
        /// </summary>
        public int ObjectType { get; set; }
        /// <summary>
        /// 所属连接ID
        /// </summary>
        public int ConnectId { get; set; }
        /// <summary>
        /// 所属数据库名称
        /// </summary>
        public string DatabaseName { get; set; }
    }
}
