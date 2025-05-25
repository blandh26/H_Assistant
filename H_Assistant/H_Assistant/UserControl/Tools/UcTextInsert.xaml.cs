using H_Assistant.Framework;
using H_Assistant.Helper;
using H_Assistant.Views;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcTextInsert : BaseUserControl
    {
        public UcTextInsert()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (ToolBox)System.Windows.Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }

        /// <summary>
        /// 清除文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextInput.Text = string.Empty;
            TextOutput.Text = string.Empty;
            TextInputStart.Text = string.Empty;
            TextInputEnd.Text = string.Empty;
        }

        /// <summary>
        /// 文本变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var surplusTextNums = TextInput.Text.Length;
            TextSurplusNum.Text = surplusTextNums.ToString();

            if (!string.IsNullOrEmpty(TextInput.Text))
            {
                var outText = new StringBuilder();
                var inputText = this.TextInput.Text;
                string[] lineSplit = inputText.Split(new[] { "\n" }, StringSplitOptions.None);
                TextLine.Text = lineSplit.Count().ToString();
                var count = 0;
                foreach (var item in lineSplit)
                {
                    var itemValue = item;
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    if (item.Contains("\r"))
                    {
                        itemValue = item.Replace("\r", "");
                    }
                    count++;
                    if (string.IsNullOrEmpty(TextInputStart.Text) && string.IsNullOrEmpty(TextInputEnd.Text))
                    {
                        outText.Append("'" + itemValue + "',\n");
                    }
                    else
                    {
                        outText.Append(TextInputStart.Text + itemValue + TextInputEnd.Text + "\n");
                    }
                }
                var result = lineSplit.Length > 1 ? outText.ToString().TrimEnd(new char[] { ',', '\n' }) : outText.ToString();
                TextOutput.Text = result;
                TextOutput.SelectAll();
            }
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextOutput.Text))
            {
                return;
            }
            TextOutput.SelectAll();
            Clipboard.SetDataObject(TextOutput.Text);
            Oops.Success(LanguageHepler.GetLanguage("TextCopyClipboard"));
        }

        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormat_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextOutput.Text))
            {
                return;
            }
            TextOutput.Text = TextOutput.Text.Replace("\n", "");
            TextOutput.SelectAll();
        }
    }
}
