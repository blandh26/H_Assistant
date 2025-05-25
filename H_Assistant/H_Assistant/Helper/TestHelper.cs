using H_Assistant.Framework;
using H_Assistant.Framework.Exporter;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSugar;

namespace H_Assistant.Helper
{
    public static class TestHelper
    {
        public static string DllName { get; set; }// dll名称
        public static DataBase selectDatabase { get; set; }// 选中数据库
        public static ConnectConfigs SelectedConnection { get; set; }// 连接类

        public static SqlSugarClient DB
        {
            get { return SugarFactory.GetInstance(SelectedConnection.DbType,SelectedConnection.DbDefaultConnectString); }
        }
    }
}
