using Microsoft.AspNetCore.Mvc;
using NotSoBoring.WebHook.Services;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotSoBoring.WebHook.Controllers
{
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                          [FromBody] Update update)
        {
            await handleUpdateService.EchoAsync(update);
            return Ok();
        }
    }
}
