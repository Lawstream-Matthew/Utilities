namespace LawstreamUpdate

{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnRefresh = new System.Windows.Forms.Button();
            this.chkWindowsAuthentication = new System.Windows.Forms.CheckBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cboDatabase = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.cboServer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClearSettings = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.radRemoveEmptyDocURLs = new System.Windows.Forms.RadioButton();
            this.radUpdateExistingURLs = new System.Windows.Forms.RadioButton();
            this.pbMain = new System.Windows.Forms.ProgressBar();
            this.radFixDocumentNames = new System.Windows.Forms.RadioButton();
            this.radExtractWithLinks = new System.Windows.Forms.RadioButton();
            this.radExtractInvalid = new System.Windows.Forms.RadioButton();
            this.radExtractUnique = new System.Windows.Forms.RadioButton();
            this.radValidateDocUrl = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radExtractInvalidLinks = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.radURLCleanup = new System.Windows.Forms.RadioButton();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnFolder = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.lblProgress = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(309, 110);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "&Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // chkWindowsAuthentication
            // 
            this.chkWindowsAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkWindowsAuthentication.AutoSize = true;
            this.chkWindowsAuthentication.Location = new System.Drawing.Point(78, 39);
            this.chkWindowsAuthentication.Name = "chkWindowsAuthentication";
            this.chkWindowsAuthentication.Size = new System.Drawing.Size(141, 17);
            this.chkWindowsAuthentication.TabIndex = 1;
            this.chkWindowsAuthentication.Text = "Windows Authentication";
            this.chkWindowsAuthentication.UseVisualStyleBackColor = true;
            this.chkWindowsAuthentication.CheckedChanged += new System.EventHandler(this.chkWindowsAuthentication_CheckedChanged);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(256, 141);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(129, 23);
            this.btnTest.TabIndex = 6;
            this.btnTest.Text = "Test Connection";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 117);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 44;
            this.label4.Text = "Database:";
            // 
            // cboDatabase
            // 
            this.cboDatabase.FormattingEnabled = true;
            this.cboDatabase.Location = new System.Drawing.Point(78, 114);
            this.cboDatabase.Name = "cboDatabase";
            this.cboDatabase.Size = new System.Drawing.Size(226, 21);
            this.cboDatabase.Sorted = true;
            this.cboDatabase.TabIndex = 4;
            this.cboDatabase.DropDown += new System.EventHandler(this.cboDatabase_DropDown);
            this.cboDatabase.Enter += new System.EventHandler(this.cboServer_Enter);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 43;
            this.label3.Text = "Password:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 42;
            this.label2.Text = "User name:";
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(78, 88);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(307, 20);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.Enter += new System.EventHandler(this.txt_Enter);
            // 
            // cboServer
            // 
            this.cboServer.FormattingEnabled = true;
            this.cboServer.Location = new System.Drawing.Point(78, 12);
            this.cboServer.Name = "cboServer";
            this.cboServer.Size = new System.Drawing.Size(307, 21);
            this.cboServer.Sorted = true;
            this.cboServer.TabIndex = 0;
            this.cboServer.DropDown += new System.EventHandler(this.cboServer_DropDown);
            this.cboServer.Enter += new System.EventHandler(this.cboServer_Enter);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "Server:";
            // 
            // txtUsername
            // 
            this.txtUsername.Enabled = false;
            this.txtUsername.Location = new System.Drawing.Point(78, 62);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(307, 20);
            this.txtUsername.TabIndex = 2;
            this.txtUsername.Enter += new System.EventHandler(this.txt_Enter);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(559, 317);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "&Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Location = new System.Drawing.Point(478, 317);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 12;
            this.btnRun.Text = "&Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClearSettings
            // 
            this.btnClearSettings.Location = new System.Drawing.Point(19, 295);
            this.btnClearSettings.Name = "btnClearSettings";
            this.btnClearSettings.Size = new System.Drawing.Size(129, 23);
            this.btnClearSettings.TabIndex = 11;
            this.btnClearSettings.Text = "Clear Registry Settings";
            this.btnClearSettings.UseVisualStyleBackColor = true;
            this.btnClearSettings.Click += new System.EventHandler(this.btnClearSettings_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(19, 266);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(129, 23);
            this.btnSaveSettings.TabIndex = 10;
            this.btnSaveSettings.Text = "Save Registry Settings";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // radRemoveEmptyDocURLs
            // 
            this.radRemoveEmptyDocURLs.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radRemoveEmptyDocURLs.AutoSize = true;
            this.radRemoveEmptyDocURLs.Location = new System.Drawing.Point(27, 214);
            this.radRemoveEmptyDocURLs.Name = "radRemoveEmptyDocURLs";
            this.radRemoveEmptyDocURLs.Size = new System.Drawing.Size(150, 17);
            this.radRemoveEmptyDocURLs.TabIndex = 8;
            this.radRemoveEmptyDocURLs.Text = "Remove Empty Doc URLs";
            this.radRemoveEmptyDocURLs.UseVisualStyleBackColor = true;
            // 
            // radUpdateExistingURLs
            // 
            this.radUpdateExistingURLs.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radUpdateExistingURLs.AutoSize = true;
            this.radUpdateExistingURLs.Location = new System.Drawing.Point(27, 237);
            this.radUpdateExistingURLs.Name = "radUpdateExistingURLs";
            this.radUpdateExistingURLs.Size = new System.Drawing.Size(175, 17);
            this.radUpdateExistingURLs.TabIndex = 9;
            this.radUpdateExistingURLs.Text = "Update URLs to reg documents";
            this.radUpdateExistingURLs.UseVisualStyleBackColor = true;
            // 
            // pbMain
            // 
            this.pbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbMain.Location = new System.Drawing.Point(0, 335);
            this.pbMain.Name = "pbMain";
            this.pbMain.Size = new System.Drawing.Size(138, 15);
            this.pbMain.TabIndex = 49;
            this.pbMain.Visible = false;
            // 
            // radFixDocumentNames
            // 
            this.radFixDocumentNames.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radFixDocumentNames.AutoSize = true;
            this.radFixDocumentNames.Location = new System.Drawing.Point(27, 168);
            this.radFixDocumentNames.Name = "radFixDocumentNames";
            this.radFixDocumentNames.Size = new System.Drawing.Size(122, 17);
            this.radFixDocumentNames.TabIndex = 6;
            this.radFixDocumentNames.Text = "Fix document names";
            this.radFixDocumentNames.UseVisualStyleBackColor = true;
            // 
            // radExtractWithLinks
            // 
            this.radExtractWithLinks.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radExtractWithLinks.AutoSize = true;
            this.radExtractWithLinks.Location = new System.Drawing.Point(27, 42);
            this.radExtractWithLinks.Name = "radExtractWithLinks";
            this.radExtractWithLinks.Size = new System.Drawing.Size(104, 17);
            this.radExtractWithLinks.TabIndex = 1;
            this.radExtractWithLinks.Text = "Extract with links";
            this.radExtractWithLinks.UseVisualStyleBackColor = true;
            // 
            // radExtractInvalid
            // 
            this.radExtractInvalid.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radExtractInvalid.AutoSize = true;
            this.radExtractInvalid.Location = new System.Drawing.Point(27, 88);
            this.radExtractInvalid.Name = "radExtractInvalid";
            this.radExtractInvalid.Size = new System.Drawing.Size(91, 17);
            this.radExtractInvalid.TabIndex = 3;
            this.radExtractInvalid.Text = "Extract invalid";
            this.radExtractInvalid.UseVisualStyleBackColor = true;
            // 
            // radExtractUnique
            // 
            this.radExtractUnique.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radExtractUnique.AutoSize = true;
            this.radExtractUnique.Location = new System.Drawing.Point(27, 65);
            this.radExtractUnique.Name = "radExtractUnique";
            this.radExtractUnique.Size = new System.Drawing.Size(93, 17);
            this.radExtractUnique.TabIndex = 2;
            this.radExtractUnique.Text = "Extract unique";
            this.radExtractUnique.UseVisualStyleBackColor = true;
            // 
            // radValidateDocUrl
            // 
            this.radValidateDocUrl.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radValidateDocUrl.AutoSize = true;
            this.radValidateDocUrl.Location = new System.Drawing.Point(27, 191);
            this.radValidateDocUrl.Name = "radValidateDocUrl";
            this.radValidateDocUrl.Size = new System.Drawing.Size(156, 17);
            this.radValidateDocUrl.TabIndex = 7;
            this.radValidateDocUrl.Text = "Validate documents / URLs";
            this.radValidateDocUrl.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radExtractInvalidLinks);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.radValidateDocUrl);
            this.groupBox3.Controls.Add(this.radExtractWithLinks);
            this.groupBox3.Controls.Add(this.radFixDocumentNames);
            this.groupBox3.Controls.Add(this.radExtractUnique);
            this.groupBox3.Controls.Add(this.radExtractInvalid);
            this.groupBox3.Controls.Add(this.radRemoveEmptyDocURLs);
            this.groupBox3.Controls.Add(this.radURLCleanup);
            this.groupBox3.Controls.Add(this.radUpdateExistingURLs);
            this.groupBox3.Location = new System.Drawing.Point(391, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(243, 298);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            // 
            // radExtractInvalidLinks
            // 
            this.radExtractInvalidLinks.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radExtractInvalidLinks.AutoSize = true;
            this.radExtractInvalidLinks.Location = new System.Drawing.Point(27, 111);
            this.radExtractInvalidLinks.Name = "radExtractInvalidLinks";
            this.radExtractInvalidLinks.Size = new System.Drawing.Size(122, 17);
            this.radExtractInvalidLinks.TabIndex = 4;
            this.radExtractInvalidLinks.Text = "Extract Invalid URLs";
            this.radExtractInvalidLinks.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 151);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(150, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Update Documents and URLs";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Extract Data";
            // 
            // radURLCleanup
            // 
            this.radURLCleanup.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radURLCleanup.AutoSize = true;
            this.radURLCleanup.Location = new System.Drawing.Point(27, 260);
            this.radURLCleanup.Name = "radURLCleanup";
            this.radURLCleanup.Size = new System.Drawing.Size(97, 17);
            this.radURLCleanup.TabIndex = 10;
            this.radURLCleanup.Text = "Clean up URLs";
            this.radURLCleanup.UseVisualStyleBackColor = true;
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(77, 203);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(274, 20);
            this.txtFolder.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(31, 207);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 51;
            this.label7.Text = "Folder:";
            // 
            // btnFolder
            // 
            this.btnFolder.Location = new System.Drawing.Point(357, 202);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(27, 23);
            this.btnFolder.TabIndex = 8;
            this.btnFolder.Text = "...";
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(145, 336);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(126, 13);
            this.lblProgress.TabIndex = 53;
            this.lblProgress.Text = "n of m records processed";
            this.lblProgress.Visible = false;
            // 
            // Main
            // 
            this.AcceptButton = this.btnRun;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(646, 348);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.btnFolder);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.pbMain);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.btnClearSettings);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.chkWindowsAuthentication);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cboDatabase);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.cboServer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRun);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Update Utility";
            this.Load += new System.EventHandler(this.DatabaseConnection_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox chkWindowsAuthentication;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.ComboBox cboServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClearSettings;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.RadioButton radRemoveEmptyDocURLs;
        private System.Windows.Forms.RadioButton radUpdateExistingURLs;
        private System.Windows.Forms.ProgressBar pbMain;
        private System.Windows.Forms.RadioButton radFixDocumentNames;
        private System.Windows.Forms.RadioButton radExtractWithLinks;
        private System.Windows.Forms.RadioButton radExtractInvalid;
        private System.Windows.Forms.RadioButton radExtractUnique;
        private System.Windows.Forms.RadioButton radValidateDocUrl;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radExtractInvalidLinks;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.RadioButton radURLCleanup;
    }
}