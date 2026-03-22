using Microsoft.AspNetCore.Mvc;
using ExonymsAPI.Service;
using ExonymsAPI.API.Requests;
using NuciAPI.Controllers;
using ExonymsAPI.API.Responses;
using ExonymsAPI.Service.Models;

namespace ExonymsAPI.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExonymsController(IExonymsService exonymsService) : NuciApiController
    {
        [HttpGet]
        public ActionResult Get([FromQuery] GetExonymsRequest request)
            => ProcessRequest(
                request,
                () =>
                {
                    Location exonyms = exonymsService.Gather(request.GeoNamesId, request.WikiDataId).Result;

                    GetExonymsResponse response = new()
                    {
                        DefaultName = exonyms.DefaultName,
                        Names = exonyms.Names
                    };

                    return response;
                },
                NuciApiAuthorisation.None);
    }
}
