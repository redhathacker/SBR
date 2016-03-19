namespace snmpd
{
     partial class PowerSnmpAgent
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
               this.label1 = new System.Windows.Forms.Label();
               this.button3 = new System.Windows.Forms.Button();
               this.button4 = new System.Windows.Forms.Button();
               this.label2 = new System.Windows.Forms.Label();
               this.SuspendLayout();
               // 
               // button1
               // 
               this.button1.Location = new System.Drawing.Point(12, 163);
               this.button1.Name = "button1";
               this.button1.Size = new System.Drawing.Size(91, 39);
               this.button1.TabIndex = 0;
               this.button1.Text = "Get w NodeName ";
               this.button1.UseVisualStyleBackColor = true;
               this.button1.Click += new System.EventHandler(this.button1_Click);
               // 
               // button2
               // 
               this.button2.Location = new System.Drawing.Point(12, 208);
               this.button2.Name = "button2";
               this.button2.Size = new System.Drawing.Size(91, 34);
               this.button2.TabIndex = 1;
               this.button2.Text = "Get w OID";
               this.button2.UseVisualStyleBackColor = true;
               this.button2.Click += new System.EventHandler(this.button2_Click);
               // 
               // label1
               // 
               this.label1.Location = new System.Drawing.Point(12, 40);
               this.label1.Name = "label1";
               this.label1.Size = new System.Drawing.Size(91, 44);
               this.label1.TabIndex = 2;
               this.label1.Text = "label1";
               // 
               // button3
               // 
               this.button3.Location = new System.Drawing.Point(169, 208);
               this.button3.Name = "button3";
               this.button3.Size = new System.Drawing.Size(91, 34);
               this.button3.TabIndex = 4;
               this.button3.Text = "Set w OID";
               this.button3.UseVisualStyleBackColor = true;
               this.button3.Click += new System.EventHandler(this.button3_Click);
               // 
               // button4
               // 
               this.button4.Location = new System.Drawing.Point(169, 163);
               this.button4.Name = "button4";
               this.button4.Size = new System.Drawing.Size(91, 39);
               this.button4.TabIndex = 3;
               this.button4.Text = "Set w NodeName";
               this.button4.UseVisualStyleBackColor = true;
               this.button4.Click += new System.EventHandler(this.button4_Click);
               // 
               // label2
               // 
               this.label2.Location = new System.Drawing.Point(166, 40);
               this.label2.Name = "label2";
               this.label2.Size = new System.Drawing.Size(91, 44);
               this.label2.TabIndex = 5;
               this.label2.Text = "label2";
               // 
               // PowerSnmpAgent
               // 
               this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
               this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
               this.ClientSize = new System.Drawing.Size(284, 262);
               this.Controls.Add(this.label2);
               this.Controls.Add(this.button3);
               this.Controls.Add(this.button4);
               this.Controls.Add(this.label1);
               this.Controls.Add(this.button2);
               this.Controls.Add(this.button1);
               this.Name = "PowerSnmpAgent";
               this.Text = "PowerSnmpAgent";
               this.Load += new System.EventHandler(this.PowerSnmpAgent_Load);
               this.ResumeLayout(false);

          }

          #endregion

          private System.Windows.Forms.Button button1;
          private System.Windows.Forms.Button button2;
          private System.Windows.Forms.Label label1;
          private System.Windows.Forms.Button button3;
          private System.Windows.Forms.Button button4;
          private System.Windows.Forms.Label label2;
     }
}