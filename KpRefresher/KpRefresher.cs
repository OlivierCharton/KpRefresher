using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.GameIntegration;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using KpRefresher.Ressources;
using KpRefresher.Services;
using KpRefresher.UI.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Controls = KpRefresher.UI.Controls;
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
        public static Controls.CornerIcon CornerIcon { get; private set; }

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
            // SOTO Fix
            if (Program.OverlayVersion < new SemVer.Version(1, 1, 0))
            {
                try
                {
                    var tacoActive = typeof(TacOIntegration).GetProperty(nameof(TacOIntegration.TacOIsRunning)).GetSetMethod(true);
                    tacoActive?.Invoke(GameService.GameIntegration.TacO, new object[] { true });
                }
                catch { /* NOOP */ }
            }

            CornerIcon = new Controls.CornerIcon(ContentsManager)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Priority = 1283537108
            };

            Gw2ApiService = new Gw2ApiService(Gw2ApiManager, Logger);
            KpMeService = new KpMeService(Logger);
            BusinessService = new BusinessService(ModuleSettings, Gw2ApiService, KpMeService, () => _apiSpinner, CornerIcon, Logger);

            Gw2ApiManager.SubtokenUpdated += OnApiSubTokenUpdated;

            GameService.Overlay.UserLocale.SettingChanged += OnLocaleChanged;
        }

        private async void OnApiSubTokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            await BusinessService.RefreshBaseData();
        }

        private void OnLocaleChanged(object sender, ValueChangedEventArgs<Locale> eventArgs)
        {
            LocalizingService.OnLocaleChanged(sender, eventArgs);
        }

        protected override async Task LoadAsync()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;

            // Load textures
            _emblemTexture = ContentsManager.GetTexture("emblem.png");
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
                Size = new Point(520, 700)
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
            CornerIcon.Click += delegate
            {
                _mainWindow.ToggleWindow();
            };

            _cornerIconContextMenu = new ContextMenuStrip();

            var refeshKpMenuItem = new Controls.ContextMenuStripItem()
            {
                SetLocalizedText = () => strings.CornerIcon_Refresh,
            };
            refeshKpMenuItem.Click += async (s, e) =>
            {
                await BusinessService.RefreshKillproofMe();
            };

            var copyKpToClipboard = new Controls.ContextMenuStripItem()
            {
                SetLocalizedText = () => strings.CornerIcon_Copy,
            };
            copyKpToClipboard.Click += (s, e) =>
            {
                _ = BusinessService.CopyKpToClipboard();
            };

            _notificationNextRefreshAvailable = new Controls.ContextMenuStripItem
            {
                SetLocalizedText = () => strings.CornerIcon_Notify,
            };
            _notificationNextRefreshAvailable.Click += async (s, e) =>
            {
                if (!BusinessService.NotificationNextRefreshAvailabledActivated)
                {
                    var notificationPlanned = await BusinessService.ActivateNotificationNextRefreshAvailable();
                    if (notificationPlanned)
                        _notificationNextRefreshAvailable.SetLocalizedText = () => strings.CornerIcon_CancelNotify;
                }
                else
                {
                    BusinessService.ResetNotificationNextRefreshAvailable();
                    _notificationNextRefreshAvailable.SetLocalizedText = () => strings.CornerIcon_Notify;
                }
            };

            var openKpUrl = new Controls.ContextMenuStripItem
            {
                SetLocalizedText = () => strings.CornerIcon_OpenWebsite,
            };
            openKpUrl.Click += (s, e) =>
            {
                _ = BusinessService.OpenKpUrl();
            };

            _cornerIconContextMenu.AddMenuItem(refeshKpMenuItem);
            _cornerIconContextMenu.AddMenuItem(copyKpToClipboard);
            _cornerIconContextMenu.AddMenuItem(_notificationNextRefreshAvailable);
            _cornerIconContextMenu.AddMenuItem(openKpUrl);

            CornerIcon.Menu = _cornerIconContextMenu;

            _apiSpinner = new Controls.LoadingSpinner()
            {
                Location = new Point(CornerIcon.Left, CornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(CornerIcon.Width, CornerIcon.Height),
                SetLocalizedTooltip = () => strings.LoadingSpinner_Fetch,
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
                    _notificationNextRefreshAvailable.SetLocalizedText = () => strings.CornerIcon_Notify;
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

            CornerIcon?.Dispose();
            _cornerIconContextMenu?.Dispose();
            _apiSpinner?.Dispose();
            _mainWindow?.Dispose();
            _emblemTexture?.Dispose();

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            KpRefresherInstance = null;
        }

        internal static KpRefresher KpRefresherInstance;
        private AsyncTexture2D _windowBackgroundTexture;
        private Texture2D _emblemTexture;
        private ContextMenuStrip _cornerIconContextMenu;
        private Controls.LoadingSpinner _apiSpinner;
        private KpRefresherWindow _mainWindow;
        private Controls.ContextMenuStripItem _notificationNextRefreshAvailable;
    }
}