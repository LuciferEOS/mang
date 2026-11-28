using Content.Client.Gameplay;
using Content.Medical.Client.Targeting;
using Content.Medical.Client.UserInterface.Systems.PartStatus;
using Content.Medical.Shared.Body;
using Content.Medical.Shared.Inkymed.PartStatus;
using Content.Shared.Alert;
using Content.Shared.Interaction;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;

namespace Content.Medical.Client._Inky.Interface.Widgets;

public sealed partial class InkyPartStatusUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<TargetingSystem>
{
    [Dependency] private IEntityManager _entities = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IPlayerManager _player = default!;

    private BodyStatusComponent? _body;
    private PartStatusDollWindow? _window;
    private PanelContainer? _overlay;
    private bool _showingOther;

    public override void Initialize()
    {
        base.Initialize();
        UIManager.GetUIController<PartStatusUIController>().OnStatusClicked += ToggleWindow;
    }

    public void OnStateEntered(GameplayState state) => CloseWindow();

    public void OnSystemLoaded(TargetingSystem system)
    {
        system.PartStatusStartup += UpdateStatus;
        system.PartStatusUpdate += UpdateStatus;
        system.PartStatusShutdown += OnShutdown;
        system.OtherPartStatusOpened += OpenOther;
    }

    public void OnSystemUnloaded(TargetingSystem system)
    {
        system.PartStatusStartup -= UpdateStatus;
        system.PartStatusUpdate -= UpdateStatus;
        system.PartStatusShutdown -= OnShutdown;
        system.OtherPartStatusOpened -= OpenOther;
        OnShutdown();
    }

    private void OnShutdown()
    {
        _body = null;
        CloseWindow();
    }

    private void UpdateStatus(BodyStatusComponent body)
    {
        _body = body;
        _entities.TryGetComponent<AlertsComponent>(body.Owner, out var alerts);
        if (!_showingOther)
            _window?.UpdateStatus(body, alerts, _prototypes);
    }

    private void OpenOther(OpenPartStatusEvent ev)
    {
        CloseWindow();
        _showingOther = true;
        OpenWindow();
        var target = _entities.GetEntity(ev.Target);
        if (_window != null)
            _window.OnFrame = () =>
            {
                if (_player.LocalEntity is not { } player
                    || !_entities.EntityExists(target)
                    || !_entities.System<SharedInteractionSystem>().InRangeUnobstructed(player, target))
                    CloseWindow();
            };
        _window?.UpdateStatus(new BodyStatusComponent { BodyStatus = ev.Status }, null, _prototypes);
    }

    private void ToggleWindow()
    {
        if (_window != null)
        {
            CloseWindow();
            return;
        }

        if (_body == null)
            return;

        _showingOther = false;
        OpenWindow();
        UpdateStatus(_body);
    }

    private void OpenWindow()
    {
        _overlay = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#00000090") },
            MouseFilter = Control.MouseFilterMode.Ignore,
        };
        LayoutContainer.SetAnchorPreset(_overlay, LayoutContainer.LayoutPreset.Wide);
        UIManager.RootControl.AddChild(_overlay);

        _window = new PartStatusDollWindow();
        UIManager.RootControl.AddChild(_window);
        LayoutContainer.SetPosition(_window, new Vector2((UIManager.RootControl.Width - 480) / 2, 20));
    }

    private void CloseWindow()
    {
        _window?.Orphan();
        _overlay?.Orphan();
        _window = null;
        _overlay = null;
        _showingOther = false;
    }
}
