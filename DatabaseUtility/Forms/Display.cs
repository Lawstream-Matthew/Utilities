using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LawstreamUpdate
{
    public partial class Display : Form
    {
        #region Members

        /// <summary>
        /// The collection of errors to display.
        /// </summary>
        private List<string> _errors = null;

        /// <summary>
        /// The SQL to display to the user.
        /// </summary>
        private string _SQL;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the list of string, string tuples. They will be displayed ordered by Item 1, then by Item 2.
        /// can be found there.
        /// </summary>
        /// <value>The system configuration object.</value>
        public List<Tuple<string, string>> DocumentList
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Display"/> class.
        /// </summary>
        /// <param name="SQL">The SQL string to display.</param>
        public Display()
        {
            InitializeComponent();
        }
        
        #endregion

        /// <summary>
        /// Form Load event handler. Displays the errors for each worksheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_Load(object sender, EventArgs e)
        {
            try
            {
                txtDisplay.Clear();

                if (this.DocumentList.Any())
                {
                    this.DocumentList = this.DocumentList.OrderBy(x => x.Item1).ThenBy(y => y.Item2).ToList();

                    txtDisplay.SelectionFont = new Font(txtDisplay.Font.FontFamily, 14, FontStyle.Bold | FontStyle.Underline);
                    txtDisplay.AppendText("Duplicate Document URLs" + Environment.NewLine + Environment.NewLine);

                    txtDisplay.SelectionFont = new Font(txtDisplay.Font.FontFamily, 10, FontStyle.Regular);
                    foreach (Tuple<string, string> duplicate in this.DocumentList)
                    {
                        txtDisplay.AppendText(duplicate.Item1 + ";" + duplicate.Item2 + Environment.NewLine);
                    }

                    txtDisplay.SelectionStart = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}