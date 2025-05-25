using H_Assistant.Framework;
using H_Assistant.Helper;
using H_Assistant.UserControl.Controls;
using H_Assistant.Views;
using System.Windows.Controls;
using System.Windows;
using H_Assistant.Framework.liteDbModel;
using LiteDB;
using System.Collections.Generic;
using H_Assistant.Views.Category;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Linq;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcClipboard : BaseUserControl
    {
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        ILiteCollection<ClipboardModel> db_ClipboardModel = liteDBHelper.db.GetCollection<ClipboardModel>();
        public UcClipboard()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 标签菜单数据列表
        /// </summary>
        public static readonly DependencyProperty ClipboardListProperty =
            DependencyProperty.Register("ClipboardList", typeof(List<ClipboardModel>), typeof(UcClipboard), new PropertyMetadata(default(List<ClipboardModel>)));
        public List<ClipboardModel> ClipboardList
        {
            get => (List<ClipboardModel>)GetValue(ClipboardListProperty);
            set { SetValue(ClipboardListProperty, value); OnPropertyChanged(nameof(ClipboardList)); }
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            ClipboardAddView clipboardAddView = new ClipboardAddView();
            clipboardAddView.ShowDialog();
            getList();
        }
        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(ListClipboard.SelectedItem is ClipboardModel selectedIten))
            {
                Oops.Oh(LanguageHepler.GetLanguage("PleaseSelectDeleteContent"));
                return;
            }
            var msResult = MessageBox.Show(LanguageHepler.GetLanguage("IsDelete"), LanguageHepler.GetLanguage("Tip"), MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
            if (msResult == MessageBoxResult.OK) { db_ClipboardModel.Delete(selectedIten.Id); getList(); }
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        private void getList()
        {
            List<ClipboardModel> datalist;
            if (SearchClipboard.Text != "") { datalist = db_ClipboardModel.Query().Where(x => x.Content.Contains(SearchClipboard.Text.Trim())).ToList().OrderBy(c => c.Index).ToList();  }
            else { datalist = db_ClipboardModel.Query().ToList().OrderBy(c => c.Index).ToList(); }
            NoDataText.Visibility = datalist.Count != 0 ? Visibility.Collapsed : Visibility.Visible;
            for (int i = 0; i < datalist.Count; i++) { datalist[i].Index = i + 1; }
            ClipboardList = datalist;
        }
        /// <summary>
        /// 保存列表
        /// </summary>
        private void saveList()
        {
            db_ClipboardModel.Update(ClipboardList);
            var aaa11 = db_ClipboardModel.Query().ToList();
            getList();
        }
        /// <summary>
        /// 双击复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListClipboard_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(ListClipboard.SelectedItem is ClipboardModel selectedIten))
            {
                Oops.Oh(LanguageHepler.GetLanguage("PleaseSelectDeleteContent"));
                return;
            }
            Clipboard.SetDataObject(selectedIten.Content);
            Oops.Success(LanguageHepler.GetLanguage("CopySuccess"));
        }
        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcClipboard_Loaded(object sender, RoutedEventArgs e) { getList(); }
        /// <summary>
        /// 实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchMenu_TextChanged(object sender, TextChangedEventArgs e) { getList(); }

        private void ListClipboard_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(ListClipboard);  // 获取位置
                #region 源位置
                HitTestResult result = VisualTreeHelper.HitTest(ListClipboard, pos);  //根据位置得到result
                if (result == null)
                {
                    return;    //找不到 返回
                }
                var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null || listBoxItem.Content != ListClipboard.SelectedItem)
                {
                    return;
                }
                #endregion

                System.Windows.DataObject dataObj = new System.Windows.DataObject(listBoxItem.Content as ClipboardModel);
                DragDrop.DoDragDrop(ListClipboard, dataObj, System.Windows.DragDropEffects.Move);  //调用方法
            }
        }

        private void ListClipboard_Drop(object sender, DragEventArgs e)
        {
            var pos = e.GetPosition(ListClipboard);   //获取位置
            var result = VisualTreeHelper.HitTest(ListClipboard, pos);   //根据位置得到result
            if (result == null)
            {
                return;   //找不到 返回
            }
            #region 查找元数据
            ClipboardModel sourcePerson = e.Data.GetData(typeof(ClipboardModel)) as ClipboardModel;
            if (sourcePerson == null)
            {
                return;
            }
            #endregion

            #region  查找目标数据
            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null)
            {
                return;
            }
            ClipboardModel targetPerson = listBoxItem.Content as ClipboardModel;
            if (ReferenceEquals(targetPerson, sourcePerson))
            {
                return;
            }
            #endregion

            // 根据结果排序
            int sourceIndex = ListClipboard.Items.IndexOf(sourcePerson);
            int targetIndex = ListClipboard.Items.IndexOf(targetPerson);
            int index = ClipboardList[sourceIndex].Index;
            ClipboardList[sourceIndex].Index = ClipboardList[targetIndex].Index;
            ClipboardList[targetIndex].Index = index;
            saveList();
        }
    }
}
