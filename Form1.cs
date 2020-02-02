using s3pi.Package;
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

        private delegate string StringAccessor();

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
            if (listBoxMusic.SelectedItems.Count != 0)
            {
                object[] selectedObjects = new object[listBoxMusic.SelectedItems.Count];
                listBoxMusic.SelectedItems.CopyTo(selectedObjects, 0);
                foreach (MusicFile current in selectedObjects)
                {
                    RemoveTrack(current);
                }
                ToggleButton();
            }
        }

        private void ListBoxMusic_MouseMoved(object sender, MouseEventArgs e)
        {
            int i = listBoxMusic.IndexFromPoint(e.Location);
            if (i > -1)
            {
                string msg = (listBoxMusic.Items[i] as MusicFile).mFullName;
                if (musicToolTip.GetToolTip(listBoxMusic) != msg)
                {
                    musicToolTip.SetToolTip(listBoxMusic, (listBoxMusic.Items[i] as MusicFile).mFullName);
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
                listBoxMusic.Items.Add(track);
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
            btnGenerate.Enabled = !string.IsNullOrWhiteSpace(cmbStation.Text) && listBoxMusic.Items.Count != 0;
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
            for (int i = listBoxMusic.Items.Count - 1; i >= 0; i--)
            {
                RemoveTrack((MusicFile)listBoxMusic.Items[i]);
            }
            cmbStation.Text = string.Empty;
        }
        
        private void RemoveTrack(MusicFile track)
        {
            listBoxMusic.Items.Remove(track);
            music.Remove(track);
        }

        private void Broadcast(object sender, DoWorkEventArgs e)
        {
            WritePackage();
        }

        private void WritePackage()
        {
            List<FileStream> files = new List<FileStream>();
            List<Stream> resources = new List<Stream>();
            try
            {
                string station = (string)cmbStation.Invoke(new StringAccessor(delegate() { return cmbStation.Text.Replace(' ', '_'); }));
                station = Helpers.FixupStation(station);
                //TODO Add Station Tuning (audt)
                //CONSIDER Add NMAP?
                //Don't forget islandlife and beachparty
                Package package = Package.NewPackage(0) as Package;
                MemoryStream musicEntries = Helpers.CreateMusicEntries(station);
                resources.Add(musicEntries);
                foreach (MusicFile track in music)
                {
                    FileStream file = Helpers.AddSnr(track, package);
                    files.Add(file);

                    Stream tuning = Helpers.AddPreviewTuning(track, package);
                    resources.Add(tuning);

                    Helpers.WriteMusicEntry(track, musicEntries);
                }
                Random random = new Random();
                string instanceName = "";
                for (int i = 0; i < 10; i++)
                {
                    instanceName += (char)random.Next(48, 58);
                }
                Helpers.FinalizeMusicEntries(package, instanceName, musicEntries);
                /*Stream s = Helpers.AddAssembly(package, randomName);
                resources.Add(s);*/
                Stream s = Helpers.AddInstantiator(package, instanceName, station);
                resources.Add(s);
                //TGIBlock tgi = new TGIBlock(0, null, 0x8070223D, 0, System.Security.Cryptography.FNV64.GetHash("TEST"));
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
                foreach (Stream s in resources)
                {
                    s.Close();
                }
            }
        }
    }
}
