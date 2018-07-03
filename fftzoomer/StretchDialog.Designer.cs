using System.Windows.Forms;

namespace fftzoomer
{
    partial class StretchDialog
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
            this.layoutControl1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.spinEdit2 = new System.Windows.Forms.NumericUpDown();
            this.spinEdit1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.simpleButton1 = new System.Windows.Forms.Button();
            this.simpleButton2 = new System.Windows.Forms.Button();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.ColumnCount = 2;
            this.layoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.51625F));
            this.layoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.48375F));
            this.layoutControl1.Controls.Add(this.checkBox1, 1, 0);
            this.layoutControl1.Controls.Add(this.spinEdit2, 1, 2);
            this.layoutControl1.Controls.Add(this.spinEdit1, 1, 1);
            this.layoutControl1.Controls.Add(this.label1, 0, 2);
            this.layoutControl1.Controls.Add(this.label2, 0, 1);
            this.layoutControl1.Location = new System.Drawing.Point(49, 28);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.RowCount = 3;
            this.layoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.layoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.layoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.layoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.layoutControl1.Size = new System.Drawing.Size(277, 143);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // checkBox1
            // 
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(118, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(156, 14);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "Keep proportions";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // spinEdit2
            // 
            this.spinEdit2.Location = new System.Drawing.Point(118, 97);
            this.spinEdit2.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spinEdit2.Name = "spinEdit2";
            this.spinEdit2.Size = new System.Drawing.Size(156, 22);
            this.spinEdit2.TabIndex = 5;
            this.spinEdit2.ValueChanged += new System.EventHandler(this.spinEdit2_EditValueChanged);
            // 
            // spinEdit1
            // 
            this.spinEdit1.Location = new System.Drawing.Point(118, 50);
            this.spinEdit1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spinEdit1.Name = "spinEdit1";
            this.spinEdit1.Size = new System.Drawing.Size(156, 22);
            this.spinEdit1.TabIndex = 4;
            this.spinEdit1.ValueChanged += new System.EventHandler(this.spinEdit1_EditValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Height";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Width";
            // 
            // simpleButton1
            // 
            this.simpleButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.simpleButton1.Location = new System.Drawing.Point(142, 216);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(82, 19);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Text = "Ok";
            // 
            // simpleButton2
            // 
            this.simpleButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.simpleButton2.Location = new System.Drawing.Point(240, 216);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(76, 19);
            this.simpleButton2.TabIndex = 2;
            this.simpleButton2.Text = "Cancel";
            // 
            // StretchDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 264);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.layoutControl1);
            this.Name = "StretchDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Stretch Options";
            this.layoutControl1.ResumeLayout(false);
            this.layoutControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel layoutControl1;
        private CheckBox checkBox1;
        private NumericUpDown spinEdit1;
        private NumericUpDown spinEdit2;
        private Button simpleButton1;
        private Button simpleButton2;
        private Label label1;
        private Label label2;
    }
}