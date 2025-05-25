using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Helper;
using H_Assistant.Views;
using H_Assistant.Views.Category;
using HandyControl.Data;
using LiteDB;
using System;
using System.Windows;
using System.Windows.Media;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcWordCount : BaseUserControl
    {
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        ILiteCollection<SystemSet> db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
        SystemSet model = new SystemSet();
        public UcWordCount()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = null;
            TextEditor.TextArea.SelectionForeground = null;
            TextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
            model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_RegEx);
        }
        /// <summary>
        /// 格式化Json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormatter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TextEditor.Text))
                {
                    Oops.Oh(LanguageHepler.GetLanguage("PleaseJson"));
                    return;
                }
                TextEditor.Text = StrUtil.JsonFormatter(TextEditor.Text);
            }
            catch (Exception)
            {
                Oops.Oh(LanguageHepler.GetLanguage("JsonFailed"));
            }
        }

        /// <summary>
        /// 压缩Json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCompress_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TextEditor.Text))
                {
                    Oops.Oh(LanguageHepler.GetLanguage("PleaseJson"));
                    return;
                }
                TextEditor.Text = StrUtil.JsonCompress(TextEditor.Text);
            }
            catch (Exception)
            {
                Oops.Oh(LanguageHepler.GetLanguage("JsonFailed"));
            }
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
        /// 复制文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextEditor.Text))
            {
                TextEditor.SelectAll();
                Clipboard.SetDataObject(TextEditor.Text);
                Oops.Success(LanguageHepler.GetLanguage("TextCopyClipboard"));
            }
        }

        /// <summary>
        /// 清空输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Text = string.Empty;
        }

        /// <summary>
        /// 设置正则输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            RegEditView regEditView = new RegEditView();
            regEditView.ShowDialog();
            model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_RegEx);//从新获取
            TextEdit();//重新统计
        }
        /// <summary>
        /// 正则替换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRegReplace_Click(object sender, RoutedEventArgs e)
        {
            string content = TextEditor.Text;
            int countRow = 0;//有效行
            int countNoRow = 0;//无效行
            int countStr = 0;
            string ruleContent = "";//处理后文字
            string[] ruleLine = model.Value.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            string[] contentLine = TextEditor.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (model.Value != "" && content != "")
            {
                foreach (string item in contentLine)
                {
                    string strcontent = item.Replace("\r", "");
                    strcontent = strcontent.Replace("\n", "");
                    bool isCount = false;//是否统计
                    foreach (string ritem in ruleLine)
                    {
                        string strrule = ritem.Replace("\r", "");
                        strrule = strrule.Replace("\n", "");
                        if (item.Contains(strrule) || strcontent.Trim() == "")
                        {
                            isCount = true;
                            break;
                        }
                    }
                    if (!isCount)
                    {//统计
                        ruleContent += strcontent + Environment.NewLine;
                        countStr += strcontent.Length;
                        ++countRow;
                    }
                    else
                    {//不统计
                        ++countNoRow;
                    }
                }
            }
            TextEditor.Text = ruleContent;
            TextEdit();//重新统计
        }

        /// <summary>
        /// 编辑器获取焦点自动粘贴剪切板文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            var clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(TextEditor.Text) && !string.IsNullOrEmpty(clipboardText))
            {
                var isTryParse = false;
                try
                {
                    ///JsonDocument.Parse(clipboardText); 
                    isTryParse = true;
                }
                catch { }
                if (isTryParse)
                {
                    TextEditor.Text = clipboardText;
                }
            }
        }

        private void TextEditor_OnTextChanged(object sender, EventArgs e)
        {
            TextEdit();
        }

        public void TextEdit()
        {
            int iAllChr = 0; //字符总数：不计字符'\n'和'\r'
            int iChineseChr = 0; //中文字符计数
            int iChinesePnct = 0;//中文标点计数
            int iEnglishChr = 0; //英文字符计数
            int iEnglishPnct = 0;//中文标点计数
            int iNumber = 0;  //数字字符：0-9
            foreach (char ch in TextEditor.Text)
            {
                if (ch != '\n' && ch != '\r')
                {
                    iAllChr++;
                }
                if ("～！＠＃￥％…＆（）—＋－＝".IndexOf(ch) != -1 || "｛｝【】：“”；‘'《》，。、？｜＼".IndexOf(ch) != -1)
                {
                    iChinesePnct++;
                }
                if (ch >= 0x4e00 && ch <= 0x9fbb)
                {
                    iAllChr++;
                    iChineseChr++;
                }
                if ("`~!@#$%^&*()_+-={}[]:\";'<>,.?/\\|".IndexOf(ch) != -1)
                {
                    iEnglishPnct++;
                }
                if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
                {
                    iEnglishChr++;
                }
                if (ch >= '0' && ch <= '9')
                {
                    iNumber++;
                }
            }

            ShieldHanZi.Status = iChineseChr;
            ShieldZiMu.Status = iEnglishChr;
            ShieldShuZi.Status = iNumber;
            ShieldBiaoDian.Status = iChinesePnct + iEnglishPnct;
            ShieldTotalZiShu.Status = iChineseChr + iEnglishChr;
            ShieldTotalZiFu.Status = iAllChr;

            #region 正则统计
            string content = TextEditor.Text;
            int countRow = 0;//有效行
            int countNoRow = 0;//无效行
            int countStr = 0;//字数
            string ruleContent = "";//处理后文字
            string[] ruleLine = model.Value.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            string[] contentLine = TextEditor.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            ShieldTotalRows.Status = contentLine.Length;//行数
            if (model.Value != "" && content != "")
            {
                foreach (string item in contentLine)
                {
                    string strcontent = item.Replace("\r", "");
                    strcontent = strcontent.Replace("\n", "");
                    bool isCount = false;//是否统计
                    foreach (string ritem in ruleLine)
                    {
                        string strrule = ritem.Replace("\r", "");
                        strrule = strrule.Replace("\n", "");
                        if (item.Contains(strrule) || strcontent.Trim() == "")
                        {
                            isCount = true;
                            break;
                        }
                    }
                    if (!isCount)
                    {//统计
                        ruleContent += strcontent + Environment.NewLine;
                        countStr += strcontent.Length;
                        ++countRow;
                    }
                    else
                    {//不统计
                        ++countNoRow;
                    }
                }
            }
            ShieldTotalRuleAfter.Status = countRow;//正则后行数
            #endregion
        }
    }
}
