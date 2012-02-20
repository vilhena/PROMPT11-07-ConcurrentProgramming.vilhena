using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace MoviesBroker.Services.Extensions
{
    public static class TaskExtensions
    {
        public static Task TranslateFieldForAll<T>(this IEnumerable<T> enumerable, Func<T,string> from, Action<T,string> to, string language)
        {
            return new Task(() => enumerable
                                      .AsParallel()
                                      .ForAll(a =>
                                                  {
                                                      var bingArtService = new BingAsync(from(a), language);

                                                      to(a,bingArtService.Result.SearchResponse.
                                                             Translation.
                                                             Results.FirstOrDefault().TranslatedTerm);
                                                  }));
        }

    }
}