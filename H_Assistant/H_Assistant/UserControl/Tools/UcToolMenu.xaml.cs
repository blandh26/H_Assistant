using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Helper;
using H_Assistant.UserControl.Controls;
using H_Assistant.ViewModels;
using H_Assistant.Views;
using H_Assistant.Views.Category;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace H_Assistant.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcToolMenu : BaseUserControl
    {
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        ILiteCollection<ExeModel> db_ExeModel = liteDBHelper.db.GetCollection<ExeModel>();
        ILiteCollection<SystemSet> db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
        string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下

        public UcToolMenu()
        {
            InitializeComponent();
            DataContext = this;
            ExeRefresh();
        }
        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcToolCard_ClickCard(object sender, RoutedEventArgs e)
        {
            var selCard = (UcToolCard)sender;
            if (selCard.Path == "System")
            {
                var title = "";
                var toolBox = new ToolBox();
                var parentWindow = Window.GetWindow(this);
                if (parentWindow is ToolBox) { toolBox = (ToolBox)parentWindow; }
                switch (selCard.Tag)
                {
                    case 1: toolBox.UcBox.Content = new UcTextInsert(); title = LanguageHepler.GetLanguage("UcTextInsert"); break;
                    case 2: toolBox.UcBox.Content = new UcBase64(); title = LanguageHepler.GetLanguage("UcBase64"); break;
                    case 3: toolBox.UcBox.Content = new UcWordCount(); title = LanguageHepler.GetLanguage("UcWordCount"); break;
                    case 4: toolBox.UcBox.Content = new UcJWT(); title = LanguageHepler.GetLanguage("UcJWT"); break;
                    case 5: toolBox.UcBox.Content = new UcBase64ToImg(); title = LanguageHepler.GetLanguage("UcBase64ToImg"); break;
                    case 6: toolBox.UcBox.Content = new UcHex(); title = LanguageHepler.GetLanguage("UcHex"); break;
                    case 7: toolBox.UcBox.Content = new UcIcoToConvert(); title = LanguageHepler.GetLanguage("UcIcoToConvert"); break;
                    default: Oops.Oh(LanguageHepler.GetLanguage("StayTuned")); return;
                }
                toolBox.Title = $"{title} - " + LanguageHepler.GetLanguage("DevelopmentKit");
                toolBox.Show();
            }
            else
            {
                System.Diagnostics.ProcessStartInfo pinfo = new System.Diagnostics.ProcessStartInfo();
                pinfo.UseShellExecute = true;
                pinfo.FileName = selCard.Path;
                //启动进程
                try { Process p = Process.Start(pinfo); }
                catch (Exception) { }
            }
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcToolCard_CloseClickCard(object sender, RoutedEventArgs e)
        {
            var selCard = (UcToolCard)sender;
            string id = selCard.Tag.ToString();
            if (selCard.Path == "System")
            {
                Oops.Oh(LanguageHepler.GetLanguage("SystemInoperable"));
            }
            else
            {
                var exeEdit = new ExeEditView(id);
                exeEdit.ShowDialog();
                ExeRefresh();
            }
        }

        /// <summary>
        /// 弹出添加框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var exeEdit = new ExeEditView(null);
            exeEdit.ShowDialog();
            ExeRefresh();
        }

        /// <summary>
        /// 检索端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchExe_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ExeRefresh();
        }

        /// <summary>
        /// exe列表刷新
        /// </summary>
        public void ExeRefresh()
        {
            WaterfallPanelList.Children.Clear();
            WaterfallPanelList.Groups = Convert.ToInt32(db_SystemSet.FindOne(x => x.Name == SysConst.Sys_Groups).Value);
            List<ExeModel> list = new List<ExeModel>();
            if (SearchExe.Text!="")
            {
                list = db_ExeModel.Query().Where(x=>x.Title.Contains(SearchExe.Text)).ToList();
            }
            else
            {
                list = db_ExeModel.Query().ToList();
            }
            list = list.OrderBy(x => x.Order).ToList(); // 排序
            foreach (ExeModel model in list)
            {
                UcToolCard umodel = new UcToolCard();
                umodel.Title = model.Path == "System" ? LanguageHepler.GetLanguage(model.Title) : model.Title;
                umodel.Icon = model.Path == "System" ? model.Icon: path+ model.Icon;
                umodel.Tag = model.Id;
                umodel.Path = model.Path;
                umodel.DoubleClickCard += UcToolCard_ClickCard;
                umodel.EditClickCard += UcToolCard_CloseClickCard;
                WaterfallPanelList.Children.Add(umodel);
            }
        }
    }
}
