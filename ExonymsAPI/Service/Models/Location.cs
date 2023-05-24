using System.Collections.Generic;

namespace ExonymsAPI.Service.Models
{
    public class Location
    {
        public string DefaultName { get; set; }

        public Dictionary<string, string> Names { get; set; }

        public Location()
            : this(null)
        {
        }

        public Location(string defaultName)
        {
            DefaultName = defaultName;
            Names = new Dictionary<string, string>();
        }
    }
}
