using NuciAPI.Requests;

namespace ExonymsAPI.API.Requests
{
    public class GetExonymsRequest : NuciApiRequest
    {
        public string GeoNamesId { get; set; }

        public string WikiDataId { get; set; }
    }
}
