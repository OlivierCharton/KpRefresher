using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace KpRefresher
{
    [Export(typeof(Module))]
    public class KpRefresher : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<KpRefresher>();

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
            settings.DefineSetting(
                "ExampleSetting",
                "This is the default value of the setting",
                () => "Display name of setting",
                () => "Tooltip text of setting");

            _stringExampleSetting = settings.DefineSetting(
                "kpId",
                "",
                () => "This is an string setting (textbox)",
                () => "Settings can be many different types");
        }

        // Allows your module to perform any initialization it needs before starting to run.
        // Please note that Initialize is NOT asynchronous and will block Blish HUD's update
        // and render loop, so be sure to not do anything here that takes too long.
        protected override void Initialize()
        {
        }

        protected override async Task LoadAsync()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged += this.CurrentMap_MapChanged;

            // Load content from the ref directory in the module.bhm automatically with the ContentsManager
            _cornerIconTexture = ContentsManager.GetTexture("killproof_logo_dark.png");
            _windowBackgroundTexture = ContentsManager.GetTexture("155985.png");

            // show a window with gw2 window style.
            _exampleWindow = new StandardWindow(
                _windowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(70, 71, 839, 605))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Kp Refresher",
                Emblem = _cornerIconTexture,
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(KpRefresher)}_My_Unique_ID_123"
            };

            var pannel = new Panel()
            {
                ShowBorder = true,
                Title = "Configuration",
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Location = new Point(0, 0),
                Parent = _exampleWindow,
            };

            var label = new Label()
            {
                Text = "Identifiant kp.me : ",
                Parent = pannel,
                AutoSizeWidth= true
            };

            var textBox = new TextBox()
            {
                Parent = pannel,
                Location = new Point(label.Right + 5, label.Top)
            };

            //textBox.EnterPressed += SaveKpId;

            // show blish hud overlay settings content inside the window
            //_exampleWindow.Show(new OverlaySettingsView());
        }

        private async void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {
            ScreenNotification.ShowNotification("Changement de carte !", ScreenNotification.NotificationType.Warning);
        }

        //private void SaveKpId(EventHandler<EventArgs<string>> e)
        //{

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
                _exampleWindow.ToggleWindow();
            };

            _cornerIconContextMenu = new ContextMenuStrip();
            _cornerIconContextMenu.AddMenuItem("Scan KP now");
            _cornerIcon.Menu = _cornerIconContextMenu;
        }

        protected override void Update(GameTime gameTime)
        {
            _runningTime += gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= this.CurrentMap_MapChanged;

            _cornerIcon?.Dispose();
            _cornerIconContextMenu?.Dispose();
            _exampleWindow?.Dispose();
            _windowBackgroundTexture?.Dispose();
            _cornerIconTexture?.Dispose();

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            KpRefresherInstance = null;
        }

        internal static KpRefresher KpRefresherInstance;
        private SettingEntry<string> _stringExampleSetting;
        private Texture2D _windowBackgroundTexture;
        private Texture2D _cornerIconTexture;
        private CornerIcon _cornerIcon;
        private ContextMenuStrip _cornerIconContextMenu;
        private double _runningTime;
        private StandardWindow _exampleWindow;
    }
}