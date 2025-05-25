﻿namespace H_Assistant.Framework
{
    ///// <summary>
    ///// 数据库类型
    ///// </summary>
    //public enum DBType
    //{
    //    SqlServer,
    //    MySQL,
    //    Oracle,
    //    PostgreSQL
    //}

    /// <summary>
    /// 脚本类型
    /// </summary>
    public enum ScriptType
    {
        /// <summary>
        /// 表
        /// </summary>
        Table = 1,
        /// <summary>
        /// 视图
        /// </summary>
        View = 2,
        /// <summary>
        /// 存储过程
        /// </summary>
        Proc = 3
    }

    public enum LangType
    {
        SQL = 0,

        Csharp = 1,

        Java = 2,

        PHP = 3,

        Python = 4,

        C = 5,

        ObjectC = 6
    }
}
