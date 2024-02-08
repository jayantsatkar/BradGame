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
    public partial class frmPickGame : Form
    {
        public int m_numberOfChoices;
        public List<Button> pickButtons;
        public List<string> currentScript;
        public int currentPick;
        public frmPickGame()
        {
            InitializeComponent();
        }
        public void buildButtons()
        {
            pickButtons = new List<Button>();
            for (int aa = 0; aa < m_numberOfChoices; aa++)
            {
                Button pickButton = new Button();
                pickButton.Location = new Point(aa == 0 ? 12 : pickButtons[aa - 1].Location.X + 125, 12);
                pickButton.Width = 125;
                pickButton.Height = 125;
                pickButton.Text = aa.ToString();
                pickButton.Font = new Font("Tahoma", 12, FontStyle.Bold);
                pickButton.Visible = true;
                pickButton.Click += new EventHandler(this.getCurrentPick);
                this.Controls.Add(pickButton);
                pickButtons.Add(pickButton);
                this.Width = pickButton.Location.X + 149;
                this.Height = pickButton.Location.Y + 173;
            }
            currentPick = 0;
        }
        protected void getCurrentPick(object sender, EventArgs e)
        {
            if (currentPick < currentScript.Count() - 1)
            {
                ((Button)sender).Text = currentScript[currentPick];
                currentPick++;
            }
            else if (currentPick == currentScript.Count() - 1)
            {
                ((Button)sender).Text = currentScript[currentPick];
                currentPick = 0;
                MessageBox.Show("You Won " + currentScript[currentPick] + "!");
                this.Close();
            }
        }
        public void fillCurrentScript(List<string> _stringsForPick, int _match, int _jackpotLevel)
        {
            List<int> eachJackpotCount = new List<int>();
            currentScript = new List<string>();
            for(int aa = 0; aa < _stringsForPick.Count(); aa++)
            {
                eachJackpotCount.Add(0);
            }
            bool jackpotWon = false;
            while(!jackpotWon)
            {
                int randomChoice = m.RandomInteger(_stringsForPick.Count());
                currentScript.Add(_stringsForPick[randomChoice]);
                eachJackpotCount[randomChoice]++;
                if (eachJackpotCount[randomChoice] == _match && randomChoice == _jackpotLevel) jackpotWon = true;
                else if (eachJackpotCount[randomChoice] == _match && randomChoice != _jackpotLevel)
                {
                    currentScript.Remove(currentScript[currentScript.Count() - 1]);
                    eachJackpotCount[randomChoice]--;
                }
            }
        }
    }
}
