﻿using System;
using System.IO;
using System.Linq;
using HttpServer.RequestHandlers;
using HttpServer.Responses;

namespace HttpServer.Handlers
{
    public class DirectoryHandler : IRequestHandler
    {
        protected readonly string _directory;
        private readonly FileHandler _fileHandler;

        public DirectoryHandler(string directory = null)
        {
            _directory = directory ?? Path.Combine(Directory.GetCurrentDirectory(), Constants.DefaultDirectory);
            _fileHandler = new FileHandler(_directory);
        }

        public virtual Response Handle(Request request)
        {
            request = request ?? throw new ArgumentException(nameof(request));

            var directoryPath = Path.Combine(_directory, request.Resource.TrimStart('/'));

            if (request.IsEndpoint)
            {
                return _fileHandler.Handle(request);
            }

            if (ResourceNotFound(directoryPath))
            {
                return new Response(HttpStatusCodes.NotFound, request);
            }

            return CreateSuccessResponse(directoryPath, request);
        }

        private bool ResourceNotFound(string directory)
        {
            return !Directory.Exists(directory);
        }

        private Response CreateSuccessResponse(string directory, Request request)
        {
            var response = new Response(HttpStatusCodes.Ok, request);

            var directories = Directory.GetFileSystemEntries(directory).Select(GetFileNameAsLink);
            var responseBody = string.Join("<br>", directories);

            response.StringBody = WrapInHtml(responseBody);

            return response;
        }

        private static string GetFileNameAsLink(string file)
        {
            return $"<a href=\"/{Path.GetFileName(file)}\">{Path.GetFileName(file)}</a>";
        }

        private string WrapInHtml(string body)
        {
            return $"<html><body>{body}</body></html>";
        }
    }
}