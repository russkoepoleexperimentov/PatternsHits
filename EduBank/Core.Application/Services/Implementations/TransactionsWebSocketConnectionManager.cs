using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Application.Dtos;
using Core.Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Core.Application.Services.Implementations
{
    public class TransactionsWebSocketConnectionManager
    {
        class Connection
        {
            public bool IsDisplayMode { get; set; }
            public bool IsManager { get; set; }
            public WebSocket Socket { get; set; } 
        }

        private readonly ConcurrentDictionary<Guid, Connection> _sockets = new();

        public void AddSocket(Guid id, bool isManager, WebSocket socket, bool isDisplayMode)
        {
            _sockets.TryAdd(id, new() { IsManager = isManager, Socket = socket, IsDisplayMode = isDisplayMode });
        }

        public async Task RemoveSocket(Guid id)
        {
            if (_sockets.TryRemove(id, out var connection))
            {
                var socket = connection.Socket;
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                }
            }
        }

        public async Task BroadcastMessageAsync(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(bytes);

            foreach (var (id, connection) in _sockets)
            {
                var socket = connection.Socket;
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task SendToClientAsync(Guid id, string message)
        {
            if (_sockets.TryGetValue(id, out var connection) && connection.Socket.State == WebSocketState.Open)
            {
                var socket = connection.Socket;
                var bytes = Encoding.UTF8.GetBytes(message);
                var buffer = new ArraySegment<byte>(bytes);
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task NotifyAllInterested(TransactionDto dto, AccountTransactionDto displayDto, Account account)
        {
            foreach (var (id, connection) in _sockets)
            {
                if(connection.IsManager || id == account.UserId)
                {
                    if (connection.IsDisplayMode)
                    {
                        await SendToClientAsync(id, Serialize(displayDto));
                    }
                    else
                    {
                        await SendToClientAsync(id, Serialize(dto));
                    }
                }
            }
        }

        public bool IsDisplayMode(Guid connectionId)
        {
            if(_sockets.TryGetValue(connectionId, out var connection))
            {
                return connection.IsDisplayMode;
            }
            return false;
        }

        public string Serialize<T>(T dto)
        {
            return JsonSerializer.Serialize(dto, typeof(T),
                        new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            Converters = { new JsonStringEnumConverter() }
                        });
        }
    }
}