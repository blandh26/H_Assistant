using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Helper;
using LiteDB;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace H_Assistant.Views.Category
{
    /// <summary>
    /// TagAddView.xaml 的交互逻辑
    /// </summary>
    public partial class TestKeywordsEditView : INotifyPropertyChanged
    {
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        ILiteCollection<SystemSet> db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
        public event ChangeRefreshHandler ChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TestKeywordsEditView()
        {
            InitializeComponent();
            DataContext = this;
            SystemSet model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_TestKeyword);// 测试关键字
            txtClipboard.Text = model.Value;
        }

        /// <summary>
        /// 保存关键词
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SystemSet model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_TestKeyword);// 测试关键字
            model.Value = txtClipboard.Text;
            db_SystemSet.Update(model);
            this.Close();
            Oops.Success(LanguageHepler.GetLanguage("SuccessfullySave"));
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}
