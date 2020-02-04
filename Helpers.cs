using s3pi.Interfaces;
using s3pi.Package;
using System;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Linq;

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

        private static byte[] StblHeader => new byte[]
        {
            0x53, 0x54, 0x42, 0x4C, 0x02, 0x00, 0x00, 0x02, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private static byte[] AudioTuningHeader => new byte[]
        {
            0xBA, 0x03, 0xD0, 0xCD, 0x00, 0x0A, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x05, 0x5F, 0x63, 
            0x17, 0xD5, 0x03, 0xE8, 0x00, 0x1C, 0x00, 
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10
        };

        private static byte[] SampleTuningHeader => new byte[]
        {
            0x70, 0x1E, 0xD9, 0x1E, 0x03, 0xE8, 0x00, 0x9C, 
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10
        };

        private static byte[] InstanceTuningFooter => new byte[]
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00
        };

        private static byte[] StationTuningFooter => new byte[]
        {
            0x9E, 0x3C, 0xE3, 0x31, 0x00, 0x01, 0x00, 0x00, 0x01,
            0xBA, 0x13, 0x41, 0x92, 0x00, 0x01, 0x00, 0x00, 0x01
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

        internal static MemoryStream[] CreateStationTuning(int fileCount)
        {
            MemoryStream[] streams = new MemoryStream[4];
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    streams[i] = new MemoryStream();
                    //Header, codec block, and start of parent block
                    int numBlocks = (i == 1 || i == 3) ? 5 : 4;
                    streams[i].Write(BitConverter.GetBytes(numBlocks).Reverse().ToArray(), 0, 4);
                    streams[i].Write(AudioTuningHeader, 0, AudioTuningHeader.Length);
                    //Parent tgi
                    byte[] b = null;
                    switch (i)
                    {
                        case 0:
                            b = new byte[] { 0x51, 0xD1, 0x81, 0xC0, 0x33, 0xC3, 0xFF, 0xE2 };
                            break;
                        case 1:
                            b = new byte[] { 0x6D, 0x77, 0x65, 0xBF, 0xCD, 0x1C, 0x9D, 0x64 };
                            break;
                        case 2:
                            b = new byte[] { 0xDD, 0xE1, 0x94, 0x24, 0x7C, 0xD0, 0xE0, 0xFF };
                            break;
                        case 3:
                            b = new byte[] { 0x91, 0x88, 0x1D, 0xA9, 0xA1, 0x8A, 0x4C, 0x27 };
                            break;
                    }
                    streams[i].Write(b, 0, b.Length);
                    streams[i].Write(InstanceTuningFooter, 0, InstanceTuningFooter.Length);
                    //Start of samples block
                    streams[i].Write(SampleTuningHeader, 0, 8);
                    streams[i].Write(BitConverter.GetBytes(fileCount).Reverse().ToArray(), 0, 4);
                    streams[i].Write(SampleTuningHeader, 12, 4);
                }
                return streams;
            }
            catch
            {
                foreach (MemoryStream s in streams)
                {
                    if (s != null)
                    {
                        s.Close();
                    }
                }
                throw;
            }
        }

        internal static void WriteStationTrack(string trackTitle, MemoryStream[] streams)
        {
            ulong hashedInstance = FNV64.GetHash(trackTitle);
            foreach (MemoryStream current in streams)
            {
                byte[] b = BitConverter.GetBytes(hashedInstance);
                current.Write(b, 0, b.Length);
                current.Write(InstanceTuningFooter, 0, InstanceTuningFooter.Length);
            }
        }

        internal static void FinalizeStationTuning(Package package, string station, MemoryStream[] streams)
        {
            for (int i = 0; i < 4; i++)
            {
                string name = "Stereo_" + ((i == 2 || i == 3) ? "Wired_" : "");
                name += station == "Island_Life" ? "IslandLife"
                      : station == "Beach_Party" ? "BeachParty"
                      : station;
                name += (i == 1 || i == 3) ? "_Virtual" : "";
                //2654790449 block, with IsVirtual block if needed
                streams[i].Write(StationTuningFooter, 0, (i == 1 || i == 3) ? 18 : 9);
                TGIBlock tgi = new TGIBlock(0, null, 0x8070223D, 0x001407EC, FNV64.GetHash(name));
                package.AddResource(tgi, streams[i], true);
            }
        }

        internal static Stream[] AddStbl(Package package, string instanceName, string station)
        {
            MemoryStream[] streams = new MemoryStream[23];
            try
            {
                byte[] instance = BitConverter.GetBytes(FNV64.GetHash("Strings_" + instanceName));
                for (byte i = 0x00; i < 0x17; i++)
                {
                    instance[7] = i;
                    TGIBlock tgi = new TGIBlock(0, null, 0x220557DA, 0, BitConverter.ToUInt64(instance, 0));
                    streams[i] = new MemoryStream();
                    streams[i].Write(StblHeader, 0, StblHeader.Length);
                    byte[] hashedName = Encoding.UTF8.GetBytes(station.Replace('_', ' ')).Reverse().ToArray();
                    //TODO Refactor
                    //Write preview menu string 
                    byte[] key = BitConverter.GetBytes(FNV64.GetHash($"Gameplay/Excel/Stereo/Stations:{station}"));
                    streams[i].Write(key, 0, key.Length);
                    streams[i].Write(BitConverter.GetBytes(station.Length), 0, 4);
                    streams[i].Write(hashedName, 0, hashedName.Length);
                    //Write pie menu string 
                    /*key = 
                    streams[i].Write();
                    streams[i].Write(BitConverter.GetBytes(station.Length), 0, 4);
                    streams[i].Write(hashedName, 0, hashedName.Length);*/
                    package.AddResource(tgi, streams[i], true);
                }
                return streams;
            }
            catch
            {
                foreach (MemoryStream s in streams)
                {
                    if (s != null)
                    {
                        s.Close();
                    }
                }
                throw;
            }
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

        internal static MemoryStream AddPreviewTuning(string trackTitle, Package package)
        {
            MemoryStream s = null;
            try
            {
                ulong hashedInstance = FNV64.GetHash(trackTitle);
                TGIBlock tgi = new TGIBlock(0, null, 0x8070223D, 0x001407EC, hashedInstance);
                s = new MemoryStream();
                //Header, codec block, and start of parent block
                s.Write(BitConverter.GetBytes(3).Reverse().ToArray(), 0, 4);
                s.Write(AudioTuningHeader, 0, AudioTuningHeader.Length);
                //Parent tgi
                s.Write(new byte[] { 0x5E, 0x87, 0x99, 0x26, 0x70, 0xE5, 0x6A, 0x25 }, 0, 8);
                s.Write(InstanceTuningFooter, 0, InstanceTuningFooter.Length);
                //Start of samples block
                s.Write(SampleTuningHeader, 0, SampleTuningHeader.Length);

                byte[] b = BitConverter.GetBytes(hashedInstance);
                s.Write(b, 0, b.Length);
                s.Write(InstanceTuningFooter, 0, InstanceTuningFooter.Length);
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
