using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using myWindowsCommunicationFoundation;
using myEmguLibrary;
using Emgu.CV.Structure;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Wpf_MyVisualizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        WindowsCommunication WCF = new WindowsCommunication();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WCF.startServer(severGetParameters);
        }
        private void Btn_startServer_Click(object sender, RoutedEventArgs e)
        {
            WCF.startServer(severGetParameters);
        }

        Mat mat_origen = new Mat();
        Mat mat_show = new Mat();
        Size show_size = new Size(640, 360);
        Rectangle show_roi = new Rectangle();
        int show_scale_per = 100;
        double showScale = 1.0;
        double roi_Left = 0;
        double roi_Top = 0;
        void severGetParameters(dynamic getMessage)
        {
            if (getMessage.GetType() == typeof(List<double>))
            {
                List<double> v = getMessage;
                return;
            }
            else if (getMessage.GetType().Name == typeof(Mat).Name)
            {
                Mat m = getMessage;
                m.CopyTo(mat_origen);
                t_mat_size.Content = $"{mat_origen.Cols}x{mat_origen.Rows}";
                t_mat_depth.Content = $"{mat_origen.Depth.ToString()}";
                if (mat_origen.Depth != Emgu.CV.CvEnum.DepthType.Cv8U)
                    MessageBox.Show("Cv8U以外只能顯示", "show unfinished");

                m.CopyTo(mat_show);
                img_main.Source = MyInvoke.MatToBitmap(mat_show);

                return;
            }
            //1920*1080
            //960 * 540
            //640 * 360
            //480 * 270

        }
        private void Grid_img_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Count() > 1)
                {
                    MessageBox.Show("one file only");
                    return;
                }
                string Droped = files[0];
                Mat mat = CvInvoke.Imread(Droped);
                mat.CopyTo(mat_show);
                mat.CopyTo(mat_origen);
                img_main.Source = MyInvoke.MatToBitmap(mat_origen);

                show_roi = new Rectangle(0, 0, mat.Cols, mat.Rows);
                show_scale_per = 100;
                showScale = 1.0;
                roi_Left = 0;
                roi_Top = 0;
            }
        }

        private void Grid_img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int scaleTick = 5;
            Point P = e.GetPosition((Grid)sender);


            if (e.Delta > 0)//放大 //wheel > 0
            {
                double Ix = P.X / show_size.Width;
                double Iy = P.Y / show_size.Height;

                double newCenterX = roi_Left + Ix * mat_origen.Cols * showScale;
                double newCenterY = roi_Top + Iy * mat_origen.Rows * showScale;

                //計算放大比例
                show_scale_per -= scaleTick;
                if (show_scale_per <= 0) show_scale_per = scaleTick;
                showScale = show_scale_per / 100.0;


                roi_Left = newCenterX - (Ix * mat_origen.Cols * showScale);
                roi_Top = newCenterY - (Iy * mat_origen.Rows * showScale);
                double newWidth = mat_origen.Cols * showScale;
                double newHeight = mat_origen.Rows * showScale;
                show_roi = new Rectangle((int)roi_Left, (int)roi_Top, (int)newWidth, (int)newHeight);


                //mat_origen.CopyTo(mat_show);
                //CvInvoke.Rectangle(mat_show, show_roi, new MCvScalar(100, 250, 100), 3);
                //CvInvoke.Circle(mat_show, new System.Drawing.Point((int)newCenterX, (int)newCenterY), 2, new MCvScalar(50, 250, 50), -1);

                Mat tmp = new Mat(mat_origen, show_roi);
                CvInvoke.Resize(tmp, mat_show, show_size);

                img_main.Source = MyInvoke.MatToBitmap(mat_show);

            }
            else if (e.Delta < 0)//縮小 //wheel < 0;
            {
                double Ix = P.X / show_size.Width;
                double Iy = P.Y / show_size.Height;

                double newCenterX = roi_Left + Ix * mat_origen.Cols * showScale;
                double newCenterY = roi_Top + Iy * mat_origen.Rows * showScale;

                show_scale_per += scaleTick;
                if (show_scale_per >= 100) show_scale_per = 100;
                showScale = show_scale_per / 100.0;

                roi_Left = newCenterX - (Ix * mat_origen.Cols * showScale);
                roi_Top = newCenterY - (Iy * mat_origen.Rows * showScale);

                double newWidth = mat_origen.Cols * showScale;
                double newHeight = mat_origen.Rows * showScale;

                if (roi_Left < 0) roi_Left = 0;
                else if (roi_Left + newWidth > mat_origen.Cols) roi_Left = mat_origen.Cols - newWidth;
                if (roi_Top < 0) roi_Top = 0;
                else if (roi_Top + newHeight > mat_origen.Rows) roi_Top = mat_origen.Rows - newHeight;

                show_roi = new Rectangle((int)roi_Left, (int)roi_Top, (int)newWidth, (int)newHeight);

                //mat_origen.CopyTo(mat_show);
                //CvInvoke.Rectangle(mat_show, show_roi, new MCvScalar(100, 250, 100), 3);
                //CvInvoke.Circle(mat_show, new System.Drawing.Point((int)newCenterX, (int)newCenterY), 2, new MCvScalar(50, 250, 50), -1);

                Mat tmp = new Mat(mat_origen, show_roi);
                CvInvoke.Resize(tmp, mat_show, show_size);

                img_main.Source = MyInvoke.MatToBitmap(mat_show);

            }
        }

        Point move_start = new Point();
        private void Grid_img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mat_show == null)
                return;

            Point P = e.GetPosition((Grid)sender);
            if (e.ChangedButton == MouseButton.Left)
            {
                double Ix = P.X / show_size.Width;
                double Iy = P.Y / show_size.Height;

                MCvScalar color = MyInvoke.GetColorM(mat_show, (int)P.Y, (int)P.X);
                tb_mat_v1.Text = color.V0.ToString();
                tb_mat_v2.Text = color.V1.ToString();
                tb_mat_v3.Text = color.V2.ToString();

                MCvScalar hsv = MyInvoke.bgr2hsv(color);
                tb_mat_h.Text = color.V0.ToString();
                tb_mat_s.Text = color.V1.ToString();
                tb_mat_v.Text = color.V2.ToString();


                tb_mat_x.Text = ((int)(show_roi.X + Ix * mat_origen.Cols * showScale)).ToString();
                tb_mat_y.Text = ((int)(show_roi.Y + Iy * mat_origen.Rows * showScale)).ToString();
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {
                move_start = P;
            }

        }
        private void Grid_img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point P = e.GetPosition((Grid)sender);
            if (e.ChangedButton == MouseButton.Middle)
            {
                Point moveVector = new Point(P.X - move_start.X, P.Y - move_start.Y);

                int rectNewX = show_roi.X - (int)(moveVector.X * showScale);
                int rectNewY = show_roi.Y - (int)(moveVector.Y * showScale);

                if (rectNewX < 0) rectNewX = 0;
                else if (rectNewX + show_roi.Width > show_size.Width) rectNewX = show_size.Width - show_roi.Width;

                if (rectNewY < 0) rectNewY = 0;
                else if (rectNewY + show_roi.Height > show_size.Height) rectNewY = show_size.Height - show_roi.Height;

                show_roi = new Rectangle(rectNewX, rectNewY, show_roi.Width, show_roi.Height);
                Mat tmp = new Mat(mat_origen, show_roi);
                CvInvoke.Resize(tmp, mat_show, show_size);
                img_main.Source = MyInvoke.MatToBitmap(mat_show);
            }
        }
    }

    public static class Ex
    {
        public static BitmapSource MatToBitmap(IInputArray mat)
        {
            WriteableBitmap bitmap = null;
            if (((Mat)mat).NumberOfChannels == 4)
                bitmap = new WriteableBitmap(((Mat)mat).Width, ((Mat)mat).Height, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgra32, null);
            else if (((Mat)mat).NumberOfChannels == 3)
                bitmap = new WriteableBitmap(((Mat)mat).Width, ((Mat)mat).Height, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            else if (((Mat)mat).NumberOfChannels == 1)
                bitmap = new WriteableBitmap(((Mat)mat).Width, ((Mat)mat).Height, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);


            bitmap.Lock();
            unsafe
            {
                var region = new Int32Rect(0, 0, ((Mat)mat).Width, ((Mat)mat).Height);
                int ch = ((Mat)mat).NumberOfChannels;
                int stride = (((Mat)mat).Width * ch);
                int bitPerPixCh = 8;
                bitmap.WritePixels(region, ((Mat)mat).DataPointer, (stride * ((Mat)mat).Height), stride);
                bitmap.AddDirtyRect(region);
            }
            bitmap.Unlock();

            return bitmap;
        }
    }


}

