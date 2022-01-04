using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using yt_downloader_bot.Services;

namespace yt_downloader_bot.Controllers;

public class WebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                          [FromBody] Update update)
    {
        await handleUpdateService.HandleMessageAsync(update);
        return Ok();
    }
}