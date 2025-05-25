using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Helper;
using H_Util;
using LiteDB;
using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = HandyControl.Controls.MessageBox;

namespace H_Assistant.Views.Category
{
    /// <summary>
    /// TagAddView.xaml 的交互逻辑
    /// </summary>
    public partial class RegEditView : INotifyPropertyChanged
    {
        public event ChangeRefreshHandler ChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        ILiteCollection<SystemSet> db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
        SystemSet model =new SystemSet();
        public RegEditView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            model = db_SystemSet.FindOne(x=>x.Name == SysConst.Sys_RegEx);
            RegEditText.Text = model.Value;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                model.Value = RegEditText.Text;
                db_SystemSet.Update(model);
                this.Close();
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("正则保存失败");
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e){this.Close();}
    }
}
