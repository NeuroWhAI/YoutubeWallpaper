namespace YoutubeWallpaper
{
    partial class Form_Wallpaper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Wallpaper));
            this.webBrowser_page = new System.Windows.Forms.WebBrowser();
            this.panel_cursor = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // webBrowser_page
            // 
            this.webBrowser_page.AllowWebBrowserDrop = false;
            this.webBrowser_page.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_page.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_page.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_page.Name = "webBrowser_page";
            this.webBrowser_page.ScrollBarsEnabled = false;
            this.webBrowser_page.Size = new System.Drawing.Size(595, 396);
            this.webBrowser_page.TabIndex = 0;
            this.webBrowser_page.Url = new System.Uri("", System.UriKind.Relative);
            this.webBrowser_page.WebBrowserShortcutsEnabled = false;
            // 
            // panel_cursor
            // 
            this.panel_cursor.BackColor = System.Drawing.SystemColors.Highlight;
            this.panel_cursor.Location = new System.Drawing.Point(0, 0);
            this.panel_cursor.Name = "panel_cursor";
            this.panel_cursor.Size = new System.Drawing.Size(16, 16);
            this.panel_cursor.TabIndex = 1;
            this.panel_cursor.Visible = false;
            // 
            // Form_Wallpaper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 396);
            this.Controls.Add(this.panel_cursor);
            this.Controls.Add(this.webBrowser_page);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Wallpaper";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Wallpaper";
            this.Load += new System.EventHandler(this.Form_Wallpaper_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser_page;
        private System.Windows.Forms.Panel panel_cursor;
    }
}