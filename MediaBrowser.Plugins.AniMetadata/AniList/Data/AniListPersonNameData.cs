﻿namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListPersonNameData
    {
        public AniListPersonNameData(string first, string last, string native)
        {
            First = first;
            Last = last;
            Native = native;
        }

        public string First { get; }

        public string Last { get; }

        public string Native { get; }
    }
}