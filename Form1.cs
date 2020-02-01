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
            WritePackage(); //TODO Close dialog and indicate working
            for (int i = listBoxMusic.Items.Count - 1; i >= 0; i--)
            {
                RemoveTrack((MusicFile)listBoxMusic.Items[i]);
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
        
        private void RemoveTrack(MusicFile track)
        {
            listBoxMusic.Items.Remove(track);
            music.Remove(track);
        }

        private void WritePackage()
        {
            List<FileStream> files = new List<FileStream>();
            List<Stream> resources = new List<Stream>();
            try
            {
                //TODO Add Station Tuning
                Package package = Package.NewPackage(0) as Package;
                foreach (MusicFile track in music)
                {
                    FileStream file = Helpers.AddSnr(track, package);
                    files.Add(file);

                    Stream tuning = Helpers.AddPreviewTuning(track, package);
                    resources.Add(tuning);
                }
                ulong hashedName = System.Security.Cryptography.FNV64.GetHash(cmbStation.Text);
                /*Stream s = Helpers.AddAssembly(package, hashedName);
                resources.Add(s);*/
                Stream s = Helpers.AddInstantiator(package, hashedName);
                resources.Add(s);
                //TGIBlock tgi = new TGIBlock(0, null, 0x8070223D, 0, System.Security.Cryptography.FNV64.GetHash("TEST"));
                package.SaveAs(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                //TEST
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
