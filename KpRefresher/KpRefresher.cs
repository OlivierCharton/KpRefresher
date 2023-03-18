using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Flurl.Http;
using Gw2Sharp.WebApi.Http;
using Gw2Sharp.WebApi.V2.Models;
using KpRefresher.Domain;
using KpRefresher.UI.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace KpRefresher
{
    [Export(typeof(Module))]
    public class KpRefresher : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<KpRefresher>();
        private FlurlClient _flurlClient;

        private bool _isFirstLoad = true;
        private int _mapId { get; set; }
        private List<int> _raidMapIds { get; set; }
        private List<int> _strikeMapIds { get; set; }
        private static List<string> _raidBossNames;
        private bool _playerWasInRaid { get; set; }
        private bool _playerWasInStrike { get; set; }
        private List<string> _baseRaidClears { get; set; }

        private int _numberOfRefreshTry;



        public static ModuleSettings ModuleSettings { get; private set; }

        #region Service Managers

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        #endregion

        // Ideally you should keep the constructor as is.
        // Use <see cref="Initialize"/> to handle initializing the module.
        [ImportingConstructor]
        public KpRefresher([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            KpRefresherInstance = this;
        }

        protected IFlurlClient GetFlurlClient()
        {
            if (this._flurlClient == null)
            {
                this._flurlClient = new FlurlClient();
                //this._flurlClient.WithHeader("User-Agent", $"{this.Name} {this.Version}");
            }

            return this._flurlClient;
        }

        // Define the settings you would like to use in your module.  Settings are persistent
        // between updates to both Blish HUD and your module.
        protected override void DefineSettings(SettingCollection settings)
        {
            ModuleSettings = new ModuleSettings(settings);
        }

        // Allows your module to perform any initialization it needs before starting to run.
        // Please note that Initialize is NOT asynchronous and will block Blish HUD's update
        // and render loop, so be sure to not do anything here that takes too long.
        protected override void Initialize()
        {
            Gw2ApiManager.SubtokenUpdated += OnApiSubTokenUpdated;

            _raidMapIds = Enum.GetValues(typeof(RaidMap))
                            .Cast<RaidMap>()
                            .Select(m => (int)m)
                            .ToList();

            _strikeMapIds = Enum.GetValues(typeof(StrikeMap))
                            .Cast<StrikeMap>()
                            .Select(m => (int)m)
                            .ToList();

            _raidBossNames = new List<string>() { "sabetha", "matthias", "xera", "deimos", "voice_in_the_void", "qadim", "qadim_the_peerless" };
        }

        private async void OnApiSubTokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            _baseRaidClears = await GetRaidClears();
        }

        private async Task<List<string>> GetRaidClears()
        {
            if (Gw2ApiManager.HasPermissions(Gw2ApiManager.Permissions) == false)
            {
                ScreenNotification.ShowNotification("Les permissions ne sont pas encore chargées", ScreenNotification.NotificationType.Warning);
                return null;
            }

            try
            {
                var data = await Gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync();
                return data?.ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while getting raid clears : {ex.Message}");
                return null;
            }
        }

        protected override async Task LoadAsync()
        {
            _baseRaidClears = await GetRaidClears();

            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;

            // Load content from the ref directory in the module.bhm automatically with the ContentsManager
            _cornerIconTexture = ContentsManager.GetTexture("killproof_logo_dark.png");
            _windowBackgroundTexture = ContentsManager.GetTexture("155985.png");

            _mainWindow = new KpRefresherWindow(
                _windowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(70, 71, 839, 605),
                _cornerIconTexture,
                ModuleSettings,
                Gw2ApiManager);
        }

        private async void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {
            _mapId = GameService.Gw2Mumble.CurrentMap.Id;

            if (_isFirstLoad)
            {
                _isFirstLoad = false;
                return;
            }

            if (_raidMapIds.Contains(_mapId))
            {
                ScreenNotification.ShowNotification("Vous êtes en raid !", ScreenNotification.NotificationType.Warning);
                _playerWasInRaid = true;
            }
            else if (_strikeMapIds.Contains(_mapId))
            {
                ScreenNotification.ShowNotification("Vous êtes en mission d'attaque !", ScreenNotification.NotificationType.Warning);
                _playerWasInStrike = true;
            }
            else
            {
                if (_playerWasInRaid)
                {
                    _playerWasInRaid = false;
                    ScreenNotification.ShowNotification("Sortie du mode raid !", ScreenNotification.NotificationType.Warning);
                    _numberOfRefreshTry = 15;

                }
                else if (_playerWasInStrike)
                {
                    _playerWasInStrike = false;
                    ScreenNotification.ShowNotification("Sortie du mode mission d'attaque !", ScreenNotification.NotificationType.Warning);
                    _numberOfRefreshTry = 15;
                }
            }
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            HandleCornerIcon();

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void HandleCornerIcon()
        {
            _cornerIcon = new CornerIcon()
            {
                Icon = _cornerIconTexture,
                BasicTooltipText = $"{Name}",
                Parent = GameService.Graphics.SpriteScreen
            };

            _cornerIcon.Click += delegate
            {
                _mainWindow.ToggleWindow();
            };

            _cornerIconContextMenu = new ContextMenuStrip();

            var refeshKpMenuItem = new ContextMenuStripItem("Scan KP now");
            refeshKpMenuItem.Click += (s, e) =>
            {
                KpMe();
            };

            _cornerIconContextMenu.AddMenuItem(refeshKpMenuItem);
            _cornerIcon.Menu = _cornerIconContextMenu;
        }

        protected override void Update(GameTime gameTime)
        {
            _runningTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_runningTime > 60000)
            {
                _runningTime -= 60000;

                if (_numberOfRefreshTry > 0)
                {
                    CheckRaidClears();
                }
            }
        }

        private async void CheckRaidClears()
        {
            bool hasNewClear = false;
            var clears = await GetRaidClears();

            //Detects if we have a new final boss clear
            foreach (var bossName in _raidBossNames)
            {
                if (!_baseRaidClears.Contains(bossName) && clears.Contains(bossName))
                {
                    hasNewClear = true;
                    break;
                }
            }

            if (hasNewClear)
            {
                //Calls Kp.Me to refresh kp
                var kpRefreshed = await KpMe();
                _baseRaidClears = clears;
                _numberOfRefreshTry = 0;
            }
            else
            {
                _numberOfRefreshTry -= 1;
            }
        }

        private async Task<bool> KpMe()
        {
            if (string.IsNullOrWhiteSpace(ModuleSettings.KpMeId.Value))
                ScreenNotification.ShowNotification("L'id Kp.Me n'est pas défini !", ScreenNotification.NotificationType.Error);

            var url = $"https://killproof.me/proof/{ModuleSettings.KpMeId.Value}/refresh";

            var client = GetFlurlClient();
            var response = await client.Request(url).GetAsync();

            if (response != null)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
                else if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                    return true;

            }

            return false;

        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            Gw2ApiManager.SubtokenUpdated -= OnApiSubTokenUpdated;
            GameService.Gw2Mumble.CurrentMap.MapChanged -= this.CurrentMap_MapChanged;

            _cornerIcon?.Dispose();
            _cornerIconContextMenu?.Dispose();
            _mainWindow?.Dispose();
            _windowBackgroundTexture?.Dispose();
            _cornerIconTexture?.Dispose();

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            KpRefresherInstance = null;
        }

        internal static KpRefresher KpRefresherInstance;
        private Texture2D _windowBackgroundTexture;
        private Texture2D _cornerIconTexture;
        private CornerIcon _cornerIcon;
        private ContextMenuStrip _cornerIconContextMenu;
        private double _runningTime;
        private KpRefresherWindow _mainWindow;
    }
}