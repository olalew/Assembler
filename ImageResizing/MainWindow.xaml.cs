using ImageResizing.Infrastructure;
using ImageResizing.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace ImageResizing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap workingImage;
        private Bitmap resultBitmap;

        [DllImport(@"ImageResizingASM.dll")]
        private static extern int MyProc();

        public MainWindow()
        {
            InitializeComponent();

            int resutl = MyProc();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            ImageSizeModifierService sizeModifierService = new ImageSizeModifierService(workingImage, workingImage.Width * 6 / 7, workingImage.Height * 6 / 7);

            resultBitmap = sizeModifierService.ExecuteAlgorithm();
            BitmapSource resultImage = BitmapParser.BitmapToBitmapSource(resultBitmap);

            OutputImage.ImageSource = resultImage;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            resultBitmap.Save(@"C:\Users\olale\Downloads\result.png");
        }

        #region FILE OPENER

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            GetFileWithDialog();
        }

        private void GetFileWithDialog()
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = @"C:\Users\olale\Downloads",
                Filter = "Image Files (*.bmp;*jpg;*.png)|*.png;*.bmp;*jpg;",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmapImage = new(new Uri(openFileDialog.FileName));
                workingImage = BitmapParser.BitmapImageToBitmap(bitmapImage);
                InputImage.ImageSource = bitmapImage;
            }
        }

        #endregion
    }
}
