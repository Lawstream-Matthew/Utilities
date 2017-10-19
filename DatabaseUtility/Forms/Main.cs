using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using LawstreamUpdate.Classes;
using System.Linq;

namespace LawstreamUpdate
{
    /// <summary>
    /// Class to store the database connection properties.
    /// </summary>
    public partial class Main : Form
    {
        #region Members

        DBConnection _db;

        #endregion

        #region Properties

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            _db = new DBConnection();
        }
       
        #region Methods

        /// <summary>
        /// Populates the controls on the form with the previously-used database
        /// settings.
        /// </summary>
        private void DisplayConnectionInfo()
        {
            cboServer.Text = _db.Server;
            cboDatabase.Text = _db.Database;
            chkWindowsAuthentication.Checked = _db.WindowsAuthentication;

            if (_db.WindowsAuthentication == false)
            {
                txtUsername.Text = _db.Username;
                txtPassword.Text = _db.Password;
            }
            else
            {
                txtUsername.Enabled = false;
                txtPassword.Enabled = false;
            }
        }

        /// <summary>
        /// Creates and logs into a SQL Server SMO server / database object.
        /// </summary>
        /// <returns>A reference to a SQL Server SMO object</returns>
        private Server GetSQLServer()
        {
            try
            {
                string serverAndInstanceName = cboServer.Text.Trim();

                if (serverAndInstanceName != "")
                {
                    Server server = new Server(serverAndInstanceName);

                    server.ConnectionContext.ServerInstance = serverAndInstanceName;
                    server.ConnectionContext.LoginSecure = chkWindowsAuthentication.Checked;
                    if (!chkWindowsAuthentication.Checked)
                    {
                        server.ConnectionContext.Login = txtUsername.Text.Trim();
                        server.ConnectionContext.Password = txtPassword.Text.Trim();
                    }

                    server.ConnectionContext.Connect();

                    return server;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Function to list all the SQL Servers available to the user
        /// </summary>
        /// <remarks></remarks>
        private void LoadSQLServers()
        {
            if (cboServer.Items.Count == 0)
            {
                cboServer.Items.Add("");

                string sqlServerName = "";
                DataTable dtServers = SmoApplication.EnumAvailableSqlServers(false);
                foreach (DataRow row in dtServers.Rows)
                {
                    if (row["Instance"] != null && row["Instance"].ToString().Length > 0)
                    {
                        sqlServerName += @"\" + row["Instance"].ToString();
                    }
                    else
                    {
                        sqlServerName = row["Server"].ToString();
                    }
                    cboServer.Items.Add(sqlServerName.ToUpper());
                }

                if (cboServer.Items.Count == 0)
                {
                    cboServer.Text = "<No available SQL Servers>";
                }
            }
        }

        /// <summary>
        /// Updates the connection properties / settings.
        /// </summary>
        private void UpdateSettings()
        {
            _db.Database = cboDatabase.Text;
            _db.Server = cboServer.Text;
            _db.Username = txtUsername.Text;
            _db.Password = txtPassword.Text;
            _db.WindowsAuthentication = chkWindowsAuthentication.Checked;

            _db.BuildConnectionString();
        }

        #endregion
        
        #region Events

        /// <summary>
        /// Form load event. We read the values from the registry so that users aren't having to
        /// continually re-enter the connection info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatabaseConnection_Load(object sender, EventArgs e)
        {
            try
            {
                txtFolder.Text = _db.Load();

                DisplayConnectionInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Save the information to the registry, open / test the database connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateSettings();

                // Execute the database changes
                int recCount = 0;
                int updateCount = -1;

                this.Cursor = Cursors.WaitCursor;
                DoWork dw = new DoWork();
                dw.FolderPath = txtFolder.Text;

                // Wires the ProgressUpdate event so we can update the progress bar
                dw.OnProgressUpdate += new DoWork.ProgressUpdateHandler(OnProgressUpdateHandler);

                //
                // Run the correct function
                //
                if (radExtractWithLinks.Checked)
                {
                    dw.ExtractDocumentLinksWithLegislation(_db, out recCount);
                }
                else if (radExtractUnique.Checked)
                {
                    dw.ExtractUniqueDocumentLinks(_db, out recCount);
                }
                else if (radExtractInvalid.Checked)
                {
                    dw.ExtractInValidDocumentLinksWithLegislation(_db, out recCount);
                }
                else if (radRemoveEmptyDocURLs.Checked)
                {
                    dw.RemoveDocumentLinks(_db, out recCount, out updateCount);
                }
                else if (radExtractInvalidLinks.Checked)
                {
                    dw.ExtractInValidDocumentLinksWithLegislation(_db, out recCount);
                }
                else if (radFixDocumentNames.Checked)
                {
                    dw.FixDocumentNames(out recCount, out updateCount);
                }
                else if (radValidateDocUrl.Checked)
                {
                    List<Tuple<string, string>> duplicateDocumentList = dw.ValidateDocumentsAndUrls(out recCount, out updateCount);

                    if (duplicateDocumentList.Any())
                    {
                        Display displayDuplicates = new Display();
                        displayDuplicates.DocumentList = duplicateDocumentList;
                        displayDuplicates.Show(this);
                    }
                }
                else if (radUpdateExistingURLs.Checked)
                {
                    dw.UpdateExistingURLsInCode(_db, out recCount, out updateCount);
                }
                else if (radURLCleanup.Checked)
                {
                    dw.CleanUpLeftoverURLs(_db, out recCount, out updateCount);
                }

                this.Cursor = Cursors.Default;

                pbMain.Value = pbMain.Maximum;
                pbMain.Visible = false;
                lblProgress.Visible = false;

                string userMsg = recCount.ToString() + " records processed";

                if (updateCount >= 0)
                {
                    userMsg += ", " + updateCount.ToString() + " records updated";
                }

                MessageBox.Show(userMsg, "Document extract", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                pbMain.Visible = false;
                lblProgress.Visible = false;
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:ProgressUpdateHandler" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ProgressUpdateEventArgs"/> instance containing the event data.</param>
        protected void OnProgressUpdateHandler(ProgressUpdateEventArgs e)
        {
            try
            {
                pbMain.Visible = true;
                lblProgress.Visible = true;

                pbMain.Maximum = e.Total;
                pbMain.Value = e.Current;

                lblProgress.Text = e.Current + " of " + e.Total + " processed";

                Application.DoEvents();
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Nothing to do, close the form and leave quietly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Display the available databases for this server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboDatabase_DropDown(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                if (cboDatabase.Items.Count == 0)
                {
                    Server server = GetSQLServer();
                    if (server != null)
                    {
                        foreach (Database db in server.Databases)
                        {
                            cboDatabase.Items.Add(db.Name);
                        }

                        cboDatabase.Sorted = true;
                        if (cboDatabase.Items.Count > 0)
                        {
                            cboDatabase.Enabled = true;
                        }
                        else
                        {
                            cboDatabase.Enabled = false;
                            cboDatabase.Text = "<No databases found on server>";
                        }

                        server.ConnectionContext.Disconnect();
                    }
                    else
                    {
                        cboDatabase.Enabled = false;
                        cboDatabase.Text = "<No databases found on server>";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        
        /// <summary>
        /// Test the database connection with these parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                DBConnection dbTest = new DBConnection(cboServer.Text, cboDatabase.Text, txtUsername.Text, txtPassword.Text, chkWindowsAuthentication.Checked);
                dbTest.Open();

                // If we get here, it all worked!
                MessageBox.Show("Database connection tested successfully!", "Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Connection errors will be picked up here
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// Load the list of SQL Servers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboServer_DropDown(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                LoadSQLServers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Handle the user selecting / deselecting the Windows Authentication checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkWindowsAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _db.WindowsAuthentication = chkWindowsAuthentication.Checked;

                txtUsername.Enabled = (!_db.WindowsAuthentication);
                txtPassword.Enabled = (!_db.WindowsAuthentication);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Refreshes the list of available databases in the dropdown combo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                cboDatabase.Items.Clear();
                cboDatabase.Text = "";

                cboDatabase_DropDown(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
        /// <summary>
        /// Handles the Enter event of the cboServer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void cboServer_Enter(object sender, EventArgs e)
        {
            try
            {
                ((ComboBox)sender).SelectionStart = 0;
                ((ComboBox)sender).SelectionLength = ((ComboBox)sender).Text.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Handles the Enter event of the txt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void txt_Enter(object sender, EventArgs e)
        {
            try
            {
                ((TextBox)sender).SelectionStart = 0;
                ((TextBox)sender).SelectionLength = ((TextBox)sender).Text.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Handles the Click event of the btnSaveSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateSettings();

                _db.Save(txtFolder.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnClearSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnClearSettings_Click(object sender, EventArgs e)
        {
            try
            {
                _db.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnFolder_Click(object sender, EventArgs e)
        {
            try
            {
                folderBrowserDialog.ShowNewFolderButton = true;
                folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
                folderBrowserDialog.SelectedPath = txtFolder.Text;
                DialogResult result = folderBrowserDialog.ShowDialog(this);
                
                if (result == DialogResult.OK)
                {
                    txtFolder.Text = folderBrowserDialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

    }
}
