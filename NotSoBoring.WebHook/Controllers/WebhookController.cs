using Microsoft.AspNetCore.Mvc;
using NotSoBoring.WebHook.Services;
using Serilog;
using System;
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
            try
            {
                await handleUpdateService.EchoAsync(update);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Information("WebhookController:Post : {ErrorMessage}", e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
