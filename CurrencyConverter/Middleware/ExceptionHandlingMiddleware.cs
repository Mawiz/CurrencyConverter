using CurrencyConverter.Business.Dto;
using CurrencyConverter.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Text;

namespace CurrencyConverter.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            this.logger = logger;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                //_logger.Error($"Something went wrong: {ex}");
                var exMsg = Log(httpContext, ex);
                await HandleExceptionAsync(httpContext, ex, exMsg);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, string exMsg)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An error has occured.";

            //TODO use swtich statments
            var actionDic = new Dictionary<Type, Action>();
            actionDic.Add(typeof(DbUpdateConcurrencyException),
                () =>
                {
                    statusCode = HttpStatusCode.Conflict;
                    message = "Current record has been updated by another user. Kindly fetch this record again.";
                });
            actionDic.Add(typeof(CustomException),
                () =>
                {
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                });

            if (actionDic.TryGetValue(exception.GetType(), out Action action))
            {
                action();
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = "An error has occured.";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonConvert.SerializeObject(new ResponseDto<string>()
            {
                StatusCode = statusCode,
                Message = message,
                Exception = exMsg,
            },
            settings: new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });

            return context.Response.WriteAsync(json);
        }

        public string Log(HttpContext context, Exception exception)
        {
            var strLogText = new StringBuilder();
            strLogText.AppendLine("Message ---").AppendLine($"{exception.Message}")
                      .AppendLine("Exception ---").AppendLine($"{exception}")
                      .AppendLine("Source ---").AppendLine($"{exception.Source}")
                      .AppendLine("StackTrace ---").AppendLine($"{exception.StackTrace}")
                      .AppendLine("TargetSite ---").AppendLine($"{exception.TargetSite}");

            if (exception.InnerException != null)
            {
                strLogText.AppendLine("Inner Exception is ").Append($"{exception.Source}")
                    .AppendLine($"{exception.InnerException}");
            }
            if (exception.HelpLink != null)
            {
                strLogText.AppendLine("HelpLink ---").Append($"{exception.Source}").AppendLine($"{exception.HelpLink}");
            }

            var requestedURi = context.Request.Path.ToString();// RequestUri.AbsoluteUri;
            var requestMethod = context.Request.Method;

            string finalMessage = string.Concat($"Message: {strLogText}{Environment.NewLine}",
                $"RequestUri: {requestedURi}{Environment.NewLine}RequestMethod: {requestMethod}{Environment.NewLine}");

            logger.LogInformation(finalMessage, exception);
            return finalMessage;
        }
    }
}
