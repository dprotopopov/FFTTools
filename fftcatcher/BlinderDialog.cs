using System.Drawing;
using DevExpress.XtraEditors;

namespace fftcatcher
{
    public partial class BlinderDialog : XtraForm
    {
        private readonly Size _size;

        public BlinderDialog(Size size)
        {
            _size = size;
            InitializeComponent();
            spinEdit1.Value = size.Width;
            spinEdit2.Value = size.Height;
        }

        public Size BlinderSize
        {
            get { return new Size((int) spinEdit1.Value, (int) spinEdit2.Value); }
        }
    }
}