using H_Assistant.Framework;
using H_ScreenCapture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H_Assistant.Helper
{
    public class ScreenCaptureHelper
    {
        static FrmCapture frm;
        /// <summary>
        /// 截图快捷键
        /// </summary>
        /// <returns></returns>
        public void ScreenCapture()
        {
            try
            {
                if (frm == null)
                {
                    frm = new FrmCapture(0);
                    frm.FormClosed += FormClosed;
                    frm.Show();
                }
            }
            catch (Exception ex) { Oops.God(LanguageHepler.GetLanguage("ScreenshotFailed")); }
        }

        /// <summary>
        /// 截图上一次快捷键
        /// </summary>
        /// <returns></returns>
        public void ScreenCapture_1()
        {
            try
            {
                if (frm == null)
                {
                    frm = new FrmCapture(1);
                    frm.FormClosed += FormClosed;
                }
                else if (frm.IsDisposed)
                {
                    frm = new FrmCapture(1);
                    frm.FormClosed += FormClosed;
                }
                frm.Show();
            }
            catch (Exception ex) { Oops.God(LanguageHepler.GetLanguage("ScreenshotFailed")); }
        }

        /// <summary>
        /// 截图对比快捷键
        /// </summary>
        /// <returns></returns>
        public void ScreenCapture_2()
        {
            try
            {
                if (frm == null)
                {
                    frm = new FrmCapture(2);
                    frm.FormClosed += FormClosed;
                    frm.Show();
                }
            }
            catch { Oops.God(LanguageHepler.GetLanguage("ContrastScreenshotFailed")); }
        }

        private void FormClosed(object sender, FormClosedEventArgs e)
        {
            frm.Dispose();
            frm = null;
        }
    }
}
