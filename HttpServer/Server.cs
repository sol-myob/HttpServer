﻿using System;
using System.Net;
using System.Text;
using HttpServer.Listeners;
using HttpServer.Loggers;
using HttpServer.Responses;

namespace HttpServer
{
    public class Server : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IListener _listener;
        private readonly Router _router;

        public Encoding Encoding = Encoding.UTF8;
        public bool IsRunning => _listener.IsListening;

        public Server(Router router, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _router = router ?? throw new ArgumentException(nameof(router));
            _listener = new TcpListener(_routeRequest, IPAddress.Loopback);
        }

        public int Port => _listener.Port;

        public void Start(int port = 0)
        {
            if (_listener.IsListening) throw new ApplicationException("Server is already running");

            _listener.Port = port;
            _listener.Start();
            _logger.Log("Waiting for connection on port: " + _listener.Port);
        }

        public void Stop()
        {
            if (_listener.IsListening) _listener.Stop();
        }

        private byte[] _routeRequest(string request)
        {
            try
            {
                _logger?.Log(request);

                var response = _router.CreateResponse(request);

                return response.Bytes(Encoding);
            }
            catch (Exception ex)
            {
                _logger?.Log($"ERROR: {ex.Message} {ex.StackTrace}");
            }

            return new byte[0];
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}