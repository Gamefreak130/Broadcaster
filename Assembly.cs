﻿using Sims3.Gameplay.Objects.Electronics;
using Sims3.SimIFace;
using System;
using System.Xml;
using static Sims3.UI.OptionsDialog;

namespace Gamefreak130.Common
{
    public delegate T GenericDelegate<T>();

    public class RepeatingFunctionTask : Task
    {
        private StopWatch mTimer;

        private readonly int mDelay;

        private readonly GenericDelegate<bool> mFunction;

        public RepeatingFunctionTask(GenericDelegate<bool> function)
        {
            mFunction = function;
            mDelay = 500;
        }

        public RepeatingFunctionTask(GenericDelegate<bool> function, int delay)
        {
            mFunction = function;
            mDelay = delay;
        }

        public override void Dispose()
        {
            if (mTimer != null)
            {
                mTimer.Dispose();
                mTimer = null;
            }
            if (ObjectId != ObjectGuid.InvalidObjectGuid)
            {
                Simulator.DestroyObject(ObjectId);
                ObjectId = ObjectGuid.InvalidObjectGuid;
            }
            base.Dispose();
        }

        public override void Simulate()
        {
            mTimer = StopWatch.Create(StopWatch.TickStyles.Milliseconds);
            mTimer.Start();
            do
            {
                mTimer.Restart();
                while (mTimer != null && mTimer.GetElapsedTime() < mDelay)
                {
                    if (Simulator.CheckYieldingContext(false))
                    {
                        Simulator.Sleep(0u);
                    }
                }
                if (!mFunction())
                {
                    Dispose();
                    break;
                }
                if (Simulator.CheckYieldingContext(false))
                {
                    Simulator.Sleep(0u);
                }
            }
            while (mTimer != null);
        }
    }
}

namespace Gamefreak130.Broadcaster
{
    internal enum GameStations : uint
    {
        Electronica = 1,
        Pop = 1,
        Latin = 1,
        Indie = 1,
        Classical = 1,
        Kids = 1,
        France = 2,
        China = 2,
        Egypt = 2,
        Roots = 8,
        Soul = 8,
        Rockabilly = 16,
        HipHop = 32,
        Country = 512,
        RnB = 512,
        Songwriter = 512,
        DarkWave = 16384,
        Disco = 65536,
        Rap = 65536,
        Rock = 65536,
        GeekRock = 131072,
        Beach_Party = 262144,
        Island_Life = 262144,
        Future = 1048576,
        Horror = 524288,
        Superhero = 524288,
        Western = 524288
    }

    public static class Bootstrap
    {
        [Tunable]
        private static readonly bool kCJackB;

        private static string mStation = "";

        private static bool mOptionsInjectionHandled;

        static Bootstrap()
        {
            World.OnStartupAppEventHandler += OnStartupApp;
        }

        private static void OnStartupApp(object sender, EventArgs e)
        {
            if (Simulator.LoadXML(typeof(Bootstrap).Assembly.GetName().Name) is XmlDocument xmlDocument)
            {
                if (xmlDocument.GetElementsByTagName("Broadcaster")[0] is XmlElement xmlElement && xmlElement.GetElementsByTagName("Station")[0] is XmlElement stationElement)
                {
                    mStation = stationElement.GetAttribute("value");
                }
            }
            AddStationIfNeeded();
            Simulator.AddObject(new Common.RepeatingFunctionTask(new Common.GenericDelegate<bool>(InjectOptions)));
        }

        private static void AddStationIfNeeded()
        {
            ProductVersion version = ProductVersion.Undefined;
            string translationKey = mStation;
            string playlistKey = mStation;
            if (Enum.IsDefined(typeof(GameStations), mStation))
            {
                version = (ProductVersion)Enum.Parse(typeof(GameStations), mStation, true);
                switch (mStation)
                {
                    case "Beach_Party":
                    case "Island_Life":
                        translationKey = translationKey.Replace("_", "");
                        playlistKey = playlistKey.Replace("_", "");
                        break;
                    case "Future":
                        translationKey += "World";
                        break;
                    case "Superhero":
                        translationKey = "Epic";
                        break;
                    case "Western":
                        translationKey = "Spaghetti_" + translationKey;
                        break;
                }
            }
            string text = "Gameplay/Excel/Stereo/Stations:" + translationKey;
            if (!GameUtils.IsInstalled(version) && !StereoStationData.sStereoStationDictionary.ContainsKey(text))
            {
                StereoStationData data = new StereoStationData(text, $"Stereo_{playlistKey}", $"Stereo_Wired_{playlistKey}",
                                                               $"Stereo_{playlistKey}_Virtual", $"Stereo_Wired_{playlistKey}_Virtual",
                                                               Sims3.SimIFace.CAS.FavoriteMusicType.Custom, false, false, false,
                                                               WorldName.Undefined, ProductVersion.BaseGame);
                StereoStationData.sStereoStationDictionary.Add(text, data);
            }
        }

        private static bool InjectOptions()
        {
            try
            {
                if (sDialog != null)
                {
                    if (!mOptionsInjectionHandled)
                    {
                        string name = typeof(Bootstrap).Assembly.GetName().Name;
                        if (Simulator.LoadXML("Music_Entries_" + name) is XmlDocument xml && xml.GetElementsByTagName("MusicSelection")[0] is XmlElement xmlElement)
                        {
                            sDialog.LoadSongData(xmlElement, "Stereo", 0);
                            if (sDialog.mItemGridGenreButtons.Count > 0)
                            {
                                sDialog.mItemGridGenreButtons.SelectedItem = 0;
                                string text = sDialog.mItemGridGenreButtons.InternalGrid.CellTags[0, 0] as string;
                                if (!string.IsNullOrEmpty(text))
                                {
                                    sDialog.mCurrentGenre = text;
                                    sDialog.UpdateTable();
                                }
                            }
                        }
                        mOptionsInjectionHandled = true;
                    }
                }
                else
                {
                    mOptionsInjectionHandled = false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}