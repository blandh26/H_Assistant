﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.Reflection;
using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.Const;

namespace H_ScreenCapture
{
    public partial class FrmCapture : Form
    {
        int controlType = 0;//0 默认截图 1 上一次自动区域截图 2 对比模式

        public FrmCapture(int type)
        {
            controlType = type;
            InitializeComponent();
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            m_rectScreen = Win32.GetDesktopRect();
            //this.Location = new Point(1920, m_rectScreen.Location.Y);
            this.Location = new Point(m_rectScreen.Location.X, m_rectScreen.Location.Y - m_rectScreen.Location.Y);
            this.Size = new Size(m_rectScreen.Size.Width, m_rectScreen.Size.Height);

            imageCroppingBox1.IsDrawMagnifier = true;
            Image imgScreen = this.GetFullScreen(true);
            this.imageCroppingBox1.Image = imgScreen;
            this.imageCroppingBox1.Dock = DockStyle.Fill;
            if (type == 2)
            {
                m_bGetVisable = false;
            }
        }
        private Point m_ptLastMouseDown;        //鼠标上一次点击位置
        private Point m_ptCurrent;              //鼠标当前位置
        private Rectangle m_rect;               //自动框选时候的区域
        private bool m_bDrawInfo;               //当前是否需要绘制被框选窗体的文字信息
        private bool m_bGetVisable = true;      //是否只框选可见窗体
        private bool m_bGetTransparent;         //是否框选透明窗体
        private Rectangle m_rectScreen;         //屏幕的矩形区域
        private string m_strShow;               //自动框选时 所需要绘制的文字信息
        private IntPtr m_hWnd;                  //自动框选是 被框选窗体的句柄
        private Image m_imgMosaic;              //马赛克图像
        private SolidBrush m_sbFill;            //FillXXX 所需要的画刷
        private TextureBrush m_tbMosaic;        //马赛克画刷
        private Image m_imgCurrentLayer;        //后期绘制时候 mousemove 中临时绘制的图像
        private Image m_imgLastLayer;           //上一次的历史记录
        private bool m_bDrawEffect;             //mousedown 中表示为是否进行后期绘制
        private List<Image> m_layer = new List<Image>();    //历史记录
        LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();

        private void FrmCaption_Load(object sender, EventArgs e)
        {
            m_sbFill = new SolidBrush(Color.Red);
            panel1.Parent = imageCroppingBox1;
            panel1.BackColor = Color.Gainsboro;
            panel1.Cursor = Cursors.Default;
            panel1.Visible = false;
            this.captureToolbar1.Parent = imageCroppingBox1;
            this.captureToolbar1.Visible = false;
            this.captureToolbar1.Cursor = Cursors.Default;
            colorBox1.ColorChanged += (s, ex) =>
            {
                textBox1.ForeColor = colorBox1.Color;
                m_sbFill.Color = colorBox1.Color;
            };
            textBox1.Visible = false;
            textBox1.TextChanged += (s, ex) => this.SetTextBoxSize();
            textBox1.Validating += textBox1_Validating;
            #region 上一次截图区域自动选区
            if (controlType == 1)
            {
                var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
                SystemSet st_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureLast);// 最后一次截图区域
                if (st_Model.Value != "")
                {
                    string[] strArr = st_Model.Value.Split(',');
                    imageCroppingBox1.IsLockSelected = true;
                    imageCroppingBox1.IsSelected = true;
                    imageCroppingBox1.SelectedRectangle = new Rectangle(
                        Convert.ToInt32(strArr[0]),
                        Convert.ToInt32(strArr[1]),
                        Convert.ToInt32(strArr[2]),
                        Convert.ToInt32(strArr[3]));
                    this.SetToolBarLocation();                              //如果有选取 那么应该设置工具条的位置并且显示 以便后期绘制
                    this.captureToolbar1.Visible = true;
                    if (m_imgLastLayer != null) m_imgLastLayer.Dispose();
                    m_imgLastLayer = imageCroppingBox1.GetSelectedImage();                          //默认图层为选取区域的图像
                    if (m_imgCurrentLayer != null) m_imgCurrentLayer.Dispose();
                    m_imgCurrentLayer = new Bitmap(m_imgLastLayer.Width, m_imgLastLayer.Height);    //绘制面板图层置空
                    if (m_imgMosaic != null) m_imgMosaic.Dispose();
                    m_imgMosaic = ImageHelper.Mosaic(m_imgLastLayer, 10);                           //设置当前区域的马赛克图像
                    if (m_tbMosaic != null) m_tbMosaic.Dispose();
                    m_tbMosaic = new TextureBrush(m_imgMosaic);
                }
                OnToolBarClick("btn_ok");// 最后一次截图的时候是自动完成按钮
            }
            #endregion
        }
        //绘图时候选择文字工具 文本框离开焦点的时候将文本绘制上去
        private void textBox1_Validating(object sender, CancelEventArgs e)
        {
            if (textBox1.Text.Trim() == string.Empty) return;
            using (Graphics g = Graphics.FromImage(m_imgCurrentLayer))
            {
                Brush brush = m_sbFill;
                g.DrawImage(m_imgLastLayer, 0, 0);
                g.DrawString(
                    textBox1.Text,
                    textBox1.Font,
                    brush,
                    textBox1.Left - imageCroppingBox1.SelectedRectangle.Left,
                    textBox1.Top - imageCroppingBox1.SelectedRectangle.Top
                    );
            }
            this.SetHistoryLayer();
            imageCroppingBox1.Invalidate(imageCroppingBox1.SelectedRectangle);
            textBox1.Visible = false;
            textBox1.Text = string.Empty;
        }

        /// <summary>
        /// 获取桌面图片
        /// </summary>
        /// <param name="bCaptureCursor">是否捕获鼠标</param>
        /// <returns>获取到的图像</returns>
        private Image GetFullScreen(bool bCaptureCursor)
        {
            try
            {
                if (bCaptureCursor) Win32.DrawCurToScreen(MousePosition);
                Bitmap bmp = new Bitmap(m_rectScreen.Width, m_rectScreen.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(m_rectScreen.X, m_rectScreen.Y, 0, 0, bmp.Size);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[异常]获取桌面图片");
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private void imageCroppingBox1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                m_ptLastMouseDown = e.Location;
                if (e.Button != MouseButtons.Left)
                { //禁止右键的时候触发
                    return;
                }
                if (!imageCroppingBox1.IsLockSelected)
                {
                    this.captureToolbar1.Visible = false;
                }
                //如果已经锁定了选取 并且 鼠标点下的位置在选取内 而且工具条上有选择工具 那么则表示可能需要绘制了 如：矩形框 箭头 等
                if (imageCroppingBox1.IsLockSelected && imageCroppingBox1.SelectedRectangle.Contains(e.Location) && captureToolbar1.GetSelectBtnName() != null)
                {
                    if (captureToolbar1.GetSelectBtnName() == "btn_text")
                    { //如果选择的是文字工具 那么特殊处理
                        textBox1.Location = e.Location;                     //将文本框位置设置为鼠标点下的位子
                        textBox1.Visible = true;                            //显示文本框以便输入文字
                        if (txtfont == null)
                        {
                            textBox1.Font = new Font(textBox1.Font.Name, 14 + getSize());
                        }
                        else
                        {
                            textBox1.Font = txtfont;
                        }
                        textBox1.Focus();                                   //获得焦点
                        return;                                             //特殊处理的角色 直接返回
                    }
                    m_bDrawEffect = true;                                   //设置标识 在MouseMove用于判断当前的操作是否是后期的绘制
                    Cursor.Clip = imageCroppingBox1.SelectedRectangle;      //设置鼠标的活动区域
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[异常]鼠标按下事件");
                Console.WriteLine(ex.Message);
            }
        }

        private void imageCroppingBox1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                m_bDrawEffect = false;                                      //无论如何 鼠标抬起的时候 后期绘制的标识都应该标记为false
                Cursor.Clip = Rectangle.Empty;                              //无论如何 鼠标抬起的时候 应该取消鼠标的活动限制
                if (e.Button == MouseButtons.Right)
                {
                    if (imageCroppingBox1.IsSelected)
                    {//如果是右键抬起并且有选择区域的情况 则有可能是右键菜单 或者 取消选择
                        imageCroppingBox1.Clear();                          //取消选择
                        m_layer.Clear();                                    //同时情况历史图层
                    }
                    else
                    {//如果没有选择区域且右键抬起 则直接关闭窗体
                        this.Close();
                        return;
                    }
                    this.captureToolbar1.Visible = false;                   //如果代码走到这里 则表示取消了选择 那么工具条应该被隐藏
                    captureToolbar1.ClearSelect();                          //清空工具条上的工具选择
                    panel1.Visible = false;                                 //配置面板也应该被关闭
                    m_imgLastLayer = m_imgCurrentLayer = null;              //临时图层也应该被清空
                }
                //如果是左键抬起 并且 抬起时候的位置和鼠标点下的位置一样 并且 还没有选择区域的情况下 则赋值为自动框选出来的区域(自动框选时)
                if (e.Button == MouseButtons.Left && (m_ptLastMouseDown == e.Location) && !imageCroppingBox1.IsSelected)
                {
                    imageCroppingBox1.SelectedRectangle = m_rect;
                }
                if (imageCroppingBox1.IsLockSelected)
                {                     //如果是鼠标抬起 并且 已经锁定了区域的情况下 则有可能在进行后期绘制
                                      //如果工具条上有选择工具 那么就可以确定是在进行后期的绘制   当然如果选择的是文字工具 那么忽略 因为特殊处理的 其他则设置历史图层
                    if (captureToolbar1.GetSelectBtnName() != null && captureToolbar1.GetSelectBtnName() != "btn_text") this.SetHistoryLayer();
                }
                else if (imageCroppingBox1.IsSelected)
                {                  //如果没有进入上面的判断 则判断当前有没有选取 可能鼠标抬起是确认选取
                    this.SetToolBarLocation();                              //如果有选取 那么应该设置工具条的位置并且显示 以便后期绘制
                    this.captureToolbar1.Visible = true;
                    if (m_imgLastLayer != null) m_imgLastLayer.Dispose();
                    m_imgLastLayer = imageCroppingBox1.GetSelectedImage();                          //默认图层为选取区域的图像
                    if (m_imgCurrentLayer != null) m_imgCurrentLayer.Dispose();
                    m_imgCurrentLayer = new Bitmap(m_imgLastLayer.Width, m_imgLastLayer.Height);    //绘制面板图层置空
                    if (m_imgMosaic != null) m_imgMosaic.Dispose();
                    m_imgMosaic = ImageHelper.Mosaic(m_imgLastLayer, 10);                           //设置当前区域的马赛克图像
                    if (m_tbMosaic != null) m_tbMosaic.Dispose();
                    m_tbMosaic = new TextureBrush(m_imgMosaic);                                     //设置马赛克画刷
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[异常]鼠标抬起事件");
                Console.WriteLine(ex.Message);
            }
        }

        private void imageCroppingBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (m_ptCurrent == e.Location) return;
                m_ptCurrent = e.Location;
                if (imageCroppingBox1.IsLockSelected)
                {                     //如果已经锁定了选取 则有可能是正在进行后期绘制
                    imageCroppingBox1.Cursor = imageCroppingBox1.SelectedRectangle.Contains(e.Location) ? Cursors.Cross : Cursors.Default;
                    if (m_bDrawEffect) this.DrawEffects();                  //如果说在mousedown中设置了绘制标识 那么则表示是在进行后期绘制
                    else if (imageCroppingBox1.SelectedRectangle.Contains(e.Location))              //如果没有设置绘制标识 判断鼠标是否在选取内
                        imageCroppingBox1.Invalidate(imageCroppingBox1.SelectedRectangle);          //重绘选取 以便在paint绘制画笔大小预览(圆和矩形的那个)
                    return;
                }//如果已经锁定选取 那么直接return掉
                 //在mousemove中还有可能是正在根据鼠标位置自动框选窗体 以下代码为获取窗体信息
                string strText = string.Empty;
                string strClassName = string.Empty;
                m_bDrawInfo = !(imageCroppingBox1.IsSelected || e.Button != MouseButtons.None);
                if (!m_bDrawInfo) return;                                   //如果已经有选取 或者移动过程中有鼠标被点下 都不需要获取窗体信息了
                IntPtr hWnd = Win32.GetWindowFromPoint(e.Location.X, e.Location.Y, this.Handle, m_bGetVisable, m_bGetTransparent);
                m_hWnd = hWnd;                                                      //更具鼠标位置获取窗体句柄
                strClassName = Win32.GetClassName(hWnd);
                strText = Win32.GetWindowText(hWnd);
                Win32.RECT rect = new Win32.RECT();
                Win32.GetWindowRect(hWnd, ref rect);                                //获取窗体大小
                m_rect = rect.ToRectangle();
                m_strShow = string.Format("TEXT:{0}\r\nHWND:0x{1} [{2}]", strText, hWnd.ToString("X").PadLeft(8, '0'), strClassName);
                if (m_rectScreen.X < 0) m_rect.X -= m_rectScreen.X;     //判断一下屏幕的坐标是否是小于0的 如果是 则在绘制区域的时候需要加上这个差值
                if (m_rectScreen.Y < 0) m_rect.Y -= m_rectScreen.Y;     //多显示器屏幕坐标可能是负数{如(-1920,0)两个显示器 主显示器在右边} 在绘制的时候窗体内部坐标是0开始的 需要转换
                m_rect.Intersect(imageCroppingBox1.DisplayRectangle);   //确定出来的区域和截图窗体的区域做一个交集 (不能让区域超出屏幕啊 比如屏幕边缘有个窗体 那么框选出来的区域是超出屏幕的)
                imageCroppingBox1.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[异常]imageCroppingBox1_MouseMove");
                Console.WriteLine(ex.Message);
            }
        }

        private void imageCroppingBox1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                if (m_layer.Count > 0)                                  //绘制最后一张图层
                    g.DrawImage(m_layer[m_layer.Count - 1], imageCroppingBox1.SelectedRectangle);
                if (m_imgCurrentLayer != null)                          //当前正在mousemove中进行绘制的临时图层
                    g.DrawImage(m_imgCurrentLayer, imageCroppingBox1.SelectedRectangle.Location);
                if (imageCroppingBox1.Cursor == Cursors.Cross)
                {        //如果鼠标是十字架 说明正在后期绘制或者准备后期绘制
                    using (Pen p = new Pen(colorBox1.Color))
                    {
                        Rectangle rect = new Rectangle(m_ptCurrent.X - getSize(), m_ptCurrent.Y - getSize(), getSize(), getSize());
                        using (SolidBrush sb = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                        {
                            g.DrawRectangle(p, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                            g.DrawEllipse(p, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                        }//绘制画笔大小预览(圆和矩形的那个)
                         //以下根据工具条上选择的工具 确定是否需要绘制辅助线条 主要用于马赛克时候让用户知道绘制的区域 非马赛克也无所谓 反正都会被挡住
                        if (m_bDrawEffect)
                        {                            //如果mousedown中标识了 后期绘制
                            rect = new Rectangle(                       //鼠标点下到当前位置的矩形区域
                                m_ptCurrent.X < m_ptLastMouseDown.X ? m_ptCurrent.X : m_ptLastMouseDown.X,
                                m_ptCurrent.Y < m_ptLastMouseDown.Y ? m_ptCurrent.Y : m_ptLastMouseDown.Y,
                                Math.Abs(m_ptLastMouseDown.X - m_ptCurrent.X),
                                Math.Abs(m_ptLastMouseDown.Y - m_ptCurrent.Y));
                            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                            p.DashPattern = new float[] { 5, 5 };
                            switch (captureToolbar1.GetSelectBtnName())
                            {
                                case "btn_rect_fill":
                                case "btn_rect":
                                case "btn_mosaic":
                                    g.DrawRectangle(p, rect);
                                    break;
                                case "btn_elips_fill":
                                case "btn_elips":
                                    g.DrawRectangle(p, rect);           //被绘制的圆形的外切矩形
                                    g.DrawEllipse(p, rect);
                                    break;
                                case "btn_line":
                                case "btn_arrow":
                                    g.DrawLine(p, m_ptLastMouseDown, m_ptCurrent);
                                    break;
                            }
                        }
                    }
                }
                if (!m_bDrawInfo) return;                               //如果在mousemove中被标识为true 则表示还出于自动框选窗体 那么进行文字信息绘制
                imageCroppingBox1.PreViewRectangle = m_rect;
                Size sz = g.MeasureString(m_strShow, this.Font).ToSize();
                Point pt = new Point(m_rect.X + sz.Width > this.Width ? this.Width - sz.Width : m_rect.X, m_rect.Y - sz.Height - 5);
                if (pt.Y < 0) pt.Y = 5;
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(125, 0, 0, 0)))
                {
                    g.FillRectangle(sb, new Rectangle(pt, sz));
                }
                g.DrawString(m_strShow, this.Font, Brushes.White, pt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[异常]imageCroppingBox1_Paint");
                Console.WriteLine(ex.Message);
            }
        }
        //一些快捷键
        private void imageCroppingBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:                                    //微距离移动鼠标
                    Win32.SetCursorPos(m_ptCurrent.X, m_ptCurrent.Y - 1);//可以不用判断 Y < 0
                    break;
                case Keys.A:
                    Win32.SetCursorPos(m_ptCurrent.X - 1, m_ptCurrent.Y);
                    break;
                case Keys.S:
                    Win32.SetCursorPos(m_ptCurrent.X, m_ptCurrent.Y + 1);
                    break;
                case Keys.D:
                    Win32.SetCursorPos(m_ptCurrent.X + 1, m_ptCurrent.Y);
                    break;
                case Keys.V:
                    m_bGetVisable = !m_bGetVisable;             //自动框选时候是只否获取可见窗体
                    new FrmTextAlert("是否只获取可见窗体:" + m_bGetVisable).Show(this);
                    break;
                case Keys.T:
                    m_bGetTransparent = !m_bGetTransparent;     //自动框选时候是否获取透明窗体
                    new FrmTextAlert("是否获取透明窗体:" + m_bGetTransparent).Show(this);
                    break;
            }
            Console.WriteLine(e.KeyCode);
        }

        /// <summary>
        /// 工具栏点击按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void captureToolbar1_ToolButtonClick(object sender, EventArgs e)
        {
            this.OnToolBarClick((sender as Control).Name);
        }


        /// <summary>
        /// 双击完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageCroppingBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (imageCroppingBox1.SelectedRectangle.Size == Size.Empty) return;
                Clipboard.SetImage(m_imgLastLayer);
                string str = imageCroppingBox1.SelectedRectangle.X + "," + imageCroppingBox1.SelectedRectangle.Y + "," + imageCroppingBox1.SelectedRectangle.Size.Width + "," + imageCroppingBox1.SelectedRectangle.Size.Height;
                var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
                SystemSet st_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureLast);// 最后一次截图区域
                st_Model.Value = str;
                liteDBHelper.Update(st_Model);
                #region 对比截图
                if (controlType == 2)
                {
                    return;// 因为有bug 直接截图模式 不允许双击
                    imageCroppingBox1.IsSelected = true;
                    try
                    {
                        closeFrom();
                        FrmShowImage frmimage = new FrmShowImage(
                            new Point(imageCroppingBox1.SelectedRectangle.X - 8,
                            imageCroppingBox1.SelectedRectangle.Y - 31),
                            new Size(imageCroppingBox1.SelectedRectangle.Size.Width + 16,
                            imageCroppingBox1.SelectedRectangle.Size.Height + 39),
                            m_imgLastLayer);
                        frmimage.Show();
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine("[异常]双击完成对比截图");
                        Console.WriteLine(ex1.Message);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("[异常]imageCroppingBox1_MouseDoubleClick");
                Console.WriteLine(ex.Message);
            }
            this.Close();
        }

        /// <summary>
        /// 工具栏被点下的时候
        /// </summary>
        /// <param name="strCtrlName">被点下的工具的控件名字</param>
        private void OnToolBarClick(string strCtrlName)
        {
            try
            {
                switch (strCtrlName)
                {
                    case "btn_close": this.Close(); break;
                    case "btn_ok":
                        Clipboard.SetImage(m_imgLastLayer);
                        string str = imageCroppingBox1.SelectedRectangle.X + "," + imageCroppingBox1.SelectedRectangle.Y + "," + imageCroppingBox1.SelectedRectangle.Size.Width + "," + imageCroppingBox1.SelectedRectangle.Size.Height;
                        var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
                        SystemSet st_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_ScreenCaptureLast);// 最后一次截图区域
                        st_Model.Value = str;
                        liteDBHelper.Update(st_Model);
                        #region 对比截图
                        if (controlType == 2)
                        {
                            try
                            {
                                closeFrom();//开对比页面之前关闭 之前页面
                                FrmShowImage frmimage = new FrmShowImage(
                                    new Point(imageCroppingBox1.SelectedRectangle.X - 8,
                                    imageCroppingBox1.SelectedRectangle.Y - 31),
                                    new Size(imageCroppingBox1.SelectedRectangle.Size.Width + 16,
                                    imageCroppingBox1.SelectedRectangle.Size.Height + 39),
                                    m_imgLastLayer);
                                frmimage.Show();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("[异常]工具箱完成对比截图");
                                Console.WriteLine(ex.Message);
                            }
                        }
                        #endregion
                        this.Close();
                        break;
                    case "btn_out":
                        new FrmOut(m_imgLastLayer).Show();          //弹出选取
                        this.Close();
                        break;
                    case "btn_cancel":
                        if (textBox1.Visible)
                        {                     //如果撤销的时候正则使用文字工具 则清理掉
                            textBox1.Text = string.Empty;
                            textBox1.Visible = false;
                            break;
                        }
                        if (m_layer.Count > 0)
                        {                    //如果存在历史图层
                            m_layer.RemoveAt(m_layer.Count - 1);    //则干掉最后一层
                            if (m_imgCurrentLayer != null) m_imgCurrentLayer.Dispose();
                            m_imgCurrentLayer = new Bitmap(m_imgLastLayer.Width, m_imgLastLayer.Height);        //当前正在绘制的图层清理掉
                            if (m_imgLastLayer != null) m_imgLastLayer.Dispose();
                            if (m_layer.Count > 0)
                            {                //如果干点最后一层还存在图层 则设置
                                m_imgLastLayer = m_layer[m_layer.Count - 1].Clone() as Bitmap;
                            }
                            else
                            {                                //否则回到刚选择好选取的时候
                                m_imgLastLayer = imageCroppingBox1.GetSelectedImage();
                                imageCroppingBox1.IsLockSelected = captureToolbar1.GetSelectBtnName() != null;
                            }
                            imageCroppingBox1.Invalidate(imageCroppingBox1.SelectedRectangle);
                        }
                        else
                        {                                    //如果已经不存在历史记录了 则直接撤销选取 重新选择区域
                            imageCroppingBox1.Clear();
                            captureToolbar1.ClearSelect();
                            captureToolbar1.Visible = false;
                            panel1.Visible = false;
                        }
                        break;
                    case "btn_save":
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "*.png|*.png|*.jpg|*.jpg|*.bmp|*.bmp|*.gif|*.gif|*.tiff|*.tiff";
                        sfd.FileName = "HCap_"/*Developer Capture*/ + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                        System.Drawing.Imaging.ImageFormat imgf = System.Drawing.Imaging.ImageFormat.Png;
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            switch (sfd.FilterIndex)
                            {
                                case 2: imgf = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                                case 3: imgf = System.Drawing.Imaging.ImageFormat.Bmp; break;
                                case 4: imgf = System.Drawing.Imaging.ImageFormat.Gif; break;
                                case 5: imgf = System.Drawing.Imaging.ImageFormat.Tiff; break;
                            }
                            using (System.IO.Stream stream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                            {
                                m_imgLastLayer.Save(stream, imgf);
                                byte[] byString = Encoding.UTF8.GetBytes("\0\0\r\nBy->blandh26@gmail.com");
                                stream.Write(byString, 0, byString.Length); ;
                            }
                            this.Close();
                        }
                        break;
                    case "btn_rect":
                    case "btn_rect_fill":
                    case "btn_elips":
                    case "btn_elips_fill":
                    case "btn_arrow":
                    case "btn_line":
                    case "btn_brush":
                    case "btn_mosaic":
                    case "btn_text":
                        if (captureToolbar1.GetSelectBtnName() == null)
                        {                       //如果没有工具被选择 那么配置面板信息隐藏
                            if (m_layer.Count == 0) imageCroppingBox1.IsLockSelected = false;   //如果没有历史图层 则取消选取的锁定
                            panel1.Visible = false;
                        }
                        else if (captureToolbar1.GetSelectBtnName() == "btn_mosaic")
                        {
                            imageCroppingBox1.IsLockSelected = true;                            //否则锁定选取 并且显示配置面板
                            this.SetToolBarLocation();
                        }
                        else if (captureToolbar1.GetSelectBtnName() == "btn_text")
                        {
                            pictureBox1.Visible = true;
                            colorBox1.Location = new Point(182, 3);
                            panel1.Size = new Size(341, 39);
                            txtfont = null;
                            textBox1.Font = new Font(textBox1.Font.Name, 14 + getSize());
                            imageCroppingBox1.IsLockSelected = true;                            //否则锁定选取 并且显示配置面板
                            panel1.Visible = true;
                            this.SetToolBarLocation();
                        }
                        else
                        {
                            pictureBox1.Visible = false;
                            colorBox1.Location = new Point(154, 3);
                            panel1.Size = new Size(314, 39);
                            imageCroppingBox1.IsLockSelected = true;                            //否则锁定选取 并且显示配置面板
                            panel1.Visible = true;
                            this.SetToolBarLocation();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[异常]imageCroppingBox1_MouseDoubleClick");
                Console.WriteLine(ex.Message);
            }
        }
        //设置工具条位置
        private void SetToolBarLocation()
        {
            this.captureToolbar1.Location = new Point(imageCroppingBox1.SelectedRectangle.Left, imageCroppingBox1.SelectedRectangle.Bottom + 5);
            int nBottom = captureToolbar1.Bottom + (panel1.Visible ? panel1.Height + 5 : 0);
            if (this.captureToolbar1.Right > this.Width)
            {
                this.captureToolbar1.Left = this.Width - this.captureToolbar1.Width;
            }
            if (nBottom > this.Height)
            {
                this.captureToolbar1.Top = this.imageCroppingBox1.SelectedRectangle.Top - 5 - this.captureToolbar1.Height - (panel1.Visible ? panel1.Height + 5 : 5);
            }
            if (this.captureToolbar1.Top < 0)
            {
                this.captureToolbar1.Top = this.imageCroppingBox1.SelectedRectangle.Top + 5;
            }
            if (panel1.Visible)
                panel1.Left = captureToolbar1.Left;
            panel1.Top = captureToolbar1.Bottom + 5;
        }
        //后期绘制
        private void DrawEffects()
        {
            //绘制的起点坐标(相对于选取内的坐标)
            Point ptStart = new Point(m_ptLastMouseDown.X < m_ptCurrent.X ? m_ptLastMouseDown.X : m_ptCurrent.X, m_ptLastMouseDown.Y < m_ptCurrent.Y ? m_ptLastMouseDown.Y : m_ptCurrent.Y);
            ptStart = (Point)((Size)ptStart - (Size)imageCroppingBox1.SelectedRectangle.Location);
            Size sz = new Size(Math.Abs(m_ptCurrent.X - m_ptLastMouseDown.X), Math.Abs(m_ptCurrent.Y - m_ptLastMouseDown.Y));
            Brush brush = m_sbFill;                                 //默认为填充画笔 否则使用马赛克画刷
            using (Pen p = new Pen(brush, getSize()))     //根据画刷设置画笔
            using (Graphics g = Graphics.FromImage(m_imgCurrentLayer))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                switch (captureToolbar1.GetSelectBtnName())
                {
                    case "btn_rect":
                        g.Clear(Color.Transparent);                 //清空上一次绘制
                        g.DrawRectangle(p, new Rectangle(ptStart, sz));
                        break;
                    case "btn_rect_fill":
                        g.Clear(Color.Transparent);                 //清空上一次绘制
                        g.FillRectangle(brush, new Rectangle(ptStart, sz));
                        break;
                    case "btn_elips":
                        g.Clear(Color.Transparent);
                        g.DrawEllipse(p, new Rectangle(ptStart, sz));
                        break;
                    case "btn_elips_fill":
                        g.Clear(Color.Transparent);
                        g.FillEllipse(brush, new Rectangle(ptStart, sz));
                        break;
                    case "btn_arrow":
                        g.Clear(Color.Transparent);
                        p.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5, true);
                        g.DrawLine(p, (Point)((Size)m_ptLastMouseDown - (Size)imageCroppingBox1.SelectedRectangle.Location), (Point)((Size)m_ptCurrent - (Size)imageCroppingBox1.SelectedRectangle.Location));
                        break;
                    case "btn_line":
                        g.Clear(Color.Transparent);
                        g.DrawLine(p, (Point)((Size)m_ptLastMouseDown - (Size)imageCroppingBox1.SelectedRectangle.Location), (Point)((Size)m_ptCurrent - (Size)imageCroppingBox1.SelectedRectangle.Location));
                        break;
                    case "btn_brush":                               //如果是画线 则就不需要g.Clear(Color.Transparent)来清空上一次才绘制了
                        p.StartCap = p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        g.DrawLine(p, (Point)((Size)m_ptLastMouseDown - (Size)imageCroppingBox1.SelectedRectangle.Location), (Point)((Size)m_ptCurrent - (Size)imageCroppingBox1.SelectedRectangle.Location));
                        m_ptLastMouseDown = m_ptCurrent;            //重置上一次点击位置 以便下一次进入此代码使用
                        break;
                    case "btn_mosaic":
                        brush = m_tbMosaic;
                        g.Clear(Color.Transparent);                 //清空上一次绘制
                        g.FillRectangle(brush, new Rectangle(ptStart, sz));
                        break;
                }
                imageCroppingBox1.Invalidate(imageCroppingBox1.SelectedRectangle);
            }
        }
        //设置历史图层
        private void SetHistoryLayer()
        {
            using (Graphics g = Graphics.FromImage(m_imgLastLayer))
            {
                g.DrawImage(m_imgCurrentLayer, 0, 0);               //先将临时绘制的图层绘制到LastLayer中
            }
            m_layer.Add(m_imgLastLayer);                            //然后保存本次图层
            m_imgLastLayer = m_imgLastLayer.Clone() as Bitmap;      //克隆一份作为下一次的背景图层
            m_imgCurrentLayer = m_imgLastLayer.Clone() as Bitmap;   //临时绘制图层也得清理掉
            if (m_imgMosaic != null) m_imgMosaic.Dispose();
            m_imgMosaic = ImageHelper.Mosaic(m_imgLastLayer, 10);   //
            if (m_tbMosaic != null) m_tbMosaic.Dispose();
            m_tbMosaic = new TextureBrush(m_imgMosaic);             //设置马赛克画刷
        }

        private void SetTextBoxSize()
        {
            Size se = TextRenderer.MeasureText(textBox1.Text, textBox1.Font);
            textBox1.Size = se.IsEmpty ? new Size(50, textBox1.Font.Height) : se;
        }

        /// <summary>
        /// 获取选中大小
        /// </summary>
        /// <returns></returns>
        private int getSize()
        {
            if (toolButton1.IsSelected) return 1;
            else if (toolButton2.IsSelected) return 3;
            else if (toolButton3.IsSelected) return 5;
            else if (toolButton4.IsSelected) return 8;
            else if (toolButton5.IsSelected) return 12;
            return 1;
        }

        Font txtfont = null;

        /// <summary>
        /// 设置字体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = textBox1.Font;
            if (fd.ShowDialog() != DialogResult.OK) return;
            textBox1.Font = fd.Font;
            txtfont = fd.Font;
            this.SetTextBoxSize();
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        public void closeFrom()
        {
            while (true)
            {
                if (Application.OpenForms["FrmShowImage"] == null)
                {
                    break;
                }
                Application.OpenForms["FrmShowImage"].Close();
            }
        }

        private void FrmCapture_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape: this.Close(); break;
            }
        }
    }
}
