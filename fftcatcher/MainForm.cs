using System;
using System.Drawing;
using System.Windows.Forms;
using FFTTools;

namespace fftcatcher
{
    /// <summary>
    ///     Main application window
    /// </summary>
    public partial class MainForm : Form
    {
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
                pictureEditFile.Image = bitmap;
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

        private bool WithColor => withcolorToolStripMenuItem.Checked;
        private bool FastMode => fastModeToolStripMenuItem.Checked;

        public class Info
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double Value { get; set; }
        }

        private void openPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                var bitmap = new Bitmap(openFileDialog1.FileName);
                pictureEditPattern.Image = bitmap;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void withcolorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            withcolorToolStripMenuItem.Checked = !withcolorToolStripMenuItem.Checked;
        }

        private void fastModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastModeToolStripMenuItem.Checked = !fastModeToolStripMenuItem.Checked;
        }

        private void catchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEditFile.Image as Bitmap;
                var pattern = pictureEditPattern.Image as Bitmap;
                if (bitmap == null || pattern == null) throw new Exception("Нет изображения");
                using (var builder = new CatchBuilder(pattern, WithColor, FastMode))
                {
                    var matrix = builder.Catch(bitmap);
                    int x, y;
                    double value;
                    CatchBuilder.Max(matrix, out x, out y, out value);
                    propertyGrid1.SelectedObject = new Info { X = x, Y = y, Value = value };
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var pen = new Pen(Color.Red);
                        graphics.DrawRectangle(pen, x, y, pattern.Width, pattern.Height);
                        pictureEditFile.Image = bitmap;
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void visualizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var bitmap = pictureEditFile.Image as Bitmap;
                var pattern = pictureEditPattern.Image as Bitmap;
                if (bitmap == null || pattern == null) throw new Exception("Нет изображения");
                using (var builder = new CatchBuilder(pattern, WithColor, FastMode))
                    pictureEditFile.Image = builder.ToBitmap(bitmap);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}