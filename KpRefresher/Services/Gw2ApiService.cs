using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
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

        public async Task<List<RaidBoss>> GetClears()
        {
            if (_gw2ApiManager.HasPermissions(_gw2ApiManager.Permissions) == false)
            {
                _logger.Warn("Permissions not granted.");
                return null;
            }

            try
            {
                var data = await _gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync();

                if (data == null)
                    return null;

                return data.Select(d => (RaidBoss)Enum.Parse(typeof(RaidBoss), d)).ToList();
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
                    res = $"{res}{GetFormatedResult(bankItems, strings.GW2APIService_Bank)}";
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
                    res = $"{res}{GetFormatedResult(sharedInventoryItems, strings.GW2APIService_SharedSlots)}";
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
                    List<(Token, int)> characterTokens = new();
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
                                            characterTokens.Add(((Token)item.Id, item.Count));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.Warn("Failed to retrieve character bags");
                        }

                        if (characterTokens.Count > 0)
                        {
                            var characterData = string.Empty;
                            foreach (var token in ListExtensions.SortByEncounter(characterTokens))
                            {
                                characterData = $"{characterData}{token.Item2}   {token.Item1.GetDisplayName()}\n";
                            }

                            res = $"{res}[{character.Name}]\n{characterData}\n";

                            characterTokens.Clear();
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

        private string GetFormatedResult(IApiV2ObjectList<AccountItem> items, string category)
        {
            var tokensId = _tokens.Select(c => (int)c).ToList();

            List<(Token, int)> tokens = new();
            foreach (var item in items)
            {
                if (item != null && tokensId.Contains(item.Id))
                {
                    tokens.Add(((Token)item.Id, item.Count));
                }
            }

            if (tokens.Count > 0)
            {
                var data = string.Empty;
                foreach (var token in ListExtensions.SortByEncounter(tokens))
                {
                    data = $"{data}{token.Item2}   {token.Item1.GetDisplayName()}\n";
                }

                return $"[{category}]\n{data}\n";
            }

            return string.Empty;
        }
    }
}