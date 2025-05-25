using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.UserControl.Connect;
using HandyControl.Controls;
using HandyControl.Data;
using SqlSugar;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static H_Assistant.MainWindow;
using DbType = SqlSugar.DbType;

namespace H_Assistant.Views
{
    //定义委托
    public delegate void ConnectChangeRefreshHandlerExt(ConnectConfigs connectConfig);
    /// <summary>
    /// GroupManage.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectManage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;

        #region DependencyProperty

        public static readonly DependencyProperty DataListProperty = DependencyProperty.Register(
            "DataList", typeof(List<ConnectConfigs>), typeof(ConnectManage), new PropertyMetadata(default(List<ConnectConfigs>)));
        public List<ConnectConfigs> DataList
        {
            get => (List<ConnectConfigs>)GetValue(DataListProperty);
            set
            {
                SetValue(DataListProperty, value);
                OnPropertyChanged(nameof(DataList));
            }
        }

        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register(
            "MainContent", typeof(System.Windows.Controls.UserControl), typeof(ConnectManage), new PropertyMetadata(default(System.Windows.Controls.UserControl)));
        /// <summary>
        /// 主界面用户控件
        /// </summary>
        public System.Windows.Controls.UserControl MainContent
        {
            get => (System.Windows.Controls.UserControl)GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }
        #endregion
        delegateDatabaseList _DatabaseList;
        public ConnectManage(delegateDatabaseList delegateDatabaseList)
        {
            InitializeComponent();
            DataContext = this;
            _DatabaseList = delegateDatabaseList;
        }

        /// <summary>
        /// 连接页面数据初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectManage_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var ucConnectMain = new ConnectMainUC();
            ucConnectMain.ChangeRefreshEvent += ChangeRefreshEvent;
            MainContent = ucConnectMain;
            Task.Run(() =>
            {
                var liteDBHelper = LiteDBHelper.GetInstance();
                var db_ConnectConfigs = liteDBHelper.db.GetCollection<ConnectConfigs>();
                var datalist = db_ConnectConfigs.Query().ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                    if (!datalist.Any())
                    {
                        NoDataText.Visibility = Visibility.Visible;
                    }
                });
            });
            #endregion
        }

        /// <summary>
        /// 加载连接信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count > 0)
            {
                OprToolGrid.Visibility = Visibility.Visible;
                BtnPrev.Visibility = Visibility.Hidden;
                var connect = (ConnectConfigs)listBox.SelectedItems[0];
                switch (connect.DbType)
                {
                    case DbType.SqlServer:
                        var ucSqlServer = new SqlServerUC();
                        ucSqlServer.ConnectConfig = connect;
                        ucSqlServer.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucSqlServer;
                        break;
                    case DbType.MySql:
                        var ucMySql = new MySqlUC();
                        ucMySql.ConnectConfig = connect;
                        ucMySql.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucMySql;
                        break;
                    case DbType.PostgreSQL:
                        var ucPostgreSql = new PostgreSqlUC();
                        ucPostgreSql.ConnectConfig = connect;
                        ucPostgreSql.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucPostgreSql;
                        break;
                    case DbType.Oracle:
                        var ucOracle = new OracleUC();
                        ucOracle.ConnectConfig = connect;
                        ucOracle.ChangeRefreshEvent += ChangeRefreshEvent;
                        MainContent = ucOracle;
                        break;
                        //case DbType.Dm:
                        //    var ucDm = new DmUC();
                        //    ucDm.ConnectConfig = connect;
                        //    ucDm.ChangeRefreshEvent += ChangeRefreshEvent;
                        //    MainContent = ucDm;
                        //    break;
                }
            }
            #endregion
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var tag = ((Button)sender).Tag;
            var isConnect = tag != null && (string)tag == $"Connect";
            //SqlServer
            if (MainContent is SqlServerUC ucSqlServer)
            {
                ucSqlServer.SaveForm(isConnect, _DatabaseList);
            }
            //MySql
            if (MainContent is MySqlUC ucMySql)
            {
                ucMySql.SaveForm(isConnect, _DatabaseList);
            }
            //PostgreSql
            if (MainContent is PostgreSqlUC ucPostgreSql)
            {
                ucPostgreSql.SaveForm(isConnect, _DatabaseList);
            }
            //Oracle
            if (MainContent is OracleUC ucOracle)
            {
                ucOracle.SaveForm(isConnect, _DatabaseList);
            }
            #endregion
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var liteDBHelper = LiteDBHelper.GetInstance();
            if (ListConnects.SelectedItem == null)
            {
                Growl.WarningGlobal(new GrowlInfo { Message = LanguageHepler.GetLanguage("SelectDeleteConnection"), WaitTime = 1, ShowDateTime = false });
                return;
            }
            var selectedConnect = (ConnectConfigs)ListConnects.SelectedItem;
            Task.Run(() =>
            {
                var db_ConnectConfigs = liteDBHelper.db.GetCollection<ConnectConfigs>();
                db_ConnectConfigs.Delete(selectedConnect.ID);
                var datalist = db_ConnectConfigs.Query().ToList();
                Dispatcher.Invoke(() =>
                {
                    ResetData();
                    DataList = datalist;
                    if (!datalist.Any())
                    {
                        NoDataText.Visibility = Visibility.Visible;
                    }
                    if (ChangeRefreshEvent != null)
                    {
                        //ChangeRefreshEvent();
                    }
                });
            });
            #endregion
        }

        /// <summary>
        /// 添加/重置表单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            ResetData();
        }

        /// <summary>
        /// 重置表单
        /// </summary>
        private void ResetData()
        {
            #region MyRegion
            MainContent = new ConnectMainUC();
            OprToolGrid.Visibility = Visibility.Visible;
            ListConnects.SelectedItem = null;
            #endregion
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTestConnect_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            //测试SqlServer
            if (MainContent is SqlServerUC ucSqlServer)
            {
                ucSqlServer.TestConnect(true);
            }
            //测试MySql
            if (MainContent is MySqlUC ucMySql)
            {
                ucMySql.TestConnect(true);
            }
            //测试PostgreSql
            if (MainContent is PostgreSqlUC ucPostgreSql)
            {
                ucPostgreSql.TestConnect(true);
            }
            //测试Oracle
            if (MainContent is OracleUC ucOracle)
            {
                ucOracle.TestConnect(true);
            }
            #endregion
        }

        /// <summary>
        /// 返回上一步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrev_OnClick(object sender, RoutedEventArgs e)
        {
            ResetData();
        }

        /// <summary>
        /// 实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextSearchConnect_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchConenct = TextSearchConnect.Text.Trim();
            var liteDBHelper = LiteDBHelper.GetInstance();
            var datalist = liteDBHelper.db.GetCollection<ConnectConfigs>().Query().Where(x => x.ConnectName.Contains(searchConenct)).ToList();
            DataList = datalist;
            NoDataText.Visibility = datalist.Any() ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
