using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TVMazeScraper
{
    public static class GetSingleCastAndSave
    {
        private static readonly HttpClient _http = new HttpClient();

        [FunctionName("GetSingleCastAndSave")]
        public static async Task Run([QueueTrigger("tv-maze-cast-queue")] Show show, [Table("ShowsTable")] CloudTable showsTable, ILogger log)
        {
            var response = await _http.GetAsync($"http://api.tvmaze.com/shows/{show.id}/cast");
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var message = $"Too many requets, take a break...";
                log.LogDebug(message);
                throw new Exception(message);
            }
            response.EnsureSuccessStatusCode();

            Stream body = await response.Content.ReadAsStreamAsync();
            var personsAndCharacters = await JsonSerializer.DeserializeAsync<List<Cast>>(body);
            var cast = personsAndCharacters.Select(c => c.person).OrderByDescending(e => e.birthday).ToList(); // TODO: verify orderBy alphabatically is correct for the birthday date structure. yyyy-MM-dd?
            show.cast = cast;
            show.PartitionKey = "Shows";
            show.RowKey = show.id.ToString();

            var insertOrReplace = TableOperation.InsertOrMerge(show);
            await showsTable.CreateIfNotExistsAsync();
            await showsTable.ExecuteAsync(insertOrReplace);
        }
    }
}
