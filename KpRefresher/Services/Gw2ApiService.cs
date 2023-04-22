using Blish_HUD;
using Blish_HUD.Modules.Managers;
using KpRefresher.Domain;
using KpRefresher.Extensions;
using KpRefresher.Ressources;
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

        private List<Token> _tokens { get; set; }

        public Gw2ApiService(Gw2ApiManager gw2ApiManager, Logger logger)
        {
            _gw2ApiManager = gw2ApiManager;
            _logger = logger;

            _tokens = Enum.GetValues(typeof(Token))
                            .Cast<Token>()
                            .ToList();
        }

        public async Task<string> GetAccountName()
        {
            if (_gw2ApiManager.HasPermissions(_gw2ApiManager.Permissions) == false)
            {
                _logger.Warn("Permissions not granted.");
                return string.Empty;
            }

            try
            {
                var account = await _gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();
                return account?.Name;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Error while getting account name : {ex.Message}");
                return null;
            }
        }

        public async Task<List<string>> GetClears()
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
                _logger.Warn($"Error while getting raid clears : {ex.Message}");
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

            try
            {
                var bankItems = await _gw2ApiManager.Gw2ApiClient.V2.Account.Bank.GetAsync();

                if (bankItems != null)
                {
                    var bankHasData = false;
                    var bankData = string.Empty;
                    foreach (var item in bankItems)
                    {
                        if (item != null && tokensId.Contains(item.Id))
                        {
                            bankHasData = true;
                            bankData = $"{bankData}{item.Count}   {((Token)item.Id).GetDisplayName()}\n";
                        }
                    }

                    if (bankHasData)
                        res = $"{res}[{strings.GW2APIService_Bank}]\n{bankData}\n";
                }
                else
                {
                    _logger.Warn("Failed to retrieve bank items.");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to retrieve bank items : {ex}");
            }

            try
            {
                var sharedInventoryItems = await _gw2ApiManager.Gw2ApiClient.V2.Account.Inventory.GetAsync();
                if (sharedInventoryItems != null)
                {
                    var sharedInventoryHasData = false;
                    var sharedInventoryData = string.Empty;
                    foreach (var item in sharedInventoryItems)
                    {
                        if (item != null && tokensId.Contains(item.Id))
                        {
                            sharedInventoryHasData = true;
                            sharedInventoryData = $"{sharedInventoryData}{item.Count}   {((Token)item.Id).GetDisplayName()}\n";
                        }
                    }

                    if (sharedInventoryHasData)
                        res = $"{res}[{strings.GW2APIService_SharedSlots}]\n{sharedInventoryData}\n";
                }
                else
                {
                    _logger.Warn("Failed to retrieve shared inventory items.");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to retrieve shared inventory items : {ex}");
            }

            try
            {
                var characters = await _gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                if (characters != null)
                {
                    var characterHasData = false;
                    var characterData = string.Empty;
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
                                        {
                                            characterHasData = true;
                                            characterData = $"{characterData}{item.Count}   {((Token)item.Id).GetDisplayName()}\n";
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.Warn("Failed to retrieve character bags");
                        }

                        if (characterHasData)
                        {
                            res = $"{res}[{character.Name}]\n{characterData}\n";

                            characterHasData = false;
                            characterData = string.Empty;
                        }
                    }
                }
                else
                {
                    _logger.Warn("Failed to retrieve characters.");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to retrieve characters : {ex}");
            }

            return res;
        }
    }
}