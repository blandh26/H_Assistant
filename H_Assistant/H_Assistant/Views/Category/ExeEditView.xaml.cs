using H_Assistant.Annotations;
using H_Assistant.Framework;
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
    public partial class ExeEditView : INotifyPropertyChanged
    {
        public event ChangeRefreshHandler ChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        ILiteCollection<ExeModel> db_ExeModel = liteDBHelper.db.GetCollection<ExeModel>();
        ExeModel model = new ExeModel();
        string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下
        string icoName = "";
        string file = "";
        int id;
        bool isAdd = true; // true 新增  false 修改
        public ExeEditView(string _id)
        {
            InitializeComponent();
            DataContext = this;
            if (_id != "" && _id != null) { id = Convert.ToInt32(_id); isAdd = false; } // 修改模式
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BtnDelete.Visibility = Visibility.Hidden;
            if (!isAdd)
            {
                model = db_ExeModel.FindOne(x => x.Id == id);
                if (model != null)
                {
                    BtnDelete.Visibility = Visibility.Visible;
                    TextExeName.Text = model.Title;
                    TextExeOrder.Text = model.Order.ToString();
                    try { image1.Source = new BitmapImage(new Uri(path + model.Icon)); }
                    catch (Exception) { }
                }
            }
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
                model.Title = TextExeName.Text;
                try
                {
                    model.Order = Convert.ToInt32(TextExeOrder.Text);
                }
                catch (Exception)
                {
                    Oops.Oh(LanguageHepler.GetLanguage("Number4"));
                    return;
                }
                if (model.Path == "" || model.Path == null) { Oops.Oh(LanguageHepler.GetLanguage("SelectFile")); return; }
                if (model.Title == "" || model.Title == null) { Oops.Oh(LanguageHepler.GetLanguage("ExeNameTip")); return; }
                var list = db_ExeModel.Query().ToList();
                if (isAdd)
                {
                    model.Id = list.Max(x => x.Id) + 1;
                    db_ExeModel.Insert(model);
                }
                else
                {
                    db_ExeModel.Update(model);
                }
                this.Close();
            }
            catch (Exception ex)
            {
                Log.WriteErrorLog("删除应用失败");
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var msResult = MessageBox.Show(LanguageHepler.GetLanguage("IsDelete"), LanguageHepler.GetLanguage("Tip"), MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
            if (msResult == MessageBoxResult.OK)
            {
                db_ExeModel.Delete(id);
                this.Close();
            }
        }

        /// <summary>
        /// 选择按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            //选择文件对话框
            var opfd = new System.Windows.Forms.OpenFileDialog { Filter = "*.exe|" };
            if (opfd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            file = opfd.FileName;

            if (string.IsNullOrEmpty(model.Title)) { model.Title = opfd.SafeFileName.Split('.')[0]; }

            string folderToSave = path + "icon\\";//指定存放图标的文件夹
            if (!Directory.Exists(folderToSave)) Directory.CreateDirectory(folderToSave);

            var iconTotalCount = ExeIcon.PrivateExtractIcons(file, 0, 0, 0, null, null, 0, 0);//选中文件中的图标总数
            IntPtr[] hIcons = new IntPtr[iconTotalCount];//用于接收获取到的图标指针
            int[] ids = new int[iconTotalCount];//对应的图标id
            var successCount = ExeIcon.PrivateExtractIcons(file, 0, 256, 256, hIcons, ids, iconTotalCount, 0);//成功获取到的图标个数
            for (var i = 0; i < successCount; i++)//遍历并保存图标
            {
                if (hIcons[i] == IntPtr.Zero) continue;//指针为空，跳过
                using (var ico = System.Drawing.Icon.FromHandle(hIcons[i]))
                {
                    if (i == 0)
                    {
                        icoName = DateTime.Now.ToString("yyyyMMddHHmmsss") + ".png";
                        using (var myIcon = ico.ToBitmap())
                        {
                            myIcon.Save(folderToSave + icoName, ImageFormat.Png);
                        }
                        image1.Source = new BitmapImage(new Uri(path + "icon\\" + icoName));
                        model.Icon = "icon\\" + icoName;
                        model.Path = file;
                        return;
                    }
                }
                ExeIcon.DestroyIcon(hIcons[i]);//内存回收
            }
        }

        /// <summary>
        /// Enter键一键保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagName_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter){BtnSave_Click(sender, e);}
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e){this.Close();}
    }
}
