﻿using SqlSugar;
using System.Collections.Generic;

namespace H_Assistant.Framework.Exporter
{
    using PhysicalDataModel;

    public abstract class Exporter : IExporter
    {
        public Exporter(string connectionString)
        {
            DbConnectString = connectionString;
        }
        public Exporter(string connectionString, string dbName)
        {
            DbConnectString = connectionString;
            DbName = dbName;
        }
        public Exporter(string tableName, List<Column> columns)
        {
            TableName = tableName;
            Columns = columns;
        }
        /// <summary>
        /// 数据库连接
        /// </summary>
        public string DbConnectString { get; private set; }
        /// <summary>
        /// 数据库
        /// </summary>
        public string DbName { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 表字段
        /// </summary>
        public List<Column> Columns { get; set; }
        /// <summary>
        /// 连接初始化获取对象列表
        /// </summary>
        /// <returns></returns>
        public abstract Model Init();
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <param name="defaultDatabase"></param>
        /// <returns></returns>
        public abstract List<DataBase> GetDatabases(string defaultDatabase = "");
        /// <summary>
        /// 获取对象列信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public abstract Columns GetColumnInfoById(string objectId);
        /// <summary>
        /// 获取脚本信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public abstract string GetScriptInfoById(string objectId, DbObjectType objectType);
        /// <summary>
        /// 更新表/视图/存储过程等对象备注
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public abstract bool UpdateObjectRemark(string objectName, string remark, DbObjectType objectType = DbObjectType.Table);
        /// <summary>
        /// 更新列注释
        /// </summary>
        /// <param name="columnInfo"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public abstract bool UpdateColumnRemark(Column columnInfo, string remark, DbObjectType objectType = DbObjectType.Table);
        /// <summary>
        /// 创建表SQL
        /// </summary>
        /// <returns></returns>
        public abstract string CreateTableSql();

        /// <summary>
        /// 查询数据sql脚本
        /// </summary>
        /// <returns></returns>
        public abstract string SelectSql();

        /// <summary>
        /// 插入数据sql脚本
        /// </summary>
        /// <returns></returns>
        public abstract string InsertSql();

        /// <summary>
        /// 更新数据sql脚本
        /// </summary>
        /// <returns></returns>
        public abstract string UpdateSql();

        /// <summary>
        /// 删除数据sql脚本
        /// </summary>
        /// <returns></returns>
        public abstract string DeleteSql();

        /// <summary>
        /// 添加列sql脚本
        /// </summary>
        /// <returns></returns>
        public abstract string AddColumnSql();

        /// <summary>
        /// 修改列sql脚本
        /// </summary>
        /// <returns></returns>
        public abstract string AlterColumnSql();

        /// <summary>
        /// 删除列sql脚本
        /// </summary>
        /// <returns></returns>
        public abstract string DropColumnSql();

        /// <summary>
        /// 获取所有表信息
        /// </summary>
        /// <returns></returns>
        public abstract List<DbTableInfo> GetTableInfoList();

        /// <summary>
        /// DbFirst
        /// </summary>
        /// <returns></returns>
        public IDbFirst DbFirst { get; }
    }
}
