using Microsoft.AspNetCore.Mvc;
using ExonymsAPI.Service;
using ExonymsAPI.API.Requests;
using NuciAPI.Controllers;
using ExonymsAPI.API.Responses;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Configuration;

namespace ExonymsAPI.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExonymsController(
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
