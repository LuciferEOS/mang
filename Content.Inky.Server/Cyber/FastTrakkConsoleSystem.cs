using System.Linq;
using System.Numerics;
using Content.Inky.Shared.Cyber;
using Content.Server.Actions;
using Content.Shared.Eye;
using Content.Shared.Pinpointer;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Station;
using Content.Shared.Station.Components;
using Content.Shared.UserInterface;
using Content.Medical.Shared.Abductor;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Movement.Components;
using Robust.Shared.Prototypes;
using Content.Shared.DeviceLinking;
using Content.Shared.Movement.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Maths;
using Robust.Shared.Map;

namespace Content.Inky.Server.Cyber;

public sealed partial class FastTrakkConsoleSystem : EntitySystem
{
    private static readonly TimeSpan TeleportTime = TimeSpan.FromSeconds(0.6); // i dont want to put trycomp in update sorry

    private static readonly SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_White/Object/Devices/experimentalsyndicateteleport.ogg");
    private static readonly EntProtoId PadEffect = "FastTrakkEffect"; // also all of it is not inside the component because fuck you i tried

    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private SharedEyeSystem _eye = default!;
    [Dependency] private SharedMoverController _mover = default!;
    [Dependency] private SharedStationSystem _station = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private AudioSystem _audio = default!;

    private readonly List<PendingTeleport> _pendingTeleports = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<FastTrakkStartEvent>(OnStart);
        SubscribeLocalEvent<FastTrakkConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeUiOpen);
        Subs.BuiEvents<FastTrakkConsoleComponent>(AbductorCameraConsoleUIKey.Key, subs =>
        {
            subs.Event<AbductorBeaconChosenBuiMsg>(OnBeaconChosen);
        });
    }

    private void OnStart(FastTrakkStartEvent args)
    {
        if (!TryComp<FastTrakkOperatorComponent>(args.Performer, out var session)
            || !Exists(session.Eye))
            return;

        if (!TryComp<FastTrakkConsoleComponent>(session.Console, out var console))
            return;

        var coords = Transform(session.Eye).Coordinates;
        SpawnAtPosition(console.HologramSpawner, coords.SnapToGrid());
        DoTeleports(session.Console, console, coords);
        Exit(args.Performer, session);
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        for (var i = _pendingTeleports.Count - 1; i >= 0; i--)
        {
            var pending = _pendingTeleports[i];
            if (!Exists(pending.Pad))
            {
                _pendingTeleports.RemoveAt(i); // i love bugs
                continue;
            }

            if (!pending.shit && _timing.CurTime >= pending.EffectAt)
            {
                foreach (var entity in _lookup.GetEntitiesInRange(pending.Pad, pending.PadRadius, LookupFlags.Dynamic))
                {
                    if (entity == pending.Pad)
                        continue;

                    var coords = pending.Coords.Offset(new Vector2(
                        _random.Next(-pending.DestinationRadius, pending.DestinationRadius + 1), // idfk
                        _random.Next(-pending.DestinationRadius, pending.DestinationRadius + 1)));

                    pending.TeleportTargets.Add(new TeleportTarget(entity, coords));
                    SpawnAtPosition(PadEffect, coords);
                }

                pending.shit = true;
                _pendingTeleports[i] = pending;
            }

            if (_timing.CurTime < pending.At)
                continue;

            _pendingTeleports.RemoveAt(i);
            TeleportPadArea(pending);
        }
    }

    private void DoTeleports(EntityUid console, FastTrakkConsoleComponent consoleComp, EntityCoordinates coords)
    {
        if (!TryComp<DeviceLinkSourceComponent>(console, out var source))
            return;

        var at = _timing.CurTime + consoleComp.TeleportDelay;
        foreach (var linked in source.LinkedPorts.Keys)
        {
            if (!HasComp<FastTrakkTeleportComponent>(linked))
                continue;

            SpawnAtPosition(consoleComp.HologramSpawnerDeparture, Transform(linked).Coordinates.SnapToGrid());
            _pendingTeleports.Add(new PendingTeleport(linked,
                coords,
                at,
                at - TeleportTime, // effect at
                consoleComp.PadRadius,
                consoleComp.DestinationRadius,
                false,
                []));
        }
    }

    private void TeleportPadArea(PendingTeleport pending)
    {
        if (!Exists(pending.Pad) || !Exists(pending.Coords.EntityId))
            return;

        foreach (var target in pending.TeleportTargets)
        {
            if (!Exists(target.Entity))
                continue;

            _transform.SetCoordinates(target.Entity, target.Destination);
            _audio.PlayPvs(TeleportSound, target.Entity);
        }
    }

    // abductorslop because it was taken from abductor system
    // shitcode yivpeeee
    // i didnt wanna touch them at all because trauma refactores them every week so heres this slop
    # region abductorslop

    private void OnBeforeUiOpen(Entity<FastTrakkConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        var stations = new Dictionary<int, StationBeacons>();
        foreach (var station in _station.GetStations())
        {
            if (_station.GetLargestGrid(station) is not { } grid
                || !TryComp<NavMapComponent>(grid, out var navMap))
                continue;

            stations[station.Id] = new StationBeacons
            {
                Name = Name(station),
                StationId = station.Id,
                Beacons = [.. navMap.Beacons.Values],
            };
        }

        _ui.SetUiState(ent.Owner, AbductorCameraConsoleUIKey.Key, new AbductorCameraConsoleBuiState { Stations = stations });
    }

    private void OnBeaconChosen(Entity<FastTrakkConsoleComponent> ent, ref AbductorBeaconChosenBuiMsg args)
    {
        var beacon = GetEntity(args.Target);
        if (!HasComp<NavMapBeaconComponent>(beacon)
            || Transform(beacon).MapID != Transform(ent).MapID)
            return;

        Start(args.Actor, ent, Transform(beacon).Coordinates);
    }

    private void Start(EntityUid user, Entity<FastTrakkConsoleComponent> console, EntityCoordinates coordinates)
    {
        Exit(user);

        var eye = SpawnAtPosition(console.Comp.RemoteEntityProto, coordinates);
        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetTarget(user, eye, eyeComp);
            _eye.SetDrawFov(user, false);
            _eye.SetRotation(user, Angle.Zero, eyeComp);
            Dirty(user, eyeComp);

            // var overlay = EnsureComp<StationAiOverlayComponent>(user);
            // overlay.AllowCrossGrid = true;
            // Dirty(user, overlay);
        }

        _mover.SetRelay(user, eye);
        EnsureComp<FastTrakkOperatorComponent>(user, out var session);
        session.Console = console;
        session.Eye = eye;

        session.RemovedActions.Clear();
        foreach (var action in _actions.GetActions(user).ToArray())
        {
            session.RemovedActions.Add(action.Owner);
            _actions.RemoveAction(user, action.Owner);
        }

        _actions.AddAction(user, ref session.DeployAction, console.Comp.DeployAction);
    }

    private void Exit(EntityUid user, FastTrakkOperatorComponent? session = null)
    {
        if (!Resolve(user, ref session, false))
            return;

        _actions.RemoveAction(user, session.DeployAction);
        foreach (var action in session.RemovedActions)
        {
            if (Exists(action))
                _actions.AddActionDirect(user, action);
        }

        RemComp<RelayInputMoverComponent>(user);
        // RemComp<StationAiOverlayComponent>(user);
        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, true);
            _eye.SetTarget(user, null, eyeComp);
        }

        if (Exists(session.Eye))
            QueueDel(session.Eye);
        RemComp<FastTrakkOperatorComponent>(user);
    }

    # endregion

    private record struct PendingTeleport(EntityUid Pad,// listen this thing is cool n all but this here is an exception
        EntityCoordinates Coords,                  // this is such fucking slop i am sorry
        TimeSpan At,
        TimeSpan EffectAt,
        float PadRadius,
        int DestinationRadius,
        bool shit,
        List<TeleportTarget> TeleportTargets);
    private readonly record struct TeleportTarget(EntityUid Entity, EntityCoordinates Destination); // slop2
}
