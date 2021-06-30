using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace TVMazeScraper.Models
{
    public class Show : TableEntity
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<Person> cast { get; set; }
        public int updated { get; set; }

    }
}