using System;
using System.Drawing;
using System.Windows.Forms;

namespace fftzoomer
{
    public partial class StretchDialog : Form
    {
        private readonly Size _size;

        public StretchDialog(Size size)
        {
            _size = size;
            InitializeComponent();
            spinEdit1.Value = size.Width;
            spinEdit2.Value = size.Height;
        }

        public bool KeepProportions => checkBox1.Checked;

        public Size ImageSize => new Size((int) spinEdit1.Value, (int) spinEdit2.Value);

        private void spinEdit1_EditValueChanged(object sender, EventArgs e)
        {
            if (KeepProportions)
            {
                spinEdit2.Value = spinEdit1.Value*_size.Height/_size.Width;
            }
        }

        private void spinEdit2_EditValueChanged(object sender, EventArgs e)
        {
            if (KeepProportions)
            {
                spinEdit1.Value = spinEdit2.Value * _size.Width / _size.Height;
            }

        }
    }
}