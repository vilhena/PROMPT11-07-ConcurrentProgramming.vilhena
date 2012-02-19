﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc.Async;
using MoviesBroker.Models;
using MoviesBroker.Models.ExternalModels;

namespace MoviesBroker.Services
{
    public class MovieInfoAsync
    {
        private readonly AsyncManager _manager;
        
        public MovieInfoAsync(AsyncManager manager)
        {
            _manager = manager;
        }

        public IEnumerable<Task> GetMovieInfo(string title, int? year, string language)
        {
            _manager.OutstandingOperations.Increment();

            var response = new MovieResponse {title = title, year = year, language = language};

            var imdbService = new ImdbAsync(title, year);
            yield return imdbService.Task;

            var imdbObj = imdbService.Result;
            var bingService = new BingAsync(imdbObj.Plot, language);
            yield return bingService.Task;

            var flickrService = new FlickrAsync(title, imdbObj.Director);
            yield return flickrService.Task;

            
            var nytService = new NYTAsync(title, imdbObj.Year);
            yield return nytService.Task;


            var nytObj = nytService.Result;

            //foreach (var nytArticle in nytObj.Results.Where(a => !String.IsNullOrEmpty(a.Capsule_Review)))
            //{
            //    var bingArtService = new BingAsync(nytArticle.Capsule_Review, language);
            //    yield return bingArtService.Task;

            //    nytArticle.Capsule_Review =
            //        bingArtService.Result.SearchResponse.
            //            Translation.
            //            Results.FirstOrDefault().TranslatedTerm;
            //}

            var fillArticlesTask = new Task(() => nytObj.Results
                                                      .Where(a => !String.IsNullOrEmpty(a.Capsule_Review))
                                                      .AsParallel()
                                                      .ForAll(a =>
                                                                  {
                                                                      var bingArtService = new BingAsync(
                                                                          a.Capsule_Review, language);

                                                                      a.Capsule_Review =
                                                                          bingArtService.Result.SearchResponse.
                                                                              Translation.
                                                                              Results.FirstOrDefault().TranslatedTerm;
                                                                  }));
            yield return fillArticlesTask;


            var bingObj = bingService.Result;
            var flickrObj = flickrService.Result;

            response.status_code = HttpStatusCode.OK;
            response.message = "OK";
            response.movie = new Movie
            {
                title = imdbObj.Title,
                year = imdbObj.Year,
                director = imdbObj.Director,
                synopsis =
                    bingObj.SearchResponse.Translation.Results.FirstOrDefault().TranslatedTerm,
                poster = new Uri(imdbObj.Poster),
                photos = flickrObj.Photos
                    .Photo
                    .Select(p => new Uri(
                                     string.Format(
                                         "http://farm{0}.static.flickr.com/{1}/{2}_{3}.jpg",
                                         1, p.Server, p.Id, p.Secret))).ToArray(),
                critics = nytObj.Results.Select(a => new Critic()
                {
                    author = a.Seo_Name,
                    capsule_review = a.Capsule_Review,
                    reference = new Uri(a.Link.Url)
                }).ToArray()
            };

            _manager.Parameters["movie"] = response;
            _manager.OutstandingOperations.Decrement();

            yield break;
        }
    }
}