using Microsoft.AspNetCore.Mvc;

using NuciAPI.Controllers;

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

                    response.SignHMAC(securitySettings.HmacSigningKey);

                    return response;
                },
                NuciApiAuthorisation.None);
    }
}
