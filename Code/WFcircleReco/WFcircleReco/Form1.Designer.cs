namespace WFcircleReco
{
  partial class Form1
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
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.label2 = new System.Windows.Forms.Label();
      this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
      this.button5 = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton3 = new System.Windows.Forms.RadioButton();
      this.radioButton4 = new System.Windows.Forms.RadioButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label7 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(83, 30);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "Open image";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(365, 30);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(124, 23);
      this.button2.TabIndex = 1;
      this.button2.Text = "Edge detection";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Visible = false;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.Location = new System.Drawing.Point(384, 80);
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(82, 20);
      this.numericUpDown1.TabIndex = 2;
      this.numericUpDown1.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
      this.numericUpDown1.Visible = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(353, 62);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(143, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Threshold for edge detection";
      this.label1.Visible = false;
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(548, 30);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(90, 23);
      this.button3.TabIndex = 4;
      this.button3.Text = "Polygons";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Visible = false;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(871, 30);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(75, 23);
      this.button4.TabIndex = 5;
      this.button4.Text = "Save circles";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Visible = false;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(83, 118);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(941, 14);
      this.progressBar1.TabIndex = 6;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(20, 200);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(600, 600);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 7;
      this.pictureBox1.TabStop = false;
      // 
      // pictureBox2
      // 
      this.pictureBox2.Location = new System.Drawing.Point(640, 200);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(600, 600);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox2.TabIndex = 8;
      this.pictureBox2.TabStop = false;
      this.pictureBox2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox2_MouseClick);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(532, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(118, 13);
      this.label2.TabIndex = 9;
      this.label2.Text = "Approximation precision";
      this.label2.Visible = false;
      // 
      // numericUpDown2
      // 
      this.numericUpDown2.DecimalPlaces = 2;
      this.numericUpDown2.Location = new System.Drawing.Point(564, 80);
      this.numericUpDown2.Name = "numericUpDown2";
      this.numericUpDown2.Size = new System.Drawing.Size(66, 20);
      this.numericUpDown2.TabIndex = 10;
      this.numericUpDown2.Value = new decimal(new int[] {
            13,
            0,
            0,
            65536});
      this.numericUpDown2.Visible = false;
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(730, 30);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(75, 23);
      this.button5.TabIndex = 11;
      this.button5.Text = "Print circles";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Visible = false;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(21, 135);
      this.label3.MaximumSize = new System.Drawing.Size(500, 30);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(41, 15);
      this.label3.TabIndex = 12;
      this.label3.Text = "label3";
      this.label3.Visible = false;
      // 
      // label5
      // 
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
      this.label5.Location = new System.Drawing.Point(228, 178);
      this.label5.MaximumSize = new System.Drawing.Size(200, 20);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(195, 19);
      this.label5.TabIndex = 19;
      // 
      // label6
      // 
      this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
      this.label6.Location = new System.Drawing.Point(838, 169);
      this.label6.MaximumSize = new System.Drawing.Size(200, 20);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(195, 19);
      this.label6.TabIndex = 20;
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.Location = new System.Drawing.Point(12, 16);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(129, 17);
      this.radioButton1.TabIndex = 15;
      this.radioButton1.Text = "Radii 10  to 106 pixels";
      this.radioButton1.UseVisualStyleBackColor = true;
      this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.Location = new System.Drawing.Point(12, 39);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(100, 17);
      this.radioButton2.TabIndex = 16;
      this.radioButton2.Text = "Radii 70  to 360";
      this.radioButton2.UseVisualStyleBackColor = true;
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // radioButton3
      // 
      this.radioButton3.AutoSize = true;
      this.radioButton3.Location = new System.Drawing.Point(18, 23);
      this.radioButton3.Name = "radioButton3";
      this.radioButton3.Size = new System.Drawing.Size(71, 17);
      this.radioButton3.TabIndex = 17;
      this.radioButton3.Text = "automatic";
      this.radioButton3.UseVisualStyleBackColor = true;
      // 
      // radioButton4
      // 
      this.radioButton4.AutoSize = true;
      this.radioButton4.Checked = true;
      this.radioButton4.Location = new System.Drawing.Point(20, 46);
      this.radioButton4.Name = "radioButton4";
      this.radioButton4.Size = new System.Drawing.Size(60, 17);
      this.radioButton4.TabIndex = 18;
      this.radioButton4.TabStop = true;
      this.radioButton4.Text = "defined";
      this.radioButton4.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.radioButton1);
      this.groupBox1.Controls.Add(this.radioButton2);
      this.groupBox1.Location = new System.Drawing.Point(180, 30);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(158, 73);
      this.groupBox1.TabIndex = 22;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Areas of radius";
      this.groupBox1.Visible = false;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.radioButton3);
      this.groupBox2.Controls.Add(this.radioButton4);
      this.groupBox2.Location = new System.Drawing.Point(952, 30);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(130, 70);
      this.groupBox2.TabIndex = 23;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Determination of file";
      this.groupBox2.Visible = false;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label7.Location = new System.Drawing.Point(533, 169);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(46, 18);
      this.label7.TabIndex = 24;
      this.label7.Text = "label7";
      this.label7.Visible = false;
      // 
      // Form1
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.ClientSize = new System.Drawing.Size(1252, 773);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.numericUpDown2);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.pictureBox2);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.numericUpDown1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.NumericUpDown numericUpDown1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    public System.Windows.Forms.ProgressBar progressBar1;
    public System.Windows.Forms.PictureBox pictureBox1;
    public System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Label label2;
    public System.Windows.Forms.NumericUpDown numericUpDown2;
    private System.Windows.Forms.Button button5;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.RadioButton radioButton2;
    private System.Windows.Forms.RadioButton radioButton3;
    private System.Windows.Forms.RadioButton radioButton4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label7;
  }
}

