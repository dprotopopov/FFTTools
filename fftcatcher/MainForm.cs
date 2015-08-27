using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using Emgu.CV;
using FFTTools;

namespace fftcatcher
{
    /// <summary>
    ///     Main application window
    /// </summary>
    public partial class MainForm : RibbonForm
    {
        public MainForm()
        {
            InitializeComponent();
            InitSkinGallery();
            openFileDialog1.Filter =
                saveFileDialog1.Filter =
                    @"Bitmap Images (*.bmp)|*.bmp|All Files (*.*)|*.*";
        }

        private void InitSkinGallery()
        {
            SkinHelper.InitSkinGallery(rgbiSkins, true);
        }

        private void openFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                var bitmap = new Bitmap(openFileDialog1.FileName);
                pictureEditFile.Image = bitmap;
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }

        private void saveAsFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var bitmap = pictureEditFile.Image as Bitmap;
                if (bitmap == null) return;
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                bitmap.Save(saveFileDialog1.FileName);
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }

        private void catch_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var bitmap = pictureEditFile.Image as Bitmap;
                var pattern = pictureEditPattern.Image as Bitmap;
                if (bitmap == null || pattern == null) return;
                using (var builder = new CatchBuilder(pattern))
                {
                    Matrix<double> matrix = builder.Catch(bitmap);
                    int x, y;
                    double value;
                    builder.Max(matrix, out x, out y, out value);
                    propertyGridControl1.SelectedObject = new Info {X = x, Y = y, Value = value};
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        var pen = new Pen(Color.Red);
                        graphics.DrawRectangle(pen, x, y, pattern.Width, pattern.Height);
                    }
                }
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }

        private void iAbout_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var aboutBox = new AboutBox())
                aboutBox.ShowDialog();
        }

        private void openPattern_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                var bitmap = new Bitmap(openFileDialog1.FileName);
                pictureEditPattern.Image = bitmap;
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }

        public class Info
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double Value { get; set; }
        }
    }
}