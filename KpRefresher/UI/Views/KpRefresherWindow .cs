using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using KpRefresher.Domain;
using KpRefresher.Ressources;
using KpRefresher.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KpRefresher.UI.Views
{
    public class KpRefresherWindow : StandardWindow
    {

        private readonly ModuleSettings _moduleSettings;
        private readonly BusinessService _businessService;
        private readonly List<StandardButton> _buttons = new();

        private static readonly Regex _regex = new("^[0-9]*$");

        private LoadingSpinner _loadingSpinner;
        private Panel _notificationsContainer;
        private Label _notificationLabel;
        private FormattedLabel _notificationFormattedLabel;
        private Checkbox _showAutoRetryNotificationCheckbox;
        private Checkbox _onlyRefreshOnFinalBossKillCheckbox;

        private bool _delayTextChangeFlag = false;

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
            Controls.FlowPanel configContainer = new()
            {
                Parent = mainContainer,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                SetLocalizedTitle = () => strings.MainWindow_Configuration_Title,
                ShowBorder = true,
                CanCollapse = true,
                OuterControlPadding = new(5),
                ControlPadding = new(5),
            };

            var checkbox_controls = CreateLabeledControl<Checkbox>(() => strings.MainWindow_EnableAutoRetry_Label, () => strings.MainWindow_EnableAutoRetry_Tooltip, configContainer);
            checkbox_controls.control.Checked = _moduleSettings.EnableAutoRetry.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.EnableAutoRetry.Value = e.Checked;
                _showAutoRetryNotificationCheckbox.Enabled = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>(() => strings.MainWindow_ShowScheduleNotification_Label, () => strings.MainWindow_ShowScheduleNotification_Tooltip, configContainer);
            _showAutoRetryNotificationCheckbox = checkbox_controls.control;
            checkbox_controls.control.Enabled = _moduleSettings.EnableAutoRetry.Value;
            checkbox_controls.control.Checked = _moduleSettings.ShowScheduleNotification.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.ShowScheduleNotification.Value = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>(() => strings.MainWindow_EnableRefreshOnKill_Label, () => strings.MainWindow_EnableRefreshOnKill_Tooltip, configContainer);
            checkbox_controls.control.Checked = _moduleSettings.EnableRefreshOnKill.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.EnableRefreshOnKill.Value = e.Checked;
                _onlyRefreshOnFinalBossKillCheckbox.Enabled = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>(() => strings.MainWindow_RefreshOnKillOnlyBoss_Label, () => strings.MainWindow_RefreshOnKillOnlyBoss_Tooltip, configContainer);
            _onlyRefreshOnFinalBossKillCheckbox = checkbox_controls.control;
            checkbox_controls.control.Enabled = _moduleSettings.EnableRefreshOnKill.Value;
            checkbox_controls.control.Checked = _moduleSettings.RefreshOnKillOnlyBoss.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.RefreshOnKillOnlyBoss.Value = e.Checked;
            };

            checkbox_controls = CreateLabeledControl<Checkbox>(() => strings.MainWindow_RefreshOnMapChange_Label, () => strings.MainWindow_RefreshOnMapChange_Tooltip, configContainer);
            checkbox_controls.control.Checked = _moduleSettings.RefreshOnMapChange.Value;
            checkbox_controls.control.CheckedChanged += (s, e) =>
            {
                _moduleSettings.RefreshOnMapChange.Value = e.Checked;
            };

            var (panel, label, control) = CreateLabeledControl<TextBox>(() => strings.MainWindow_DelayBeforeRefreshOnMapChange_Label, () => strings.MainWindow_DelayBeforeRefreshOnMapChange_Tooltip, configContainer);
            control.Text = _moduleSettings.DelayBeforeRefreshOnMapChange.Value.ToString();
            control.InputFocusChanged += (s, e) =>
            {
                //Prevents empty value
                string txt = (s as TextBox).Text.Trim();
                if (string.IsNullOrWhiteSpace(txt))
                {
                    _delayTextChangeFlag = true;

                    control.Text = "1";
                    _moduleSettings.DelayBeforeRefreshOnMapChange.Value = 1;

                    _delayTextChangeFlag = false;
                }
            };
            control.TextChanged += (s, e) =>
            {
                //Prevent double change
                if (_delayTextChangeFlag)
                    return;

                _delayTextChangeFlag = true;

                string txt = (s as TextBox).Text.Trim();

                //Prevent action on field empty
                if (string.IsNullOrWhiteSpace(txt))
                {
                    _delayTextChangeFlag = false;
                    return;
                }

                //Prevent action on wrong input
                if (!_regex.IsMatch(txt))
                {
                    control.Text = ((ValueChangedEventArgs<string>)e).PreviousValue;
                    control.CursorIndex = control.Text.Length;

                    _delayTextChangeFlag = false;
                    return;
                }

                if (!int.TryParse(txt, out int newValue))
                {
                    //This should never happen
                    control.Text = ((ValueChangedEventArgs<string>)e).PreviousValue;
                    //control.CursorIndex--;

                    _delayTextChangeFlag = false;
                    return;
                }

                //Only allow value between 1 and 60
                if (newValue < 1)
                {
                    newValue = 1;
                    control.Text = "1";
                    control.CursorIndex = 1;
                }
                else if (newValue > 60)
                {
                    newValue = 60;
                    control.Text = "60";
                    control.CursorIndex = 2;
                }

                _moduleSettings.DelayBeforeRefreshOnMapChange.Value = newValue;
                _delayTextChangeFlag = false;
            };

            var (panelCustomId, labelCustomId, controlCustomId) = CreateLabeledControl<TextBox>(() => strings.MainWindow_CustomId_Label, () => strings.MainWindow_CustomId_Tooltip, configContainer);
            controlCustomId.Text = _moduleSettings.CustomId.Value;
            controlCustomId.TextChanged += (s, e) =>
            {
                var value = (s as TextBox).Text.Trim();
                if (value == _moduleSettings.CustomId.Value || string.IsNullOrEmpty(value))
                    ClearNotifications();
                else
                    ShowInsideNotification(strings.MainWindow_CustomId_EditNotif, true);
            };
            controlCustomId.EnterPressed += async (s, e) =>
            {
                var value = (s as TextBox).Text.Trim();
                var result = await _businessService.SetCustomId(value);

                ShowInsideNotification(result.Item2, true);
            };
            #endregion Config

            #region Actions
            Controls.FlowPanel actionContainer = new()
            {
                Parent = mainContainer,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                SetLocalizedTitle = () => strings.MainWindow_Actions_Title,
                ShowBorder = true,
                CanCollapse = true,
                OuterControlPadding = new(5),
                ControlPadding = new(5),
            };
            actionContainer.ContentResized += ActionContainer_ContentResized;

            StandardButton button;
            _buttons.Add(button = new Controls.StandardButton()
            {
                SetLocalizedText = () => strings.MainWindow_Button_Refresh_Label,
                SetLocalizedTooltip = () => strings.MainWindow_Button_Refresh_Tooltip,
                Parent = actionContainer
            });
            button.Click += async (s, e) => await RefreshRaidClears();

            _buttons.Add(button = new Controls.StandardButton()
            {
                SetLocalizedText = () => strings.MainWindow_Button_RefreshLinkedAccounts_Label,
                SetLocalizedTooltip = () => strings.MainWindow_Button_RefreshLinkedAccounts_Tooltip,
                Parent = actionContainer
            });
            button.Click += async (s, e) =>
            {
                if (_businessService.LinkedKpId?.Count > 0)
                {
                    string res = await _businessService.RefreshLinkedAccounts();
                    ShowInsideNotification(string.Format(strings.MainWindow_Notif_LinkedAccounts, _businessService.LinkedKpId?.Count, _businessService.LinkedKpId?.Count > 1 ? "s" : string.Empty, res), true);
                }
                else
                {
                    ShowInsideNotification(strings.MainWindow_Notif_NoLinkedAccount);
                }
            };

            _buttons.Add(button = new Controls.StandardButton()
            {
                SetLocalizedText = () => strings.MainWindow_Button_ShowClears_Label,
                SetLocalizedTooltip = () => strings.MainWindow_Button_ShowClears_Tooltip,
                Parent = actionContainer
            });
            button.Click += async (s, e) => await DisplayRaidDifference();

            _buttons.Add(button = new Controls.StandardButton()
            {
                SetLocalizedText = () => strings.MainWindow_Button_ShowKP_Label,
                SetLocalizedTooltip = () => strings.MainWindow_Button_ShowKP_Tooltip,
                Parent = actionContainer
            });
            button.Click += async (s, e) => await DisplayCurrentKp();

            _buttons.Add(button = new Controls.StandardButton()
            {
                SetLocalizedText = () => strings.MainWindow_Button_ClearSchedule_Label,
                SetLocalizedTooltip = () => strings.MainWindow_Button_ClearSchedule_Tooltip,
                Parent = actionContainer
            });
            button.Click += (s, e) => StopRetry();

            _buttons.Add(button = new Controls.StandardButton()
            {
                SetLocalizedText = () => strings.MainWindow_Button_ClearNotif_Label,
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
                CanScroll = true,
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
                    _loadingSpinner.BasicTooltipText = string.Format(strings.MainWindow_Spinner_Minutes, totalMinutes, totalMinutes > 1 ? "s" : string.Empty);
                else
                    _loadingSpinner.BasicTooltipText = string.Format(strings.MainWindow_Spinner_Seconds, (int)nextRefresh.TotalSeconds, (int)nextRefresh.TotalSeconds > 1 ? "s" : string.Empty);
            };
            #endregion Spinner

            _notificationLabel = new Label()
            {
                Location = new(_loadingSpinner.Right + 5, _loadingSpinner.Top),
                Parent = _notificationsContainer,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Font = GameService.Content.DefaultFont18,
                WrapText = true,
                AutoSizeHeight = true,
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
            ShowInsideNotification(strings.MainWindow_Notif_Loading, true);

            var data = await _businessService.GetFullRaidStatus();
            ShowFormattedNotification(data, true);
        }

        private void StopRetry()
        {
            if (_businessService.RefreshScheduled)
            {
                _businessService.CancelSchedule();
                ShowInsideNotification(strings.MainWindow_Notif_ScheduleDisabled);
            }
            else
            {
                ShowInsideNotification(strings.MainWindow_Notif_NoSchedule);
            }

            _loadingSpinner.Visible = false;
        }

        private void ShowInsideNotification(string message, bool persistMessage = false)
        {
            ClearNotifications();

            if (string.IsNullOrWhiteSpace(message))
                return;

            _notificationLabel.Text = message;
            _notificationLabel.Visible = true;
            _notificationLabel.Width = _notificationsContainer.Width;

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

        private void ShowFormattedNotification(List<(string, Color?)> parts, bool persistMessage = false)
        {
            ClearNotifications();

            if (parts == null || parts.Count == 0)
                return;

            var builder = new FormattedLabelBuilder();

            foreach (var part in parts)
            {
                if (part.Item2.HasValue)
                {
                    if (part.Item2.Value == Colors.OnlyGw2)
                        builder = builder.CreatePart(part.Item1, b => b.SetFontSize(ContentService.FontSize.Size18)
                                         .SetTextColor(part.Item2.Value)
                                         .MakeBold());
                    else
                        builder = builder.CreatePart(part.Item1, b => b.SetFontSize(ContentService.FontSize.Size18)
                                     .SetTextColor(part.Item2.Value));
                }
                else
                    builder = builder.CreatePart(part.Item1, b => b.SetFontSize(ContentService.FontSize.Size18));
            }


            _notificationFormattedLabel?.Dispose();

            _notificationFormattedLabel = builder
                             .SetWidth(_notificationsContainer.Width)
                             .SetHeight(_notificationsContainer.Height)
                             .SetHorizontalAlignment(HorizontalAlignment.Left)
                             .SetVerticalAlignment(VerticalAlignment.Top)
                             .Build();

            _notificationFormattedLabel.Location = new(_loadingSpinner.Right + 5, _loadingSpinner.Top);
            _notificationFormattedLabel.Parent = _notificationsContainer;

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
            ShowInsideNotification(strings.MainWindow_Notif_Loading, true);

            var data = await _businessService.DisplayCurrentKp();
            ShowInsideNotification(data, true);
        }

        private void ClearNotifications()
        {
            _notificationLabel.Text = string.Empty;
            _notificationLabel.Visible = false;

            _notificationFormattedLabel?.Dispose();
        }

        private (FlowPanel panel, Label label, T control) CreateLabeledControl<T>(Func<string> labelText, Func<string> tooltipText, FlowPanel parent, int amount = 2, int ctrlWidth = 50) where T : Control, new()
        {
            Controls.FlowPanel panel = new()
            {
                Parent = parent,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new(5),
                SetLocalizedTooltip = tooltipText,
                HeightSizingMode = SizingMode.AutoSize,
            };

            Controls.Label label = new()
            {
                Parent = panel,
                SetLocalizedText = labelText,
                Height = 25,
                VerticalAlignment = VerticalAlignment.Middle,
                SetLocalizedTooltip = tooltipText,
            };

            T control = new()
            {
                Parent = panel,
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
