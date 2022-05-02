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
            //确认是否WebSocket请求
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                //接受请求并返回websocket对象
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                var code = webSocket.GetHashCode();

                //声明缓冲区
                var buffer = new byte[1024 * 4];

                while (webSocket.State == WebSocketState.Open)
                {
                    //接收到客户端的消息
                    var incoming = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    if (incoming != null)
                    {
                        var result = Encoding.UTF8.GetString(buffer, 0, incoming.Count);

                        //如果客户端消息是标识用户身份的，则通过字典给当前socket对象命名
                        if (result.StartsWith("conn:"))
                        {
                            string client = result.Split("conn:")[1];
                            if (!sockets.ContainsKey(client))
                            {
                                sockets.Add(client, webSocket);
                            }                            
                        }
                        //如果是正常消息，则从消息中取出发送者和接收者，并从字典中取出数据用于发送
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
}