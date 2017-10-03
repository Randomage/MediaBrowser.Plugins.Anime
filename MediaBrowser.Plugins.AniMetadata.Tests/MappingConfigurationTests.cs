﻿using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class MappingConfigurationTests
    {
        [Test]
        public void GetEpisodeMappings_AddsNullMappings()
        {
            var sourceMappingConfiguration = Substitute.For<ISourceMappingConfiguration>();

            var propertyMappingA =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(o => o.Summary, (s, v) => { },
                    "TestSource1");
            var propertyMappingB =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(o => o.Summary, (s, v) => { },
                    "TestSource2");

            sourceMappingConfiguration.GetEpisodeMappings().Returns(new[] { propertyMappingA, propertyMappingB });

            var mappingConfiguration = new MappingConfiguration(new[] { sourceMappingConfiguration });

            mappingConfiguration.GetEpisodeMappings()
                .ShouldBeEquivalentTo(new IPropertyMapping[]
                {
                    propertyMappingA,
                    propertyMappingB,
                    new NullMapping("Summary")
                });
        }

        [Test]
        public void GetSeasonMappings_AddsNullMappings()
        {
            var sourceMappingConfiguration = Substitute.For<ISourceMappingConfiguration>();

            var propertyMappingA =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(o => o.Summary, (s, v) => { },
                    "TestSource1");
            var propertyMappingB =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(o => o.Summary, (s, v) => { },
                    "TestSource2");

            sourceMappingConfiguration.GetSeasonMappings(1, true).Returns(new[] { propertyMappingA, propertyMappingB });

            var mappingConfiguration = new MappingConfiguration(new[] { sourceMappingConfiguration });

            mappingConfiguration.GetSeasonMappings(1, true)
                .ShouldBeEquivalentTo(new IPropertyMapping[]
                {
                    propertyMappingA,
                    propertyMappingB,
                    new NullMapping("Summary")
                });
        }

        [Test]
        public void GetSeriesMappings_AddsNullMappings()
        {
            var sourceMappingConfiguration = Substitute.For<ISourceMappingConfiguration>();

            var propertyMappingA =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(o => o.Summary, (s, v) => { },
                    "TestSource1");
            var propertyMappingB =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(o => o.Summary, (s, v) => { },
                    "TestSource2");

            sourceMappingConfiguration.GetSeriesMappings(1, true, false)
                .Returns(new[] { propertyMappingA, propertyMappingB });

            var mappingConfiguration = new MappingConfiguration(new[] { sourceMappingConfiguration });

            mappingConfiguration.GetSeriesMappings(1, true, false)
                .ShouldBeEquivalentTo(new IPropertyMapping[]
                {
                    propertyMappingA,
                    propertyMappingB,
                    new NullMapping("Summary")
                });
        }
    }
}