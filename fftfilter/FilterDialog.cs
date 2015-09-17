using System.Drawing;
using DevExpress.XtraEditors;

namespace fftfilter
{
    public partial class FilterDialog : XtraForm
    {
        private readonly Size _size;

        public FilterDialog(Size size)
        {
            _size = size;
            InitializeComponent();
            spinEdit1.Value = size.Width;
            spinEdit2.Value = size.Height;
        }

        public Size FilterSize
        {
            get { return new Size((int) spinEdit1.Value, (int) spinEdit2.Value); }
        }
    }
}