using EquityPositions.Api.Dtos;
using EquityPositions.Api.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EquityPositions.Api.Controllers
{
    [ApiController]
    [Route("v1/equity-positions")]
    public class EquityPositionsController : ControllerBase
    {
        private readonly ILogger<EquityPositionsController> _logger;
        private readonly IEquityPositionService _equityPositionService;

        public EquityPositionsController(ILogger<EquityPositionsController> logger, IEquityPositionService equityPositionService)
        {
            _logger = logger;
            _equityPositionService = equityPositionService;
        }

        [HttpGet("details", Name = "GetDetails")]
        public async Task<IActionResult> Get()
        {
            Dtos.EquityPositions equityPositions = await _equityPositionService.GetEquityPositions();
            return Ok(equityPositions);
        }

        [HttpPost("execute", Name = "ExecuteTransaction")]
        public async Task<IActionResult> Post(TransactionRequest transactionRequest)
        {
            bool result = await _equityPositionService.ExecuteTransaction(transactionRequest);

            if (result)
            {
                return Ok(new { Message = "Transaction executed successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Failed to execute transaction." });

            }
        }
    }
}
