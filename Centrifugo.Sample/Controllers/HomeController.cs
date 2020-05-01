using Centrifugo.Client.Abstractions;
using Centrifugo.Sample.Models;
using Centrifugo.Sample.TokenProvider;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Centrifugo.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICentrifugoClient _client;
        private readonly ICentrifugoTokenProvider _tokenProvider;

        public HomeController(ICentrifugoClient client, ICentrifugoTokenProvider tokenProvider)
        {
            _client = client;
            _tokenProvider = tokenProvider;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Token"] = await _tokenProvider.GenerateTokenAsync("1");
            ViewData["Url"] = "ws://localhost:8000/connection/websocket";

            return View();
        }

        [HttpGet("new")]
        public async Task<IActionResult> Add(string text)
        {
            var json = JsonConvert.SerializeObject(new TestMessage
            {
                Msg = text ?? "Hello",
                Id = Guid.NewGuid()
            });

            await _client.PublishAsync("test-channel", Encoding.UTF8.GetBytes(json));

            return Ok();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    [ApiController]
    [Route("centrifuge")]
    public class Centrifuge : ControllerBase
    {
        private readonly ICentrifugoTokenProvider _tokenProvider;

        public Centrifuge(ICentrifugoTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync()
        {
            var token = await _tokenProvider.GenerateTokenAsync("1");
            return Ok(new
            {
                token
            });
        }
    }
}
