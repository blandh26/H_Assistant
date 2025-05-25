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
    public partial class TestAddView : INotifyPropertyChanged
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
            "SelectedConnection", typeof(ConnectConfigs), typeof(GroupAddView), new PropertyMetadata(default(ConnectConfigs)));
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(GroupAddView), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
            "SelectedGroup", typeof(GroupInfo), typeof(GroupAddView), new PropertyMetadata(default(GroupInfo)));
        public GroupInfo SelectedGroup
        {
            get => (GroupInfo)GetValue(SelectedGroupProperty);
            set
            {
                SetValue(SelectedGroupProperty, value);
                OnPropertyChanged(nameof(SelectedGroup));
            }
        }
        #endregion

        public TestAddView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                {
                    Keyboard.Focus(TextGroupName);
                    if (SelectedGroup != null)
                    {
                        Title = "修改分组";
                        CheckCurrent.IsChecked = SelectedGroup.OpenLevel == 1;
                        CheckChild.IsChecked = SelectedGroup.OpenLevel == 2;
                        CheckNone.IsChecked = SelectedGroup.OpenLevel == null || SelectedGroup.OpenLevel == 0;
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
            var groupName = TextGroupName.Text.Trim();
            if (string.IsNullOrEmpty(groupName))
            {
                TextErrorMsg.Visibility = Visibility.Visible;
                return;
            }
            var liteDBHelperInstance = LiteDBHelper.GetInstance();
            if (SelectedGroup == null)
            {
                var tag = liteDBHelperInstance.db.GetCollection<GroupInfo>().FindOne(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.GroupName == groupName);
                if (tag != null)
                {
                    Oops.Oh("已存在相同名称的分组.");
                    return;
                }
                //插入分组数据
                liteDBHelperInstance.db.GetCollection<GroupInfo>().Insert(new GroupInfo()
                {
                    ConnectId = SelectedConnection.ID,
                    DataBaseName = SelectedDataBase,
                    OpenLevel = CheckCurrent.IsChecked == true ? 1 : (CheckChild.IsChecked == true ? 2 : 0),
                    GroupName = groupName
                });
            }
            else
            {
                var tag = liteDBHelperInstance.db.GetCollection<GroupInfo>().FindOne(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.Id != SelectedGroup.Id &&
                    x.GroupName == groupName);
                if (tag != null)
                {
                    Oops.Oh("已存在相同名称的分组.");
                    return;
                }
                tag = liteDBHelperInstance.db.GetCollection<GroupInfo>().FindOne(x => x.Id == SelectedGroup.Id);
                tag.GroupName = groupName;
                tag.OpenLevel = CheckCurrent.IsChecked == true ? 1 : (CheckChild.IsChecked == true ? 2 : 0);
                liteDBHelperInstance.db.GetCollection<GroupInfo>().Update(tag);
            }
            if (ChangeRefreshEvent != null)
            {
                ChangeRefreshEvent();
            }
            this.Close();
            Oops.Success("保存成功.");
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
        private void TextGroupName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextGroupName.Text.Trim()))
            {
                TextErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
    }
}
