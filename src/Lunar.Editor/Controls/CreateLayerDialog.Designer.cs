﻿namespace Lunar.Editor.Controls
{
    partial class CreateLayerDialog
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtLayer = new DarkUI.Controls.DarkTextBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.SuspendLayout();
            // 
            // txtLayer
            // 
            this.txtLayer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.txtLayer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLayer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtLayer.Location = new System.Drawing.Point(96, 28);
            this.txtLayer.Name = "txtLayer";
            this.txtLayer.Size = new System.Drawing.Size(291, 20);
            this.txtLayer.TabIndex = 2;
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(7, 30);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(67, 13);
            this.darkLabel1.TabIndex = 3;
            this.darkLabel1.Text = "Layer Name:";
            // 
            // CreateLayerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 118);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.txtLayer);
            this.MaximizeBox = false;
            this.Name = "CreateLayerDialog";
            this.Text = "Create Layer";
            this.TopMost = true;
            this.Controls.SetChildIndex(this.txtLayer, 0);
            this.Controls.SetChildIndex(this.darkLabel1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkTextBox txtLayer;
        private DarkUI.Controls.DarkLabel darkLabel1;
    }
}
