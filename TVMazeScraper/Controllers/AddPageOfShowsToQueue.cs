using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TVMazeScraper.Models;
using TVMazeScraper.Interfaces;

namespace TVMazeScraper.Controllers
{
    public  class AddPageOfShowsToQueue
    {
        private readonly ITvMazeApiClient _client;

        public AddPageOfShowsToQueue(ITvMazeApiClient client)
        {
            _client = client;
        }
        [FunctionName("AddPageOfShowsToQueue")]
        public async Task Run([QueueTrigger("tv-maze-show-pages-queue")] string pageIndex, [Queue("tv-maze-cast-queue")] IAsyncCollector<Show> queue, ILogger log)
        {
            var shows = await _client.GetShows(pageIndex);
            await AddShowsToQueue(queue, log, shows);
            log.LogInformation($"Page '{pageIndex}' has succesfully been added to the queue.");
        }

        private static async Task AddShowsToQueue(IAsyncCollector<Show> queue, ILogger log, List<Show> shows)
        {
            foreach (var show in shows)
            {
                log.LogTrace($"Show with name:'{show.name}' and id: '{show.id}', has been added to the queue to resolve missing values.");
                await queue.AddAsync(show);
            }
        }
    }
}
