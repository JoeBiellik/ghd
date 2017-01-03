using Newtonsoft.Json;

namespace ghd.Settings
{
    public class Profile
    {
        [JsonProperty("SSH")]
        public Ssh Ssh { get; set; } = new Ssh();
        public GitHub GitHub { get; set; } = new GitHub();
        public Deploy Deploy { get; set; } = new Deploy();
    }
}
