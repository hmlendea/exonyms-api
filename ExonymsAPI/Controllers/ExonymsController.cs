using System;
using System.Web;

using Microsoft.AspNetCore.Mvc;

using ExonymsAPI.Service;

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
        public ActionResult Get(
            [FromQuery] string wikiDataId)
        {
            return Ok();
        }
    }
}
