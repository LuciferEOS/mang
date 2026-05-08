using System.Linq;
using System.Numerics;
using Content.Medical.Common.Wounds;
using Content.Medical.Shared.Body;
using Content.Shared.Alert;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Medical.Client._Inky.Interface.Widgets;

// it carves through my brain like a knife wielding roomba carves through children
public sealed class PartStatusDollWindow : Control
{
    private static readonly ResPath BaseRsi = new("/Textures/_Inky/Status/base.rsi"); // 128x128
    private static readonly ResPath BaseLegsRsi = new("/Textures/_Inky/Status/base_legs.rsi"); // oh my god bro 128x196
    private static readonly (string CategoryId, string RsiState, Vector2 AlertOffset)[] PartMapping =
    [
        // vectors are here relative to the center of the doll at something like 0,-250 (dont ask) and they basically there to tell alerts(AND LIMBS) where to go
        // edit from future dr. autism: ^ info about 0, -250  may be outdated i did not check do not trust this autistic person (me)
        ("Head","Head",new Vector2(0, -240)),
        ("Torso", "Torso", new Vector2(0, 0)),

        ("ArmLeft","ArmLeft",new Vector2(190, 0)),
        ("ArmRight","ArmRight",new Vector2(-190, 00)),

        ("LegLeft", "LegLeft",new Vector2(120, 260)),
        ("LegRight","LegRight",new Vector2(-110, 260)),
    ]; // go fuck yourself

    private static readonly Dictionary<WoundableSeverity, (float Amplitude, float Speed)> JitterCfg = new()
    { // jittering config, wow
        { WoundableSeverity.Healthy, (0f, 0f) },
        { WoundableSeverity.Minor, (2f, 2f) },
        { WoundableSeverity.Moderate, (2f, 8f) },
        { WoundableSeverity.Severe, (2f, 32f) },
        { WoundableSeverity.Critical, (2f, 128f) },
        { WoundableSeverity.Mangled, (3f, 128f) },
        { WoundableSeverity.Severed, (0f, 0f) },
    };
    private BodyStatusComponent? _lastComp;
    private float _jitterTime;

    private const int DollSize = 256; // if you have a small monitor then im sorry
    private const int DollLegHeight = 392; // oh my god bro.
    private const int AlertSize = 96; // also these two here upscale the alerts/doll size so lower it if its too big for you

    private readonly Dictionary<string, TextureRect> _rects = new();
    private readonly Dictionary<string, BoxContainer> _alertContainers = new();

    public Action<string>? OnLimbClicked;

    public PartStatusDollWindow()
    {
        SetSize = new Vector2(DollSize, DollSize);
        var screen = new LayoutContainer // todo figure out if its better than Control
        {
            // MinSize = new Vector2(DollSize, DollSize), todo figure out whats better
            SetSize = new Vector2(DollSize, DollSize),

        };

        var spriteSys = IoCManager.Resolve<IEntityManager>().System<SpriteSystem>();
        foreach (var (categoryId, state, offset) in PartMapping)
        {
            var isLeg = categoryId == "LegLeft" || categoryId == "LegRight"; // TODO INKYMED: HOLY. FUCKING. SLOP. this is beggining to look alot like shitmed ammirite??

            var rsi = new SpriteSpecifier.Rsi(isLeg
                ? BaseLegsRsi
                : BaseRsi,
                state);
            var texture = spriteSys.Frame0(rsi);

            var limbX = (DollSize / 2f) + offset.X - (DollSize / 2f);
            var limbY = (DollSize / 2f) + offset.Y - (DollSize / 2f);

            var rect = new TextureRect
            {
                Texture = texture,
                Stretch = TextureRect.StretchMode.KeepAspectCentered,
                SetSize = new Vector2(DollSize, // kill me
                    isLeg
                    ? DollLegHeight
                    : DollSize),
                MouseFilter = MouseFilterMode.Stop,
            };
            LayoutContainer.SetPosition(rect, new Vector2(limbX, limbY));

            var capturedId = categoryId;
            rect.OnKeyBindDown += args =>
            {
                if (args.Function == EngineKeyFunctions.UIClick)
                    OnLimbClicked?.Invoke(capturedId);
            };

            var alertX = (DollSize / 2f) + offset.X - (AlertSize / 2f);
            var alertY = (DollSize / 2f) + offset.Y - (AlertSize / 2f);

            var alertBox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                Margin = new Thickness(alertX, alertY, 0, 0),
                MouseFilter = MouseFilterMode.Ignore,
                SeparationOverride = -64 // NO FUCKING IDEA HOW TO MAKE IT BE SOMEWHAT MORE VERTICAL BTW todo inkymed
            };

            _rects[categoryId] = rect;
            _alertContainers[categoryId] = alertBox;

            screen.AddChild(rect);
            screen.AddChild(alertBox);
        }

        MouseFilter = MouseFilterMode.Ignore; // if you ever think of making the doll move - think again
        AddChild(screen);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_lastComp == null)
            return;

        _jitterTime += args.DeltaSeconds; // delta "deathstroke" nedas

        foreach (var (categoryId, rect) in _rects)
        {
            if (!_lastComp.BodyStatus.TryGetValue(categoryId, out var severity)
                || !JitterCfg.TryGetValue(severity, out var jitter)
                || jitter.Amplitude == 0f)
            {
                rect.Margin = new Thickness(0); // jittery limbs even on full health sound cool but idk todo
                continue;
            }

            var shit = categoryId.GetHashCode() * 0.025f;

            /*
             * X axis
             * time x speed = how fast it wiggles
             * shit = to make the limbs not move in sync if they all are damaged
             * amplitude = how far away it moves in px
             * I HATE MATH
             */
            var dx = MathF.Sin((_jitterTime * jitter.Speed) + shit) * jitter.Amplitude;

            /*
             * same as X axis math but with the only diference being that the jitter speed is 30% slower
             * why? it looks better this way
             */
            var dy = MathF.Cos((_jitterTime * jitter.Speed * 0.7f) + shit) * jitter.Amplitude;

            rect.Margin = new Thickness(dx, dy, -dx, -dy);
        }
    }

    public void UpdateStatus(BodyStatusComponent bodyComp, AlertsComponent? alertsComp, IPrototypeManager protoMan)
    {
        var spriteSys = IoCManager.Resolve<IEntityManager>().System<SpriteSystem>();
        foreach (var container in _alertContainers.Values)
            container.DisposeAllChildren();

        // color
        foreach (var (categoryId, rect) in _rects)
        {
            if (!bodyComp.BodyStatus.TryGetValue(categoryId, out var status))
            {
                rect.Visible = false;
                continue;
            }

            rect.Visible = true;
            rect.Modulate = SeverityToColorIdk(status);
        }

        if (alertsComp == null)
            return;

        // alerts
        foreach (var (limbId, activeAlerts) in bodyComp.LimbAlerts)
        {
            if (!_alertContainers.TryGetValue(limbId, out var container))
                continue;

            foreach (var alertId in activeAlerts)
            {
                if (!protoMan.TryIndex<AlertPrototype>(alertId, out var alertProto))
                    continue;

                var severity = (short)0; // TODO INKYMED: DESHITCODE THIS SHIT BELOW
                if (alertId == "Bleed" && bodyComp.LimbBleedSeverity.TryGetValue(limbId, out var limbSevrity)) // no autism^2 for you, even if youre a tajara (or eels)
                    severity = limbSevrity;
                else if (alertsComp != null)
                {
                    foreach (var (_, alertState) in alertsComp.Alerts)
                    {
                        if (protoMan.TryIndex<AlertPrototype>(alertState.Type, out var a) && a.ID == alertId)
                        {
                            severity = alertState.Severity ?? 0;
                            break;
                        }
                    }
                }

                var iconTex = spriteSys.Frame0(alertProto.GetIcon(severity));
                var alertRect = new TextureRect
                {
                    Texture = iconTex,
                    SetSize = new Vector2(AlertSize, AlertSize),
                    Stretch = TextureRect.StretchMode.KeepAspectCentered,
                    MouseFilter = MouseFilterMode.Ignore
                };

                container.AddChild(alertRect);
                _lastComp = bodyComp;
            }
        }
    }

    private static Color SeverityToColorIdk(WoundableSeverity severity) => severity switch // holy goida
    {// this all is just some shapes of pink and red except for the .Healthy and .Severed btw
        WoundableSeverity.Healthy => Color.White,
        WoundableSeverity.Minor => Color.FromHex("#cfb8b8"),
        WoundableSeverity.Moderate => Color.FromHex("#c99797"),
        WoundableSeverity.Severe => Color.FromHex("#d15e5e"),
        WoundableSeverity.Critical => Color.FromHex("#b81414"),
        WoundableSeverity.Mangled => Color.FromHex("#4a0a0a"),
        WoundableSeverity.Severed => Color.FromHex("#0a0a0a00"),
        _ => Color.White,
    };
}
