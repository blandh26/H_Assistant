using H_Assistant.DocUtils;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.Util;
using H_Assistant.Helper;
using H_Assistant.Views;
using HandyControl.Controls;
using HandyControl.Data;
using SqlSugar;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static H_Assistant.MainWindow;
using Window = System.Windows.Window;

namespace H_Assistant.UserControl.Connect
{
    /// <summary>
    /// PostgreSqlUC.xaml 的交互逻辑
    /// </summary>
    public partial class PostgreSqlUC : System.Windows.Controls.UserControl
    {
        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;

        public static readonly DependencyProperty ConnectConfigProperty = DependencyProperty.Register(
            "ConnectConfig", typeof(ConnectConfigs), typeof(PostgreSqlUC), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 连接配置信息
        /// </summary>
        public ConnectConfigs ConnectConfig
        {
            get => (ConnectConfigs)GetValue(ConnectConfigProperty);
            set => SetValue(ConnectConfigProperty, value);
        }
        public PostgreSqlUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化加载页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostgreSqlUC_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded || ConnectConfig == null)
            {
                return;
            }
            var connect = ConnectConfig;
            var pwd = EncryptHelper.Decode(connect.Password);
            HidId.Text = connect.ID.ToString();
            TextConnectName.Text = connect.ConnectName;
            TextServerAddress.Text = connect.ServerAddress;
            TextServerPort.Value = connect.ServerPort;
            TextServerName.Text = connect.UserName;
            TextServerPassword.Password = pwd;
            TextDefaultDatabase.Text = connect.DefaultDatabase;
            #endregion
        }

        /// <summary>
        /// 重置表单
        /// </summary>
        public bool VerifyForm()
        {
            #region MyRegion
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = TextServerPort.Value;
            var userName = TextServerName.Text.Trim();
            var password = TextServerPassword.Password.Trim();
            var tipMsg = new StringBuilder();
            if (string.IsNullOrEmpty(connectName))
            {
                tipMsg.Append(LanguageHepler.GetLanguage("PleaseConnectionName") + Environment.NewLine);
            }
            if (string.IsNullOrEmpty(serverAddress))
            {
                tipMsg.Append(LanguageHepler.GetLanguage("PleaseServerAddress") + Environment.NewLine);
            }
            if (serverPort < 1)
            {
                tipMsg.Append(LanguageHepler.GetLanguage("PleasePortNumber") + Environment.NewLine);
            }
            if (string.IsNullOrEmpty(userName))
            {
                tipMsg.Append(LanguageHepler.GetLanguage("PleaseLoginName") + Environment.NewLine);
            }
            if (string.IsNullOrEmpty(password))
            {
                tipMsg.Append(LanguageHepler.GetLanguage("PleasePassword"));
            }
            if (tipMsg.ToString().Length > 0)
            {
                Growl.WarningGlobal(new GrowlInfo { Message = tipMsg.ToString(), WaitTime = 1, ShowDateTime = false });
                return false;
            }
            return true;
            #endregion
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="isTest"></param>
        public void TestConnect(bool isTest)
        {
            #region MyRegion
            if (!VerifyForm())
            {
                return;
            }
            var mainWindow = (ConnectManage)Window.GetWindow(this);
            if (mainWindow == null)
            {
                return;
            }
            mainWindow.LoadingG.Visibility = Visibility.Visible;
            var connectId = Convert.ToInt32(HidId.Text);
            var connectionString = ConnectionStringUtil.PostgreSqlString(TextServerAddress.Text.Trim(),
                Convert.ToInt32(TextServerPort.Value), "postgres", TextServerName.Text.Trim(),
                EncryptHelper.Encode(TextServerPassword.Password.Trim()));
            Task.Run(() =>
            {
                try
                {
                    var exporter = ExporterFactory.CreateInstance(DbType.PostgreSQL, connectionString);
                    exporter.GetDatabases();
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                        if (isTest)
                        {
                            Oops.Success(LanguageHepler.GetLanguage("SuccessfullyConnected"));
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God(LanguageHepler.GetLanguage("ConnectionFailed") + ex.ToMsg());
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                    }));
                }
            });
            #endregion
        }

        /// <summary>
        /// 保存表单
        /// </summary>
        public void SaveForm(bool isConnect, delegateDatabaseList _DatabaseList)
        {
            #region MyRegion
            if (!VerifyForm())
            {
                return;
            }
            var mainWindow = (ConnectManage)Window.GetWindow(this);
            if (mainWindow == null)
            {
                return;
            }
            var connectId = Convert.ToInt32(HidId.Text);
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = Convert.ToInt32(TextServerPort.Value);
            var userName = TextServerName.Text.Trim();
            var password = EncryptHelper.Encode(TextServerPassword.Password.Trim());
            var defaultDataBase = TextDefaultDatabase.Text.Trim();
            var connectionString =
                ConnectionStringUtil.PostgreSqlString(serverAddress, serverPort, defaultDataBase, userName, password);
            var liteDBHelper = LiteDBHelper.GetInstance();
            ConnectConfigs connectConfig;

            mainWindow.LoadingG.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    var db_ConnectConfigs = liteDBHelper.db.GetCollection<ConnectConfigs>();
                    if (isConnect)
                    {
                        var exporter = ExporterFactory.CreateInstance(DbType.PostgreSQL, connectionString);
                        exporter.GetDatabases();
                    }
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                        if (isConnect)
                        {
                            Growl.SuccessGlobal(new GrowlInfo { Message = LanguageHepler.GetLanguage("SuccessfullyConnected"), WaitTime = 1, ShowDateTime = false });
                        }
                        if (connectId > 0)
                        {
                            connectConfig = db_ConnectConfigs.FindOne(x => x.ID == connectId);
                            if (connectConfig == null)
                            {
                                Growl.WarningGlobal(new GrowlInfo { Message = LanguageHepler.GetLanguage("ConnectionNotExist"), WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            var connectAny = db_ConnectConfigs.FindOne(x => x.ConnectName == connectName && x.ID != connectId);
                            if (connectAny != null)
                            {
                                Growl.WarningGlobal(new GrowlInfo { Message = LanguageHepler.GetLanguage("ConnectionAlreadyExists"), WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            connectConfig.ConnectName = connectName;
                            connectConfig.DbType = DbType.PostgreSQL;
                            connectConfig.ServerAddress = serverAddress;
                            connectConfig.ServerPort = serverPort;
                            connectConfig.UserName = userName;
                            connectConfig.Password = password;
                            connectConfig.DefaultDatabase = defaultDataBase;
                            connectConfig.Authentication = 1;
                            db_ConnectConfigs.Update(connectConfig);
                        }
                        else
                        {
                            var connect = db_ConnectConfigs.FindOne(x => x.ConnectName.ToLower() == connectName.ToLower());
                            if (connect != null)
                            {
                                Growl.WarningGlobal(new GrowlInfo { Message = LanguageHepler.GetLanguage("ConnectionAlreadyExists"), WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            connectConfig = new ConnectConfigs()
                            {
                                ConnectName = connectName,
                                DbType = DbType.PostgreSQL,
                                ServerAddress = serverAddress,
                                ServerPort = serverPort,
                                Authentication = 1,
                                UserName = userName,
                                Password = password,
                                CreateDate = DateTime.Now,
                                DefaultDatabase = defaultDataBase
                            };
                            db_ConnectConfigs.Insert(connectConfig);
                        }

                        Task.Run(() =>
                        {
                            var datalist = db_ConnectConfigs.Query().ToList();
                            Dispatcher.Invoke(() =>
                            {
                                mainWindow.NoDataText.Visibility = Visibility.Collapsed;
                                mainWindow.DataList = datalist;
                                if (!isConnect)
                                {
                                    Growl.SuccessGlobal(new GrowlInfo { Message = LanguageHepler.GetLanguage("SuccessfullySave"), WaitTime = 1, ShowDateTime = false });
                                    _DatabaseList();
                                }
                                if (isConnect && ChangeRefreshEvent != null)
                                {
                                    ChangeRefreshEvent(connectConfig);
                                    mainWindow.Close();
                                }
                            });
                        });
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Oops.God(LanguageHepler.GetLanguage("ConnectionFailed") + ex.ToMsg());
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                    });
                }
            });
            #endregion
        }
    }
}
