﻿using System;

namespace Gamefreak130.Broadcaster
{
    partial class BroadcasterMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BroadcasterMain));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.lstMusic = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.cboStation = new System.Windows.Forms.ComboBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.lblInstructions = new System.Windows.Forms.Label();
            this.chkWorkout = new System.Windows.Forms.CheckBox();
            this.chkSlowDance = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "MP3 files|*.mp3";
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog_FileOk);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Sims 3 Package|*.package";
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveFileDialog_FileOk);
            // 
            // lstMusic
            // 
            this.lstMusic.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstMusic.FormattingEnabled = true;
            this.lstMusic.ItemHeight = 18;
            this.lstMusic.Location = new System.Drawing.Point(119, 14);
            this.lstMusic.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstMusic.Name = "lstMusic";
            this.lstMusic.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstMusic.Size = new System.Drawing.Size(365, 130);
            this.lstMusic.TabIndex = 0;
            this.lstMusic.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ListBoxMusic_MouseMoved);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(130, 153);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(159, 42);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add Music...";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(313, 153);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(159, 42);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "Remove Selected";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Broadcast);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.Cleanup);
            // 
            // cboStation
            // 
            this.cboStation.Font = new System.Drawing.Font("Trebuchet MS", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboStation.FormattingEnabled = true;
            this.cboStation.Items.AddRange(new object[] {
            "Beach Party",
            "Chinese",
            "Classical",
            "Country",
            "Dark Wave",
            "Digitunes",
            "Disco",
            "Egyptian",
            "Electronica",
            "Epic",
            "French",
            "Geek Rock",
            "Hip Hop",
            "Indie",
            "Island Life",
            "Kids",
            "Latin",
            "Pop",
            "R&B",
            "Rap",
            "Rock",
            "Rockabilly",
            "Roots",
            "Songwriter",
            "Soul",
            "Spooky",
            "Western"});
            this.cboStation.Location = new System.Drawing.Point(119, 261);
            this.cboStation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboStation.Name = "cboStation";
            this.cboStation.Size = new System.Drawing.Size(365, 26);
            this.cboStation.Sorted = true;
            this.cboStation.TabIndex = 3;
            this.cboStation.TextChanged += new System.EventHandler(this.CmbStation_TextChanged);
            this.cboStation.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CmbStation_KeyPressed);
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.Transparent;
            this.btnGenerate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnGenerate.Enabled = false;
            this.btnGenerate.Font = new System.Drawing.Font("Trebuchet MS", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerate.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnGenerate.ImageKey = "Icon_sound_radio.jpg";
            this.btnGenerate.ImageList = this.imageList;
            this.btnGenerate.Location = new System.Drawing.Point(129, 358);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(343, 68);
            this.btnGenerate.TabIndex = 4;
            this.btnGenerate.Text = "   Broadcast";
            this.btnGenerate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGenerate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.BtnGenerate_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.White;
            this.imageList.Images.SetKeyName(0, "Icon_sound_radio.jpg");
            // 
            // lblInstructions
            // 
            this.lblInstructions.AutoSize = true;
            this.lblInstructions.Location = new System.Drawing.Point(36, 217);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(557, 22);
            this.lblInstructions.TabIndex = 5;
            this.lblInstructions.Text = "Select an existing station to add music to, or type in a new station to create";
            // 
            // chkWorkout
            // 
            this.chkWorkout.AutoSize = true;
            this.chkWorkout.Location = new System.Drawing.Point(121, 311);
            this.chkWorkout.Name = "chkWorkout";
            this.chkWorkout.Size = new System.Drawing.Size(159, 26);
            this.chkWorkout.TabIndex = 6;
            this.chkWorkout.Text = "Is Workout Station";
            this.chkWorkout.UseVisualStyleBackColor = true;
            // 
            // chkSlowDance
            // 
            this.chkSlowDance.AutoSize = true;
            this.chkSlowDance.Location = new System.Drawing.Point(309, 311);
            this.chkSlowDance.Name = "chkSlowDance";
            this.chkSlowDance.Size = new System.Drawing.Size(181, 26);
            this.chkSlowDance.TabIndex = 7;
            this.chkSlowDance.Text = "Is Slow Dance Station";
            this.chkSlowDance.UseVisualStyleBackColor = true;
            // 
            // BroadcasterMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.PaleGreen;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.chkSlowDance);
            this.Controls.Add(this.chkWorkout);
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.cboStation);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstMusic);
            this.Font = new System.Drawing.Font("Trebuchet MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "BroadcasterMain";
            this.Text = "Broadcaster";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ListBox lstMusic;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.ComboBox cboStation;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.CheckBox chkWorkout;
        private System.Windows.Forms.CheckBox chkSlowDance;
    }
}

