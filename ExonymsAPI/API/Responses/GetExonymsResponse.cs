using System.Collections.Generic;
using ExonymsAPI.Service.Models;
using NuciAPI.Responses;

namespace ExonymsAPI.API.Responses
{
    public class GetExonymsResponse : NuciApiSuccessResponse
    {
        public string DefaultName { get; set; }

        public IDictionary<string, Name> Names { get; set; } = new Dictionary<string, Name>();

        public int Count => Names.Count;
    }
}
