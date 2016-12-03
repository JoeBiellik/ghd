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
}
