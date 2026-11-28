using System.Numerics;
using Content.Medical.Common.Wounds;
using Content.Medical.Shared.Body;
using Content.Shared.Alert;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Medical.Client._Inky.Interface.Widgets;

public sealed class PartStatusDollWindow : Control
{
    private const int DollSize = 192;
    private const int AlertSize = 36;
    private static readonly ResPath BaseRsi = new("/Textures/_Inky/Interface/Inkymed/Status/base.rsi");
    private static readonly ResPath LegsRsi = new("/Textures/_Inky/Interface/Inkymed/Status/base_legs.rsi");
    private static readonly (string Id, Vector2 Position, ResPath Rsi, int Height)[] Parts =
    [
        ("Head", new(0, -180), BaseRsi, DollSize),
        ("Torso", new(0, 0), BaseRsi, DollSize),
        ("ArmLeft", new(-142f, 0), BaseRsi, DollSize),
        ("ArmRight", new(142f, 0), BaseRsi, DollSize),
        ("LegLeft", new(85, 154f), LegsRsi, 292),
        ("LegRight", new(-85f, 154f), LegsRsi, 292),
    ];

    private readonly SpriteSystem _sprites = IoCManager.Resolve<IEntityManager>().System<SpriteSystem>();
    private readonly Dictionary<string, TextureRect> _parts = new();
    private readonly Dictionary<string, BoxContainer> _alerts = new();

    public Action? OnFrame;

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        OnFrame?.Invoke();
    }

    public PartStatusDollWindow()
    {
        SetSize = new Vector2(480, 690);
        MouseFilter = MouseFilterMode.Ignore;
        var layout = new LayoutContainer { SetSize = new Vector2(480, 690), MouseFilter = MouseFilterMode.Ignore };
        AddChild(layout);

        foreach (var (id, position, rsi, height) in Parts)
        {
            var part = new TextureRect
            {
                SetSize = new Vector2(DollSize, height),
                Texture = _sprites.Frame0(new SpriteSpecifier.Rsi(rsi, id)),
                Stretch = TextureRect.StretchMode.KeepAspectCentered,
                MouseFilter = MouseFilterMode.Ignore,
            };
            var alerts = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                SeparationOverride = -24,
                MouseFilter = MouseFilterMode.Ignore,
            };
            LayoutContainer.SetPosition(part, position + new Vector2(144, 195));
            LayoutContainer.SetPosition(alerts, position + new Vector2(144 + (DollSize - AlertSize) / 2f, 195 + (DollSize - AlertSize) / 2f)); // AUGHHHAUHFHGHHHHH
            _parts.Add(id, part);
            _alerts.Add(id, alerts);
            layout.AddChild(part);
        }

        foreach (var alerts in _alerts.Values)
            layout.AddChild(alerts);
    }

    public void UpdateStatus(BodyStatusComponent body, AlertsComponent? alerts, IPrototypeManager prototypes)
    {
        foreach (var (id, part) in _parts)
        {
            part.Visible = body.BodyStatus.TryGetValue(id, out var severity);
            if (part.Visible)
            {
                part.Modulate = severity switch
                {
                    WoundableSeverity.Minor => Color.FromHex("#cfb8b8"),
                    WoundableSeverity.Moderate => Color.FromHex("#c99797"),
                    WoundableSeverity.Severe => Color.FromHex("#d15e5e"),
                    WoundableSeverity.Critical => Color.FromHex("#b81414"),
                    WoundableSeverity.Mangled => Color.FromHex("#4a0a0a"),
                    WoundableSeverity.Severed => Color.FromHex("#0a0a0a00"),
                    _ => Color.White,
                };
            }
        }

        foreach (var container in _alerts.Values)
            container.DisposeAllChildren();

        foreach (var (limb, activeAlerts) in body.LimbAlerts)
        {
            if (!_alerts.TryGetValue(limb, out var container))
                continue;

            foreach (var id in activeAlerts)
            {
                if (!prototypes.TryIndex<AlertPrototype>(id, out var prototype))
                    continue;

                short severity = 0; // todo what the fuck is this
                if (alerts != null)
                {
                    foreach (var (_, state) in alerts.Alerts)
                    {
                        if (prototypes.TryIndex<AlertPrototype>(state.Type, out var active) && active.ID == id)
                        {
                            severity = state.Severity ?? 0;
                            break;
                        }
                    }
                }

                container.AddChild(new TextureRect
                {
                    Texture = _sprites.Frame0(prototype.GetIcon(severity)),
                    SetSize = new Vector2(AlertSize, AlertSize),
                    Stretch = TextureRect.StretchMode.KeepAspectCentered,
                    MouseFilter = MouseFilterMode.Ignore,
                });
            }
        }
    }

}
