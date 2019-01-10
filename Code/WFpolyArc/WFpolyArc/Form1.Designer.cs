namespace WFpolyArc
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
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.button5 = new System.Windows.Forms.Button();
      this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
      this.label8 = new System.Windows.Forms.Label();
      this.numericUpDown8 = new System.Windows.Forms.NumericUpDown();
      this.button8 = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown8)).BeginInit();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(50, 34);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "Open image";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.Location = new System.Drawing.Point(323, 76);
      this.numericUpDown1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(58, 20);
      this.numericUpDown1.TabIndex = 1;
      this.numericUpDown1.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
      this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(285, 55);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(143, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Threshold for edge detection";
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(547, 24);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 3;
      this.button2.Text = "Polygons";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(784, 24);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(75, 23);
      this.button3.TabIndex = 4;
      this.button3.Text = "Arcs";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(20, 180);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(600, 600);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 5;
      this.pictureBox1.TabStop = false;
      // 
      // pictureBox2
      // 
      this.pictureBox2.Location = new System.Drawing.Point(640, 180);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(600, 600);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox2.TabIndex = 6;
      this.pictureBox2.TabStop = false;
      this.pictureBox2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox2_MouseClick);
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(20, 139);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(1195, 13);
      this.progressBar1.TabIndex = 7;
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(1015, 24);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(75, 23);
      this.button5.TabIndex = 11;
      this.button5.Text = "Save";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(531, 55);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(118, 13);
      this.label8.TabIndex = 25;
      this.label8.Text = "Approximation precision";
      // 
      // numericUpDown8
      // 
      this.numericUpDown8.DecimalPlaces = 2;
      this.numericUpDown8.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.numericUpDown8.Location = new System.Drawing.Point(564, 75);
      this.numericUpDown8.Name = "numericUpDown8";
      this.numericUpDown8.Size = new System.Drawing.Size(54, 20);
      this.numericUpDown8.TabIndex = 26;
      this.numericUpDown8.Value = new decimal(new int[] {
            18,
            0,
            0,
            65536});
      this.numericUpDown8.ValueChanged += new System.EventHandler(this.numericUpDown8_ValueChanged);
      // 
      // button8
      // 
      this.button8.Location = new System.Drawing.Point(300, 21);
      this.button8.Name = "button8";
      this.button8.Size = new System.Drawing.Size(107, 23);
      this.button8.TabIndex = 27;
      this.button8.Text = "Detect edges";
      this.button8.UseVisualStyleBackColor = true;
      this.button8.Click += new System.EventHandler(this.button8_Click);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(20, 109);
      this.label2.MaximumSize = new System.Drawing.Size(600, 30);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(448, 18);
      this.label2.TabIndex = 28;
      this.label2.Text = "Opened image: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      this.label2.Visible = false;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(300, 158);
      this.label3.MaximumSize = new System.Drawing.Size(500, 30);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(108, 18);
      this.label3.TabIndex = 29;
      this.label3.Text = "Opened image:";
      this.label3.Visible = false;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(900, 158);
      this.label4.MaximumSize = new System.Drawing.Size(500, 30);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(108, 18);
      this.label4.TabIndex = 30;
      this.label4.Text = "Opened image:";
      this.label4.Visible = false;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(587, 159);
      this.label5.MaximumSize = new System.Drawing.Size(500, 30);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(132, 18);
      this.label5.TabIndex = 31;
      this.label5.Text = "Click Detect edges";
      this.label5.Visible = false;
      // 
      // Form1
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.ClientSize = new System.Drawing.Size(1272, 997);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.button8);
      this.Controls.Add(this.numericUpDown8);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.pictureBox2);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.numericUpDown1);
      this.Controls.Add(this.button1);
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown8)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.NumericUpDown numericUpDown1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    public System.Windows.Forms.PictureBox pictureBox1;
    public System.Windows.Forms.PictureBox pictureBox2;
    public System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Button button5;
    private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    private System.Windows.Forms.Label label8;
    public System.Windows.Forms.NumericUpDown numericUpDown8;
    private System.Windows.Forms.Button button8;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
  }
}

