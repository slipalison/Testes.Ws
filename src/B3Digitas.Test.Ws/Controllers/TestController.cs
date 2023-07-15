using Microsoft.AspNetCore.Mvc;

namespace B3Digitas.Test.Ws.Controllers
{
    [ApiController, ApiVersion("1.0")]
    [Route("api/V{version:apiVersion}/[controller]")]
    public class TestController : ControllerBase
    {
        private Ws _ws;
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger, Ws ws)
        {
            _ws = ws;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {

            await _ws.SendMessageAsync(new
            {
                d = new { },
                q = "v2/exchange.market/orderBookState",
                sid = 100
            }, cancellationToken);

            return Ok("Ok");
        }

    }
}
