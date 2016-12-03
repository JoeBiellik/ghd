namespace ghd.Settings
{
    public class Ssh
    {
        public bool Enabled { get; set; } = false;
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 22;
        public string Username { get; set; } = "root";
        public string Password { get; set; } = string.Empty;
        public string KeyFile { get; set; } = null;
        public string KeyPassword { get; set; } = null;
    }
}
