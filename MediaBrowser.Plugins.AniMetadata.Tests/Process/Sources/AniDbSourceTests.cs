﻿using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process.Sources
{
    [TestFixture]
    public class AniDbSourceTests
    {
        [SetUp]
        public virtual void Setup()
        {
            AniDbClient = Substitute.For<IAniDbClient>();
            TvDbClient = Substitute.For<ITvDbClient>();
            EpisodeMatcher = Substitute.For<IEpisodeMatcher>();
            Configuration = Substitute.For<ITitlePreferenceConfiguration>();
            TitleSelector = Substitute.For<ITitleSelector>();

            Configuration.TitlePreference.Returns(TitleType.Localized);

            AniDbSource = new AniDbSource(AniDbClient, TvDbClient, EpisodeMatcher, Configuration, TitleSelector, Substitute.For<ISources>(), Substitute.For<IAnimeMappingListFactory>(), Substitute.For<IDataMapperFactory>());
        }

        internal IAniDbClient AniDbClient;
        internal ITvDbClient TvDbClient;
        internal IEpisodeMatcher EpisodeMatcher;
        internal ITitlePreferenceConfiguration Configuration;
        internal ITitleSelector TitleSelector;
        internal AniDbSource AniDbSource;

        internal static class Data
        {
            public static EmbyItemData EmbyItemData(string name)
            {
                return new EmbyItemData(MediaItemTypes.Series, new ItemIdentifier(Option<int>.None, Option<int>.None, name),
                    null, "en", Enumerable.Empty<EmbyItemId>());
            }
        }

        [TestFixture]
        public class LookupFromEmbyData : AniDbSourceTests
        {
            [TestFixture]
            public class Series : LookupFromEmbyData
            {
                [Test]
                public async Task CreatesSourceData()
                {
                    var titles = new[] { new ItemTitleData() };
                    AniDbClient.FindSeriesAsync("SeriesName")
                        .Returns(new AniDbSeriesData
                        {
                            Id = 332,
                            Titles = titles
                        });

                    TitleSelector.SelectTitle(titles, TitleType.Localized, "en")
                        .Returns(new ItemTitleData
                        {
                            Title = "FoundSeriesName"
                        });

                    var result =
                        (await AniDbSource.LookupFromEmbyData(Data.EmbyItemData("SeriesName"))).ValueUnsafe();

                    result.Source.Should().Be(AniDbSource);
                    result.Id.ValueUnsafe().Should().Be(332);
                    result.Identifier.Name.Should().Be("FoundSeriesName");
                    result.Identifier.ParentIndex.IsNone.Should().BeTrue();
                    result.Identifier.Index.IsNone.Should().BeTrue();
                }

                [Test]
                public async Task FindsSeriesByName()
                {
                    await AniDbSource.LookupFromEmbyData(Data.EmbyItemData("SeriesName"));

                    AniDbClient.Received(1).FindSeriesAsync("SeriesName");
                }

                [Test]
                public async Task NoSeriesFound_ReturnsFailedResult()
                {
                    var result = await AniDbSource.LookupFromEmbyData(Data.EmbyItemData("SeriesName"));

                    result.IsLeft.Should().BeTrue();
                }

                [Test]
                public async Task NoTitle_ReturnsFailedResult()
                {
                    AniDbClient.FindSeriesAsync("SeriesName").Returns(new AniDbSeriesData());

                    var result = await AniDbSource.LookupFromEmbyData(Data.EmbyItemData("SeriesName"));

                    result.IsLeft.Should().BeTrue();
                }

                [Test]
                public async Task SelectsTitle()
                {
                    var titles = new[] { new ItemTitleData() };
                    AniDbClient.FindSeriesAsync("SeriesName")
                        .Returns(new AniDbSeriesData
                        {
                            Titles = titles
                        });

                    await AniDbSource.LookupFromEmbyData(Data.EmbyItemData("SeriesName"));

                    TitleSelector.Received(1).SelectTitle(titles, TitleType.Localized, "en");
                }
            }
        }
    }
}