using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Slot_Simulator
{
    public partial class frmProgressives : Form
    {
        public int leftStart = 0;
        public string progressiveType = "";
        List<TextBox> ProgressiveNameTBs;
        public List<TextBox> ProgressiveValueTBs;
        public frmProgressives()
        {
            InitializeComponent();
        }

        private void frmProgressives_Load(object sender, EventArgs e)
        {
            this.Left = leftStart;
        }

        public void setProgressiveTextBoxes(List<ProgressiveData> _progressives)
        {
            if(_progressives.Count() > 0)
            {
                ProgressiveNameTBs = new List<TextBox>();
                ProgressiveValueTBs = new List<TextBox>();
                for(int aa = 0; aa < _progressives.Count(); aa++)
                {
                    TextBox nameTB = new TextBox();
                    TextBox valueTB = new TextBox();
                    nameTB.Location = new Point(12, aa == 0 ? 12 : ProgressiveValueTBs[aa - 1].Location.Y + 58);
                    valueTB.Location = new Point(12, nameTB.Location.Y + 52);
                    nameTB.Width = this.Width - 38;
                    valueTB.Width = this.Width - 38;
                    nameTB.BorderStyle = BorderStyle.None;
                    valueTB.BorderStyle = BorderStyle.FixedSingle;
                    nameTB.Font = new Font("Tahoma", 24, FontStyle.Bold);
                    valueTB.Font = new Font("Tahoma", 24, FontStyle.Bold);
                    nameTB.TextAlign = HorizontalAlignment.Center;
                    valueTB.TextAlign = HorizontalAlignment.Right;
                    nameTB.BackColor = Color.FromKnownColor(KnownColor.Control);
                    valueTB.BackColor = Color.White;
                    nameTB.ForeColor = Color.Black;
                    valueTB.ForeColor = Color.Black;
                    nameTB.Text = _progressives[aa].Name;
                    valueTB.Text = string.Format("{0:$0,0.00}", (double)_progressives[aa].CurrentValue / (double)100);
                    nameTB.Visible = true;
                    valueTB.Visible = true;
                    nameTB.ReadOnly = true;
                    valueTB.ReadOnly = true;
                    this.Controls.Add(nameTB);
                    this.Controls.Add(valueTB);
                    ProgressiveNameTBs.Add(nameTB);
                    ProgressiveValueTBs.Add(valueTB);
                    this.Height = valueTB.Location.Y + 88;
                }
            }
        }
    }
}
