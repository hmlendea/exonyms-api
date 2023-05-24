using System;
using System.Threading.Tasks;
using System.Web;

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
            [FromQuery] string wikiDataId)
        {
            Location location = await this.exonymsService.Gather(wikiDataId);

            return Ok(location);
        }
    }
}
