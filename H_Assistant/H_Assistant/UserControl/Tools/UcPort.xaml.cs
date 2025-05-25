using H_Assistant.DocUtils;
using H_Assistant.Framework;
using H_Assistant.Helper;
using H_Assistant.UserControl.Controls;
using H_Assistant.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static H_Assistant.DocUtils.WhoUsePort;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcPort : BaseUserControl
    {
        public List<PortUserInfo> portlist;
        public UcPort()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 检索端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchColumns_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            getList();
        }
        /// <summary>
        /// 结束进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnKill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < portlist.Count; i++)
                {
                    if (portlist[i].IsChecked)
                    {
                        try
                        {
                            portlist[i].Process.Kill();
                        }
                        catch (Exception) { }
                    }
                }
                getList();
                Oops.Success(LanguageHepler.GetLanguage("ProcessKill"));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        private void getList() {
            string str = SearchColumns.Text;
            portlist = null;
            try
            {
                str = Convert.ToInt32(str).ToString();
            }
            catch (Exception)
            {
                SearchColumns.Text = "";
                return;
            }
            Task.Run(() =>
            {
                portlist = NetStatus(Convert.ToInt32(str));
                this.Dispatcher.BeginInvoke(new Action(() => { TableGrid.ItemsSource = portlist; }));
            });
        }
    }
}
