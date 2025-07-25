﻿using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.NukeOps;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.NukeOps;

[GenerateTypedNameReferences]
public sealed partial class WarDeclaratorWindow : FancyWindow
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ILocalizationManager _localizationManager = default!;

    public event Action<string>? OnActivated;

    private TimeSpan _endTime;
    private TimeSpan _shuttleDisabledTime;
    private WarConditionStatus _status;

    public WarDeclaratorWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        WarButton.OnPressed += (_) => OnActivated?.Invoke(Rope.Collapse(MessageEdit.TextRope));

        MessageEdit.Placeholder = new Rope.Leaf(_localizationManager.GetString("war-declarator-message-placeholder"));
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        UpdateTimer();
    }

    public void UpdateState(WarDeclaratorBoundUserInterfaceState state)
    {
        if (state.Status == null)
            return;

        WarButton.Disabled = state.Status == WarConditionStatus.WarReady;

        _endTime = state.EndTime;
        _shuttleDisabledTime = state.ShuttleDisabledTime;
        _status = state.Status.Value;

        UpdateStatus(state.Status.Value);

    }

    private void UpdateStatus(WarConditionStatus status)
    {
        switch (status)
        {
            case WarConditionStatus.WarReady:
                WarButton.Disabled = true;
                StatusLabel.Text = Loc.GetString("war-declarator-boost-declared");
                UpdateTimer();
                StatusLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateLow);
                break;
            case WarConditionStatus.YesWar:
                WarButton.Text = Loc.GetString("war-declarator-ui-war-button");
                StatusLabel.Text = Loc.GetString("war-declarator-boost-possible");
                UpdateTimer();
                StatusLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateGood);
                break;
            case WarConditionStatus.NoWarSmallCrew:
                StatusLabel.Text = Loc.GetString("war-declarator-boost-impossible");
                InfoLabel.Text = Loc.GetString("war-declarator-conditions-small-crew");
                StatusLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateNone);
                break;
            case WarConditionStatus.NoWarShuttleDeparted:
                StatusLabel.Text = Loc.GetString("war-declarator-boost-impossible");
                InfoLabel.Text = Loc.GetString("war-declarator-conditions-left-outpost-starcup"); // starcup: Syndicate to NT
                StatusLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateNone);
                break;
            case WarConditionStatus.NoWarTimeout:
                StatusLabel.Text = Loc.GetString("war-declarator-boost-impossible");
                InfoLabel.Text = Loc.GetString("war-declarator-conditions-time-out");
                StatusLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateNone);
                break;
            case WarConditionStatus.NoWarUnknown:
                StatusLabel.Text = Loc.GetString("war-declarator-boost-impossible");
                InfoLabel.Text = Loc.GetString("war-declarator-conditions-unknown");
                StatusLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateNone);
                break;
            default:
                StatusLabel.Text = Loc.GetString("war-declarator-boost-impossible");
                InfoLabel.Text = Loc.GetString("war-declarator-conditions-unknown");
                StatusLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateNone);
                break;
        }
    }

    private void UpdateTimer()
    {
        switch(_status)
        {
            case WarConditionStatus.YesWar:
                var timeLeft = _endTime.Subtract(_gameTiming.CurTime);
                if (timeLeft > TimeSpan.Zero)
                    InfoLabel.Text = Loc.GetString("war-declarator-boost-timer", ("time", timeLeft.ToString("mm\\:ss")));
                else
                    UpdateStatus(WarConditionStatus.NoWarTimeout);
                break;

            case WarConditionStatus.WarReady:
                var time = _shuttleDisabledTime.Subtract(_gameTiming.CurTime);
                if (time > TimeSpan.Zero)
                    InfoLabel.Text = Loc.GetString("war-declarator-boost-timer", ("time", time.ToString("mm\\:ss")));
                else
                    InfoLabel.Text = Loc.GetString("war-declarator-conditions-ready");
                break;
            default:
                return;
        }
    }
}
