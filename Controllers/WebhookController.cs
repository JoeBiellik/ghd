using System;
using System.Diagnostics;
using System.Linq;
using ghd.Extentions;
using ghd.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ghd.Controllers
{
    public class WebhookController : Controller
    {
        private Settings.Settings Settings { get; }
        private ILogger<WebhookController> Logger { get; }
        private JsonSerializer JsonSerializer { get; }

        public WebhookController(IOptions<Settings.Settings> settings, ILogger<WebhookController> logger, IOptions<MvcJsonOptions> serializer)
        {
            this.Settings = settings.Value;
            this.Logger = logger;
            this.JsonSerializer = new JsonSerializer
            {
                ContractResolver = serializer.Value.SerializerSettings.ContractResolver
            };
        }

        [HttpHead]
        public StatusCodeResult Head()
        {
            return this.StatusCode(200);
        }

        [HttpPost]
        public StatusCodeResult Post([FromBody] JObject value)
        {
            if (!this.Request.Headers["User-Agent"].ToString().StartsWith("GitHub-Hookshot/")) return this.StatusCode(400);

            var eventName = this.Request.Headers["X-GitHub-Event"].ToString();

            switch (eventName)
            {
                case "ping":
                    var ping = value.ToObject<GitHub.PingWebhook>(this.JsonSerializer);

                    this.Logger.LogInformation("[{Event}] {Repo}: Ping: \"{Zen}\"", eventName, ping.Repository.FullName, ping.Zen);
                    break;
                case "push":
                    var push = value.ToObject<GitHub.PushWebhook>(this.JsonSerializer);

                    this.Logger.LogInformation("[{Event}] {Repo}: [{Ref}] \"{CommitMessage}\"", eventName, push.Repository.FullName, push.Ref.StartsWith("refs/heads/") ? push.Ref.Substring(11) : push.Ref, push.HeadCommit.Message);

                    var settings = this.Settings.Profiles.FirstOrDefault(p => p.GitHub.Repository == push.Repository.FullName && $"refs/heads/{p.GitHub.Branch}" == push.Ref);

                    if (settings != null)
                    {
                        this.Logger.LogInformation("[{Event}] {Repo}: Triggering deployment...", eventName, push.Repository.FullName);

                        int exitCode;

                        if (settings.Ssh.Enabled)
                        {
                            var command = Ssh.Run(settings.Ssh, $"{settings.Deploy.Command.FormatWith(push)} {settings.Deploy.Arguments.FormatWith(push)}", new Progress<SshOutputLine>(s => this.LogPlain(s.Line, s.IsError)));

                            exitCode = command.ExitStatus;
                        }
                        else
                        {
                            var process = Process.Start(new ProcessStartInfo(settings.Deploy.Command.FormatWith(push), settings.Deploy.Arguments.FormatWith(push))
                            {
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            });

                            process.OutputDataReceived += (sender, args) => this.LogPlain(args.Data);
                            process.ErrorDataReceived += (sender, args) => this.LogPlain(args.Data, true);

                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            process.WaitForExit();

                            exitCode = process.ExitCode;
                        }

                        if (exitCode != 0)
                        {
                            this.Logger.LogWarning("[{Event}] {Repo}: Deployment finished with status {DeployStatus}", eventName, push.Repository.FullName, exitCode);
                        }
                        else
                        {
                            this.Logger.LogInformation("[{Event}] {Repo}: Deployment finished with status {DeployStatus}", eventName, push.Repository.FullName, exitCode);
                        }
                    }
                    else
                    {
                        this.Logger.LogInformation(this.Settings.Profiles.All(p => p.GitHub.Repository != push.Repository.FullName)
                            ? "[{Event}] {Repo}: Skipping deployment, repo does not match monitored"
                            : "[{Event}] {Repo}: Skipping deployment, branch does not match monitored", eventName, push.Repository.FullName);
                    }

                    break;
                default:
                    this.Logger.LogWarning("[{Event}] {Repo}: Unknown webhook event \"{Event}\"", eventName, value["repository"]["full_name"].Value<string>());
                    return this.StatusCode(400);
            }

            return this.StatusCode(200);
        }

        protected void LogPlain(string data, bool error = false)
        {
            if (data == null) return;

            Console.ForegroundColor = error ? ConsoleColor.DarkYellow : ConsoleColor.Gray;

            Console.Write(data);
        }
    }
}
