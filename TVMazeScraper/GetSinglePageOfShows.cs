using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TVMazeScraper
{
    public static class GetSinglePageOfShows
    {
        private static readonly HttpClient _http = new HttpClient();
        private static readonly string url = "http://api.tvmaze.com/shows?page="; //TODO: move to configuration

        [FunctionName("GetSinglePageOfShows")]
        public static async Task Run([QueueTrigger("tv-maze-show-pages-queue")] string pageIndex, [Queue("tv-maze-cast-queue")] IAsyncCollector<Show> queue, ILogger log)
        {
            var response = await _http.GetAsync($"{url}{pageIndex}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return;

            }
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var message = $"Too many requets, take a break...";
                log.LogDebug(message);
                throw new Exception(message);
            }
            response.EnsureSuccessStatusCode();

            Stream body = await response.Content.ReadAsStreamAsync();
            List<Show> pages = await JsonSerializer.DeserializeAsync<List<Show>>(body);

            foreach (var show in pages)
            {
                log.LogTrace($"Show with name:'{show.name}' and id: '{show.id}', has been added to the queue to resolve missing values.");
                await queue.AddAsync(show);
            }
            log.LogInformation($"Page '{pageIndex}' has succesfully been added to the queue.");
        }
    }
}
