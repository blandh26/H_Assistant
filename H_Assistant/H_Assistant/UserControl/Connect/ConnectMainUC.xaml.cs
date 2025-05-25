using H_Assistant.UserControl.Controls;
using H_Assistant.Views;
using System.Windows;

namespace H_Assistant.UserControl.Connect
{
    /// <summary>
    /// ConnectMainUC.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectMainUC : System.Windows.Controls.UserControl
    {
        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;

        public ConnectMainUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 选中连接类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectType_OnClickCard(object sender, RoutedEventArgs e)
        {
            var senderInfo = (ConnectType)sender;
            var mainWindow = (ConnectManage)Window.GetWindow(this);
            if (mainWindow == null)
            {
                return;
            }
            mainWindow.OprToolGrid.Visibility = Visibility.Visible;
            mainWindow.BtnPrev.Visibility = Visibility.Visible;
            switch (senderInfo.DataBaseName)
            {
                case "SQLServer":
                    var ucSqlServerUc = new SqlServerUC();
                    ucSqlServerUc.ChangeRefreshEvent += ChangeRefreshEvent;
                    mainWindow.MainContent = ucSqlServerUc;
                    break;
                case "MySQL":
                    var ucMySqlUc = new MySqlUC();
                    ucMySqlUc.ChangeRefreshEvent += ChangeRefreshEvent;
                    mainWindow.MainContent = ucMySqlUc;
                    break;
                case "PostgreSQL":
                    var ucPostgreSqlUc = new PostgreSqlUC();
                    ucPostgreSqlUc.ChangeRefreshEvent += ChangeRefreshEvent;
                    mainWindow.MainContent = ucPostgreSqlUc;
                    break;
                case "Oracle":
                    var ucOracleUc = new OracleUC();
                    ucOracleUc.ChangeRefreshEvent += ChangeRefreshEvent;
                    mainWindow.MainContent = ucOracleUc;
                    break;
                //case "达梦":
                //    var ucDMUc = new DmUC();
                //    ucDMUc.ChangeRefreshEvent += ChangeRefreshEvent;
                //    mainWindow.MainContent = ucDMUc;
                //    break;
                default:
                    return;
            }
        }

        private void ConnectMainUC_OnLoaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = (ConnectManage)Window.GetWindow(this);
            if (mainWindow != null) mainWindow.OprToolGrid.Visibility = Visibility.Collapsed;
        }
    }
}
