using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CaptchaController : ControllerBase
    {
        private readonly HubConnection hubConnection;
        public CaptchaController(HubInfo hubInfo)
        {
            // Create HubConnection to Given Hub
            hubConnection = new HubConnectionBuilder()
                .WithUrl(hubInfo.Url)
                .WithAutomaticReconnect()
                .Build();

            // Start HubConnection
            hubConnection.StartAsync();
        }

        [HttpPost]
        public async Task<IActionResult> SendCaptchaAsync(IFormFile file)
        {
            string result = null;
            
            if (file.ContentType.Contains("image"))
            {
                Captcha captcha = null;

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    captcha = new Captcha
                    {
                        ImageHeaders = "data:" + file.ContentType + ";base64,",
                        ImageBinary = memoryStream.ToArray()
                    };
                }

                if (captcha is not null)
                    // Call 'SendCaptcha' method in Hub with given captcha 
                    await hubConnection.SendAsync("SendCaptcha", captcha);
                else
                    return BadRequest("failed to create Captcha");
            }

            // hub is listening to 'GetCaptchaResult' method in Clients
            hubConnection.On<string>("GetCaptchaResult", value =>
            {
                // when result get value, while loop doesn't execute anymore
                result = value;
            });

            // hub is listening to 'GetCaptchaExpired' method in Clients
            hubConnection.On("GetCaptchaExpired", () => {

                // when result get value, while loop doesn't execute anymore
                result = "Captcha Expired";
            });

            while (result is null) { }

            return Ok(result);
        }
    }
}
