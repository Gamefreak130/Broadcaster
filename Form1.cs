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
                foreach (object current in selectedObjects)
                {
                    listBoxMusic.Items.Remove(current);
                }
                ToggleButton();
            }
        }

        private void CmbStation_KeyPress(object sender, KeyPressEventArgs e)
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
            foreach (string file in openFileDialog.FileNames)
            {
                listBoxMusic.Items.Add(file);
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

        private void WritePackage()
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
