using H_Assistant.Framework.Exporter;
using H_Assistant.Framework.PhysicalDataModel;
using SqlSugar;
using System.Collections.Generic;

namespace H_Assistant.Framework
{
    public class ExporterFactory
    {
        /// <summary>
        /// 创建访问数据库的实例工厂
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <param name="dbConnectionString">数据库连接字符串，访问数据库时必填</param>
        /// <returns></returns>
        public static Exporter.Exporter CreateInstance(DbType type, string dbConnectionString)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(dbConnectionString);
                case DbType.MySql: return new MySqlExporter(dbConnectionString);
                case DbType.PostgreSQL: return new PostgreSqlExporter(dbConnectionString);
                case DbType.Oracle: return new OracleExporter(dbConnectionString);
                default: return new SqlServerExporter(dbConnectionString);
            }
        }

        public static Exporter.Exporter CreateInstance(DbType type, string dbConnectionString, string dbName)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(dbConnectionString, dbName);
                case DbType.MySql: return new MySqlExporter(dbConnectionString, dbName);
                case DbType.PostgreSQL: return new PostgreSqlExporter(dbConnectionString, dbName);
                case DbType.Oracle: return new OracleExporter(dbConnectionString, dbName);
                default: return new SqlServerExporter(dbConnectionString, dbName);
            }
        }

        public static Exporter.Exporter CreateInstance(DbType type, string tableName, List<Column> columns)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(tableName, columns);
                case DbType.MySql: return new MySqlExporter(tableName, columns);
                case DbType.PostgreSQL: return new PostgreSqlExporter(tableName, columns);
                case DbType.Oracle: return new OracleExporter(tableName, columns);
                default: return new SqlServerExporter(tableName, columns);
            }
        }

        public static IDbFirst CreateInstanceDbFirst(DbType type, string tableName, string dbName)
        {
            switch (type)
            {
                case DbType.SqlServer: return new SqlServerExporter(tableName, dbName).DbFirst;
                case DbType.MySql: return new MySqlExporter(tableName, dbName).DbFirst;
                case DbType.PostgreSQL: return new PostgreSqlExporter(tableName, dbName).DbFirst;
                case DbType.Oracle: return new OracleExporter(tableName, dbName).DbFirst;
                default: return new SqlServerExporter(tableName, dbName).DbFirst;
            }
        }
    }
}
