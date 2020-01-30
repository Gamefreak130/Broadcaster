using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gamefreak130.Broadcaster
{
    internal class MusicFile
    {
        internal string FullName { get; }

        internal string DisplayName { get; }

        internal MusicFile(string fullName, string displayName)
        {
            FullName = fullName;
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
