﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace LatteGrab
{
    /// <summary>
    /// Interaction logic for ScreenshotWindow.xaml
    /// </summary>
    public partial class ScreenshotWindow : Window
    {
        public double x;
        public double y;
        public double width;
        public double height;
        public bool isMouseDown = false;

        public ScreenshotWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            x = e.GetPosition(null).X;
            y = e.GetPosition(null).Y;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.isMouseDown)
            {
                double curx = e.GetPosition(null).X;
                double cury = e.GetPosition(null).Y;

                System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
                SolidColorBrush brush = new SolidColorBrush(Colors.White);
                r.Stroke = brush;
                r.Fill = brush;
                r.StrokeThickness = 1;
                r.Width = Math.Abs(curx - x);
                r.Height = Math.Abs(cury - y);
                cnv.Children.Clear();
                cnv.Children.Add(r);
                Canvas.SetLeft(r, x);
                Canvas.SetTop(r, y);
                if (e.LeftButton == MouseButtonState.Released)
                {
                    cnv.Children.Clear();
                    width = e.GetPosition(null).X - x;
                    height = e.GetPosition(null).Y - y;
                    this.CaptureScreen(x, y, width, height);
                    this.x = this.y = 0;
                    this.isMouseDown = false;
                    this.Close();
                }
            }
        }

        public void CaptureScreen(double x, double y, double width, double height)
        {
            int ix, iy, iw, ih;
            ix = Convert.ToInt32(x);
            iy = Convert.ToInt32(y);
            iw = Convert.ToInt32(width);
            ih = Convert.ToInt32(height);
            Bitmap image = new Bitmap(iw, ih, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(image);
            g.CopyFromScreen(ix, iy, 0, 0, new System.Drawing.Size(iw, ih), CopyPixelOperation.SourceCopy);

            var fn = Path.GetTempFileName();

            image.Save(fn, ImageFormat.Png);

            var url = LatteShareConnection.Instance.UploadFile(fn);

            System.Windows.Forms.Clipboard.SetText(url);

            System.Diagnostics.Debug.WriteLine(url);
        }

        public void SaveScreen(double x, double y, double width, double height)
        {
            int ix, iy, iw, ih;
            ix = Convert.ToInt32(x);
            iy = Convert.ToInt32(y);
            iw = Convert.ToInt32(width);
            ih = Convert.ToInt32(height);
            try
            {
                Bitmap myImage = new Bitmap(iw, ih);

                Graphics gr1 = Graphics.FromImage(myImage);
                IntPtr dc1 = gr1.GetHdc();
                IntPtr dc2 = NativeMethods.GetWindowDC(NativeMethods.GetForegroundWindow());
                NativeMethods.BitBlt(dc1, ix, iy, iw, ih, dc2, ix, iy, 13369376);
                gr1.ReleaseHdc(dc1);

                var fn = Path.GetTempFileName();

                myImage.Save(fn, ImageFormat.Png);

                var url = LatteShareConnection.Instance.UploadFile(fn);

                System.Windows.Forms.Clipboard.SetText(url);

                System.Diagnostics.Debug.WriteLine(url);
            }
            catch { }
        }

        internal class NativeMethods
        {

            [DllImport("user32.dll")]
            public extern static IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hwnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetForegroundWindow();
            [DllImport("gdi32.dll")]
            public static extern UInt64 BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, System.Int32 dwRop);

        }
    }
}
