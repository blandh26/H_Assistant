using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace H_Assistant.Framework.Util
{
    public static class ConnectionStringUtil
    {
        /// <summary>
        /// SQLServer连接字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="database"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string SqlServerString(string serverAddress, int port, string database, string userName, string password)
        {
            var serAddress = $"{serverAddress},{port}";
            if (serverAddress.Equals(".") || serverAddress.Contains(@"\"))
            {
                serAddress = serverAddress;
            }
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serAddress,
                InitialCatalog = database,
                UserID = userName,
                Password = EncryptHelper.Decode(password)
            };
            return sqlConnectionStringBuilder.ConnectionString;
        }

        /// <summary>
        /// MySql数据库字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="database"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MySqlString(string serverAddress, int port, string database, string userName, string password)
        {
            var mySqlConnectionStringBuild = new MySqlConnectionStringBuilder
            {
                Server = serverAddress,
                Port = Convert.ToUInt32(port),
                UserID = userName,
                Password = EncryptHelper.Decode(password),
                SslMode = MySqlSslMode.Preferred,
                AllowUserVariables = true,
                AllowPublicKeyRetrieval = true
            };
            if (!string.IsNullOrEmpty(database))
            {
                mySqlConnectionStringBuild.Database = database;
            }
            return mySqlConnectionStringBuild.ConnectionString;
        }

        /// <summary>
        /// PostgreSQL数据库字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="database"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string PostgreSqlString(string serverAddress, int port, string database, string userName, string password)
        {
            database = database.Contains(":") ? database.Split(':')[0] : database;
            var npgSqlConnectionStringBuild = new NpgsqlConnectionStringBuilder
            {
                Host = serverAddress,
                Port = port,
                Database = database,
                Username = userName,
                Password = EncryptHelper.Decode(password)
            };
            return npgSqlConnectionStringBuild.ConnectionString;
        }

        /// <summary>
        /// Oracle数据库字符串
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="port"></param>
        /// <param name="serviceName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string OracleString(string serverAddress, int port, string serviceName, string userName, string password)
        {
            var oracleConnectionStringBuilder = new OracleConnectionStringBuilder
            {
                DataSource = $"{serverAddress}:{port}/{serviceName}",
                UserID = userName,
                Password = EncryptHelper.Decode(password),
                Pooling = false
            };
            return oracleConnectionStringBuilder.ConnectionString;
        }
    }
}
