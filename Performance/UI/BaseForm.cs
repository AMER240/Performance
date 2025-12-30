using System.Drawing;
using System.Windows.Forms;

namespace Performance.UI
{
    public class BaseForm : DevExpress.XtraEditors.XtraForm
    {
        public BaseForm()
        {
            InitializeBaseStyle();
        }

        private void InitializeBaseStyle()
        {
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(250, 250, 250);
            this.Padding = new Padding(8);
        }
    }
}
