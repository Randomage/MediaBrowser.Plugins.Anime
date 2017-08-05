﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class AniDbFileCache
    {
        private readonly FileDownloader _fileDownloader;

        public AniDbFileCache(FileDownloader fileDownloader)
        {
            _fileDownloader = fileDownloader;
        }

        public async Task<FileInfo> GetFileAsync(AniDbFileSpec fileSpec, CancellationToken cancellationToken)
        {
            var cacheFile = new FileInfo(fileSpec.DestinationFilePath);

            if (!IsRefreshRequired(cacheFile))
            {
                return cacheFile;
            }

            CreateDirectoryIfNotExists(cacheFile.DirectoryName);

            ClearCacheFilesFromDirectory(cacheFile.DirectoryName);

            await DownloadFileAsync(fileSpec, cancellationToken);

            return cacheFile;
        }

        private async Task DownloadFileAsync(AniDbFileSpec fileSpec, CancellationToken cancellationToken)
        {
            await _fileDownloader.DownloadFileAsync(fileSpec.Url, fileSpec.DestinationFilePath, cancellationToken);
        }

        private void CreateDirectoryIfNotExists(string directoryPath)
        {
            var titlesDirectoryExists = Directory.Exists(directoryPath);
            if (!titlesDirectoryExists)
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void ClearCacheFilesFromDirectory(string directoryPath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories))
                    File.Delete(file);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        private bool IsRefreshRequired(FileInfo cacheFile)
        {
            return !cacheFile.Exists ||
                cacheFile.LastWriteTime < DateTime.Now.AddDays(-7);
        }
    }
}