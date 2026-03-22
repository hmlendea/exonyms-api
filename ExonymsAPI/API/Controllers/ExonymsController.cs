using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ExonymsAPI.Service;
using ExonymsAPI.API.Requests;
using NuciAPI.Controllers;

namespace ExonymsAPI.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExonymsController(IExonymsService exonymsService) : NuciApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetExonymsRequest request)
            => ProcessRequest(
                request,
                async () => await exonymsService.Gather(request.GeoNamesId, request.WikiDataId),
                NuciApiAuthorisation.None);
    }
}
