using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using ExonymsAPI.Service;
using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExonymsController : ControllerBase
    {
        IExonymsService exonymsService;

        public ExonymsController(IExonymsService exonymsService)
        {
            this.exonymsService = exonymsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
            [FromQuery] string geoNamesId,
            [FromQuery] string wikiDataId)
        {
            Location location = await this.exonymsService.Gather(geoNamesId, wikiDataId);

            return Ok(location);
        }
    }
}
