using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Helper;
using System;
using System.ComponentModel;
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
    public partial class TagAddView : INotifyPropertyChanged
    {
        public event ChangeRefreshHandler ChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region DependencyProperty

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(TagAddView), new PropertyMetadata(default(ConnectConfigs)));
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(TagAddView), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty.Register(
            "SelectedTag", typeof(TagInfo), typeof(TagAddView), new PropertyMetadata(default(TagInfo)));
        public TagInfo SelectedTag
        {
            get => (TagInfo)GetValue(SelectedTagProperty);
            set
            {
                SetValue(SelectedTagProperty, value);
                OnPropertyChanged(nameof(SelectedTag));
            }
        }
        #endregion

        public TagAddView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                {
                    Keyboard.Focus(TagName);
                    if (SelectedTag != null)
                    {
                        Title = LanguageHepler.GetLanguage("UpdateLabel");
                    }
                }));
        }

        /// <summary>
        /// 保存标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var tagName = TagName.Text.Trim();
            if (string.IsNullOrEmpty(tagName))
            {
                //Oops.Oh("标签名称为空.");

                TextErrorMsg.Visibility = Visibility.Visible;
                return;
            }
            var liteDBInstance = LiteDBHelper.GetInstance();
            if (SelectedTag == null)
            {
                var tag = liteDBInstance.db.GetCollection<TagInfo>().FindOne(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.TagName == tagName);
                if (tag != null)
                {
                    Oops.Oh(LanguageHepler.GetLanguage("SameLabel"));
                    return;
                }
                //插入标签数据
                liteDBInstance.db.GetCollection<TagInfo>().Insert(new TagInfo()
                {
                    ConnectId = SelectedConnection.ID,
                    DataBaseName = SelectedDataBase,
                    TagName = tagName,
                    SubCount = 0
                });
            }
            else
            {
                var tag = liteDBInstance.db.GetCollection<TagInfo>().FindOne(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.TagId != SelectedTag.TagId &&
                    x.TagName == tagName);
                if (tag != null)
                {
                    Oops.Oh(LanguageHepler.GetLanguage("SameLabel"));
                    return;
                }
                tag = liteDBInstance.db.GetCollection<TagInfo>().FindOne(x => x.TagId == SelectedTag.TagId);
                tag.TagName = tagName;
                liteDBInstance.db.GetCollection<TagInfo>().Update(tag);
            }
            if (ChangeRefreshEvent != null)
            {
                ChangeRefreshEvent();
            }
            this.Close();
            Oops.Success(LanguageHepler.GetLanguage("SuccessfullySave"));
        }

        /// <summary>
        /// Enter键一键保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagName_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSave_Click(sender, e);
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 文本框变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TagName.Text.Trim()))
            {
                TextErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
    }
}
