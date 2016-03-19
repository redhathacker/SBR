
namespace snmpd
{
     partial class MainForm
     {
          /// <summary>
          /// Designer variable used to keep track of non-visual components.
          /// </summary>
          private System.ComponentModel.IContainer components = null;

          /// <summary>
          /// Disposes resources used by the form.
          /// </summary>
          /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
          protected override void Dispose(bool disposing)
          {
               if (disposing)
               {
                    if (components != null)
                    {
                         components.Dispose();
                    }
               }
               base.Dispose(disposing);
          }

          /// <summary>
          /// This method is required for Windows Forms designer support.
          /// Do not change the method contents inside the source code editor. The Forms designer might
          /// not be able to load this method if it was changed manually.
          /// </summary>
          private void InitializeComponent()
          {
               this.components = new System.ComponentModel.Container();
               this.btnDiscover = new System.Windows.Forms.Button();
               this.txtOutput = new System.Windows.Forms.TextBox();
               this.txtToIP = new System.Windows.Forms.TextBox();
               this.label3 = new System.Windows.Forms.Label();
               this.chkSpecificIP = new System.Windows.Forms.CheckBox();
               this.txtSpecificIP = new System.Windows.Forms.TextBox();
               this.btnAllBoards = new System.Windows.Forms.Button();
               this.lblOnOff = new System.Windows.Forms.Label();
               this.btnRunInLoop = new System.Windows.Forms.Button();
               this.btnClearText = new System.Windows.Forms.Button();
               this.menuStrip1 = new System.Windows.Forms.MenuStrip();
               this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
               this.mnuFile_ButtonsForm = new System.Windows.Forms.ToolStripMenuItem();
               this.btnGetTpl1 = new System.Windows.Forms.Button();
               this.btnGet_SmTskLib = new System.Windows.Forms.Button();
               this.btnLightsOn = new System.Windows.Forms.Button();
               this.btnLightsOff = new System.Windows.Forms.Button();
               this.txtOID = new System.Windows.Forms.TextBox();
               this.label1 = new System.Windows.Forms.Label();
               this.btnReadChannels = new System.Windows.Forms.Button();
               this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
               this.btnGetTpl2 = new System.Windows.Forms.Button();
               this.panel1 = new System.Windows.Forms.Panel();
               this.btnBoardAsTask = new System.Windows.Forms.Button();
               this.btnInitTpl = new System.Windows.Forms.Button();
               this.btnInitGetSet = new System.Windows.Forms.Button();
               this.btnCompareGets = new System.Windows.Forms.Button();
               this.btnCancelAllTasks = new System.Windows.Forms.Button();
               this.btnDoGetAll = new System.Windows.Forms.Button();
               this.menuStrip1.SuspendLayout();
               this.panel1.SuspendLayout();
               this.SuspendLayout();
               // 
               // btnDiscover
               // 
               this.btnDiscover.Location = new System.Drawing.Point(3, 3);
               this.btnDiscover.Name = "btnDiscover";
               this.btnDiscover.Size = new System.Drawing.Size(100, 23);
               this.btnDiscover.TabIndex = 13;
               this.btnDiscover.Text = "Discover";
               this.btnDiscover.UseVisualStyleBackColor = true;
               this.btnDiscover.Click += new System.EventHandler(this.btnDiscover_Click);
               // 
               // txtOutput
               // 
               this.txtOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
               this.txtOutput.Location = new System.Drawing.Point(0, 224);
               this.txtOutput.Multiline = true;
               this.txtOutput.Name = "txtOutput";
               this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
               this.txtOutput.Size = new System.Drawing.Size(644, 229);
               this.txtOutput.TabIndex = 14;
               // 
               // txtToIP
               // 
               this.txtToIP.Location = new System.Drawing.Point(54, 26);
               this.txtToIP.Name = "txtToIP";
               this.txtToIP.Size = new System.Drawing.Size(100, 20);
               this.txtToIP.TabIndex = 15;
               this.txtToIP.Text = "192.168.1.40";
               // 
               // label3
               // 
               this.label3.Location = new System.Drawing.Point(1, 27);
               this.label3.Name = "label3";
               this.label3.Size = new System.Drawing.Size(47, 23);
               this.label3.TabIndex = 16;
               this.label3.Text = "IP Addr";
               this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
               // 
               // chkSpecificIP
               // 
               this.chkSpecificIP.AutoSize = true;
               this.chkSpecificIP.Location = new System.Drawing.Point(15, 32);
               this.chkSpecificIP.Name = "chkSpecificIP";
               this.chkSpecificIP.Size = new System.Drawing.Size(77, 17);
               this.chkSpecificIP.TabIndex = 19;
               this.chkSpecificIP.Text = "Specific IP";
               this.chkSpecificIP.UseVisualStyleBackColor = true;
               this.chkSpecificIP.CheckedChanged += new System.EventHandler(this.chkSpecificIP_CheckedChanged);
               // 
               // txtSpecificIP
               // 
               this.txtSpecificIP.Location = new System.Drawing.Point(3, 55);
               this.txtSpecificIP.Name = "txtSpecificIP";
               this.txtSpecificIP.Size = new System.Drawing.Size(100, 20);
               this.txtSpecificIP.TabIndex = 20;
               this.txtSpecificIP.Visible = false;
               // 
               // btnAllBoards
               // 
               this.btnAllBoards.BackColor = System.Drawing.Color.PaleGreen;
               this.btnAllBoards.Location = new System.Drawing.Point(14, 141);
               this.btnAllBoards.Name = "btnAllBoards";
               this.btnAllBoards.Size = new System.Drawing.Size(93, 23);
               this.btnAllBoards.TabIndex = 22;
               this.btnAllBoards.Text = "Start All Boards";
               this.btnAllBoards.UseVisualStyleBackColor = false;
               this.btnAllBoards.Click += new System.EventHandler(this.btnLoad_Click);
               // 
               // lblOnOff
               // 
               this.lblOnOff.BackColor = System.Drawing.Color.Red;
               this.lblOnOff.Location = new System.Drawing.Point(113, 143);
               this.lblOnOff.Name = "lblOnOff";
               this.lblOnOff.Size = new System.Drawing.Size(23, 18);
               this.lblOnOff.TabIndex = 24;
               this.lblOnOff.Text = "Off";
               this.lblOnOff.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
               // 
               // btnRunInLoop
               // 
               this.btnRunInLoop.Location = new System.Drawing.Point(519, 122);
               this.btnRunInLoop.Name = "btnRunInLoop";
               this.btnRunInLoop.Size = new System.Drawing.Size(111, 23);
               this.btnRunInLoop.TabIndex = 35;
               this.btnRunInLoop.Text = "Use Loop no timer";
               this.btnRunInLoop.UseVisualStyleBackColor = true;
               this.btnRunInLoop.Click += new System.EventHandler(this.btnRunInLoop_Click);
               // 
               // btnClearText
               // 
               this.btnClearText.Location = new System.Drawing.Point(604, 180);
               this.btnClearText.Name = "btnClearText";
               this.btnClearText.Size = new System.Drawing.Size(26, 20);
               this.btnClearText.TabIndex = 36;
               this.btnClearText.Text = "...";
               this.btnClearText.UseVisualStyleBackColor = true;
               this.btnClearText.Click += new System.EventHandler(this.btnClearText_Click);
               // 
               // menuStrip1
               // 
               this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
               this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
               this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
               this.menuStrip1.Location = new System.Drawing.Point(0, 0);
               this.menuStrip1.Name = "menuStrip1";
               this.menuStrip1.Size = new System.Drawing.Size(644, 23);
               this.menuStrip1.TabIndex = 37;
               this.menuStrip1.Text = "menuStrip1";
               // 
               // fileToolStripMenuItem
               // 
               this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile_ButtonsForm});
               this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
               this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 19);
               this.fileToolStripMenuItem.Text = "File";
               // 
               // mnuFile_ButtonsForm
               // 
               this.mnuFile_ButtonsForm.Name = "mnuFile_ButtonsForm";
               this.mnuFile_ButtonsForm.Size = new System.Drawing.Size(146, 22);
               this.mnuFile_ButtonsForm.Text = "Buttons Form";
               this.mnuFile_ButtonsForm.Click += new System.EventHandler(this.mnuFile_ButtonsForm_Click);
               // 
               // btnGetTpl1
               // 
               this.btnGetTpl1.BackColor = System.Drawing.Color.PaleGreen;
               this.btnGetTpl1.Location = new System.Drawing.Point(113, 82);
               this.btnGetTpl1.Name = "btnGetTpl1";
               this.btnGetTpl1.Size = new System.Drawing.Size(116, 23);
               this.btnGetTpl1.TabIndex = 38;
               this.btnGetTpl1.Text = "TPL Get/Set Async";
               this.toolTip1.SetToolTip(this.btnGetTpl1, "Get and then Set channels values using the TPL (Async)");
               this.btnGetTpl1.UseVisualStyleBackColor = false;
               this.btnGetTpl1.Click += new System.EventHandler(this.btnGetTpl1_Click);
               // 
               // btnGet_SmTskLib
               // 
               this.btnGet_SmTskLib.Location = new System.Drawing.Point(519, 151);
               this.btnGet_SmTskLib.Name = "btnGet_SmTskLib";
               this.btnGet_SmTskLib.Size = new System.Drawing.Size(110, 23);
               this.btnGet_SmTskLib.TabIndex = 39;
               this.btnGet_SmTskLib.Text = "Get SmTskLib";
               this.btnGet_SmTskLib.UseVisualStyleBackColor = true;
               // 
               // btnLightsOn
               // 
               this.btnLightsOn.BackColor = System.Drawing.Color.PaleGreen;
               this.btnLightsOn.Location = new System.Drawing.Point(14, 53);
               this.btnLightsOn.Name = "btnLightsOn";
               this.btnLightsOn.Size = new System.Drawing.Size(93, 23);
               this.btnLightsOn.TabIndex = 40;
               this.btnLightsOn.Text = "All On";
               this.btnLightsOn.UseVisualStyleBackColor = false;
               this.btnLightsOn.Click += new System.EventHandler(this.btnLightsOn_Async_Click);
               // 
               // btnLightsOff
               // 
               this.btnLightsOff.BackColor = System.Drawing.Color.PaleGreen;
               this.btnLightsOff.Location = new System.Drawing.Point(14, 82);
               this.btnLightsOff.Name = "btnLightsOff";
               this.btnLightsOff.Size = new System.Drawing.Size(93, 23);
               this.btnLightsOff.TabIndex = 41;
               this.btnLightsOff.Text = "All Off";
               this.btnLightsOff.UseVisualStyleBackColor = false;
               this.btnLightsOff.Click += new System.EventHandler(this.btnLightsOff_Async_Click);
               // 
               // txtOID
               // 
               this.txtOID.Location = new System.Drawing.Point(197, 27);
               this.txtOID.Name = "txtOID";
               this.txtOID.Size = new System.Drawing.Size(158, 20);
               this.txtOID.TabIndex = 43;
               this.txtOID.Text = "1.3.6.1.4.1.19865.1.2.1.1.0";
               // 
               // label1
               // 
               this.label1.Location = new System.Drawing.Point(160, 24);
               this.label1.Name = "label1";
               this.label1.Size = new System.Drawing.Size(31, 23);
               this.label1.TabIndex = 44;
               this.label1.Text = "OID";
               this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
               // 
               // toolTip1
               // 
               this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
               this.toolTip1.ToolTipTitle = "ToolTip beotch";
               // 
               // btnGetTpl2
               // 
               this.btnGetTpl2.BackColor = System.Drawing.Color.PaleGreen;
               this.btnGetTpl2.Location = new System.Drawing.Point(113, 111);
               this.btnGetTpl2.Name = "btnGetTpl2";
               this.btnGetTpl2.Size = new System.Drawing.Size(116, 23);
               this.btnGetTpl2.TabIndex = 50;
               this.btnGetTpl2.Text = "TPL Get/Set NonAsync";
               this.toolTip1.SetToolTip(this.btnGetTpl2, "Get and then Set channels values using the TPL (Non Async)");
               this.btnGetTpl2.UseVisualStyleBackColor = false;
               this.btnGetTpl2.Click += new System.EventHandler(this.btnGetTpl2_Click);
               // 
               // panel1
               // 
               this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
               this.panel1.Controls.Add(this.txtSpecificIP);
               this.panel1.Controls.Add(this.chkSpecificIP);
               this.panel1.Controls.Add(this.btnDiscover);
               this.panel1.Location = new System.Drawing.Point(521, 27);
               this.panel1.Name = "panel1";
               this.panel1.Size = new System.Drawing.Size(109, 89);
               this.panel1.TabIndex = 55;
               this.toolTip1.SetToolTip(this.panel1, "Discover all network devices that respond to an SNMP pings");
               // 
               // btnBoardAsTask
               // 
               this.btnBoardAsTask.BackColor = System.Drawing.Color.PaleGreen;
               this.btnBoardAsTask.Location = new System.Drawing.Point(14, 111);
               this.btnBoardAsTask.Name = "btnBoardAsTask";
               this.btnBoardAsTask.Size = new System.Drawing.Size(93, 23);
               this.btnBoardAsTask.TabIndex = 49;
               this.btnBoardAsTask.Text = "Board as Task";
               this.btnBoardAsTask.UseVisualStyleBackColor = false;
               this.btnBoardAsTask.Click += new System.EventHandler(this.btnBoardAsTask_Click);
               // 
               // btnInitTpl
               // 
               this.btnInitTpl.BackColor = System.Drawing.Color.PaleGreen;
               this.btnInitTpl.Location = new System.Drawing.Point(237, 54);
               this.btnInitTpl.Name = "btnInitTpl";
               this.btnInitTpl.Size = new System.Drawing.Size(93, 23);
               this.btnInitTpl.TabIndex = 51;
               this.btnInitTpl.Text = "Get Init - TPL";
               this.btnInitTpl.UseVisualStyleBackColor = false;
               this.btnInitTpl.Click += new System.EventHandler(this.btnInitTpl_Click);
               // 
               // btnInitGetSet
               // 
               this.btnInitGetSet.BackColor = System.Drawing.Color.PaleGreen;
               this.btnInitGetSet.Location = new System.Drawing.Point(237, 83);
               this.btnInitGetSet.Name = "btnInitGetSet";
               this.btnInitGetSet.Size = new System.Drawing.Size(93, 23);
               this.btnInitGetSet.TabIndex = 52;
               this.btnInitGetSet.Text = "Init Get/Set";
               this.btnInitGetSet.UseVisualStyleBackColor = false;
               this.btnInitGetSet.Click += new System.EventHandler(this.btnInitGetSet_Click);
               // 
               // btnCompareGets
               // 
               this.btnCompareGets.BackColor = System.Drawing.Color.PaleGreen;
               this.btnCompareGets.Location = new System.Drawing.Point(237, 112);
               this.btnCompareGets.Name = "btnCompareGets";
               this.btnCompareGets.Size = new System.Drawing.Size(93, 23);
               this.btnCompareGets.TabIndex = 53;
               this.btnCompareGets.Text = "Compare Gets";
               this.btnCompareGets.UseVisualStyleBackColor = false;
               this.btnCompareGets.Click += new System.EventHandler(this.btnCompareGets_Click);
               // 
               // btnCancelAllTasks
               // 
               this.btnCancelAllTasks.BackColor = System.Drawing.Color.PaleGreen;
               this.btnCancelAllTasks.Location = new System.Drawing.Point(336, 54);
               this.btnCancelAllTasks.Name = "btnCancelAllTasks";
               this.btnCancelAllTasks.Size = new System.Drawing.Size(136, 23);
               this.btnCancelAllTasks.TabIndex = 54;
               this.btnCancelAllTasks.Text = "Cancel All Tasks";
               this.btnCancelAllTasks.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
               this.btnCancelAllTasks.UseVisualStyleBackColor = false;
               this.btnCancelAllTasks.Click += new System.EventHandler(this.btnCancelAllTasks_Click);
               // 
               // btnDoGetAll
               // 
               this.btnDoGetAll.BackColor = System.Drawing.Color.PaleGreen;
               this.btnDoGetAll.Location = new System.Drawing.Point(237, 138);
               this.btnDoGetAll.Name = "btnDoGetAll";
               this.btnDoGetAll.Size = new System.Drawing.Size(93, 23);
               this.btnDoGetAll.TabIndex = 56;
               this.btnDoGetAll.Text = "Do GetAll";
               this.btnDoGetAll.UseVisualStyleBackColor = false;
               this.btnDoGetAll.Click += new System.EventHandler(this.btnDoGetAll_Click);
               // 
               // MainForm
               // 
               this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
               this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
               this.ClientSize = new System.Drawing.Size(644, 453);
               this.Controls.Add(this.btnDoGetAll);
               this.Controls.Add(this.panel1);
               this.Controls.Add(this.btnCancelAllTasks);
               this.Controls.Add(this.btnCompareGets);
               this.Controls.Add(this.btnInitGetSet);
               this.Controls.Add(this.btnInitTpl);
               this.Controls.Add(this.btnGetTpl2);
               this.Controls.Add(this.btnBoardAsTask);
               this.Controls.Add(this.btnReadChannels);
               this.Controls.Add(this.label1);
               this.Controls.Add(this.txtOID);
               this.Controls.Add(this.btnLightsOff);
               this.Controls.Add(this.btnLightsOn);
               this.Controls.Add(this.btnGet_SmTskLib);
               this.Controls.Add(this.btnGetTpl1);
               this.Controls.Add(this.btnClearText);
               this.Controls.Add(this.btnRunInLoop);
               this.Controls.Add(this.lblOnOff);
               this.Controls.Add(this.btnAllBoards);
               this.Controls.Add(this.label3);
               this.Controls.Add(this.txtToIP);
               this.Controls.Add(this.txtOutput);
               this.Controls.Add(this.menuStrip1);
               this.MainMenuStrip = this.menuStrip1;
               this.MaximizeBox = false;
               this.MinimizeBox = false;
               this.Name = "MainForm";
               this.Text = "Ricks SNMP Agent";
               this.Load += new System.EventHandler(this.MainForm_Load);
               this.menuStrip1.ResumeLayout(false);
               this.menuStrip1.PerformLayout();
               this.panel1.ResumeLayout(false);
               this.panel1.PerformLayout();
               this.ResumeLayout(false);
               this.PerformLayout();

          }
          private System.Windows.Forms.Button btnDiscover;
          private System.Windows.Forms.TextBox txtOutput;
          private System.Windows.Forms.TextBox txtToIP;
          private System.Windows.Forms.Label label3;
          private System.Windows.Forms.TextBox txtSpecificIP;
          private System.Windows.Forms.CheckBox chkSpecificIP;
          private System.Windows.Forms.Button btnAllBoards;
          private System.Windows.Forms.Label lblOnOff;
          private System.Windows.Forms.Button btnRunInLoop;
          private System.Windows.Forms.Button btnClearText;
          private System.Windows.Forms.MenuStrip menuStrip1;
          private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
          private System.Windows.Forms.ToolStripMenuItem mnuFile_ButtonsForm;
          private System.Windows.Forms.Button btnGetTpl1;
          private System.Windows.Forms.Button btnGet_SmTskLib;
          private System.Windows.Forms.Button btnLightsOn;
          private System.Windows.Forms.Button btnLightsOff;
          private System.Windows.Forms.TextBox txtOID;
          private System.Windows.Forms.Label label1;
          private System.Windows.Forms.Button btnReadChannels;
          private System.Windows.Forms.ToolTip toolTip1;
          private System.Windows.Forms.Button btnBoardAsTask;
          private System.Windows.Forms.Button btnGetTpl2;
          private System.Windows.Forms.Button btnInitTpl;
          private System.Windows.Forms.Button btnInitGetSet;
          private System.Windows.Forms.Button btnCompareGets;
          private System.Windows.Forms.Button btnCancelAllTasks;
          private System.Windows.Forms.Panel panel1;
          private System.Windows.Forms.Button btnDoGetAll;
     }
}
