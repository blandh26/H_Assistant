﻿using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Assistant.UserControl.Tags;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace H_Assistant.Views.Category
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class TagsView : INotifyPropertyChanged
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
            "SelectedConnection", typeof(ConnectConfigs), typeof(TagsView), new PropertyMetadata(default(ConnectConfigs)));
        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(TagsView), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DbDataProperty = DependencyProperty.Register(
            "DbData", typeof(Model), typeof(TagsView), new PropertyMetadata(default(Model)));
        public new static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(TagsView), new PropertyMetadata(default(string)));
        /// <summary>
        /// 标签菜单数据列表
        /// </summary>
        public static readonly DependencyProperty TagMenuListProperty = DependencyProperty.Register(
            "TagMenuList", typeof(List<TagInfo>), typeof(TagsView), new PropertyMetadata(default(List<TagInfo>)));

        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register(
            "MainContent", typeof(System.Windows.Controls.UserControl), typeof(TagsView), new PropertyMetadata(default(System.Windows.Controls.UserControl)));
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        public Model DbData
        {
            get => (Model)GetValue(DbDataProperty);
            set => SetValue(DbDataProperty, value);
        }
        public new string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public List<TagInfo> TagMenuList
        {
            get => (List<TagInfo>)GetValue(TagMenuListProperty);
            set
            {
                SetValue(TagMenuListProperty, value);
                OnPropertyChanged(nameof(TagMenuList));
            }
        }
        /// <summary>
        /// 主界面用户控件
        /// </summary>
        public System.Windows.Controls.UserControl MainContent
        {
            get => (System.Windows.Controls.UserControl)GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }
        #endregion

        public TagsView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void TagsView_Loaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            Title = $"{SelectedConnection.ConnectName} - {LanguageHepler.GetLanguage("LabelManager")}";
            var selConn = SelectedConnection;
            var selectDataBase = SelectedDataBase;
            Task.Run(() =>
            {
                var liteInstance = LiteDBHelper.GetInstance();
                var tagMenuList = liteInstance.ToList<TagInfo>(x =>
                   x.ConnectId == selConn.ID &&
                   x.DataBaseName == selectDataBase);
                Dispatcher.Invoke(() =>
                {
                    TagMenuList = tagMenuList;
                    if (!tagMenuList.Any())
                    {
                        NoDataText.Visibility = Visibility.Visible;
                    }
                });
            });
            MainContent = new UcTagObjects();
            #endregion
        }

        public void Tag_ChangeRefreshEvent()
        {
            ReloadMenu();
        }

        /// <summary>
        /// 重新加载标签菜单
        /// </summary>
        public void ReloadMenu()
        {
            #region MyRegion
            var liteInstance = LiteDBHelper.GetInstance();
            var datalist = liteInstance.ToList<TagInfo>(x =>
                x.ConnectId == SelectedConnection.ID &&
                x.DataBaseName == SelectedDataBase
                );
            TagMenuList = datalist;
            NoDataText.Visibility = datalist.Any() ? Visibility.Collapsed : Visibility.Visible;
            #endregion
        }

        /// <summary>
        /// 选择标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var selConn = SelectedConnection;
            var selectDataBase = SelectedDataBase;
            if (listBox.SelectedItems.Count > 0)
            {
                var tag = (TagInfo)listBox.SelectedItems[0];

                var ucTagObjects = new UserControl.Tags.UcTagObjects();
                ucTagObjects.SelectedConnection = selConn;
                ucTagObjects.SelectedDataBase = selectDataBase;
                ucTagObjects.DbData = DbData;
                ucTagObjects.SelectedTag = tag;
                ucTagObjects.LoadPageData();
                MainContent = ucTagObjects;
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var tagAdd = new TagAddView();
            tagAdd.SelectedConnection = SelectedConnection;
            tagAdd.SelectedDataBase = SelectedDataBase;
            tagAdd.ChangeRefreshEvent += Tag_ChangeRefreshEvent;
            tagAdd.Owner = this;
            tagAdd.ShowDialog();
        }

        /// <summary>
        /// 实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchMenu_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTag = SearchTag.Text.Trim();
            var liteDBInstance = LiteDBHelper.GetInstance();
            var datalist = liteDBInstance.db.GetCollection<TagInfo>().Query().
                Where(x => x.ConnectId == SelectedConnection.ID && x.DataBaseName == SelectedDataBase && x.TagName.Contains(searchTag)).ToList();
            TagMenuList = datalist;
            NoDataText.Visibility = datalist.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// 修改标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(ListTag.SelectedItem is TagInfo selectedTag))
            {
                Oops.Oh(LanguageHepler.GetLanguage("PleaseSelectUpdateLabel"));
                return;
            }
            var tagAdd = new TagAddView();
            tagAdd.SelectedTag = selectedTag;
            tagAdd.SelectedConnection = SelectedConnection;
            tagAdd.SelectedDataBase = SelectedDataBase;
            tagAdd.ChangeRefreshEvent += Tag_ChangeRefreshEvent;
            tagAdd.Owner = this;
            tagAdd.ShowDialog();
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(ListTag.SelectedItem is TagInfo selectedTag))
            {
                Oops.Oh(LanguageHepler.GetLanguage("PleaseSelectDeleteLabel"));
                return;
            }
            var msResult = MessageBox.Show(LanguageHepler.GetLanguage("IsDeleteLabelAll"), LanguageHepler.GetLanguage("Tip"), MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
            if (msResult == MessageBoxResult.OK)
            {
                var liteDBInstance = LiteDBHelper.GetInstance();
                var selectedDatabase = SelectedDataBase;
                var connKey = SelectedConnection.ID;
                Task.Run(() =>
                {
                    liteDBInstance.db.GetCollection<TagInfo>().Delete(selectedTag.Id);
                    var datalist = liteDBInstance.db.GetCollection<TagInfo>().Query().
                        Where(x => x.ConnectId == connKey && x.DataBaseName == selectedDatabase).ToList();
                    var list = liteDBInstance.db.GetCollection<TagObjects>().Query().Where(x =>
                        x.ConnectId == connKey &&
                        x.DatabaseName == selectedDatabase &&
                        x.TagId == selectedTag.TagId).ToList();
                    if (list.Any())
                    {
                        foreach (var tagObj in list)
                        {
                            liteDBInstance.db.GetCollection<GroupObjects>().Delete(tagObj.Id);
                        }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        TagMenuList = datalist;
                        ReloadMenu();
                        MainContent = new UcTagObjects();
                    });
                });
            }
        }
    }
}
