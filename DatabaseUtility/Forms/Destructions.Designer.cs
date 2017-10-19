namespace LawstreamUpdate.Forms
{
    partial class Destructions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Destructions));
            this.txtDestructions = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtDestructions
            // 
            this.txtDestructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDestructions.Location = new System.Drawing.Point(0, 0);
            this.txtDestructions.Multiline = true;
            this.txtDestructions.Name = "txtDestructions";
            this.txtDestructions.ReadOnly = true;
            this.txtDestructions.Size = new System.Drawing.Size(818, 494);
            this.txtDestructions.TabIndex = 0;
            this.txtDestructions.TextChanged += new System.EventHandler(this.txtDestructions_TextChanged);
            // 
            // Destructions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 494);
            this.Controls.Add(this.txtDestructions);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Destructions";
            this.Text = "Destructions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDestructions;
    }
}