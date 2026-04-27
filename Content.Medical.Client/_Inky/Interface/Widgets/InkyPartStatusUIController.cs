using System.Numerics;
using Content.Client.Gameplay;
using Content.Medical.Client.Targeting;
using Content.Medical.Client.UserInterface.Systems.PartStatus;
using Content.Medical.Common._Inky.Events;
using Content.Medical.Shared._Inky.Healing;
using Content.Medical.Shared.Body;
using Content.Shared.Alert;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Medical.Client._Inky.Interface.Widgets;

public sealed class InkyPartStatusUIController // todo unfuck
    : UIController,
      IOnStateEntered<GameplayState>,
      IOnSystemChanged<TargetingSystem>
{
    private FancyHealingSystem? _shitHeal = default!;

    private PartStatusDollWindow? _window;
    private PanelContainer? _overlay;
    private BodyStatusComponent? _comp; // todo unfuch

    private IPrototypeManager _protoMan = default!;
    private IEntityManager _entMan = default!;

    public override void Initialize()
    {
        base.Initialize();
        UIManager.GetUIController<PartStatusUIController>().OnStatusClicked += OnStatusClicked; // todo remove partstatus shitting your chat onclick

        _entMan = IoCManager.Resolve<IEntityManager>();
        _protoMan = IoCManager.Resolve<IPrototypeManager>();
    }

    public void OnStateEntered(GameplayState state) => CloseWindow();

    public void OnSystemLoaded(TargetingSystem system)
    {
        system.PartStatusStartup += OnPartStatusStartup;
        system.PartStatusShutdown += OnPartStatusShutdown;
        system.PartStatusUpdate += OnPartStatusUpdate;
    }

    public void OnSystemUnloaded(TargetingSystem system)
    {
        system.PartStatusStartup -= OnPartStatusStartup;
        system.PartStatusShutdown -= OnPartStatusShutdown;
        system.PartStatusUpdate -= OnPartStatusUpdate;
    }

    private void OnPartStatusStartup(BodyStatusComponent comp)
        => _comp = comp;

    private void OnPartStatusShutdown()
    {
        _comp = null;
        CloseWindow();
    }

    private void OnPartStatusUpdate(BodyStatusComponent comp)
    {
        _comp = comp;
        if (_window == null)
            return;

        if (!_entMan.TryGetComponent<AlertsComponent>(comp.Owner, out var alertsComp)) // todo inkymed: unfuck
            return;

        _window.UpdateStatus(comp, alertsComp, _protoMan);
    }

    private void OnStatusClicked()
    {
        if (_comp == null)
            return;

        if (_window != null)
        {
            CloseWindow();
            return;
        }

        OpenWindow();
    }

    private void OpenWindow()
    {
        if (_comp == null)
            return;

        if (!_entMan.TryGetComponent<AlertsComponent>(_comp.Owner, out var alertsComp))
            return;

        _overlay = new PanelContainer // ok so this basically creates a second window that has nothing but bg, to make everything a bit darker when you open the window for barotrauma parity ofc :godo:
        {
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#00000090") }, // horrible
            MouseFilter = Control.MouseFilterMode.Ignore,
        };
        LayoutContainer.SetAnchorPreset(_overlay, LayoutContainer.LayoutPreset.Wide);
        UIManager.RootControl.AddChild(_overlay);

        _window = new PartStatusDollWindow();
        _window.OnLimbClicked += OnLimbClicked;
        _window.UpdateStatus(_comp, alertsComp, _protoMan);

        UIManager.RootControl.AddChild(_window);
        Pos();
    }

    private void CloseWindow()
    {
        _window?.Orphan();
        _window = null;

        _overlay?.Orphan(); // joker, im an orphan joker you hear me
        _overlay = null;    // joker...
    }

    private void Pos()
    {
        if (_window == null)
            return;

        // due to RootControl.Width being the entirety of the widght, to get the centre you halve it
        var centre = (UIManager.RootControl.Width - _window.Width) / 2; // (yes i need to explain it here because it took me 40min to come up with ts again, its a reminder)
        LayoutContainer.SetPosition(_window, new Vector2(centre, 20));
    }

    private void OnLimbClicked(string categoryId)
    {
        // shit is initting before entitysystems so thats why this atrocity is here
        _shitHeal ??= _entMan.System<FancyHealingSystem>();
        _shitHeal.RequestHealLimb(categoryId);
    }
}
