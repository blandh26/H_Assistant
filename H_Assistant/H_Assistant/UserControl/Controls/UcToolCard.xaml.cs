using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace H_Assistant.UserControl.Controls
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class UcToolCard : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(UcToolCard), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(UcToolCard), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(UcToolCard), new PropertyMetadata(default(string)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }


        /// <summary>
        /// 当前选中对象
        /// </summary>
        public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public string Path { get => (string)GetValue(PathProperty); set => SetValue(PathProperty, value); }

        /// <summary>
        /// 单击事件
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("ClickCard", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UcToolCard));

        /// <summary>
        /// 点击卡片的操作.
        /// </summary>
        public event RoutedEventHandler ClickCard { add => AddHandler(ClickEvent, value); remove => RemoveHandler(ClickEvent, value); }

        /// <summary>
        /// 双击事件
        /// </summary>
        public static readonly RoutedEvent DoubleClickEvent = EventManager.RegisterRoutedEvent("DoubleClickCard", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UcToolCard));

        /// <summary>
        /// 双击卡片的操作.
        /// </summary>
        public event RoutedEventHandler DoubleClickCard { add => AddHandler(DoubleClickEvent, value); remove => RemoveHandler(DoubleClickEvent, value); }

        // <summary>
        /// 修改事件
        /// </summary>
        public static readonly RoutedEvent EditClickEvent = EventManager.RegisterRoutedEvent("CloseClickCard", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UcToolCard));

        /// <summary>
        /// 修改的操作.
        /// </summary>
        public event RoutedEventHandler EditClickCard { add => AddHandler(EditClickEvent, value); remove => RemoveHandler(EditClickEvent, value); }

        public UcToolCard()
        {
            InitializeComponent();
            DataContext = this;
        }
        /// <summary>
        /// 按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) { UIElement_Click(sender, e); }
            if (e.ClickCount > 1) { UIElement_DoubleClick(sender, e); }
        }
        /// <summary>
        /// 点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_Click(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(DoubleClickEvent, this));
        }
        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseLeftButtonUp_Edit(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(EditClickEvent, this));
        }
    }
}
