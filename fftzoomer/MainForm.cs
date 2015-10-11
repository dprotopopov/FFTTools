using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using Emgu.CV;
using Emgu.CV.Structure;
using FFTTools;

namespace fftzoomer
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

        private void pictureEdit1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) return;
                using (var image = new Image<Bgr, byte>(bitmap))
                {
                    var length = image.Data.Length;
                    var bytes = new byte[length];

                    var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                    Marshal.Copy(handle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                    handle.Free();

                    var average = bytes.Average(x => (double) x);
                    var delta = Math.Sqrt(bytes.Average(x => (double) x*x) - average*average);
                    var minValue = bytes.Min(x => (double) x);
                    var maxValue = bytes.Max(x => (double) x);
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format("Length {0}", length));
                    sb.AppendLine(string.Format("Average {0}", average));
                    sb.AppendLine(string.Format("Delta {0}", delta));
                    sb.AppendLine(string.Format("minValue {0}", minValue));
                    sb.AppendLine(string.Format("maxValue {0}", maxValue));
                    siInfo.Caption = sb.ToString();
                }
            }
            catch (Exception)
            {
            }
        }

        private void openFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                var bitmap = new Bitmap(openFileDialog1.FileName);
                pictureEdit1.Image = bitmap;
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
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) return;
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                bitmap.Save(saveFileDialog1.FileName);
            }
            catch (Exception exception)
            {
                XtraMessageBox.Show(exception.Message);
            }
        }

        private void stretch_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) return;
                var size = bitmap.Size;
                using (var stretchDialog = new StretchDialog(new Size(size.Width, size.Height)))
                {
                    if (stretchDialog.ShowDialog() != DialogResult.OK) return;

                    var imageSize = stretchDialog.ImageSize;

                    using (var builder = new StretchBuilder(imageSize))
                    {
                        pictureEdit1.Image = builder.Stretch(bitmap);
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
    }
}