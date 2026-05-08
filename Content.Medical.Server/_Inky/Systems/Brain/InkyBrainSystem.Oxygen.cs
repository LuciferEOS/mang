using Content.Medical.Common._Inky;
using Content.Medical.Common._Inky.Events;
using Content.Medical.Common.Targeting;
using Content.Medical.Shared._Inky.Components;
using Content.Medical.Shared.Body;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Robust.Shared.Timing;

namespace Content.Medical.Server._Inky.Systems.Brain;

public sealed partial class InkyBrainSystem
{
    private void InitializeOxygen()
    {
        SubscribeLocalEvent<BodyStatusComponent, BrainOxygenRestoreEvent>(OnBrainOxygenRestore);
    }

    private void UpdateOxy(float frameTime)
    {
        _oxygenUpdateTimer += frameTime;
        if (_oxygenUpdateTimer < OxygenUpdateInterval) // todo inkymed put inside a comp?
            return;

        _oxygenUpdateTimer -= OxygenUpdateInterval;


        var eqe = EntityQueryEnumerator<InkyBrainDamageableComponent, InkyBrainComponent>();
        while (eqe.MoveNext(out var uid, out var brainDmgComp, out var brainComp))
        {
            UpdateBrainOxy(uid, brainDmgComp);
            UpdateBrainAlerts(uid, brainDmgComp);
        }
    }

    private void UpdateBrainOxy(EntityUid uid, InkyBrainDamageableComponent brainDmgComp)
    {
        brainDmgComp.OxygenLevel -= brainDmgComp.ConsumeRate;
        brainDmgComp.OxygenLevel = Math.Clamp(brainDmgComp.OxygenLevel, 0f, 100f); // its shit but i feel like making min-max values inside the comp is useless

        Dirty(uid, brainDmgComp);
    }

    private void UpdateBrainAlerts(EntityUid uid, InkyBrainDamageableComponent brainDmgComp)
    {
        if (!_bodyStatusQuery.TryComp(_body.GetBody(uid), out var bodyStat)) // todo test
            return;

        var alerts = bodyStat.LimbAlerts["Head"];

        alerts.Remove(brainDmgComp.OxyAlertId);

        if (brainDmgComp.OxygenLevel < 70f)
            alerts.Add(brainDmgComp.OxyAlertId);

        Dirty(uid, bodyStat);
    }

    private void OnBrainOxygenRestore(Entity<BodyStatusComponent> ent, ref BrainOxygenRestoreEvent args)
    {
        if (!TryGetShitBrain(ent, out var brainComp)
            || brainComp == null)
            return;

        if (!TryGetLungs(ent, out var lungComp)
            || lungComp == null)
            return;

        var oxyRestored = (args.GasVolume * lungComp.OxygenRestoreRate) * args.OxygenModifier;

        brainComp.OxygenLevel += oxyRestored;
        brainComp.OxygenLevel = Math.Clamp(brainComp.OxygenLevel, 0f, 100f);

        Dirty(ent, ent.Comp);
    }
}
