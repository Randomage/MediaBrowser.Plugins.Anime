namespace MediaBrowser.Plugins.Anime.Mapping
{
    public class AniDbEpisode
    {
        public string Series { get; set; }

        /// <summary>
        ///     Either 1 for regular episodes, or 0 for specials
        /// </summary>
        public int Season { get; set; }

        public int Index { get; set; }
    }
}