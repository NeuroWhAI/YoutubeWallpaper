namespace YoutubeWallpaper
{
    partial class Form_Touchpad
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Touchpad));
            this.SuspendLayout();
            // 
            // Form_Touchpad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 191);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "Form_Touchpad";
            this.Opacity = 0.7D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Touchpad";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Touchpad_FormClosing);
            this.Load += new System.EventHandler(this.Form_Touchpad_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form_Touchpad_MouseClick);
            this.MouseEnter += new System.EventHandler(this.Form_Touchpad_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.Form_Touchpad_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_Touchpad_MouseMove);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.Form_Touchpad_PreviewKeyDown);
            this.ResumeLayout(false);

        }

        #endregion
    }
}