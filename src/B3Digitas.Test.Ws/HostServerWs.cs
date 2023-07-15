using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace B3Digitas.Test.Ws
{
    public class HostServerWs : IHostedService
    {
        private readonly Ws _ws;
        private ClientWebSocket _clientWebSocket;

        public HostServerWs(Ws ws)
        {

            //_clientWebSocket = new ClientWebSocket();

            _ws = ws;
            // _ws.AddWSClient(_clientWebSocket);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);///.ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Task.Run(async () =>
            {

                do
                {
                    if (_ws.IsConnect())
                        continue;
                    // Todo: validar se ha necessidade de cria um novo WebSockerClient em caso de Disposed
                    if (await _ws.ConnectAsync(stoppingToken))
                    {
                        await _ws.Login(stoppingToken);
                        await _ws.Listen(stoppingToken);
                    }

                }
                while (!stoppingToken.IsCancellationRequested);
            }, stoppingToken).ConfigureAwait(false);


            // throw new NotImplementedException();
        }
    }


    public class Ws : IExbereyWs//, IDisposable
    {
        private readonly string _apikey;
        private readonly string _secretKey;
        private readonly byte[] _secretKeyBytes;
        private readonly Uri _serverUri;
        private readonly ILogger<Ws> _logger;
        private ClientWebSocket _clientWebSocket;
        private bool disposedValue;
        private readonly Memory<byte> _buffer;
        public Ws(ILogger<Ws> logger, ClientWebSocket clientWebSocket)
        {
            _logger = logger;
            _clientWebSocket = clientWebSocket;
            _apikey = "a31a75f4-0bf7-4264-8842-473fb77f7bca";
            _secretKey = "8e46b743e9cfa5c3ec1dfdfec2efe14b04d8f72dc574ea50582edf8ce5dbe792";
            _secretKeyBytes = Encoding.UTF8.GetBytes(_secretKey);
            _serverUri = new Uri("wss://sandbox-shared.staging.exberry-uat.io");
            _buffer = new Memory<byte>(new byte[4096 * 2]);
        }


        //  public void AddWSClient(ClientWebSocket clientWebSocket) => _clientWebSocket = clientWebSocket;


        public Task Stop(CancellationToken cancellationToken = default)
            => _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Processo_encerrado", cancellationToken);

        public Task Login(CancellationToken cancellationToken = default)
        {
            if (!IsConnect())
                throw new Exception("Socket desconectado");

            var credentials = CreateCredentials();

            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(credentials));

            return _clientWebSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken);
        }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {

            await _clientWebSocket.ConnectAsync(_serverUri, cancellationToken);

            _logger.LogInformation("Ws Connect");

            return IsConnect();
        }

        public bool IsConnect() => _clientWebSocket is not null && _clientWebSocket.State == WebSocketState.Open;

        public Task SendMessageAsync(object obj, CancellationToken cancellationToken = default)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj)));
            _clientWebSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken);
            return Task.CompletedTask;
        }

        private string CreateCredentials()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var inputBytes = Encoding.UTF8.GetBytes($"\"apiKey\":\"{_apikey}\",\"timestamp\":\"{timestamp}\"");

            using var hmac = new HMACSHA256(_secretKeyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            var t = new
            {
                d = new
                {
                    apiKey = _apikey,
                    timestamp = timestamp,
                    signature = hash
                },
                q = "exchange.market/createSession",
                sid = 1
            };

            var json = JsonSerializer.Serialize(t, new JsonSerializerOptions { WriteIndented = false });

            return json;
        }

        public async Task Listen(CancellationToken stoppingToken)
        {
            while (IsConnect())
            {
                await ReceiveMessage(stoppingToken);
            }
        }
        private async Task ReceiveMessage(CancellationToken stoppingToken)
        {
            var result = await _clientWebSocket.ReceiveAsync(_buffer, stoppingToken);

            if (result.MessageType != WebSocketMessageType.Text)
                return;

            var message = Encoding.UTF8.GetString(_buffer.Span.Slice(0, result.Count));

            _logger.LogTrace(message);
            Console.WriteLine("\n" );
            Console.WriteLine("\n" );
            Console.WriteLine("\n" );
            Console.WriteLine("LOG - " + message);
            Console.WriteLine("\n");
            Console.WriteLine("\n");
            Console.WriteLine("\n");

        }



        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //            _clientWebSocket?.Dispose();
        //        disposedValue = true;
        //    }
        //}

        //public void Dispose()
        //{
        //    Dispose(disposing: true);
        //    GC.SuppressFinalize(this);
        //}
    }

}
