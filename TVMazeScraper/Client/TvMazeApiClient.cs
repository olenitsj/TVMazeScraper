using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TVMazeScraper.Models;
using TVMazeScraper.Client;
using TVMazeScraper.Interfaces;

namespace TVMazeScraper.Client
{
    public class TvMazeApiClient : ITvMazeApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TvMazeApiClient> _log;

        public TvMazeApiClient(HttpClient client, ILogger<TvMazeApiClient> log)
        {
            _http = client;
            _log = log;
        }

        private void VerifyTooManyRequestsAndThrow(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var message = $"Too many requets, take a break...";
                _log.LogDebug(message);
                throw new Exception(message);
            }
        }

        public async Task<List<Show>> GetShows(string pageIndex)
        {
            var response = await _http.GetAsync($"http://api.tvmaze.com/shows?page={pageIndex}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Show>();

            }
            VerifyTooManyRequestsAndThrow(response);
            response.EnsureSuccessStatusCode();
            Stream body = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<List<Show>>(body);
        }


        public async Task<int> GetHighestShowId()
        {
            var response = await _http.GetAsync("http://api.tvmaze.com/updates/shows");
            VerifyTooManyRequestsAndThrow(response);
            response.EnsureSuccessStatusCode();
            Stream body = await response.Content.ReadAsStreamAsync();
            Dictionary<string, int> allShowIndexes = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(body);
            var lastShowIndex = allShowIndexes.Keys.Select(int.Parse).Max();
            return lastShowIndex;
        }

        public async Task<List<Person>> GetCast(Show show)
        {
            var response = await _http.GetAsync($"http://api.tvmaze.com/shows/{show.id}/cast");
            VerifyTooManyRequestsAndThrow(response);
            response.EnsureSuccessStatusCode();
            Stream body = await response.Content.ReadAsStreamAsync();
            var personsAndCharacters = await JsonSerializer.DeserializeAsync<List<Cast>>(body);
            var persons = personsAndCharacters.Select(c => c.person).OrderByDescending(e => e.birthday).ToList(); // TODO: verify orderBy alphabatically is correct for the birthday date structure. yyyy-MM-dd?
            return persons;
        }
    }
}
