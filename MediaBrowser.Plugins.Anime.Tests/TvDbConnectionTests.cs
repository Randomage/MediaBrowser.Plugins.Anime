﻿using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.Anime.Tests.TestHelpers;
using MediaBrowser.Plugins.Anime.TvDb;
using MediaBrowser.Plugins.Anime.TvDb.Data;
using MediaBrowser.Plugins.Anime.TvDb.Requests;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class TvDbConnectionTests
    {
        [Test]
        public async Task PostRequest_SuccessfulRequest_ReturnsResponse()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.Post(Arg.Is<HttpRequestOptions>(o => o.AcceptHeader == "application/json" &&
                    o.Url == "https://api.thetvdb.com/login" &&
                    o.RequestContent == "{\"apikey\": \"E32490FAD276FF5E\"}" &&
                    o.RequestContentType == "application/json"))
                .Returns(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream(
                        "{\"token\": \"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MDM4MjQwNTUsImlkIjoiTWVkaWFCcm93c2VyLlBsdWdpbnMuQW5pRGJGb3JUdkRiIiwib3JpZ19pYXQiOjE1MDM3Mzc2NTV9.jEVPlHoFFURb3lZU9Svis42YXwDN5GEI-LdZhhjFaRm26XV6DPahm68HTYmL9koMqlIwfGR5a-m4pULFok7B0OCiZPAQOOHlaNxqYEBleSG-saz_Bj3A3mq9ht8pj-xc7pMFb4mR2X6-zL6xoLO1A0h_r4oMAQCkCk8NApDdIdqyCi9nV0EeICfEU1AM84wVV0i-jxRDXaq3TLQynPeLhdefXx8sV0dye7cZo9bebfk18soE8lnc0QkBApv3RcqfoFKxyxAOTKOhHfMGZlB7NSG_duTWciiyFZXlIND6GP7zKScaes3fNu8tbpLAOiNQAyK-o-jq-5cI0y69zR2dBA\"}"),
                    StatusCode = HttpStatusCode.OK
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new LoginRequest("ApiKey");

            jsonSerialiser.Serialise(request.Data).Returns("{\"apikey\": \"E32490FAD276FF5E\"}");
            jsonSerialiser.Deserialise<LoginRequest.Response>(null)
                .ReturnsForAnyArgs(new LoginRequest.Response("Token"));

            var connection = new TvDbConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());

            var response = await connection.PostAsync(request, Maybe<string>.Nothing);

            response.ResultType().Should().Be(typeof(Response<LoginRequest.Response>));

            response.Match(
                r => r.Data.Token.Should().Be("Token"), 
                x => { });
        }

        [Test]
        public async Task PostRequest_FailedRequest_ReturnsStatusCodeAndResponseContent()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.Post(null)
                .ReturnsForAnyArgs(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream("{\"Error\": \"Not Authorized\"}"),
                    StatusCode = HttpStatusCode.Unauthorized
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new LoginRequest("ApiKey");

            jsonSerialiser.Serialise(request.Data).Returns("{\"apikey\": \"E32490FAD276FF5E\"}");

            var connection = new TvDbConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());

            var response = await connection.PostAsync(request, Maybe<string>.Nothing);

            response.ResultType().Should().Be(typeof(FailedRequest));

            response.Match(
                x => { }, 
                fr =>
                {
                    fr.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
                    fr.ResponseContent.Should().Be("{\"Error\": \"Not Authorized\"}");
                });
        }

        [Test]
        public async Task GetRequest_SuccessfulRequest_ReturnsResponse()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.GetResponse(Arg.Is<HttpRequestOptions>(o => o.AcceptHeader == "application/json" &&
                    o.Url == "https://api.thetvdb.com/series/122/episodes?page=1" &&
                    o.RequestContent == null &&
                    o.RequestContentType == null))
                .Returns(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream(
                        @"{
  ""data"": [
    {
      ""absoluteNumber"": 1,
      ""airedEpisodeNumber"": 2,
      ""airedSeason"": 3,
      ""dvdEpisodeNumber"": 4,
      ""dvdSeason"": 5,
      ""episodeName"": ""EpisodeName1"",
      ""firstAired"": ""01/01/2017"",
      ""id"": 6,
      ""lastUpdated"": 7,
      ""overview"": ""EpisodeOverview1""
    },
    {
      ""absoluteNumber"": 8,
      ""airedEpisodeNumber"": 9,
      ""airedSeason"": 10,
      ""dvdEpisodeNumber"": 11,
      ""dvdSeason"": 12,
      ""episodeName"": ""EpisodeName2"",
      ""firstAired"": ""01/01/2015"",
      ""id"": 13,
      ""lastUpdated"": 14,
      ""overview"": ""EpisodeOverview2""
    }
  ],
  ""errors"": {
    ""invalidFilters"": [
      ""string""
    ],
    ""invalidLanguage"": ""string"",
    ""invalidQueryParams"": [
      ""string""
    ]
  },
  ""links"": {
    ""first"": 1,
    ""last"": 2,
    ""next"": 3,
    ""previous"": 4
  }
}"),
                    StatusCode = HttpStatusCode.OK
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new GetEpisodesRequest(122, 1);

            jsonSerialiser.Deserialise<GetEpisodesRequest.Response>(null)
                .ReturnsForAnyArgs(new GetEpisodesRequest.Response(new []
                {
                    new TvDbEpisodeData(6, "EpisodeName1", 1, 2, 3, 7),
                    new TvDbEpisodeData(13, "EpisodeName2", 8, 9, 10, 17)
                }, new GetEpisodesRequest.PageLinks(1, 2, 3, 4)));

            var connection = new TvDbConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());

            var response = await connection.GetAsync(request, Maybe<string>.Nothing);

            response.ResultType().Should().Be(typeof(Response<GetEpisodesRequest.Response>));

            response.Match(
                r => r.Data.Data.Should().HaveCount(2),
                x => { });
        }

        [Test]
        public async Task GetRequest_FailedRequest_ReturnsStatusCodeAndResponseContent()
        {
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.GetResponse(Arg.Is<HttpRequestOptions>(o => o.AcceptHeader == "application/json" &&
                    o.Url == "https://api.thetvdb.com/series/122/episodes?page=1" &&
                    o.RequestContent == null &&
                    o.RequestContentType == null))
                 .ReturnsForAnyArgs(Task.FromResult(new HttpResponseInfo
                {
                    Content = Streams.ToStream("{\"Error\": \"Not Authorized\"}"),
                    StatusCode = HttpStatusCode.Unauthorized
                }));

            var jsonSerialiser = Substitute.For<ICustomJsonSerialiser>();

            var request = new GetEpisodesRequest(122, 1);
            
            var connection = new TvDbConnection(httpClient, jsonSerialiser, Substitute.For<ILogManager>());
            
            var response = await connection.GetAsync(request, Maybe<string>.Nothing);

            response.ResultType().Should().Be(typeof(FailedRequest));

            response.Match(
                x => { },
                fr =>
                {
                    fr.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
                    fr.ResponseContent.Should().Be("{\"Error\": \"Not Authorized\"}");
                });
        }
    }
}