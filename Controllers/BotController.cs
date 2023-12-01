using BSUIRQueueTelegramBot.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace BSUIRQueueTelegramBot.Controllers
{

    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly DayOfWeek FORBIDDEN_DAY = DayOfWeek.Saturday;

        [HttpPost("/")]
        public async Task<IActionResult> Post(
         [FromBody] Update update,
         [FromServices] UpdateHandlers handleUpdateService,
         CancellationToken cancellationToken)
        {
            if(DateTime.UtcNow.AddHours(3).Date.DayOfWeek == FORBIDDEN_DAY)
            {
                await handleUpdateService.HandleServiceRestriction(update, cancellationToken);
                return Ok();
            }
            await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
            return Ok();
        }

        [HttpGet("/")]
        public async Task<IActionResult> Get()
        {
            return Ok("App is working");
        }
    }
}