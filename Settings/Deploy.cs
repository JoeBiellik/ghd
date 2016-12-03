namespace ghd.Settings
{
    public class Deploy
    {
        public string Command { get; set; } = "bash";
        public string Arguments { get; set; } = "-c \"echo Deploying {Repository.FullName} [{HeadCommit.Timestamp}]: {HeadCommit.Message}\"";
    }
}
