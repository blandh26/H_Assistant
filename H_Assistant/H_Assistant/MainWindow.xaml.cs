using H_Assistant.Annotations;
using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Assistant.Models;
using H_Assistant.UserControl;
using H_Assistant.UserControl.Dialog;
using H_Assistant.Views;
using H_Assistant.Views.Category;
using H_Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MessageBox = HandyControl.Controls.MessageBox;

namespace H_Assistant
{
    public partial class MainWindow : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ConnectConfigs SelectendConnection = null;

        #region 快捷键 系统api
        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, HotkeyModifiers fsModifiers, uint vk);
        /// <summary>
        /// 卸载快捷键
        /// </summary>
        /// <param name="hWnd">定义热键的内容</param>
        /// <param name="id">定义热键ID</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// 快捷键
        /// </summary>
        public enum HotkeyModifiers
        {
            MOD_ALT = 0x1,
            MOD_CONTROL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8,
            MOD_ALT_CONTROL = (0x1 | 0x2),
            MOD_ALT_SHIFT = (0x1 | 0x4),
            MOD_CONTROLT_SHIFT = (0x2 | 0x4),
            MOD_CONTROLT_SHIFT_ALT = (0x1 | 0x2 | 0x4)
        }
        #endregion

        #region 快捷键函数
        static int keyid = 10;
        public static Dictionary<int, HotKeyCallBackHanlder> keymap = new Dictionary<int, HotKeyCallBackHanlder>();
        public delegate void HotKeyCallBackHanlder();

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="window">持有快捷键窗口</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="key">快捷键</param>
        /// <param name="callBack">回调函数</param>
        public void Regist(Window window, HotkeyModifiers fsModifiers, Key key, HotKeyCallBackHanlder callBack)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            AddClipboardFormatListener(hwnd);
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            // 如果 _hwndSource 尚未初始化，则初始化并添加钩子
            if (_hwndSource == null)
            {
                _hwndSource = HwndSource.FromHwnd(hwnd);
                _hwndSource.AddHook(new MainWindow().WndProc);
            }

            int id = keyid++;

            var vk = KeyInterop.VirtualKeyFromKey(key);
            if (!RegisterHotKey(hwnd, id, fsModifiers, (uint)vk)) { throw new Exception(); }
            keymap[id] = callBack;
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        /// <param name="hWnd">持有快捷键窗口的句柄</param>
        /// <param name="callBack">回调函数</param>
        public static void UnRegist(IntPtr hWnd, HotKeyCallBackHanlder callBack)
        {
            foreach (KeyValuePair<int, HotKeyCallBackHanlder> var in keymap) { UnregisterHotKey(hWnd, var.Key); }
            RemoveClipboardFormatListener(hWnd);
            keymap.Clear();
        }

        /// <summary>
        /// select 转enum
        /// </summary>
        /// <param name="select">select</param>
        public HotkeyModifiers GetHotkeyModifiers(string select)
        {
            switch (select)
            {
                case "Alt+Shift": return HotkeyModifiers.MOD_ALT_SHIFT;
                case "Alt+Ctrl": return HotkeyModifiers.MOD_ALT_CONTROL;
                case "Ctrl+Shift": return HotkeyModifiers.MOD_CONTROLT_SHIFT;
                case "Alt": return HotkeyModifiers.MOD_ALT;
                case "Shift": return HotkeyModifiers.MOD_SHIFT;
                case "Ctrl": return HotkeyModifiers.MOD_CONTROL;
            }
            return HotkeyModifiers.MOD_WIN;
        }
        #endregion

        #region 本页面快捷键有关
        const int WM_HOTKEY = 0x312;// 当用户按下由REGISTERHOTKEY函数注册的热键时提交此消息
        /// https://blog.csdn.net/u011555996/article/details/113785700 参考 msg 数字
        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (keymap.TryGetValue(id, out var callback))
                {
                    callback();
                }
            }
            return hwnd;
        }
        #endregion

        #region 注册页面快捷键
        /// <summary>
        /// 注册页面快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void _WindowRegist()
        {
            ScreenCaptureHelper scHelper = new ScreenCaptureHelper();// 截图工具类
            var liteDBHelper = LiteDBHelper.GetInstance();
            var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
            SystemSet sk_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureKey);// 截图快捷键
            SystemSet sl_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureLastKey);// 最后一次截图快捷键
            SystemSet sd_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureDiffKey);// 对比截图快捷键
            SystemSet c_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ClipboardKey);// 剪贴板快捷键
            try
            {//卸载快捷键
                var hwnd = new WindowInteropHelper(this).Handle;
                UnRegist(hwnd, () => { });//停止共享
            }
            catch (Exception) { }
            string value;
            uint keyNumber = 0;
            try // 截图快捷键
            {
                value = sk_Model.Value;
                keyNumber = 0;
                if (value.Split('|')[0] == "1") { keyNumber = keyNumber | 0x2; }// Ctrl
                if (value.Split('|')[1] == "1") { keyNumber = keyNumber | 0x1; }// Alt
                if (value.Split('|')[2] == "1") { keyNumber = keyNumber | 0x4; }// Shift 
                Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), value.Split('|')[3]), () => { scHelper.ScreenCapture(); });
            }
            catch (Exception) { Oops.OhGlobal(LanguageHepler.GetLanguage("ScreenshotShortcutKeyRegistrationFailed")); }
            try // 最后一次截图快捷键
            {
                value = sl_Model.Value;
                keyNumber = 0;
                if (value.Split('|')[0] == "1") { keyNumber = keyNumber | 0x2; }// Ctrl
                if (value.Split('|')[1] == "1") { keyNumber = keyNumber | 0x1; }// Alt
                if (value.Split('|')[2] == "1") { keyNumber = keyNumber | 0x4; }// Shift 
                Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), value.Split('|')[3]), () => { scHelper.ScreenCapture_1(); 
                });
            }
            catch (Exception) { Oops.OhGlobal(LanguageHepler.GetLanguage("LastScreenshotShortcutKeyRegistrationFailed")); }
            try // 对比截图快捷键
            {
                value = sd_Model.Value;
                keyNumber = 0;
                if (value.Split('|')[0] == "1") { keyNumber = keyNumber | 0x2; }// Ctrl
                if (value.Split('|')[1] == "1") { keyNumber = keyNumber | 0x1; }// Alt
                if (value.Split('|')[2] == "1") { keyNumber = keyNumber | 0x4; }// Shift 
                Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), value.Split('|')[3]), () => { scHelper.ScreenCapture_2(); });
            }
            catch (Exception) { Oops.OhGlobal(LanguageHepler.GetLanguage("ContrastScreenshotShortcutKeyRegistrationFailed")); }
            try // 剪贴板快捷键
            {
                value = c_Model.Value;
                keyNumber = 0;
                if (value.Split('|')[0] == "1") { keyNumber = keyNumber | 0x2; }// Ctrl
                if (value.Split('|')[1] == "1") { keyNumber = keyNumber | 0x1; }// Alt
                if (value.Split('|')[2] == "1") { keyNumber = keyNumber | 0x4; }// Shift 
                if (value.Split('|')[3] == "S")
                {
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad0.ToString()), () => { Clipboard(10); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad1.ToString()), () => { Clipboard(1); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad2.ToString()), () => { Clipboard(2); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad3.ToString()), () => { Clipboard(3); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad4.ToString()), () => { Clipboard(4); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad5.ToString()), () => { Clipboard(5); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad6.ToString()), () => { Clipboard(6); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad7.ToString()), () => { Clipboard(7); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad8.ToString()), () => { Clipboard(8); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.NumPad9.ToString()), () => { Clipboard(9); });
                }
                if (value.Split('|')[3] == "T")
                {
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D0.ToString()), () => { Clipboard(10); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D1.ToString()), () => { Clipboard(1); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D2.ToString()), () => { Clipboard(2); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D3.ToString()), () => { Clipboard(3); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D4.ToString()), () => { Clipboard(4); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D5.ToString()), () => { Clipboard(5); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D6.ToString()), () => { Clipboard(6); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D7.ToString()), () => { Clipboard(7); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D8.ToString()), () => { Clipboard(8); });
                    Regist(this, (HotkeyModifiers)keyNumber, (Key)Enum.Parse(typeof(Key), Key.D9.ToString()), () => { Clipboard(9); });
                }
            }
            catch (Exception) { Oops.OhGlobal(LanguageHepler.GetLanguage("ClipboardKeyRegistrationFailed")); }
        }
        #endregion
        public delegate void delegateLanguage();// 定义一个委托 设置
        public delegate void delegateRegist();// 定义一个委托 设置
        public delegate void delegateDatabaseList();// 定义一个委托 新建数据库
        /// <summary>
        /// 剪贴板
        /// </summary>
        /// <param name="index">序号</param>
        private void Clipboard(int index)
        {
            var liteDBHelper = LiteDBHelper.GetInstance();
            var db_ClipboardModel = liteDBHelper.db.GetCollection<ClipboardModel>();
            ClipboardModel model = db_ClipboardModel.FindOne(x => x.Index == index);
            if (model != null)
            {
                try { System.Windows.Clipboard.SetDataObject(model.Content); }
                catch (Exception) { }
            }
        }
        /// <summary>
        /// 加载数据库
        /// </summary>
        public void _DatabaseList()
        {
            var liteDbInstance = LiteDBHelper.GetInstance();
            var connectConfigs = liteDbInstance.ToList<ConnectConfigs>();
            SwitchMenu.ItemsSource = null;
            SwitchMenu.ItemsSource = connectConfigs;
            if (!connectConfigs.Any())
            {
                SwitchMenu.Header = LanguageHepler.GetLanguage("NewConnection");
            }
        }
        /// <summary>
        /// 系统中设置语言
        /// </summary>
        /// <returns></returns>
        public static void _SetLanguage()
        {
            try
            {
                string aa = LanguageHepler.GetLanguage("NiceNameTip");
                string requestedCulture = @"Language/xaml/" + LanguageHepler.GetDbLanguage() + ".xaml";
                List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
                foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
                {
                    if (dictionary.Source != null) { dictionaryList.Add(dictionary); }
                }
                ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            catch (Exception ee)
            {
                Oops.Oh(LanguageHepler.GetLanguage("FailedSetSystemLanguage"));
            }
        }

        #region 左侧菜单

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(MainWindow), new PropertyMetadata(default(List<TreeNodeItem>)));

        /// <summary>
        /// 左侧菜单数据
        /// </summary>
        public List<TreeNodeItem> TreeViewData
        {
            get => (List<TreeNodeItem>)GetValue(TreeViewDataProperty);
            set
            {
                SetValue(TreeViewDataProperty, value);
                OnPropertyChanged(nameof(TreeViewData));
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            RegisterEvents();
        }

        /// <summary>
        /// 页面初始化加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _SetLanguage();// 设定语言
            _DatabaseList();// 数据库加载
            _WindowRegist();// 注册快捷键
            MainContent.RegistLoad(_DatabaseList);
            Helper.License.Verify();
            // 快捷键重复被调用所以钩子事件挪动位置
            var hwnd = new WindowInteropHelper(this).Handle;
            AddClipboardFormatListener(hwnd);
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            _hwndSource.AddHook(WndProc);
        }

        /// <summary>
        /// 切换数据库连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchMenu_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.SelectedIndex = 0;
            var menuItem = (MenuItem)sender;
            var connectConfig = (ConnectConfigs)menuItem.DataContext;
            SwitchConnect(connectConfig);
        }

        /// <summary>
        /// 切换连接
        /// </summary>
        /// <param name="connectConfig"></param>
        public void SwitchConnect(ConnectConfigs connectConfig)
        {
            #region MyRegion
            SwitchMenu.Header = connectConfig.ConnectName;
            SelectendConnection = connectConfig;
            var liteDBHelper = LiteDBHelper.GetInstance();
            liteDBHelper.SetSysValue(SysConst.Sys_SelectedConnection, connectConfig.ConnectName);
            var connectConfigs = liteDBHelper.db.GetCollection<ConnectConfigs>().Query().ToList();
            SwitchMenu.ItemsSource = null;
            SwitchMenu.ItemsSource = connectConfigs;
            //加载主界面
            MainContent.PageLoad(connectConfig);
            #endregion
        }

        /// <summary>
        /// 分组管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuGroup_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectDatabase"));
                return;
            }
            var group = new GroupsView();
            group.SelectedConnection = SelectendConnection;
            group.SelectedDataBase = selectDatabase.DbName;
            group.DbData = MainContent.MenuData;
            group.Owner = this;
            group.ChangeRefreshEvent += MainContent.ChangeRefreshMenuEvent;
            group.ShowDialog();
        }

        /// <summary>
        /// 标签管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuTag_Click(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectDatabase"));
                return;
            }
            var tags = new TagsView();
            tags.SelectedConnection = SelectendConnection;
            tags.SelectedDataBase = selectDatabase.DbName;
            tags.DbData = MainContent.MenuData;
            tags.Owner = this;
            tags.ShowDialog();
        }
        /// <summary>
        /// 生成管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuTest_Click(object sender, RoutedEventArgs e)
        {
            //var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            //if (SelectendConnection == null || selectDatabase == null)
            //{
            //    Oops.Oh(LanguageHepler.GetLanguage("SelectDatabase"));
            //    return;
            //}
            //var tags = new TagsView();
            //tags.SelectedConnection = SelectendConnection;
            //tags.SelectedDataBase = selectDatabase.DbName;
            //tags.DbData = MainContent.MenuData;
            //tags.Owner = this;
            //tags.ShowDialog();
        }

        /// <summary>
        /// 全局设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuSetting_OnClick(object sender, RoutedEventArgs e)
        {
            var set = new SettingWindow(_WindowRegist, _SetLanguage);
            set.Owner = this;
            set.ShowDialog();
        }

        /// <summary>
        /// 新建连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            if (menuItem.Header.Equals(LanguageHepler.GetLanguage("NewConnection")))
            {
                var connect = new ConnectManage(_DatabaseList);
                connect.Owner = this;
                connect.ChangeRefreshEvent += SwitchConnect;
                connect.ShowDialog();
            }
        }

        /// <summary>
        /// 新建连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddConnect_OnClick(object sender, RoutedEventArgs e)
        {
            var connect = new ConnectManage(_DatabaseList);
            connect.Owner = this;
            connect.ChangeRefreshEvent += SwitchConnect;
            connect.ShowDialog();
        }

        /// <summary>
        /// 导出文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportDoc_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectDatabase"));
                return;
            }
            var exportDoc = new ExportDoc
            {
                Owner = this,
                MenuData = MainContent.MenuData,
                SelectedConnection = SelectendConnection,
                SelectedDataBase = selectDatabase
            };
            exportDoc.ShowDialog();
        }

        /// <summary>
        /// 导出模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportTemplate_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectDatabase"));
                return;
            }
            var exportTemplate = new ExportTemplate
            {
                Owner = this,
                MenuData = MainContent.MenuData,
                SelectedConnection = SelectendConnection,
                SelectedDataBase = selectDatabase
            };
            exportTemplate.ShowDialog();
        }

        /// <summary>
        /// 导入备注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportMark_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)MainContent.SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Oops.Oh(LanguageHepler.GetLanguage("SelectDatabase"));
                return;
            }
            var importMark = new ImportMark();
            importMark.Owner = this;
            importMark.SelectedConnection = SelectendConnection;
            importMark.SelectedDataBase = selectDatabase;
            importMark.ShowDialog();
        }

        /// <summary>
        /// 关于H_Assistant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuAbout_OnClick(object sender, RoutedEventArgs e)
        {
            //蒙板
            Grid layer = new Grid() { Background = new SolidColorBrush(Color.FromRgb(132, 132, 132)), Opacity = 0.7 };
            //父级窗体原来的内容
            UIElement original = Content as UIElement;//MainWindows父窗体
            Content = null;
            //容器Grid
            Grid container = new Grid();
            container.Children.Add(original);//放入原来的内容
            container.Children.Add(layer);//在上面放一层蒙板
            //将装有原来内容和蒙板的容器赋给父级窗体
            Content = container;
            var about = new About();
            about.Owner = this;
            about.ShowDialog();
        }

        /// <summary>
        /// 置顶
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuTopping_Click(object sender, RoutedEventArgs e)
        {
            if (Topmost)
            {
                SolidColorBrush scb = new SolidColorBrush();
                scb.Color = Color.FromRgb(50, 108, 243);
                MenuTopping.Foreground = scb;
                MenuTopping_Icon.Fill = scb;
                Topmost = false;
            }
            else
            {
                SolidColorBrush scb = new SolidColorBrush();
                scb.Color = Color.FromRgb(255, 39, 0);
                MenuTopping.Foreground = scb;
                MenuTopping_Icon.Fill = scb;
                Topmost = true;
            }
        }

        private void MenuManager_Selected(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            DockUcManager.Visibility = Visibility.Visible;
            DockUcTools.Visibility = Visibility.Collapsed;
            //DockUcDbCompare.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 工具箱
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuTool_Selected(object sender, RoutedEventArgs e)
        {
            UcMainTools.Content = new UcMainTools();
            DockUcTools.Visibility = Visibility.Visible;
            DockUcManager.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 剪贴板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuClipboard_Selected(object sender, RoutedEventArgs e)
        {
            UcMainTools.Content = new UcMainClipboard();
            DockUcTools.Visibility = Visibility.Visible;
            DockUcManager.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 端口工具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuPort_Selected(object sender, RoutedEventArgs e)
        {
            UcMainTools.Content = new UcMainPort();
            DockUcTools.Visibility = Visibility.Visible;
            DockUcManager.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 测试工具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuTest_Selected(object sender, RoutedEventArgs e)
        {
            UcMainTools.Content = new UcMainTest();
            DockUcTools.Visibility = Visibility.Visible;
            DockUcManager.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 关闭程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlowWindow_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0); //这是最彻底的退出方式，不管什么线程都被强制退出，把程序结束的很干净。
        }

        #region 全局异常处理
        private void RegisterEvents()
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;//Task异常 UI线程未捕获异常处理事件（UI主线程）
            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        //Task线程内未捕获异常处理事件
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            H_Util.Log.WriteException("Task线程内未捕获异常处理事件");
            try
            {
                Exception exception = e.Exception as Exception;
                if (exception != null)
                {
                    H_Util.Log.WriteException(exception.Message);
                    H_Util.Log.WriteException(exception.StackTrace);
                    H_Util.Log.WriteException(exception.Source);
                }
            }
            catch (Exception ex)
            {
                H_Util.Log.WriteException(ex.Message);
                H_Util.Log.WriteException(ex.StackTrace);
                H_Util.Log.WriteException(ex.Source);
            }
            finally
            {
                e.SetObserved();
            }
        }
        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)      
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            H_Util.Log.WriteException("非UI线程未捕获异常处理事件(例如自己创建的一个子线程)");
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    H_Util.Log.WriteException(exception.Message);
                    H_Util.Log.WriteException(exception.StackTrace);
                    H_Util.Log.WriteException(exception.Source);
                }
            }
            catch (Exception ex)
            {
                H_Util.Log.WriteException(ex.Message);
                H_Util.Log.WriteException(ex.StackTrace);
                H_Util.Log.WriteException(ex.Source);
            }
            finally
            {
                //ignore
            }
        }
        //UI线程未捕获异常处理事件（UI主线程）
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                H_Util.Log.WriteException(e.Exception.Message);
                H_Util.Log.WriteException(e.Exception.StackTrace);
                H_Util.Log.WriteException(e.Exception.Source);
            }
            catch (Exception ex)
            {
                H_Util.Log.WriteException(e.Exception.Message);
                H_Util.Log.WriteException(e.Exception.StackTrace);
                H_Util.Log.WriteException(e.Exception.Source);
            }
            finally
            {
                e.Handled = true;
            }
        }

        #endregion
    }
}
