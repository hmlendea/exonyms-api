using System.Collections.Generic;

namespace ExonymsAPI.Service.Models
{
    public class Location(string defaultName)
    {
        public string DefaultName { get; set; } = defaultName;

        public IDictionary<string, Name> Names { get; set; } = new Dictionary<string, Name>();

        public Location()
            : this(null)
        {
        }
    }
}
