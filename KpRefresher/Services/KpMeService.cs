using Blish_HUD;
using Blish_HUD.Controls;
using KpRefresher.Domain;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KpRefresher.Services
{
    public class KpMeService
    {
        private readonly ModuleSettings _moduleSettings;
        private readonly Logger _logger;

        private const string _kpMeBaseUrl = "https://killproof.me/";

        private string _kpId { get; set; }
        private string _accountName { get; set; } = string.Empty;

        public string KpId => string.IsNullOrWhiteSpace(_kpId) ? _accountName : _kpId;

        public KpMeService(ModuleSettings moduleSettings, Logger logger)
        {
            _moduleSettings = moduleSettings;
            _logger = logger;
        }

        public void SetAccountName(string accountName)
        {
            _accountName = accountName;
        }

        public void SetKpId(string kpId)
        {
            _kpId = kpId;
        }

        public async Task<KpApiModel> GetAccountData()
        {
            if (string.IsNullOrWhiteSpace(KpId))
                return null;

            try
            {
                var url = $"{_kpMeBaseUrl}api/kp/{KpId}?lang=en";

                using var client = new HttpClient();

                var response = await client.GetAsync(url);
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<KpApiModel>(content);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        ScreenNotification.ShowNotification($"[KpRefresher] KillProof.me Id {KpId} does not exist !", ScreenNotification.NotificationType.Error);
                    else
                        _logger.Error($"Unknown status while getting account data : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while getting account info : {ex.Message}");
            }

            return null;
        }

        public async Task<bool?> RefreshApi()
        {
            if (string.IsNullOrWhiteSpace(KpId))
                return null;

            try
            {
                var url = $"{_kpMeBaseUrl}proof/{KpId}/refresh";

                using var client = new HttpClient();

                var response = await client.GetAsync(url);
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return true;
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                        return false;
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        ScreenNotification.ShowNotification($"[KpRefresher] KillProof.me Id {KpId} does not exist !", ScreenNotification.NotificationType.Error);
                    else
                        _logger.Error($"Unknown status while refreshing kp.me : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while refreshing kp.me : {ex.Message}");
            }

            return null;
        }
    }
}
