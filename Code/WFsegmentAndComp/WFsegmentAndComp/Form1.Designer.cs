namespace WFsegmentAndComp
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
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.button5 = new System.Windows.Forms.Button();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.button6 = new System.Windows.Forms.Button();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.label2 = new System.Windows.Forms.Label();
      this.button8 = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.button1.Location = new System.Drawing.Point(63, 27);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 25);
      this.button1.TabIndex = 0;
      this.button1.Text = "Open image";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.button2.Location = new System.Drawing.Point(380, 27);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(120, 25);
      this.button2.TabIndex = 1;
      this.button2.Text = "Impulse noise";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Visible = false;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.button3.Location = new System.Drawing.Point(600, 27);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(120, 25);
      this.button3.TabIndex = 3;
      this.button3.Text = "Breadth First Search";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Visible = false;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.button4.Location = new System.Drawing.Point(228, 29);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(75, 23);
      this.button4.TabIndex = 2;
      this.button4.Text = "Segment";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Visible = false;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // button5
      // 
      this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.button5.Location = new System.Drawing.Point(954, 27);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(114, 25);
      this.button5.TabIndex = 4;
      this.button5.Text = "Root method";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Visible = false;
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(31, 167);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(1127, 15);
      this.progressBar1.TabIndex = 5;
      this.progressBar1.Visible = false;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(10, 250);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(600, 600);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 6;
      this.pictureBox1.TabStop = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.BackColor = System.Drawing.SystemColors.Control;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(148, 217);
      this.label1.MaximumSize = new System.Drawing.Size(400, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(333, 20);
      this.label1.TabIndex = 7;
      this.label1.Text = "                                                                                 " +
          "";
      this.label1.Visible = false;
      // 
      // menuStrip1
      // 
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(1227, 24);
      this.menuStrip1.TabIndex = 12;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.Location = new System.Drawing.Point(362, 86);
      this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(68, 20);
      this.numericUpDown1.TabIndex = 13;
      this.numericUpDown1.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numericUpDown1.Visible = false;
      // 
      // numericUpDown2
      // 
      this.numericUpDown2.Location = new System.Drawing.Point(459, 86);
      this.numericUpDown2.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.numericUpDown2.Name = "numericUpDown2";
      this.numericUpDown2.Size = new System.Drawing.Size(68, 20);
      this.numericUpDown2.TabIndex = 14;
      this.numericUpDown2.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numericUpDown2.Visible = false;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(367, 65);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(81, 16);
      this.label4.TabIndex = 15;
      this.label4.Text = "Delete. dark";
      this.label4.Visible = false;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(463, 65);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(75, 16);
      this.label5.TabIndex = 16;
      this.label5.Text = "Delete light";
      this.label5.Visible = false;
      // 
      // button6
      // 
      this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.button6.Location = new System.Drawing.Point(1086, 27);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(113, 25);
      this.button6.TabIndex = 17;
      this.button6.Text = "Save image of \'Root\'";
      this.button6.UseVisualStyleBackColor = true;
      this.button6.Visible = false;
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // pictureBox2
      // 
      this.pictureBox2.Location = new System.Drawing.Point(615, 250);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(600, 600);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox2.TabIndex = 18;
      this.pictureBox2.TabStop = false;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(829, 217);
      this.label2.MaximumSize = new System.Drawing.Size(400, 30);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(333, 20);
      this.label2.TabIndex = 19;
      this.label2.Text = "                                                                                 " +
          "";
      this.label2.Visible = false;
      // 
      // button8
      // 
      this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.button8.Location = new System.Drawing.Point(743, 27);
      this.button8.Name = "button8";
      this.button8.Size = new System.Drawing.Size(128, 25);
      this.button8.TabIndex = 22;
      this.button8.Text = "Save image of \'Breadth\'";
      this.button8.UseVisualStyleBackColor = true;
      this.button8.Visible = false;
      this.button8.Click += new System.EventHandler(this.button8_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(35, 139);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(101, 16);
      this.label3.TabIndex = 23;
      this.label3.Text = "Opened image:";
      this.label3.Visible = false;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label6.Location = new System.Drawing.Point(474, 217);
      this.label6.MaximumSize = new System.Drawing.Size(400, 30);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(333, 20);
      this.label6.TabIndex = 24;
      this.label6.Text = "                                                                                 " +
          "";
      this.label6.Visible = false;
      // 
      // Form1
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.ClientSize = new System.Drawing.Size(1227, 912);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.button8);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.pictureBox2);
      this.Controls.Add(this.button6);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.numericUpDown2);
      this.Controls.Add(this.numericUpDown1);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.menuStrip1);
      this.Controls.Add(this.label1);
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.Button button5;
    public System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.NumericUpDown numericUpDown1;
    private System.Windows.Forms.NumericUpDown numericUpDown2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Button button6;
    private System.Windows.Forms.PictureBox pictureBox2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button button8;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label6;
  }
}

