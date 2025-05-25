using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Util;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static H_Assistant.MainWindow;

namespace H_Assistant.Views
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow
    {
        delegateRegist WindowRegist;
        delegateLanguage Language;
        public SettingWindow(delegateRegist _WindowRegist, delegateLanguage _Language)
        {
            InitializeComponent();
            DataContext = this;
            WindowRegist = _WindowRegist;
            Language = _Language;
            Grid1.Visibility = Visibility.Visible;
            Grid2.Visibility = Visibility.Collapsed;
            ListGroup.SelectedItem = ListItemRoutine_Common;
            ListItemRoutine_Common.Focus();
            var liteDBHelper = LiteDBHelper.GetInstance();
            var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
            var sysSets = liteDBHelper.db.GetCollection<SystemSet>().Query().Where(x => KeyList.Contains(x.Name)).ToList();
            sysSets.ForEach(x =>
            {
                var value = Convert.ToBoolean(x.Value);
                switch (x.Name)
                {
                    case "IsMultipleTab":
                        ChkIsMultipleTab.IsChecked = value; break;
                    case "IsLikeSearch":
                        ChkIsLikeSearch.IsChecked = value; break;
                    case "IsContainsObjName":
                        ChkIsContainsObjName.IsChecked = value; break;
                    case "IsShowSaveWin":
                        ChkIsShowSaveWin.IsChecked = value; break;
                }
            });
            SystemSet a_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_AutoStart);// 开机自动启动
            ChkAutoStart.IsChecked = false;
            if (a_Model != null)
            {
                if (a_Model.Value == "1") { ChkAutoStart.IsChecked = true; }
            }
            SystemSet l_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_Language);// 语言设定
            if (l_Model != null)
            {
                switch (l_Model.Value)
                {
                    case "cn": ComboDefaultLanguage.SelectedIndex = 0; break;
                    case "kr": ComboDefaultLanguage.SelectedIndex = 1; break;
                    case "jp": ComboDefaultLanguage.SelectedIndex = 2; break;
                    case "en": ComboDefaultLanguage.SelectedIndex = 3; break;
                }
            }
            SystemSet g_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_Groups);// 工具箱行数
            if (g_Model != null)
            {
                WaterfallPanelList_Groups.Text = g_Model.Value;
            }
            SystemSet st_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureTitle);// 对比窗体名称
            if (st_Model != null)
            {
                ContrastTitle.Text = st_Model.Value;
            }
            SystemSet so_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureOpacity);// 对比窗体透明度
            if (so_Model != null)
            {
                ContrastOpacity.Value = Convert.ToDouble(so_Model.Value);
            }
            SystemSet sk_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureKey);// 截图快捷键
            if (sk_Model != null)
            {
                ScreenCaptureCtrl.IsChecked = sk_Model.Value.Split('|')[0] == "1" ? true : false;
                ScreenCaptureAlt.IsChecked = sk_Model.Value.Split('|')[1] == "1" ? true : false;
                ScreenCaptureShift.IsChecked = sk_Model.Value.Split('|')[2] == "1" ? true : false;
                ScreenCaptureKey.Text = sk_Model.Value.Split('|')[3];
            }
            SystemSet sl_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureLastKey);// 最后一次截图快捷键
            if (sl_Model != null)
            {
                LastScreenCaptureCtrl.IsChecked = sl_Model.Value.Split('|')[0] == "1" ? true : false;
                LastScreenCaptureAlt.IsChecked = sl_Model.Value.Split('|')[1] == "1" ? true : false;
                LastScreenCaptureShift.IsChecked = sl_Model.Value.Split('|')[2] == "1" ? true : false;
                LastScreenCaptureKey.Text = sl_Model.Value.Split('|')[3];
            }
            SystemSet sd_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureDiffKey);// 对比截图快捷键
            if (sd_Model != null)
            {
                ContrastScreenCaptureCtrl.IsChecked = sd_Model.Value.Split('|')[0] == "1" ? true : false;
                ContrastScreenCaptureAlt.IsChecked = sd_Model.Value.Split('|')[1] == "1" ? true : false;
                ContrastScreenCaptureShift.IsChecked = sd_Model.Value.Split('|')[2] == "1" ? true : false;
                ContrastScreenCaptureKey.Text = sd_Model.Value.Split('|')[3];
            }
            SystemSet c_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ClipboardKey);// 剪贴板快捷键
            if (c_Model != null)
            {
                ClipboardCtrl.IsChecked = c_Model.Value.Split('|')[0] == "1" ? true : false;
                ClipboardAlt.IsChecked = c_Model.Value.Split('|')[1] == "1" ? true : false;
                ClipboardShift.IsChecked = c_Model.Value.Split('|')[2] == "1" ? true : false;
                if (c_Model != null)
                {
                    switch (c_Model.Value.Split('|')[3])
                    {
                        case "S": ClipboardKey.SelectedIndex = 0; break;
                        case "T": ClipboardKey.SelectedIndex = 1; break;
                    }
                }
            }
        }

        private readonly List<string> KeyList = new List<string> { SysConst.Sys_IsMultipleTab, SysConst.Sys_IsLikeSearch, SysConst.Sys_IsContainsObjName, SysConst.Sys_IsShowSaveWin };
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var liteDBHelper = LiteDBHelper.GetInstance();
            var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
            #region 原有保存
            var isMultipleTab = ChkIsMultipleTab.IsChecked == true;
            var isLikeSearch = ChkIsLikeSearch.IsChecked == true;
            var isContainsObjName = ChkIsContainsObjName.IsChecked == true;
            var isShowSaveWin = ChkIsShowSaveWin.IsChecked == true;
            var sysSets = liteDBHelper.db.GetCollection<SystemSet>().Query().Where(x => KeyList.Contains(x.Name)).ToList();
            sysSets.ForEach(x =>
            {
                if (x.Name == SysConst.Sys_IsMultipleTab) { x.Value = isMultipleTab.ToString(); }
                if (x.Name == SysConst.Sys_IsLikeSearch) { x.Value = isLikeSearch.ToString(); }
                if (x.Name == SysConst.Sys_IsContainsObjName) { x.Value = isContainsObjName.ToString(); }
                if (x.Name == SysConst.Sys_IsShowSaveWin) { x.Value = isShowSaveWin.ToString(); }
            });
            liteDBHelper.db.GetCollection<SystemSet>().Update(sysSets);
            #endregion
            #region 工具箱行数
            SystemSet g_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_Groups);
            if (g_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_Groups, Type = 99, Value = "5" }); }
            else
            {
                try
                {
                    Convert.ToInt32(WaterfallPanelList_Groups.Text);
                    g_Model.Value = WaterfallPanelList_Groups.Text;// 工具箱行数
                }
                catch (Exception)
                {
                    g_Model.Value = "5";// 工具箱行数
                }
                db_SystemSet.Update(g_Model);
                Language();
            }
            #endregion
            #region 语言设定+开机启动
            SystemSet l_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_Language);
            if (l_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_Language, Type = 99, Value = "cn" }); }
            else
            {
                l_Model.Value = ((System.Windows.UIElement)ComboDefaultLanguage.SelectedItem).Uid;// 语言设定
                db_SystemSet.Update(l_Model);
                Language();
            }
            SystemSet a_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_AutoStart);// 开机自动启动
            if (a_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_AutoStart, Type = 99, Value = "0" }); }
            else
            {
                a_Model.Value = ChkAutoStart.IsChecked == true ? "1" : "0";// 语言设定
                db_SystemSet.Update(a_Model);
            }
            if (ChkAutoStart.IsChecked == true) { RegeditHelp.AutoStart("H_Assistant", System.Windows.Forms.Application.ExecutablePath); }
            #endregion

            #region 截图有关
            SystemSet st_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureTitle);// 对比窗体名称
            if (st_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_ScreenCaptureTitle, Type = 99, Value = "Contrast${yyyy-MM-dd HH:mm:ss}" }); }
            else { st_Model.Value = ContrastTitle.Text; db_SystemSet.Update(st_Model); }

            SystemSet so_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureOpacity); // 对比窗体透明度
            if (so_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_ScreenCaptureOpacity, Type = 99, Value = "60" }); }
            else { so_Model.Value = ContrastOpacity.Value.ToString(); db_SystemSet.Update(so_Model); }

            string ScreenCapture_Key =  // 截图快捷键
                (ScreenCaptureCtrl.IsChecked == true ? "1" : "0") + "|" +
                (ScreenCaptureAlt.IsChecked == true ? "1" : "0") + "|" +
                (ScreenCaptureShift.IsChecked == true ? "1" : "0") + "|" +
                ScreenCaptureKey.Text;
            SystemSet sk_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureKey);
            if (sk_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_ScreenCaptureKey, Type = 99, Value = "0|1|0|Q" }); }
            else { sk_Model.Value = ScreenCapture_Key; db_SystemSet.Update(sk_Model); }

            string ScreenCapture_LastKey =  // 最后一次截图快捷键
                (LastScreenCaptureCtrl.IsChecked == true ? "1" : "0") + "|" +
                (LastScreenCaptureAlt.IsChecked == true ? "1" : "0") + "|" +
                (LastScreenCaptureShift.IsChecked == true ? "1" : "0") + "|" +
                LastScreenCaptureKey.Text;
            SystemSet sl_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureLastKey);
            if (sl_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_ScreenCaptureLastKey, Type = 99, Value = "0|1|0|W" }); }
            else { sl_Model.Value = ScreenCapture_LastKey; db_SystemSet.Update(sl_Model); }

            string ScreenCapture_DiffKey =  // 对比截图快捷键
                (ContrastScreenCaptureCtrl.IsChecked == true ? "1" : "0") + "|" +
                (ContrastScreenCaptureAlt.IsChecked == true ? "1" : "0") + "|" +
                (ContrastScreenCaptureShift.IsChecked == true ? "1" : "0") + "|" +
                ContrastScreenCaptureKey.Text;
            SystemSet sd_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureDiffKey);
            if (sd_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_ScreenCaptureDiffKey, Type = 99, Value = "0|1|0|E" }); }
            else { sd_Model.Value = ScreenCapture_DiffKey; db_SystemSet.Update(sd_Model); }
            #endregion

            string clipboardKey =  // 剪贴板快捷键
               (ClipboardCtrl.IsChecked == true ? "1" : "0") + "|" +
               (ClipboardAlt.IsChecked == true ? "1" : "0") + "|" +
               (ClipboardShift.IsChecked == true ? "1" : "0") + "|" +
               ((System.Windows.UIElement)ClipboardKey.SelectedItem).Uid;
            SystemSet c_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ClipboardKey);
            if (c_Model == null) { db_SystemSet.Insert(new SystemSet { Name = SysConst.Sys_ClipboardKey, Type = 99, Value = "0|1|0|S" }); }
            else { c_Model.Value = clipboardKey; db_SystemSet.Update(c_Model); }
            // 重新注册快捷键
            WindowRegist();
            this.Close();
        }

        /// <summary>
        /// 获取辅助按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            (sender as System.Windows.Controls.TextBox).Text = e.Key.ToString();   //显示点下的按键
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #region 左侧选项卡控制
        /// <summary>
        /// 常规
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItemRoutine_Common_Selected(object sender, RoutedEventArgs e)
        {
            Grid1.Visibility = Visibility.Visible;
            Grid2.Visibility = Visibility.Collapsed;
            Grid3.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItemRoutine_ScreenCapture_Selected(object sender, RoutedEventArgs e)
        {
            Grid1.Visibility = Visibility.Collapsed;
            Grid2.Visibility = Visibility.Visible;
            Grid3.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItemRoutine_Clipboard_Selected(object sender, RoutedEventArgs e)
        {
            Grid1.Visibility = Visibility.Collapsed;
            Grid2.Visibility = Visibility.Collapsed;
            Grid3.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
