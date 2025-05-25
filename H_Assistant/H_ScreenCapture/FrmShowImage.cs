using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace H_ScreenCapture
{
    public partial class FrmShowImage : Form
    {
        Point poit;
        Size size;
        string title;
        Image img;
        public FrmShowImage(Point poit_tmp, Size size_tmp, Image img_tmp)
        {
            try
            {
                InitializeComponent();
                var liteDBHelper = LiteDBHelper.GetInstance();
                var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
                SystemSet st_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureTitle);// 对比窗体名称
                string strTitle = st_Model.Value;
                string strFormat = Regex.Match(strTitle, @"\${(.*)\}", RegexOptions.Singleline).Groups[1].Value;//大括号{}
                title = strTitle.Replace("${" + strFormat + "}", DateTime.Now.ToString(strFormat));
                poit = poit_tmp;
                size = size_tmp;
                img = img_tmp;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("[异常]FrmShowImage初期化");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 检查图片是否空白图片
        /// </summary>
        /// <param name="img">Image</param>
        /// <returns>是否空白图片</returns>
        private bool CheckTransparentImg(Image img)
        {
            bool blnIsTransparent = false;
            //加载位图
            Bitmap bitMap = new Bitmap(img);
            //图片总像素
            int intAll = img.Height * img.Width;
            int intCount = 0;
            //按像素遍历
            for (int intY = 0; intY < img.Height; intY++)
            {
                for (int intX = 0; intX < img.Width; intX++)
                {
                    Color color = bitMap.GetPixel(intX, intY);
                    //无任何颜色视为透明
                    if (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0)
                    {
                        intCount += 1;
                    }
                }
            }
            //释放资源
            bitMap.Dispose();
            double dblScale = intCount / intAll * 100;
            if (dblScale == 100)
            {
                blnIsTransparent = true;
            }
            return blnIsTransparent;
        }

        private void FrmShowImage_Load(object sender, EventArgs e)
        {
            try
            {
                var liteDBHelper = LiteDBHelper.GetInstance();
                var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
                SystemSet so_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureOpacity); // 对比窗体透明度
                this.Opacity = Convert.ToInt32(so_Model.Value) / 100.00;
                this.Text = title;
                this.Location = poit;
                this.Size = size;
                pictureBox1.Image = img;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("[异常]FrmShowImage加载");
                Console.WriteLine(ex.Message);
            }
        }

        private void FrmShowImage_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.KeyCode.ToString());//这里捕获不到方向键

            switch (e.KeyCode)
            {
                case Keys.Escape: this.Close(); break;
                case Keys.Right: this.Location = new Point(this.Location.X + 1, this.Location.Y); break;
                case Keys.Left: this.Location = new Point(this.Location.X - 1, this.Location.Y); break;
                case Keys.Up: this.Location = new Point(this.Location.X , this.Location.Y - 1); break;
                case Keys.Down: this.Location = new Point(this.Location.X , this.Location.Y + 1); break;
            }
        }
    }
}
