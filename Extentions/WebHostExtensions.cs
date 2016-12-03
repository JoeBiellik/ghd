using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ghd.Extentions
{
    public static class WebHostExtensions
    {
        /// <summary>
        /// Quietly runs a web application and block the calling thread until host shutdown.
        /// </summary>
        /// <remarks>
        /// Modified from https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNetCore.Hosting/WebHostExtensions.cs
        /// </remarks>
        /// <param name="host">The <see cref="IWebHost"/> to run.</param>
        public static void RunQuiet(this IWebHost host)
        {
            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Console.WriteLine("Application is shutting down...");
                    cts.Cancel();

                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                using (host)
                {
                    host.Start();

                    var applicationLifetime = host.Services.GetService<IApplicationLifetime>();

                    cts.Token.Register(state =>
                    {
                        ((IApplicationLifetime)state).StopApplication();
                    },
                    applicationLifetime);

                    applicationLifetime.ApplicationStopping.WaitHandle.WaitOne();
                }
            }
        }
    }
}
