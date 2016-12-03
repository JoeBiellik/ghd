using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;

namespace ghd
{
    public class Startup
    {
        private const string APP_NAME = "ghd";
        private const string CONFIG_FILE = "settings.json";

        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .CreateLogger();

            Log.Logger.Information("{AppName} is starting up", APP_NAME);

            this.Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(CONFIG_FILE, true)
                .Build();

            if (!File.Exists(Path.Combine(env.ContentRootPath, CONFIG_FILE)))
            {
                Log.Logger.Error("Settings file not found, generating default {SettingsFile} file", CONFIG_FILE);

                File.WriteAllText(Path.Combine(env.ContentRootPath, CONFIG_FILE), JsonConvert.SerializeObject(new Settings.Settings(), Formatting.Indented));

                Log.Logger.Warning("Please update {SettingsFile} and restart {AppName}", CONFIG_FILE, APP_NAME);
                Environment.Exit(1);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() });

            services
                .AddOptions()
                .Configure<Settings.Settings>(this.Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<Settings.Settings> settings)
        {
            loggerFactory.AddSerilog();

            if (settings.Value.Ssh.Enabled)
            {
                var sshUseKey = !string.IsNullOrWhiteSpace(settings.Value.Ssh.KeyFile);

                Log.Logger.Information("SSH is enabled, testing SSH connection to {SshUsername}@{SshHost}:{SshPort} using {SshAuth}", settings.Value.Ssh.Username, settings.Value.Ssh.Host, settings.Value.Ssh.Port, sshUseKey ? $"{settings.Value.Ssh.KeyFile} keyfile" : "password auth");

                try
                {
                    Ssh.Test(settings.Value.Ssh);
                    Log.Logger.Information("SSH connection test passed");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("SSH connection test failed: {SshError}", ex.Message);
                    Log.Logger.Warning("Please update {SettingsFile} and restart {AppName}", CONFIG_FILE, APP_NAME);
                    Environment.Exit(1);
                }
            }

            Log.Logger.Information("Server listening on {@ServerAddress}", app.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses);

            app.UseMvc(r => r.MapRoute("webhook", "{*anything}", new { controller = "Webhook", action = "Post" }));

            Log.Logger.Information("{AppName} ready for incoming webhooks for repo {Repo} on branch {Branch}", APP_NAME, settings.Value.GitHub.Repository, settings.Value.GitHub.Branch);
        }
    }
}
