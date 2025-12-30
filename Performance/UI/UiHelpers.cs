using System.Drawing;
using System.Windows.Forms;

namespace Performance.UI
{
    public static class UiHelpers
    {
        public static void StyleGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = UiColors.GridHeader;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UiColors.LightText;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            grid.RowTemplate.Height = 35;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = UiColors.Background;
            grid.GridColor = UiColors.GridBorder;
            grid.BorderStyle = BorderStyle.None;
            grid.DefaultCellStyle.BackColor = UiColors.GridRow;
            grid.DefaultCellStyle.ForeColor = UiColors.PrimaryText;
            grid.DefaultCellStyle.SelectionBackColor = UiColors.GridSelection;
            grid.DefaultCellStyle.SelectionForeColor = UiColors.LightText;
            grid.AlternatingRowsDefaultCellStyle.BackColor = UiColors.GridAlternateRow;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }

        public static void ApplyButtonStyle(Button btn, bool isPrimary = true)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.ForeColor = UiColors.ButtonText;
            btn.BackColor = isPrimary ? UiColors.ButtonPrimary : UiColors.ButtonSecondary;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            
            // Hover effects
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = UiColors.ButtonHover;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = isPrimary ? UiColors.ButtonPrimary : UiColors.ButtonSecondary;
            };
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.BackColor = UiColors.InputBackground;
            txt.ForeColor = UiColors.InputText;
            txt.Font = new Font("Segoe UI", 10F);
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.BackColor = UiColors.InputBackground;
            cmb.ForeColor = UiColors.InputText;
            cmb.Font = new Font("Segoe UI", 10F);
            cmb.FlatStyle = FlatStyle.Flat;
        }

        public static void StyleLabel(Label lbl, bool isTitle = false)
        {
            lbl.ForeColor = isTitle ? UiColors.PrimaryText : UiColors.SecondaryText;
            lbl.Font = isTitle ? new Font("Segoe UI", 11F, FontStyle.Bold) : new Font("Segoe UI", 9F);
        }

        public static Panel CreateCard(int width, int height, string? title = null)
        {
            var card = new Panel()
            {
                Width = width,
                Height = height,
                BackColor = UiColors.CardBackground,
                BorderStyle = BorderStyle.FixedSingle
            };

            if (!string.IsNullOrEmpty(title))
            {
                var lblTitle = new Label()
                {
                    Text = title,
                    Dock = DockStyle.Top,
                    Height = 40,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = UiColors.PrimaryText,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0)
                };
                card.Controls.Add(lblTitle);
            }

            return card;
        }
    }
}
