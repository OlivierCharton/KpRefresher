using Blish_HUD;
using Blish_HUD.Controls;
using KpRefresher.Domain;
using KpRefresher.Extensions;
using KpRefresher.Ressources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KpRefresher.Services
{
    public class KpMeService
    {
        private readonly Logger _logger;

        private const string _kpMeBaseUrl = "https://killproof.me/";

        public KpMeService(Logger logger)
        {
            _logger = logger;
        }

        public async Task<KpApiModel> GetAccountData(string kpId)
        {
            if (string.IsNullOrWhiteSpace(kpId))
                return null;

            try
            {
                var url = $"{_kpMeBaseUrl}api/kp/{kpId}?lang=en";

                _logger.Info($"[KpRefresher] Calling {url}");

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
                        ScreenNotification.ShowNotification(string.Format(strings.Notification_KpAccountUnknown, kpId), ScreenNotification.NotificationType.Error);
                    else
                        _logger.Warn($"Unknown status while getting account data : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Error while getting account info : {ex.Message}");
            }

            return null;
        }

        public async Task<List<RaidBoss>> GetClearData(string kpId)
        {
            if (string.IsNullOrWhiteSpace(kpId))
                return null;

            try
            {
                var url = $"{_kpMeBaseUrl}api/clear/{kpId}";

                _logger.Info($"[KpRefresher] Calling {url}");

                using var client = new HttpClient();

                var res = new List<RaidBoss>();

                var response = await client.GetAsync(url);
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        JObject jo = JObject.Parse(content);
                        foreach (var wing in jo)
                        {
                            foreach (var encounters in wing.Value)
                            {
                                var encounter = encounters.First;

                                var encounterName = ((JProperty)encounter).Name;
                                var encounterValue = (bool)((JProperty)encounter).Value;

                                if (encounterValue)
                                    res.Add(encounterName.GetValueFromName<RaidBoss>());
                            }
                        }

                        return res;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        ScreenNotification.ShowNotification(string.Format(strings.Notification_KpAccountUnknown, kpId), ScreenNotification.NotificationType.Error);
                    else
                        _logger.Warn($"Unknown status while getting clear data : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Error while refreshing kp.me : {ex.Message}");
            }

            return null;
        }

        public async Task<bool?> RefreshApi(string kpId)
        {
            if (string.IsNullOrWhiteSpace(kpId))
                return null;

            try
            {
                var url = $"{_kpMeBaseUrl}proof/{kpId}/refresh";

                _logger.Info($"[KpRefresher] Calling {url}");

                using var client = new HttpClient();

                var response = await client.GetAsync(url);
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return true;
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                        return false;
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        ScreenNotification.ShowNotification(strings.Notification_KpAccountAnonymous, ScreenNotification.NotificationType.Error);
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        ScreenNotification.ShowNotification(string.Format(strings.Notification_KpAccountUnknown, kpId), ScreenNotification.NotificationType.Error);
                    else
                        _logger.Warn($"Unknown status while refreshing kp.me : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Error while refreshing kp.me : {ex.Message}");
            }

            return null;
        }

        public string GetBaseUrl()
        {
            return _kpMeBaseUrl;
        }
    }
}
