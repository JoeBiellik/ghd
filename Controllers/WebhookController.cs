using System;
using System.Diagnostics;
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

                    if (push.Repository.FullName == this.Settings.GitHub.Repository && push.Ref == $"refs/heads/{this.Settings.GitHub.Branch}")
                    {
                        this.Logger.LogInformation("[{Event}] {Repo}: Triggering deployment...", eventName, push.Repository.FullName);

                        string stdOut;
                        string stdErr;
                        int exitCode;

                        if (this.Settings.Ssh.Enabled)
                        {
                            var command = Ssh.Run(this.Settings.Ssh, $"{this.Settings.Deploy.Command.FormatWith(push)} {this.Settings.Deploy.Arguments.FormatWith(push)}");

                            stdOut = command.Result;
                            stdErr = command.Error;
                            exitCode = command.ExitStatus;
                        }
                        else
                        {
                            var process = Process.Start(new ProcessStartInfo(this.Settings.Deploy.Command.FormatWith(push), this.Settings.Deploy.Arguments.FormatWith(push))
                            {
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            });

                            process.WaitForExit();

                            stdOut = process.StandardOutput.ReadToEnd();
                            stdErr = process.StandardError.ReadToEnd();
                            exitCode = process.ExitCode;
                        }

                        if (string.IsNullOrWhiteSpace(stdOut))
                        {
                            this.Logger.LogInformation("[{Event}] {Repo}: Deployment sent no standard output", eventName, push.Repository.FullName);
                        }
                        else
                        {
                            this.Logger.LogInformation("[{Event}] {Repo}: Deployment standard output:\n" + stdOut.Trim(), eventName, push.Repository.FullName);
                        }

                        if (!string.IsNullOrWhiteSpace(stdErr))
                        {
                            this.Logger.LogWarning("[{Event}] {Repo}: Deployment standard error:\n" + stdErr.Trim(), eventName, push.Repository.FullName);
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
                        if (push.Repository.FullName != this.Settings.GitHub.Repository)
                        {
                            this.Logger.LogInformation("[{Event}] {Repo}: Skipping deployment, repo does not match {WatchRepo}", eventName, push.Repository.FullName, this.Settings.GitHub.Repository);
                        }
                        else
                        {
                            this.Logger.LogInformation("[{Event}] {Repo}: Skipping deployment, branch does not match {WatchBranch}", eventName, push.Repository.FullName, this.Settings.GitHub.Branch);
                        }
                    }

                    break;
                default:
                    this.Logger.LogWarning("[{Event}] {Repo}: Unknown webhook event \"{Event}\"", eventName, value["repository"]["full_name"].Value<string>());
                    return this.StatusCode(400);
            }

            return this.StatusCode(200);
        }
    }
}
