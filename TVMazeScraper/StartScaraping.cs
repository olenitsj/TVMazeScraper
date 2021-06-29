using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TVMazeScraper
{
    public static class StartScaraping
    {
        private static readonly HttpClient _http = new HttpClient();
        private static readonly int pageSize = 250;
        private static readonly string url = "http://api.tvmaze.com/updates/shows"; //TODO: move to configuration

        [FunctionName("StartScaraping")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, [Queue("tv-maze-show-pages-queue")] IAsyncCollector<string> queue,
            ILogger log)
        {
            int lastShowId = await GetLastShowId();
            int lastShowPageIndex = getLastPageIndex(lastShowId);

            for (int i = 0; i < lastShowPageIndex; i++)
            {
                await queue.AddAsync(i.ToString());
                log.LogInformation($"Added page '{i}' to the queue.");
            }
            return new OkObjectResult($"Added '{lastShowPageIndex}' pages to the queue.");
        }

        private static async Task<int> GetLastShowId()
        {
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            Stream body = await response.Content.ReadAsStreamAsync();
            Dictionary<string, int> allShowIndexes = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(body);
            var lastShowIndex = allShowIndexes.Keys.Select(int.Parse).Max();
            return lastShowIndex;
        }

        private static int getLastPageIndex(int lastShowIndex)
        {
            var maxPageSizeIndex = lastShowIndex / pageSize;
            if (lastShowIndex % pageSize != 0)
            {
                maxPageSizeIndex++;
            }

            return maxPageSizeIndex;
        }
    }
}
