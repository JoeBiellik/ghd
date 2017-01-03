using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace ghd
{
    public static class Ssh
    {
        public static bool Test(Settings.Ssh settings)
        {
            using (var client = Client(settings))
            {
                client.Connect();
                client.Disconnect();

                return true;
            }
        }

        public static SshCommand Run(Settings.Ssh settings, string command)
        {
            using (var client = Client(settings))
            {
                client.Connect();
                var result = client.RunCommand(command);
                client.Disconnect();

                return result;
            }
        }

        public static SshCommand Run(Settings.Ssh settings, string command, IProgress<SshOutputLine> progress)
        {
            using (var client = Client(settings))
            {
                client.Connect();
                var cmd = client.CreateCommand(command);

                cmd.ExecuteAsync(progress, CancellationToken.None).GetAwaiter().GetResult();

                client.Disconnect();

                return cmd;
            }
        }

        private static SshClient Client(Settings.Ssh settings)
        {
            return new SshClient(new ConnectionInfo(settings.Host, settings.Port, settings.Username, Auth(settings)));
        }

        private static AuthenticationMethod Auth(Settings.Ssh settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.KeyFile))
            {
                using (var key = new PrivateKeyFile(settings.KeyFile, settings.KeyPassword))
                {
                    return new PrivateKeyAuthenticationMethod(settings.Username, key);
                }
            }

            return new PasswordAuthenticationMethod(settings.Username, settings.Password);
        }
    }

    public static class SshCommandExtensions
    {
        public static async Task ExecuteAsync(this SshCommand sshCommand, IProgress<SshOutputLine> progress, CancellationToken cancellationToken)
        {
            var asyncResult = sshCommand.BeginExecute();

            using (var stdoutStreamReader = new StreamReader(sshCommand.OutputStream))
            using (var stderrStreamReader = new StreamReader(sshCommand.ExtendedOutputStream))
            {
                while (!asyncResult.IsCompleted)
                {
                    await CheckOutputAndReportProgress(sshCommand, stdoutStreamReader, stderrStreamReader, progress, cancellationToken);

                    await Task.Yield();
                }

                sshCommand.EndExecute(asyncResult);

                await CheckOutputAndReportProgress(sshCommand, stdoutStreamReader, stderrStreamReader, progress, cancellationToken);
            }
        }

        private static async Task CheckOutputAndReportProgress(SshCommand sshCommand, TextReader stdoutStreamReader, TextReader stderrStreamReader, IProgress<SshOutputLine> progress, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                sshCommand.CancelAsync();
            }

            cancellationToken.ThrowIfCancellationRequested();

            await CheckStdoutAndReportProgressAsync(stdoutStreamReader, progress);
            await CheckStderrAndReportProgressAsync(stderrStreamReader, progress);
        }

        private static async Task CheckStdoutAndReportProgressAsync(TextReader stdoutStreamReader, IProgress<SshOutputLine> stdoutProgress)
        {
            var stdoutLine = await stdoutStreamReader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(stdoutLine))
            {
                stdoutProgress.Report(new SshOutputLine(stdoutLine, false));
            }
        }

        private static async Task CheckStderrAndReportProgressAsync(TextReader stderrStreamReader, IProgress<SshOutputLine> stderrProgress)
        {
            var stderrLine = await stderrStreamReader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(stderrLine))
            {
                stderrProgress.Report(new SshOutputLine(stderrLine, true));
            }
        }
    }

    public class SshOutputLine
    {
        public string Line { get; private set; }
        public bool IsError { get; private set; }

        public SshOutputLine(string line, bool isError)
        {
            this.Line = line;
            this.IsError = isError;
        }
    }
}
