using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace My.WebSocketServer.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Dictionary<string, WebSocket> sockets;

        public HomeController(ILogger<HomeController> logger, Dictionary<string, WebSocket> sockets)
        {
            _logger = logger;
            this.sockets = sockets;
        }

        [HttpGet("/chat")]
        public async Task<IActionResult> chat()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                //声明缓冲区
                var buffer = new byte[1024 * 4];

                while (webSocket.State == WebSocketState.Open)
                {
                    var incoming = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    if (incoming != null)
                    {
                        var result = Encoding.UTF8.GetString(buffer, 0, incoming.Count);

                        if (result.StartsWith("conn:"))
                        {
                            string client = result.Split("conn:")[1];
                            if (!sockets.ContainsKey(client))
                            {
                                sockets.Add(client, webSocket);
                            }                            
                        }
                        else
                        { 
                            var parse = result.Split("<custom_separator>");
                            string to = parse[0];
                            string data = string.Empty;
                            if (!string.IsNullOrEmpty(parse[1]))
                            { 
                                data = parse[1];
                            }
                            string from = parse[2];

                            if (sockets.ContainsKey(to))
                            {
                                await sockets[to].SendAsync(
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None
                                );
                            }
                        }
                    }
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            return Ok();
        }
    }

    public class WebSocketCollection
    {
        private List<WebSocketClient> WebSockets = new List<WebSocketClient>();

        /// <summary>
        /// 添加WebSocket
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public WebSocketClient Add(WebSocketClient webSocket)
        {
            WebSockets.Add(webSocket);
            return webSocket;
        }

        public List<WebSocketClient> GetAll()
        {
            return WebSockets;
        }
    }

    public class WebSocketClient
    {
        public WebSocket WebSocket { get; set; }
        public string Client { get; set; } = Guid.NewGuid().ToString();
    }
}