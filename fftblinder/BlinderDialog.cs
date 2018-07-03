using System.Drawing;
using System.Windows.Forms;

namespace fftblinder
{
    public partial class BlinderDialog : Form
    {
        private readonly Size _size;

        public BlinderDialog(Size size)
        {
            _size = size;
            InitializeComponent();
            spinEdit1.Value = size.Width;
            spinEdit2.Value = size.Height;
        }

        public Size BlinderSize => new Size((int) spinEdit1.Value, (int) spinEdit2.Value);
    }
}