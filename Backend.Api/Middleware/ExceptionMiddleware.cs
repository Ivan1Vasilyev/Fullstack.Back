using Backend.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Api.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private const string DefaultTitle = "Неизвестная ошибка сервера";

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ProblemDetails
            {
                Status = statusCode,
                Detail = exception.Message,
                Title = GetTitle(exception)
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static int GetStatusCode(Exception exception) => exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetTitle(Exception exception)
        {
            var applicationException = exception as ApplicationCustomException;

            return string.IsNullOrEmpty(applicationException?.Title) ? DefaultTitle : applicationException.Title;
        }
    }
}
