using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KpRefresher.Domain
{
    /// <summary>
    /// Basic model of KillProof.me api. Only necessary fields are implemented.
    /// </summary>
    public class KpApiModel
    {
        [JsonProperty("kpid")]
        public string Id { get; set; }

        [JsonProperty("last_refresh")]
        public DateTime LastRefresh { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("killproofs")]
        public List<Killproof> Killproofs { get; set; }
        
        [JsonProperty("linked")]
        public List<KpApiModel> LinkedAccounts { get; set; }
    }

    public class Killproof
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}