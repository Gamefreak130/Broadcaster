﻿using s3pi.Package;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace Gamefreak130.Broadcaster
{
    public partial class BroadcasterMain : Form
    {
        private readonly List<MusicFile> music = new List<MusicFile>();

        private static readonly ToolTip musicToolTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            ShowAlways = true
        };

        private delegate void ControlAccessor();

        public BroadcasterMain()
        {
            // Auto-generated form initialization
            InitializeComponent();
            // Manual form initializers
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Electronic Arts\The Sims 3\Mods\Packages";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }
        
        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstMusic.SelectedItems.Count != 0)
            {
                object[] selectedObjects = new object[lstMusic.SelectedItems.Count];
                lstMusic.SelectedItems.CopyTo(selectedObjects, 0);
                foreach (MusicFile current in selectedObjects)
                {
                    RemoveTrack(current);
                }
                ToggleButton();
            }
        }

        private void ListBoxMusic_MouseMoved(object sender, MouseEventArgs e)
        {
            int i = lstMusic.IndexFromPoint(e.Location);
            if (i > -1)
            {
                string msg = (lstMusic.Items[i] as MusicFile).mFullName;
                if (musicToolTip.GetToolTip(lstMusic) != msg)
                {
                    musicToolTip.SetToolTip(lstMusic, (lstMusic.Items[i] as MusicFile).mFullName);
                }
            }
            else
            {
                musicToolTip.RemoveAll();
            }
        }

        private void CmbStation_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void CmbStation_TextChanged(object sender, EventArgs e)
        {
            ToggleButton();
        }

        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < openFileDialog.FileNames.Length; i++)
            {
                MusicFile track = new MusicFile(openFileDialog.FileNames[i], openFileDialog.SafeFileNames[i]);
                music.Add(track);
                lstMusic.Items.Add(track);
            }
            ToggleButton();
        }
        
        private void SaveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            ToggleWorkingStatus();
            btnGenerate.Text = "Broadcasting...";
            backgroundWorker.RunWorkerAsync();
        }

        private void ToggleWorkingStatus()
        {
            foreach (Control control in Controls)
            {
                control.Enabled = !control.Enabled;
            }
        }

        private void ToggleButton()
        {
            btnGenerate.Enabled = !string.IsNullOrWhiteSpace(cboStation.Text) && lstMusic.Items.Count != 0;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog();
        }

        private void Cleanup(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                System.Media.SystemSounds.Exclamation.Play();
                MessageBox.Show(string.Format("Broadcast to {0} successful.", Path.GetFileName(saveFileDialog.FileName)), "On Air", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ToggleWorkingStatus();
            btnGenerate.Text = "   Broadcast";
            for (int i = lstMusic.Items.Count - 1; i >= 0; i--)
            {
                RemoveTrack((MusicFile)lstMusic.Items[i]);
            }
            cboStation.Text = string.Empty;
            chkSlowDance.Checked = false;
            chkWorkout.Checked = false;
        }
        
        private void RemoveTrack(MusicFile track)
        {
            lstMusic.Items.Remove(track);
            music.Remove(track);
        }

        private void Broadcast(object sender, DoWorkEventArgs e)
        {
            WritePackage();
        }

        private void WritePackage()
        {
            List<FileStream> files = new List<FileStream>();
            List<IDisposable> resources = new List<IDisposable>();
            try
            {
                Package package = Package.NewPackage(0) as Package;
                Random random = new Random();
                string instanceName = "";
                for (int i = 0; i < 17; i++)
                {
                    instanceName += (char)random.Next(48, 58);
                }
                string station = "";
                cboStation.Invoke(new ControlAccessor(delegate () 
                { 
                    station = cboStation.Text.Trim().Replace(' ', '_');
                    if (!cboStation.Items.Contains(cboStation.Text))
                    {
                        IDisposable[] streams = Helpers.AddStbl(package, instanceName, station);
                        resources.AddRange(streams);
                    }
                }));
                station = Helpers.FixupStation(station);
                //CONSIDER translatable string table?
                MemoryStream musicEntries = Helpers.CreateMusicEntries(station);
                resources.Add(musicEntries);
                MemoryStream[] stationTuning = Helpers.CreateStationTuning(music.Count);
                resources.AddRange(stationTuning);

                foreach (MusicFile track in music)
                {
                    FileStream file = Helpers.AddSnr(track, package);
                    files.Add(file);

                    Stream tuning = Helpers.AddPreviewTuning(track.mDisplayName, package);
                    resources.Add(tuning);

                    Helpers.WriteMusicEntry(track, musicEntries);

                    Helpers.WriteStationTrack(track.mDisplayName, stationTuning);
                    //Stations require at least two tracks to function properly
                    //So if there is only one track, it is added to the AUDT twice
                    if (music.Count == 1)
                    {
                        Helpers.WriteStationTrack(track.mDisplayName, stationTuning);
                    }
                }
                
                Helpers.FinalizeMusicEntries(package, instanceName, musicEntries);
                Helpers.FinalizeStationTuning(package, station, stationTuning);
                IDisposable res = Helpers.AddAssembly(package, instanceName);
                resources.Add(res);
                res = Helpers.AddBootstrap(package);
                resources.Add(res);
                res = Helpers.AddInstantiator(package, instanceName, station, chkWorkout.Checked, chkSlowDance.Checked);
                resources.Add(res);
                package.SaveAs(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                string error = ex is IOException
                        ? "A file and/or memory access error occurred.\n" +
                          "Make sure no other programs are currently accessing the MP3 files or any other Broadcaster resources."
                    : ex is FileNotFoundException
                        ? "An error occurred while converting the specified MP3 files to a game-readable format.\n" +
                          "Make sure that the files are valid."
                    : ex is UnauthorizedAccessException
                        ? "A file access error occurred.\n" +
                          "Make sure that you have the proper permissions to access the specified files."
                    : ex is DuplicateFileException
                        ? "A duplicate resource error occurred.\n" +
                          "Make sure that all the specified MP3 files have a unique name."
                        : "Congratulations!\n" +
                          "You have broken Broadcaster in a new and unexpected way.\n\n" +
                          "Please report to the developer with the following information:\n" + 
                          ex.ToString();
                Helpers.ShowError(error);
                throw;
            }
            finally
            {
                foreach (FileStream s in files)
                {
                    string path = s.Name;
                    s.Close();
                    File.Delete(path);
                }
                foreach (IDisposable s in resources)
                {
                    s.Dispose();
                }
            }
        }
    }
}
