using System.Windows.Forms;

namespace fftdeblur
{
    partial class FilterDialog
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
            this.spinEdit2 = new System.Windows.Forms.NumericUpDown();
            this.spinEdit1 = new System.Windows.Forms.NumericUpDown();
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
            this.layoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 141F));
            this.layoutControl1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 136F));
            this.layoutControl1.Controls.Add(this.spinEdit2, 1, 2);
            this.layoutControl1.Controls.Add(this.spinEdit1, 1, 1);
            this.layoutControl1.Location = new System.Drawing.Point(49, 28);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.RowCount = 3;
            this.layoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.layoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.layoutControl1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
            this.layoutControl1.Size = new System.Drawing.Size(277, 163);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // spinEdit2
            // 
            this.spinEdit2.Location = new System.Drawing.Point(144, 89);
            this.spinEdit2.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spinEdit2.Name = "spinEdit2";
            this.spinEdit2.Size = new System.Drawing.Size(130, 22);
            this.spinEdit2.TabIndex = 5;
            // 
            // spinEdit1
            // 
            this.spinEdit1.Location = new System.Drawing.Point(144, 42);
            this.spinEdit1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spinEdit1.Name = "spinEdit1";
            this.spinEdit1.Size = new System.Drawing.Size(130, 22);
            this.spinEdit1.TabIndex = 4;
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
            // FilterDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 264);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.layoutControl1);
            this.Name = "FilterDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter Options";
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel layoutControl1;
        private NumericUpDown spinEdit1;
        private NumericUpDown spinEdit2;
        private Button simpleButton1;
        private Button simpleButton2;
    }
}