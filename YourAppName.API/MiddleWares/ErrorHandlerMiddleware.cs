using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourAppName.Core.Bases; // 🟢 Updated to YourAppName namespace
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace YourAppName.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger; // 🟢 Added Logger

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                // 1. Base response model
                var responseModel = new Response<string>()
                {
                    Succeeded = false,
                    Message = error?.Message
                };

                // 2. Map the Exception to the correct HTTP Status Code
                switch (error)
                {
                    case UnauthorizedAccessException:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        responseModel.StatusCode = HttpStatusCode.Unauthorized;
                        break;

                    case ValidationException:
                        response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                        responseModel.StatusCode = HttpStatusCode.UnprocessableEntity;
                        break;

                    case KeyNotFoundException:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        responseModel.StatusCode = HttpStatusCode.NotFound;
                        break;

                    case DbUpdateException:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.StatusCode = HttpStatusCode.BadRequest;
                        break;

                    // 🟢 NEW: Gracefully catch the CancellationToken abort!
                    case OperationCanceledException:
                        _logger.LogWarning("A request was canceled by the client.");
                        response.StatusCode = 499; // 499 is standard for Client Closed Request
                        responseModel.StatusCode = (HttpStatusCode)499;
                        responseModel.Message = "The request was canceled by the client.";
                        break;

                    default:
                        // Log the actual critical error to your console/file
                        _logger.LogError(error, "An unhandled exception occurred.");

                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        responseModel.StatusCode = HttpStatusCode.InternalServerError;

                        // Optional Pro-Tip: In production, do not send the real error.Message to the client
                        // as it might expose database logic. Send a generic message instead.
                        responseModel.Message = "An unexpected error occurred. Please try again later.";
                        break;
                }

                // 3. Serialize using standard Web Options (camelCase)
                var result = JsonSerializer.Serialize(responseModel, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await response.WriteAsync(result);
            }
        }
    }
}