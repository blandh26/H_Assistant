using H_Assistant.DocUtils;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace H_Assistant.Views
{
    /// <summary>
    /// ScriptWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptWindow
    {
        public static readonly DependencyProperty SelectDatabaseProperty = DependencyProperty.Register(
            "SelectDatabase", typeof(DataBase), typeof(ScriptWindow), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectDatabase
        {
            get => (DataBase)GetValue(SelectDatabaseProperty);
            set => SetValue(SelectDatabaseProperty, value);
        }
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(ScriptWindow), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(ScriptWindow), new PropertyMetadata(default(TreeNodeItem)));
        /// <summary>
        /// 选中对象
        /// </summary>
        public TreeNodeItem SelectedObject
        {
            get => (TreeNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        public static readonly DependencyProperty SelectedColumnsProperty = DependencyProperty.Register(
            "SelectedColumns", typeof(List<Column>), typeof(ScriptWindow), new PropertyMetadata(default(List<Column>)));
        /// <summary>
        /// 选中字段
        /// </summary>
        public List<Column> SelectedColumns
        {
            get => (List<Column>)GetValue(SelectedColumnsProperty);
            set => SetValue(SelectedColumnsProperty, value);
        }
        public ScriptWindow()
        {
            InitializeComponent();
        }

        private void ScriptWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var objectName = SelectedObject.DisplayName;
            var colList = SelectedColumns;
            var instance = ExporterFactory.CreateInstance(SelectedConnection.DbType, objectName, colList);
            if (TabSelectSql.IsSelected)
            {
                //查询sql
                var selSql = instance.SelectSql();
                TxtSelectSql.SqlText = selSql;
            }
            var selectedConnection = SelectedConnection;
            var selectDatabase = SelectDatabase;
            var selectedObject = SelectedObject;
            Task.Run(() =>
            {
                var ddlSql = instance.CreateTableSql();
                //插入sql
                var insSql = instance.InsertSql();
                //更新sql
                var updSql = instance.UpdateSql();
                //删除sql
                var delSql = instance.DeleteSql();
                //生成C#实体类
                var csharp = ExportDLL.CsharpEntity(selectDatabase, selectedConnection, selectedObject.Name);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TxtCreateSql.SqlText = ddlSql;
                    TxtInsertSql.SqlText = insSql;
                    TxtUpdateSql.SqlText = updSql;
                    TxtDeleteSql.SqlText = delSql;
                    TxtCreateCsharp.SqlText = csharp;
                }));
            });
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
