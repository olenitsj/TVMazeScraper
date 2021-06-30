using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TVMazeScraper.Interfaces;

namespace TVMazeScraper.Controllers
{
    public class StartScaraping
    {
        private readonly ITvMazeApiClient _client;

        public StartScaraping(ITvMazeApiClient client)
        {
            _client = client;
        }
        private readonly int pageSize = 250;

        [FunctionName("StartScaraping")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, [Queue("tv-maze-show-pages-queue")] IAsyncCollector<string> queue,
            ILogger log)
        {
            int lastShowId = await _client.GetHighestShowId();
            int lastShowPageIndex = getLastPageIndex(lastShowId);
            await PopulateQueueWithPageIndexes(queue, log, lastShowPageIndex);
            return new OkObjectResult($"Added '{lastShowPageIndex}' pages to the queue.");
        }

        private async Task PopulateQueueWithPageIndexes(IAsyncCollector<string> queue, ILogger log, int lastShowPageIndex)
        {
            for (int i = 0; i < lastShowPageIndex; i++)
            {
                await queue.AddAsync(i.ToString());
                log.LogInformation($"Added page '{i}' to the queue.");
            }
        }

        private int getLastPageIndex(int lastShowIndex)
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
