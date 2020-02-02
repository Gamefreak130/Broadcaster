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
            "    <kStation value=\"{0}\">\n" +
            "      <!--Name of the station associated with this package. Do not change or bad things will happen.-->\n" +
            "    </kStation>\n" +
            "  </Current_Tuning>\n" +
            "</base>";

        private const string kMusicEntriesHeader =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
            "<MusicSelection>\n" +
            "  <Stereo>\n" +
            "    <Genre name=\"{0}\" localizedName=\"Gameplay/Excel/Stereo/Stations:{1}\">\n";

        private const string kMusicEntry = "      <Entry songkey=\"{0}\" title=\"{1}\" artist=\"{2}\" ToBeLocalized=\"0\" />\n";

        private const string kMusicEntriesFooter =
            "    </Genre>\n" +
            "  </Stereo>\n" +
            "</MusicSelection>";

        private static byte[] PreviewTuningHeader => new byte[]
        {
            0x00, 0x00, 0x00, 0x03, 0x5F, 0x63, 0x17, 0xD5, 0x03, 0xE8, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x01, 
            0x00, 0x00, 0x00, 0x10, 0x5E, 0x87, 0x99, 0x26, 0x70, 0xE5, 0x6A, 0x25, 0xFF, 0xFF, 0xFF, 0xFF, 
            0x00, 0x00, 0x00, 0x00, 0xBA, 0x03, 0xD0, 0xCD, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05,
            0x70, 0x1E, 0xD9, 0x1E, 0x03, 0xE8, 0x00, 0x9C, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10
        };

        private static byte[] TrackTuningFooter => new byte[]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00
        };
        
        internal static MemoryStream CreateMusicEntries(string station)
        {
            MemoryStream s = null;
            try
            {
                string header = string.Format(kMusicEntriesHeader, station, station == "Superhero" ? "Epic" 
                                                                            : station == "Western" ? "Spaghetti_Western" 
                                                                            : station);
                byte[] headerSequence = Encoding.ASCII.GetBytes(header);
                s = new MemoryStream();
                s.Write(headerSequence, 0, headerSequence.Length);
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

        internal static void WriteMusicEntry(MusicFile file, Stream musicEntries)
        {
            byte[] entry = Encoding.ASCII.GetBytes(string.Format(kMusicEntry, file.mDisplayName, file.mTitle, file.mArtist));
            musicEntries.Write(entry, 0, entry.Length);
        }

        internal static void FinalizeMusicEntries(Package package, string instanceName, Stream musicEntries)
        {
            byte[] footer = Encoding.ASCII.GetBytes(kMusicEntriesFooter);
            musicEntries.Write(footer, 0, footer.Length);
            TGIBlock tgi = new TGIBlock(0, null, 0x0333406C, 0, FNV64.GetHash("Music_Entries_" + instanceName));
            package.AddResource(tgi, musicEntries, true);
        }

        internal static FileStream AddSnr(MusicFile file, Package package)
        {
            FileStream s = null;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"lib\ealayer3.exe"),
                                                                  $"-E \"{file.mFullName}\" --single-block")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process proc = new Process())
                {
                    proc.StartInfo = startInfo;
                    proc.Start();
                    proc.WaitForExit();
                }
                TGIBlock tgi = new TGIBlock(0, null, 0x01A527DB, 0x001407EC, FNV64.GetHash(file.mDisplayName));
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
                ulong hashedInstance = FNV64.GetHash(file.mDisplayName);
                TGIBlock tgi = new TGIBlock(0, null, 0x8070223D, 0x001407EC, hashedInstance);
                s = new MemoryStream();
                s.Write(PreviewTuningHeader, 0, PreviewTuningHeader.Length);
                byte[] b = BitConverter.GetBytes(hashedInstance);
                s.Write(b, 0, b.Length);
                s.Write(TrackTuningFooter, 0, TrackTuningFooter.Length);
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

        internal static MemoryStream AddInstantiator(Package package, string instanceName, string station)
        {
            MemoryStream s = null;
            try
            {
                TGIBlock tgi = new TGIBlock(0, null, 0x0333406C, 0, FNV64.GetHash(instanceName));
                s = new MemoryStream(Encoding.ASCII.GetBytes(string.Format(kInstantiator, station)));
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
            System.Media.SystemSounds.Hand.Play();
            MessageBox.Show(message, "Technical Difficulties", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static string FixupStation(string station)
        {
            switch (station)
            {
                case "Chinese":
                    return "China";
                case "Dark_Wave":
                    return "DarkWave";
                case "Digitunes":
                    return "Future";
                case "Egyptian":
                    return "Egypt";
                case "Epic":
                    return "Superhero";
                case "French":
                    return "France";
                case "Geek_Rock":
                    return "GeekRock";
                case "Hip_Hop":
                    return "HipHop";
                case "R&B":
                    return "RnB";
                case "Spooky":
                    return "Horror";
                default:
                    return station;
            }
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
            mDisplayName = Path.GetFileNameWithoutExtension(displayName);
            using (TagLib.File file = TagLib.File.Create(mFullName))
            {
                mTitle = file.Tag.Title ?? mDisplayName;
                mArtist = file.Tag.Performers.Length > 0 ? string.Join(", ", file.Tag.Performers) : "-";
            }
        }

        public override string ToString()
        {
            return mDisplayName;
        }
    }
}
