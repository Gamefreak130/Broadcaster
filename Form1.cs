using s3pi.Package;
using s3pi.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

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
                    listBoxMusic.Items.Remove(current);
                    music.Remove(current);
                }
                ToggleButton();
            }
        }

        private void ListBoxMusic_MouseMoved(object sender, MouseEventArgs e)
        {
            int i = listBoxMusic.IndexFromPoint(e.Location);
            if (i > -1)
            {
                string msg = (listBoxMusic.Items[i] as MusicFile).FullName;
                if (musicToolTip.GetToolTip(listBoxMusic) != msg)
                {
                    musicToolTip.SetToolTip(listBoxMusic, (listBoxMusic.Items[i] as MusicFile).FullName);
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
            WritePackage();
        }
        
        private void ToggleButton()
        {
            btnGenerate.Enabled = !string.IsNullOrWhiteSpace(cmbStation.Text) && listBoxMusic.Items.Count != 0;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog();
        }

        private static void WritePackage()
        {
            throw new NotImplementedException();
            Package test = Package.NewPackage(0) as Package;
            TGIBlock tgi = new TGIBlock(0, null, 0x8070223D, 0, System.Security.Cryptography.FNV64.GetHash("TEST"));
            System.IO.FileStream s = System.IO.File.OpenRead(@"C:\test.audt");
            System.IO.FileStream newS = System.IO.File.Create(@"C:\test.package");
            test.AddResource(tgi, s, true);
            test.SaveAs(newS);
        }
    }
}
