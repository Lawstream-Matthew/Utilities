using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LawstreamUpdate.Forms
{
    public partial class Destructions : Form
    {
        public Destructions()
        {
            InitializeComponent();
        }

        private void txtDestructions_TextChanged(object sender, EventArgs e)
        {
            //StringBuilder destructions = new StringBuilder();
            //destructions.Append("1) Run the 'Replace invalid' option WITHOUT 'Replace Values' checked. This will build a file in the D:\Temp folder ");
            //destructions.AppendLine("containing all of the href values without an document name - can never be clicked by the user so remove them");
            //destructions.AppendLine("");
            //destructions.Append("2) Run the 'Fix documents' option. This will build a file in the D:\Temp folder ");
            //destructions.AppendLine("containing all of the href values without an document name - can never be clicked by the user so remove them");
        }
    }
}
