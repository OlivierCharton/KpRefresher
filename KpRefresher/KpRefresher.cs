using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using KpRefresher.Services;
using KpRefresher.UI.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace KpRefresher
{
    [Export(typeof(Module))]
    public class KpRefresher : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<KpRefresher>();

        //FEATURE CHANGE MAP
        //private bool _isFirstLoad = true;
        //private int _mapId { get; set; }
        //private List<int> _raidMapIds { get; set; }
        //private List<int> _strikeMapIds { get; set; }
        //private bool _playerWasInRaid { get; set; }
        //private bool _playerWasInStrike { get; set; }

        private const int _delayBetweenKpRefresh = 5 * 60 * 1000;

        public static ModuleSettings ModuleSettings { get; private set; }
        public static RaidService RaidService { get; private set; }

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
            RaidService = new RaidService(ModuleSettings, Gw2ApiManager, Logger);

            Gw2ApiManager.SubtokenUpdated += OnApiSubTokenUpdated;

            //FEATURE CHANGE MAP
            //_raidMapIds = Enum.GetValues(typeof(RaidMap))
            //                .Cast<RaidMap>()
            //                .Select(m => (int)m)
            //                .ToList();

            //_strikeMapIds = Enum.GetValues(typeof(StrikeMap))
            //                .Cast<StrikeMap>()
            //                .Select(m => (int)m)
            //                .ToList();
        }

        private async void OnApiSubTokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            await RaidService.InitBaseRaidClears();
        }

        protected override async Task LoadAsync()
        {
            await RaidService.InitBaseRaidClears();

            //FEATURE CHANGE MAP
            //GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;

            // Load content from the ref directory in the module.bhm automatically with the ContentsManager
            _cornerIconTexture = ContentsManager.GetTexture("killproof_logo_dark.png");
            _windowBackgroundTexture = ContentsManager.GetTexture("155985.png");

            _mainWindow = new KpRefresherWindow(
                _windowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(50, 36, 893, 671),
                _cornerIconTexture,
                ModuleSettings,
                RaidService);
        }

        //FEATURE CHANGE MAP
        //private async void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        //{
        //    _mapId = GameService.Gw2Mumble.CurrentMap.Id;

        //    if (_isFirstLoad)
        //    {
        //        _isFirstLoad = false;
        //        return;
        //    }

        //    if (_raidMapIds.Contains(_mapId))
        //    {
        //        ScreenNotification.ShowNotification("Vous êtes en raid !", ScreenNotification.NotificationType.Warning);
        //        _playerWasInRaid = true;
        //    }
        //    else if (_strikeMapIds.Contains(_mapId))
        //    {
        //        ScreenNotification.ShowNotification("Vous êtes en mission d'attaque !", ScreenNotification.NotificationType.Warning);
        //        _playerWasInStrike = true;
        //    }
        //    else
        //    {
        //        if (_playerWasInRaid)
        //        {
        //            _playerWasInRaid = false;
        //            ScreenNotification.ShowNotification("Sortie du mode raid !", ScreenNotification.NotificationType.Warning);
        //            _numberOfRefreshTry = 15;

        //        }
        //        else if (_playerWasInStrike)
        //        {
        //            _playerWasInStrike = false;
        //            ScreenNotification.ShowNotification("Sortie du mode mission d'attaque !", ScreenNotification.NotificationType.Warning);
        //            _numberOfRefreshTry = 15;
        //        }
        //    }
        //}

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

            var refeshKpMenuItem = new ContextMenuStripItem("Refresh Killproof.me data");
            refeshKpMenuItem.Click += async (s, e) =>
            {
                await RaidService.RefreshKillproofMe();
            };

            _cornerIconContextMenu.AddMenuItem(refeshKpMenuItem);
            _cornerIcon.Menu = _cornerIconContextMenu;
        }

        protected override void Update(GameTime gameTime)
        {
            _runningTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            RaidService.TriggerTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (RaidService.RefreshTriggered)
            {
                if (RaidService.TriggerTimer > _delayBetweenKpRefresh)
                {
                    _ = RaidService.RefreshKillproofMe();
                }
            }
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            Gw2ApiManager.SubtokenUpdated -= OnApiSubTokenUpdated;

            //FEATURE CHANGE MAP
            //GameService.Gw2Mumble.CurrentMap.MapChanged -= this.CurrentMap_MapChanged;

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
        private KpRefresherWindow _mainWindow;
        private double _runningTime;
    }
}