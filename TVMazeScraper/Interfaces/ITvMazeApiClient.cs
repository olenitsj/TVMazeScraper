using System.Collections.Generic;
using System.Threading.Tasks;
using TVMazeScraper.Models;

namespace TVMazeScraper.Interfaces
{
    public interface ITvMazeApiClient
    {
        Task<int> GetHighestShowId();
        Task<List<Person>> GetCast(Show show);
        Task<List<Show>> GetShows(string pageIndex);
    }
}