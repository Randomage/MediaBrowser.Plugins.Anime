﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class AniDbDataCache : IAniDbDataCache
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IAniDbFileCache _fileCache;
        private readonly IAniDbFileParser _fileParser;
        private readonly ISeiyuuCache _seiyuuCache;
        private readonly Lazy<IEnumerable<TitleListItem>> _titleListLazy;

        public AniDbDataCache(IApplicationPaths applicationPaths, IAniDbFileCache fileCache, IAniDbFileParser fileParser, ISeiyuuCache seiyuuCache)
        {
            _applicationPaths = applicationPaths;
            _fileCache = fileCache;
            _fileParser = fileParser;
            _seiyuuCache = seiyuuCache;

            _titleListLazy = new Lazy<IEnumerable<TitleListItem>>(() =>
            {
                var fileSpec = new TitlesFileSpec(_applicationPaths.CachePath);
                var titlesFile = _fileCache.GetFileAsync(fileSpec, CancellationToken.None).Result;

                return _fileParser.ParseTitleListXml(File.ReadAllText(titlesFile.FullName)).Titles;
            });
        }

        public IEnumerable<TitleListItem> TitleList => _titleListLazy.Value;

        public async Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken)
        {
            var fileSpec = new SeriesFileSpec(_applicationPaths.CachePath, aniDbSeriesId);

            var seriesFile = await _fileCache.GetFileAsync(fileSpec, cancellationToken);

            var series = _fileParser.ParseSeriesXml(File.ReadAllText(seriesFile.FullName));

            UpdateSeiyuuList(series);

            return series;
        }

        public IEnumerable<Seiyuu> GetSeiyuuList()
        {
            return _seiyuuCache.GetAll();
        }

        private void UpdateSeiyuuList(AniDbSeries aniDbSeries)
        {
            var seiyuu = aniDbSeries?.Characters?.Select(c => c.Seiyuu) ?? new List<Seiyuu>();

            _seiyuuCache.Add(seiyuu);
        }
    }
}