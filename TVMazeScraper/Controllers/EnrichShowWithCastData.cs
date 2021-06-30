using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;
using TVMazeScraper.Models;
using TVMazeScraper.Interfaces;

namespace TVMazeScraper.Controllers
{
    public class EnrichShowWithCastData
    {
        private readonly ITvMazeApiClient _client;

        public EnrichShowWithCastData(ITvMazeApiClient client)
        {
            _client = client;
        }

        [FunctionName("EnrichShowWithCastData")]
        public async Task Run([QueueTrigger("tv-maze-cast-queue")] Show show, [Table("ShowsTable")] CloudTable showsTable, ILogger log)
        {
            List<Person> persons = await _client.GetCast(show);
            EnrichShowWithCastAndMetaData(show, persons);
            await SaveToTables(show, showsTable);
        }

        private static void EnrichShowWithCastAndMetaData(Show show, List<Person> persons)
        {

            show.cast = persons;
            show.PartitionKey = "Shows";
            show.RowKey = show.id.ToString();
        }

        private static async Task SaveToTables(Show show, CloudTable showsTable)
        {
            var insertOrReplace = TableOperation.InsertOrMerge(show);
            await showsTable.CreateIfNotExistsAsync();
            await showsTable.ExecuteAsync(insertOrReplace);
        }
    }
}
