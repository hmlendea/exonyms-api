using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;
using NuciAPI.Responses;

using ExonymsAPI.API.Requests;
using ExonymsAPI.API.Responses;
using ExonymsAPI.Configuration;
using ExonymsAPI.Service;
using ExonymsAPI.Service.Models;

namespace ExonymsAPI.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class ExonymsController(
        IExonymsService exonymsService,
        SecuritySettings securitySettings) : NuciApiController
    {
        [HttpGet]
        public ActionResult Get([FromQuery] GetExonymsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GeoNamesId) && string.IsNullOrWhiteSpace(request.WikiDataId))
            {
                return BadRequest(NuciApiErrorResponse.FromMessage("At least one of geoNamesId or wikiDataId must be provided."));
            }

            return ProcessRequest(
                request,
                () =>
                {
                    Location exonyms = exonymsService.Gather(request.GeoNamesId, request.WikiDataId).Result;

                    GetExonymsResponse response = new()
                    {
                        DefaultName = exonyms.DefaultName,
                        Names = exonyms.Names
                    };

                    response.SignHMAC(securitySettings.HmacSigningKey);

                    return response;
                },
                NuciApiAuthorisation.None);
        }
    }
}
