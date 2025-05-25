using H_Assistant.Annotations;
using H_Assistant.Framework;
using HandyControl.Controls;
using HandyControl.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace H_Assistant.UserControl.Controls
{
    /// <summary>
    /// CsharpScript.xaml 的交互逻辑
    /// </summary>
    public partial class CsharpScript : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty SqlTextProperty = DependencyProperty.Register(
            "SqlText", typeof(string), typeof(CsharpScript), new PropertyMetadata(default(string)));
        /// <summary>
        /// Sql文本
        /// </summary>
        public string SqlText
        {
            get => (string)GetValue(SqlTextProperty);
            set
            {
                SetValue(SqlTextProperty, value);
                OnPropertyChanged(nameof(SqlText));
            }
        }

        public static readonly DependencyProperty HasCloseProperty = DependencyProperty.Register(
            "HasClose", typeof(bool), typeof(CsharpScript), new PropertyMetadata(default(bool)));
        /// <summary>
        /// 是否有关闭按钮
        /// </summary>
        public bool HasClose
        {
            get => (bool)GetValue(HasCloseProperty);
            set
            {
                SetValue(HasCloseProperty, value);
                OnPropertyChanged(nameof(HasClose));
            }
        }
        public CsharpScript()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = null;
            TextEditor.TextArea.SelectionForeground = null;
            TextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        private void CsharpScript_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (HasClose)
            {
                BtnCopyScript.Margin = new Thickness(0, 0, 95, 5);
            }
            TextEditor.Text = SqlText;
        }

        /// <summary>
        /// 复制脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopyScript_OnClick(object sender, RoutedEventArgs e)
        {
            TextEditor.SelectAll();
            Clipboard.SetDataObject(SqlText.SqlFormat());
            Growl.SuccessGlobal(new GrowlInfo { Message = LanguageHepler.GetLanguage("ScriptCopyClipboard"), WaitTime = 1, ShowDateTime = false });
        }
    }
}
