using RequestFlow.Common.Reporter;

namespace CurrencyConverter.API.Middleware
{
    public class SentryMiddleware
    {
        private readonly RequestDelegate requestDelegate;

        public SentryMiddleware(RequestDelegate requestDelegate)
        {
            this.requestDelegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
        }

        public async Task Invoke(HttpContext httpContext, IErrorReporter errorReporter)
        {
            try
            {
                await requestDelegate(httpContext);
            }
            catch (Exception ex)
            {
                await errorReporter.CaptureAsync(ex);

                // We're not handling, just logging. Throw it for someone else to take care of it.
                throw;
            }
        }
    }
}
