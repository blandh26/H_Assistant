using H_Assistant.Annotations;
using H_Assistant.Framework;
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
    public partial class ClipboardAddView : INotifyPropertyChanged
    {
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        ILiteCollection<ClipboardModel> db_ClipboardModel = liteDBHelper.db.GetCollection<ClipboardModel>();
        public event ChangeRefreshHandler ChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ClipboardAddView()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 保存标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ClipboardModel model = new ClipboardModel();
            model.Content = txtClipboard.Text;
            int index = 1;
            if (db_ClipboardModel.Query().ToList().Count != 0)
            {
                index = db_ClipboardModel.Query().ToList().Max(x => x.Index) + 1;
            }
            model.Index = index;
            db_ClipboardModel.Insert(model);
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
