using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using ExonymsAPI.Service;
using ExonymsAPI.Service.Models;
using ExonymsAPI.API.Requests;

namespace ExonymsAPI.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExonymsController(IExonymsService exonymsService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetExonymsRequest request)
        {
            Location location = await exonymsService.Gather(request.GeoNamesId, request.WikiDataId);

            return Ok(location);
        }
    }
}
