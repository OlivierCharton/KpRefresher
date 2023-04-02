using Blish_HUD;
using Blish_HUD.Content;
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
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace KpRefresher
{
    [Export(typeof(Module))]
    public class KpRefresher : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<KpRefresher>();
        
        public static ModuleSettings ModuleSettings { get; private set; }
        public static Gw2ApiService Gw2ApiService { get; private set; }
        public static KpMeService KpMeService { get; private set; }
        public static BusinessService BusinessService { get; private set; }

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
            Gw2ApiService = new Gw2ApiService(Gw2ApiManager, Logger);
            KpMeService = new KpMeService(Logger);
            BusinessService = new BusinessService(ModuleSettings, Gw2ApiService, KpMeService, () => _apiSpinner);

            Gw2ApiManager.SubtokenUpdated += OnApiSubTokenUpdated;
        }

        private async void OnApiSubTokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            await BusinessService.RefreshBaseData();
        }

        protected override async Task LoadAsync()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;

            // Load textures
            _emblemTexture = ContentsManager.GetTexture("emblem.png");
            _cornerIconTexture = ContentsManager.GetTexture("corner.png");
            _cornerIconHoverTexture = ContentsManager.GetTexture("corner-hover.png");
            _windowBackgroundTexture = AsyncTexture2D.FromAssetId(155985);

            _mainWindow = new(
                _windowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(50, 26, 893, 681),
                _emblemTexture,
                ModuleSettings,
                BusinessService)
            {
                //Resize the window to prevent background texture to overflow
                Size = new Point(500, 700)
            };
            _mainWindow.BuildUi();
        }

        private void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {
            if (ModuleSettings.RefreshOnMapChange.Value)
            {
                BusinessService.MapChanged();
                _mainWindow.RefreshLoadingSpinnerState();
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
                Parent = GameService.Graphics.SpriteScreen,
                HoverIcon = _cornerIconHoverTexture
            };

            _cornerIcon.Click += delegate
            {
                _mainWindow.ToggleWindow();
            };

            _cornerIconContextMenu = new ContextMenuStrip();

            var refeshKpMenuItem = new ContextMenuStripItem("Refresh KillProof.me data");
            refeshKpMenuItem.Click += async (s, e) =>
            {
                await BusinessService.RefreshKillproofMe();
            };

            var copyKpToClipboard = new ContextMenuStripItem("Copy KillProof.me Id to clipboard");
            copyKpToClipboard.Click += (s, e) =>
            {
                _ = BusinessService.CopyKpToClipboard();
            };

            _notificationNextRefreshAvailable = new ContextMenuStripItem("Notify when refresh available");
            _notificationNextRefreshAvailable.Click += async (s, e) =>
            {
                if (!BusinessService.NotificationNextRefreshAvailabledActivated)
                {
                    await BusinessService.ActivateNotificationNextRefreshAvailable();
                    _notificationNextRefreshAvailable.Text = "Cancel notification for next refresh";
                }
                else
                {
                    BusinessService.ResetNotificationNextRefreshAvailable();
                    _notificationNextRefreshAvailable.Text = "Notify when refresh available";
                }
            };

            var openKpUrl = new ContextMenuStripItem("Open KillProof.me website");
            openKpUrl.Click += (s, e) =>
            {
                _ = BusinessService.OpenKpUrl();
            };

            _cornerIconContextMenu.AddMenuItem(refeshKpMenuItem);
            _cornerIconContextMenu.AddMenuItem(copyKpToClipboard);
            _cornerIconContextMenu.AddMenuItem(_notificationNextRefreshAvailable);
            _cornerIconContextMenu.AddMenuItem(openKpUrl);

            _cornerIcon.Menu = _cornerIconContextMenu;

            _apiSpinner = new LoadingSpinner()
            {
                Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(_cornerIcon.Width, _cornerIcon.Height),
                BasicTooltipText = "Fetching Api Data",
                Visible = false
            };
        }

        protected override void Update(GameTime gameTime)
        {
            if (BusinessService.RefreshScheduled)
            {
                BusinessService.ScheduleTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (BusinessService.ScheduleTimer > BusinessService.ScheduleTimerEndValue)
                {
                    _ = BusinessService.RefreshKillproofMe(true).ContinueWith(task => _mainWindow.RefreshLoadingSpinnerState());
                }
            }

            if (BusinessService.NotificationNextRefreshAvailabledActivated)
            {
                BusinessService.NotificationNextRefreshAvailabledTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (BusinessService.NotificationNextRefreshAvailabledTimer > BusinessService.NotificationNextRefreshAvailabledTimerEndValue)
                {
                    BusinessService.NextRefreshIsAvailable();
                    _notificationNextRefreshAvailable.Text = "Notify when refresh available";
                }
            }
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            Gw2ApiManager.SubtokenUpdated -= OnApiSubTokenUpdated;

            GameService.Gw2Mumble.CurrentMap.MapChanged -= CurrentMap_MapChanged;

            _cornerIcon?.Dispose();
            _cornerIconContextMenu?.Dispose();
            _apiSpinner?.Dispose();
            _mainWindow?.Dispose();
            _windowBackgroundTexture?.Dispose();
            _emblemTexture?.Dispose();
            _cornerIconTexture?.Dispose();
            _cornerIconHoverTexture?.Dispose();

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            KpRefresherInstance = null;
        }

        internal static KpRefresher KpRefresherInstance;
        private AsyncTexture2D _windowBackgroundTexture;
        private Texture2D _emblemTexture;
        private Texture2D _cornerIconTexture;
        private Texture2D _cornerIconHoverTexture;
        private CornerIcon _cornerIcon;
        private ContextMenuStrip _cornerIconContextMenu;
        private LoadingSpinner _apiSpinner;
        private KpRefresherWindow _mainWindow;
        private ContextMenuStripItem _notificationNextRefreshAvailable;
    }
}