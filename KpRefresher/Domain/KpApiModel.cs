using Newtonsoft.Json;
using System;

namespace KpRefresher.Domain
{
    /// <summary>
    /// Basic model of Killproof.me api. Only necessary fields are implemented.
    /// </summary>
    public class KpApiModel
    {
        [JsonProperty("kpid")]
        public string Id { get; set; }

        [JsonProperty("last_refresh")]
        public DateTime LastRefresh { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }
    }
}
