﻿using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public interface IAniDbMapper
    {
        Maybe<SeriesIds> GetMappedSeriesIds(int aniDbSeriesId);

        Task<MappedEpisodeResult>
            GetMappedTvDbEpisodeIdAsync(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber);
    }
}