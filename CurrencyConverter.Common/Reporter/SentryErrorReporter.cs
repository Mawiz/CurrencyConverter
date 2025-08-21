using CurrencyConverter.Common.Settings;
using Microsoft.Extensions.Options;
using RequestFlow.Common.Reporter;
using SharpRaven;
using SharpRaven.Data;

namespace CurrencyConverter.Common.Reporter
{
    public class SentryErrorReporter : IErrorReporter
    {
        private readonly IRavenClient client;
        private readonly SentrySettings options;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SentryErrorReporter" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">
        ///     options
        ///     or
        ///     Can not construct a SentryErrorReporter without a valid DSN!
        /// </exception>
        public SentryErrorReporter(IOptions<SentrySettings> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.Value.Dsn))
                throw new ArgumentNullException("Can not construct a SentryErrorReporter without a valid DSN!");

            this.options = options.Value;
            client = new RavenClient(this.options.Dsn) { Environment = this.options.Environment };
        }

        /// <summary>
        ///     Captures the specified exception asynchronously and hands it off to an error handling service.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">exception</exception>
        public async Task CaptureAsync(Exception exception)
        {
            if (options.Enabled)
            {
                if (exception == null)
                    throw new ArgumentNullException(nameof(exception));

                await client.CaptureAsync(new SentryEvent(exception));
            }
        }

        /// <summary>
        ///     Captures the specified message asynchronously and hands it off to an error handling service.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">message</exception>
        public async Task CaptureAsync(string message)
        {
            if (options.Enabled)
            {
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentNullException(nameof(message));

                await client.CaptureAsync(new SentryEvent(message));
            }
        }
    }
}
