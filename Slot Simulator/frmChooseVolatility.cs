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
    public partial class frmChooseVolatility : Form
    {
        public List<string> m_choiceStrings;
        public List<Button> m_choiceButtons;
        public List<TextBox> m_choiceTexts;
        public int m_currentChoice;
        public frmChooseVolatility()
        {
            InitializeComponent();
        }
        public void createChoices()
        {
            m_choiceTexts = new List<TextBox>();
            m_choiceButtons = new List<Button>();
            m_currentChoice = -1;
            for (int aa = 0; aa < m_choiceStrings.Count(); aa++)
            {
                Button choiceButton = new Button();
                choiceButton.Location = new Point(aa == 0 ? 12 : m_choiceButtons[aa - 1].Location.X + 125, 12);
                choiceButton.Width = 125;
                choiceButton.Height = 125;
                choiceButton.TextAlign = ContentAlignment.MiddleCenter;
                choiceButton.Text = m_choiceStrings[aa];
                choiceButton.Font = new Font("Tahoma", 10, FontStyle.Bold);
                choiceButton.Visible = true;
                choiceButton.Click += new EventHandler(this.getChoice);
                this.Controls.Add(choiceButton);
                m_choiceButtons.Add(choiceButton);
                this.Width = choiceButton.Location.X + 149;
                this.Height = choiceButton.Location.Y + 173;
            }
        }
        protected void getChoice(object sender, EventArgs e)
        {
            for(int ab = 0; ab < m_choiceButtons.Count(); ab++)
            {
                if (((Button)sender).Text == m_choiceStrings[ab]) m_currentChoice = ab;
            }
            this.Close();
        }
    }
}
