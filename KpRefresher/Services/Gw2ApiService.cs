using Blish_HUD;
using Blish_HUD.Modules.Managers;
using KpRefresher.Domain;
using KpRefresher.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KpRefresher.Services
{
    public class Gw2ApiService
    {
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Logger _logger;

        public List<string> BaseRaidClears { get; set; }

        private List<Token> _tokens { get; set; }

        public Gw2ApiService(Gw2ApiManager gw2ApiManager, Logger logger)
        {
            _gw2ApiManager = gw2ApiManager;
            _logger = logger;

            _tokens = Enum.GetValues(typeof(Token))
                            .Cast<Token>()
                            .ToList();
        }

        /// <summary>
        /// Sets <c>_baseRaidClears</c> with current raid progression exposed by GW2 API
        /// </summary>
        /// <returns></returns>
        public async Task RefreshBaseRaidClears()
        {
            BaseRaidClears = await GetRaidClears();
        }

        public async Task<string> GetAccountName()
        {
            if (_gw2ApiManager.HasPermissions(_gw2ApiManager.Permissions) == false)
            {
                _logger.Warn("Permissions not granted.");
                return string.Empty;
            }

            var account = await _gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();
            return account?.Name;
        }

        public async Task<List<string>> GetRaidClears()
        {
            if (_gw2ApiManager.HasPermissions(_gw2ApiManager.Permissions) == false)
            {
                _logger.Warn("Permissions not granted.");
                return null;
            }

            try
            {
                var data = await _gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync();
                return data?.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while getting raid clears : {ex.Message}");
                return null;
            }
        }

        public async Task<string> ScanAccountForKp()
        {
            if (_gw2ApiManager.HasPermissions(_gw2ApiManager.Permissions) == false)
            {
                _logger.Warn("Permissions not granted.");
                return string.Empty;
            }

            string res = string.Empty;
            var tokensId = _tokens.Select(c => (int)c).ToList();

            var bankItems = await _gw2ApiManager.Gw2ApiClient.V2.Account.Bank.GetAsync();
            if (bankItems != null)
            {
                foreach (var item in bankItems)
                {
                    if (item != null && tokensId.Contains(item.Id))
                    {
                        res = $"{res}{item.Count} {((Token)item.Id).GetDisplayName()} (bank)\n";
                    }
                }
            }
            else
            {
                _logger.Warn("Failed to retrieve bank items.");
            }

            var sharedInventoryItems = await _gw2ApiManager.Gw2ApiClient.V2.Account.Inventory.GetAsync();
            if (sharedInventoryItems != null)
            {
                foreach (var item in sharedInventoryItems)
                {
                    if (item != null && tokensId.Contains(item.Id))
                        res = $"{res}{item.Count} {((Token)item.Id).GetDisplayName()} (shared slot)\n";
                }
            }
            else
            {
                _logger.Warn("Failed to retrieve shared inventory items.");
            }

            var characters = await _gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
            if (characters != null)
            {
                foreach (var character in characters)
                {
                    if (character.Bags != null)
                    {
                        foreach (var bag in character.Bags)
                        {
                            if (bag != null)
                            {
                                foreach (var item in bag.Inventory)
                                {
                                    if (item != null && tokensId.Contains(item.Id))
                                        res = $"{res}{item.Count} {((Token)item.Id).GetDisplayName()} ({character.Name})\n";
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.Warn("Failed to retrieve character bags");
                    }
                }
            }
            else
            {
                _logger.Warn("Failed to retrieve characters.");
            }

            return res;
        }
    }
}
