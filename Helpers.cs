using s3pi.Interfaces;
using s3pi.Package;
using System;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace Gamefreak130.Broadcaster
{
    [Serializable]
    public class DuplicateFileException : Exception
    {
    }

    internal static class Helpers
    {
        private const string kInstantiator =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
            "<base>\n" +
            "  <Current_Tuning>\n" +
            "    <kCJackB value=\"True\">\n" +
            "      <!--True: Enables Crackerjack Bonanza mode. False: Triggers the apocalypse. Maybe.-->\n" +
            "    </kCJackB>\n" +
            "  </Current_Tuning>\n" +
            "</base>";
        internal static FileStream AddSnr(MusicFile file, Package package)
        {
            FileStream s = null;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"lib\ealayer3.exe"), 
                                                                  string.Format("-E \"{0}\" --single-block", file.mFullName))
                {
                    CreateNoWindow = true
                };
                using (Process proc = new Process())
                {
                    proc.StartInfo = startInfo;
                    proc.Start();
                    proc.WaitForExit();
                }
                TGIBlock tgi = new TGIBlock(0, null, 0x01A527DB, 0x001407EC, FNV64.GetHash(Path.GetFileNameWithoutExtension(file.mDisplayName)));
                string snrName = Path.ChangeExtension(file.mFullName, ".ealayer3");
                s = File.OpenRead(snrName);
                if (package.AddResource(tgi, s, true) == null)
                {
                    //TEST
                    throw new DuplicateFileException();
                }
                return s;
            }
            catch
            {
                if (s != null)
                {
                    string path = s.Name;
                    s.Close();
                    File.Delete(path);
                }
                throw;
            }
        }

        internal static MemoryStream AddPreviewTuning(MusicFile file, Package package)
        {
            MemoryStream s = null;
            try
            {
                TGIBlock tgi = new TGIBlock(0, null, 0x8070223D, 0x001407EC, FNV64.GetHash(Path.GetFileNameWithoutExtension(file.mDisplayName)));
                s = new MemoryStream();
                //TODO
                //s.Write();
                if (package.AddResource(tgi, s, true) == null)
                {
                    //TEST
                    throw new DuplicateFileException();
                }
                return s;
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                throw;
            }
        }

        internal static FileStream AddAssembly(Package package, string name)
        {
            FileStream s = null;
            try
            {
                TGIBlock tgi = new TGIBlock(0, null, 0x073FAA07, 0, FNV64.GetHash(name));
                //TODO 
                //s = File.OpenRead();
                package.AddResource(tgi, s, true);
                return s;
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                throw;
            }
        }

        internal static MemoryStream AddInstantiator(Package package, string name)
        {
            MemoryStream s = null;
            try
            {
                TGIBlock tgi = new TGIBlock(0, null, 0x0333406C, 0, FNV64.GetHash(name));
                s = new MemoryStream(Encoding.ASCII.GetBytes(kInstantiator));
                package.AddResource(tgi, s, true);
                return s;
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                throw;
            }
        }

        internal static void ShowError(string message)
        {
            MessageBox.Show(message, "Technical Difficulties", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal class MusicFile
    {
        internal readonly string mFullName;

        internal readonly string mDisplayName;

        internal readonly string mTitle;

        internal readonly string mArtist;

        internal MusicFile(string fullName, string displayName)
        {
            mFullName = fullName;
            mDisplayName = displayName;
            //TODO grab metadata
        }

        public override string ToString()
        {
            return mDisplayName;
        }
    }
}
