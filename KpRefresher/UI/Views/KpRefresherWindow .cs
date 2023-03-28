using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using KpRefresher.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;

namespace KpRefresher.UI.Views
{
    public class KpRefresherWindow : StandardWindow
    {
        private readonly ModuleSettings _moduleSettings;
        private readonly BusinessService _businessService;
        private readonly List<StandardButton> _buttons = new();

        private LoadingSpinner _loadingSpinner;
        private Panel _notificationsContainer;
        private Label _notificationLabel;
        private Checkbox _showAutoRetryNotificationCheckbox;
        private Checkbox _onlyRefreshOnFinalBossKillCheckbox;

        public KpRefresherWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion,
            AsyncTexture2D cornerIconTexture, ModuleSettings moduleSettings, BusinessService businessService) : base(background, windowRegion, contentRegion)
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = "KillProof.me Refresher";
            Emblem = cornerIconTexture;
            Location = new Point(300, 300);
            SavesPosition = true;

            _moduleSettings = moduleSettings;
            _businessService = businessService;
        }

        public void BuildUi()
        {
            FlowPanel mainContainer = new()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new(3, 3)
            };

            #region Config
            FlowPanel configContainer = new()
            {
                Parent = mainContainer,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title = "Configuration",
                ShowBorder = true,
                CanCollapse = true,
                OuterControlPadding = new(5),
                ControlPadding = new(5),
            };

            var checkbox_controls = CreateLabeledControl<Checkbox>("Enable auto-retry", "Schedule automatically a new try when KillProof.me was not available for a refresh", configContainer);
            checkbox_controls.control.Checked = _moduleSettings.EnableAutoRetry.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.EnableAutoRetry.Value = e.Checked;
                _showAutoRetryNotificationCheckbox.Enabled = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>("Show auto-retry notifications", "Display notification when retry is scheduled", configContainer);
            _showAutoRetryNotificationCheckbox = checkbox_controls.control;
            checkbox_controls.control.Enabled = _moduleSettings.EnableAutoRetry.Value;
            checkbox_controls.control.Checked = _moduleSettings.ShowScheduleNotification.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.ShowScheduleNotification.Value = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>("Condition refresh to clear", "Only allow refresh if a clear was made and is visible by GW2 API", configContainer);
            checkbox_controls.control.Checked = _moduleSettings.EnableRefreshOnKill.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.EnableRefreshOnKill.Value = e.Checked;
                _onlyRefreshOnFinalBossKillCheckbox.Enabled = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>("Refresh on final boss kill", "Only refresh if a final raid wing boss was cleared (e.g. Sabetha)", configContainer);
            _onlyRefreshOnFinalBossKillCheckbox = checkbox_controls.control;
            checkbox_controls.control.Enabled = _moduleSettings.EnableRefreshOnKill.Value;
            checkbox_controls.control.Checked = _moduleSettings.RefreshOnKillOnlyBoss.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.RefreshOnKillOnlyBoss.Value = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>("Refresh on map change", "Schedule a refresh when leaving a raid or strike map", configContainer);
            checkbox_controls.control.Checked = _moduleSettings.RefreshOnMapChange.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.RefreshOnMapChange.Value = e.Checked;
            };

            var (panel, label, control) = CreateLabeledControl<TextBox>("Delay before refresh", "Time in minutes before refresh is triggered after map change (between 1 and 60)", configContainer);
            control.Text = _moduleSettings.DelayBeforeRefreshOnMapChange.Value.ToString();
            control.TextChanged += (s, e) =>
            {
                string txt = (s as TextBox).Text.Trim();

                if (string.IsNullOrWhiteSpace(txt))
                {
                    _moduleSettings.DelayBeforeRefreshOnMapChange.Value = 1;
                }
                else if (int.TryParse(txt, out int newValue))
                {
                    //Only allow value between 1 and 60
                    newValue = Math.Max(newValue, 1);
                    newValue = Math.Min(newValue, 60);

                    _moduleSettings.DelayBeforeRefreshOnMapChange.Value = newValue;
                };

                control.Text = _moduleSettings.DelayBeforeRefreshOnMapChange.Value.ToString();
            };
            #endregion Config

            #region Actions
            FlowPanel actionContainer = new()
            {
                Parent = mainContainer,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title = "Actions",
                ShowBorder = true,
                CanCollapse = true,
                OuterControlPadding = new(5),
                ControlPadding = new(5),
            };
            actionContainer.ContentResized += ActionContainer_ContentResized;

            StandardButton button;
            _buttons.Add(button = new StandardButton()
            {
                Text = "Refresh KillProof.me",
                BasicTooltipText = "Attempts to refresh KillProof.me\nIf auto-retry is enable, a new refresh will be scheduled in case of failure",
                Parent = actionContainer
            });
            button.Click += async (s, e) => await RefreshRaidClears();

            _buttons.Add(button = new StandardButton()
            {
                Text = "Refresh linked accounts",
                BasicTooltipText = "Attempts to refresh all linked KillProof.me accounts",
                Parent = actionContainer
            });
            button.Click += async (s, e) =>
            {
                if (_businessService.LinkedKpId?.Count > 0)
                {
                    string res = await _businessService.RefreshLinkedAccounts();
                    ShowInsideNotification($"{_businessService.LinkedKpId?.Count} linked account{(_businessService.LinkedKpId?.Count > 1 ? "s" : string.Empty)} found !\n{res}", true);
                }
                else
                {
                    ShowInsideNotification("No linked account found !");
                }
            };

            _buttons.Add(button = new StandardButton()
            {
                Text = "Show new clears",
                BasicTooltipText = "Displays new clears made since last KillProof.me refresh",
                Parent = actionContainer
            });
            button.Click += async (s, e) => await DisplayRaidDifference();

            _buttons.Add(button = new StandardButton()
            {
                Text = "Show current KP",
                BasicTooltipText = "Scan your bank, shared slots and characters and displays current KP according GW2 API.\nEvery kp in the list is able to be scanned by KillProof.me, if not already scanned. You can use this feature to check if a newly opened chest is already visible for KillProof.me.",
                Parent = actionContainer
            });
            button.Click += async (s, e) => await DisplayCurrentKp();

            _buttons.Add(button = new StandardButton()
            {
                Text = "Clear schedule",
                BasicTooltipText = "Resets any scheduled refresh",
                Parent = actionContainer
            });
            button.Click += (s, e) => StopRetry();

            _buttons.Add(button = new StandardButton()
            {
                Text = "Clear notifications",
                Parent = actionContainer
            });
            button.Click += (s, e) => ClearNotifications();
            #endregion Actions

            #region Notifications
            _notificationsContainer = new()
            {
                Parent = mainContainer,
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
            };

            #region Spinner
            _loadingSpinner = new LoadingSpinner()
            {
                Parent = _notificationsContainer,
                Size = new Point(29, 29),
                Visible = false,
            };
            _loadingSpinner.MouseEntered += (s, e) =>
            {
                var nextRefresh = _businessService.GetNextScheduledTimer();
                var totalMinutes = (int)nextRefresh.TotalMinutes;
                if (totalMinutes >= 1)
                    _loadingSpinner.BasicTooltipText = $"Next retry in {totalMinutes} minute{(totalMinutes > 1 ? "s" : string.Empty)}.";
                else
                    _loadingSpinner.BasicTooltipText = $"Next retry in {(int)nextRefresh.TotalSeconds} second{((int)nextRefresh.TotalSeconds > 1 ? "s" : string.Empty)}.";
            };
            #endregion Spinner

            _notificationLabel = new Label()
            {
                Location = new(_loadingSpinner.Right + 5, _loadingSpinner.Top),
                Parent = _notificationsContainer,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Font = GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size24, FontStyle.Regular),

                WrapText = true
            };
            #endregion Notifications
        }

        private void ActionContainer_ContentResized(object sender, RegionChangedEventArgs e)
        {
            if (_buttons?.Count >= 0)
            {
                int columns = 2;
                var parent = _buttons.FirstOrDefault()?.Parent as FlowPanel;
                int width = (parent?.ContentRegion.Width - (int)parent.OuterControlPadding.X - ((int)parent.ControlPadding.X * (columns - 1))) / columns ?? 100;

                foreach (var button in _buttons)
                {
                    button.Width = width;
                }
            }
        }

        public void RefreshLoadingSpinnerState()
        {
            _loadingSpinner.Visible = _businessService.RefreshScheduled;
        }

        private async Task RefreshRaidClears()
        {
            //TODO: maybe disable Refresh btn if we can find a way to auto reactivate it from Service ?

            _loadingSpinner.Visible = true;

            await _businessService.RefreshKillproofMe();

            //Keeps the spinner visible if a refresh has been scheduled
            RefreshLoadingSpinnerState();
        }

        private async Task DisplayRaidDifference()
        {
            ShowInsideNotification("Loading ...", true);

            var data = await _businessService.GetDelta();
            ShowInsideNotification(data, true);
        }

        private void StopRetry()
        {
            if (_businessService.RefreshScheduled)
            {
                _businessService.CancelSchedule();
                ShowInsideNotification("Scheduled refresh disabled !");
            }
            else
            {
                ShowInsideNotification("No scheduled refresh");
            }

            _loadingSpinner.Visible = false;
        }

        private void ShowInsideNotification(string message, bool persistMessage = false)
        {
            _notificationLabel.Text = message;
            _notificationLabel.Visible = true;
            _notificationLabel.Width = _notificationsContainer.Width;
            _notificationLabel.Height = _notificationsContainer.Height;

            if (!persistMessage)
            {
                Task.Run(async delegate
                {
                    await Task.Delay(4000);

                    ClearNotifications();

                    return;
                });
            }
        }

        private async Task DisplayCurrentKp()
        {
            ShowInsideNotification("Loading ...", true);

            var data = await _businessService.DisplayCurrentKp();
            ShowInsideNotification(data, true);
        }

        private void ClearNotifications()
        {
            _notificationLabel.Text = string.Empty;
            _notificationLabel.Visible = false;
        }

        private (FlowPanel panel, Label label, T control) CreateLabeledControl<T>(string labelText, string tooltipText, FlowPanel parent, int amount = 2, int ctrlWidth = 50) where T : Control, new()
        {
            FlowPanel panel = new()
            {
                Parent = parent,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new(5),
                BasicTooltipText = tooltipText,
                HeightSizingMode = SizingMode.AutoSize,
            };

            Label label = new()
            {
                Parent = panel,
                Text = labelText,
                Height = 25,
                VerticalAlignment = VerticalAlignment.Middle,
                BasicTooltipText = tooltipText,
            };

            T control = new()
            {
                Parent = panel,
                BasicTooltipText = tooltipText,
                Height = label.Height,
                Width = ctrlWidth,
            };

            void FitToPanel(object sender, RegionChangedEventArgs e)
            {
                label.Width = panel.ContentRegion.Width - control.Width - ((int)panel.ControlPadding.X * amount);
                panel.Invalidate();
            }

            void FitToParent(object sender, RegionChangedEventArgs e)
            {
                int width = (parent.ContentRegion.Width - (int)(parent.ControlPadding.X * (amount - 1))) / amount;
                panel.Width = width;
                panel.Invalidate();
            }

            panel.ContentResized += FitToPanel;
            parent.ContentResized += FitToParent;

            return new(panel, label, control);
        }
    }
}
