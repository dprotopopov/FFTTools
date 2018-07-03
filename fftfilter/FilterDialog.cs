using System.Drawing;
using System.Windows.Forms;

namespace fftfilter
{
    public partial class FilterDialog : Form
    {
        private readonly Size _size;

        public FilterDialog(Size size)
        {
            _size = size;
            InitializeComponent();
            spinEdit1.Value = size.Width;
            spinEdit2.Value = size.Height;
        }

        public Size FilterSize => new Size((int) spinEdit1.Value, (int) spinEdit2.Value);
    }
}