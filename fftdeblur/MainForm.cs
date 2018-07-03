using System;
using System.Drawing;
using System.Windows.Forms;
using FFTTools;

namespace fftdeblur
{
    /// <summary>
    ///     Main application window
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly FilterDialog _filterDialog = new FilterDialog(new Size(3, 3));

        public MainForm()
        {
            InitializeComponent();
            openFileDialog1.Filter =
                saveFileDialog1.Filter =
                    @"Bitmap Images (*.bmp)|*.bmp|Jpeg Images (*.jpg)|*.jpg|All Files (*.*)|*.*";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                var bitmap = new Bitmap(openFileDialog1.FileName);
                pictureEdit1.Image = bitmap;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) throw new Exception("Нет изображения");
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                bitmap.Save(saveFileDialog1.FileName);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void blurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) throw new Exception("Нет изображения");
                if (_filterDialog.ShowDialog() != DialogResult.OK) return;

                var filterSize = _filterDialog.FilterSize;

                using (var builder = new FilterBuilder(filterSize))
                {
                    pictureEdit1.Image = builder.Blur(bitmap);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void visualizeBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) throw new Exception("Нет изображения");
                if (_filterDialog.ShowDialog() != DialogResult.OK) return;

                var filterSize = _filterDialog.FilterSize;

                using (var builder = new FilterBuilder(filterSize))
                {
                    pictureEdit1.Image = builder.ToBitmap(bitmap);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void deblurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) throw new Exception("Нет изображения");
                using (var deblurBuilder = new DeblurBuilder())
                {
                    pictureEdit1.Image = deblurBuilder.Deblur(bitmap);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void visualizeDeblurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEdit1.Image as Bitmap;
                if (bitmap == null) throw new Exception("Нет изображения");
                using (var deblurBuilder = new DeblurBuilder())
                {
                    pictureEdit1.Image = deblurBuilder.ToBitmap(bitmap);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}