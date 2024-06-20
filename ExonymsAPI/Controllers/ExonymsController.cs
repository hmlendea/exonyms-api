using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using ExonymsAPI.Service;
using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExonymsController(IExonymsService exonymsService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync(
            [FromQuery] string geoNamesId,
            [FromQuery] string wikiDataId)
        {
            Location location = await exonymsService.Gather(geoNamesId, wikiDataId);

            return Ok(location);
        }
    }
}
