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
using Emgu.CV.CvEnum;
using myWindowsCommunicationFoundation;

namespace Wpf_testVisualizer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        WindowsCommunication WCF = new WindowsCommunication();
        private void Btn_connect_Click(object sender, RoutedEventArgs e)
        {
            WCF.ClientConnect((str) => { Console.WriteLine("client:" + str); });
        }

        private void Btn_Mat_Click(object sender, RoutedEventArgs e)
        {
            Mat mat = new Mat(100, 300, DepthType.Cv8U, 1);
            //WCF.sendToServer(mat, typeof(Mat));
            //WCF.sendMatToServer(mat);


            List<double> i = new List<double>();
            for (int n = 0; n < 3; n++)
                i.Add(n/100.0);
            WCF.sendStreamToServer(i);
        }


        private void Btn_Mat_Drop(object sender, DragEventArgs e)
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
                WCF.sendMatToServer(mat);
            }
        }
    }
}
