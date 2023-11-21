using System.Collections.Generic;

namespace ExonymsAPI.Service.Models
{
    public class Location
    {
        public string DefaultName { get; set; }

        public IDictionary<string, Name> Names { get; set; }

        public Location()
            : this(null)
        {
        }

        public Location(string defaultName)
        {
            DefaultName = defaultName;
            Names = new Dictionary<string, Name>();
        }
    }
}
