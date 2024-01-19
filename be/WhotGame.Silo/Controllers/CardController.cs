using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhoteGame.Services;
using WhotGame.Core.DTO.Response;
using WhotGame.Silo.ViewModels;

namespace WhotGame.Silo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : BaseController
    {
        private readonly ICardService _cardService;
        private readonly IHubContext<GameHub> _gameHub;

        public CardController(IHttpContextAccessor httpContext,
            ICardService cardService,
            IHubContext<GameHub> gameHub)
            : base(httpContext)
        {
            _cardService = cardService;
            _gameHub = gameHub;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<GameResponse>>), 200)]
        public async Task<IActionResult> GetGames()
        {
            var cards = (await _cardService.GenerateCards()).Select(x => (CardResponse)x);

            return ApiResponse(message: "Success", codes: ApiResponseCodes.OK, data: cards);
        }
    }
}
