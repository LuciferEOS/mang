using Content.Medical.Shared.Wounds;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.FixedPoint;

namespace Content.Medical.Shared.Body;

public sealed partial class BodyBloodstreamSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private void OnBodyUpdateLimbAlerts(Entity<BodyComponent> ent, FixedPoint2 total)
    {
        if (!TryComp<BodyStatusComponent>(ent.Owner, out var bodyStatus)
            || !TryComp<BloodstreamComponent>(ent.Owner, out var blood))
            return;

        foreach (var limbAlerts in bodyStatus.LimbAlerts.Values)
            limbAlerts.Remove(blood.BleedingAlert);

        bodyStatus.LimbBleedSeverity.Clear();
    }

    private void OnBodyUpdateLimbBleeding(Entity<BodyComponent> ent, Entity<WoundableComponent> part, FixedPoint2 totalPartBleeds)
    {
        if (!TryComp<BodyStatusComponent>(ent.Owner, out var bodyStatus)
            || totalPartBleeds <= 0)
            return;

        if (!TryComp<OrganComponent>(part.Owner, out var organ)
            || organ.Category is not { } category
            || !TryComp<BloodstreamComponent>(ent.Owner, out var blood))
            return;

        var limbId = _proto.Index(category).ID;

        bodyStatus.LimbAlerts.TryAdd(limbId, new HashSet<string>()); // no i cant use List here fuck off pheenty <3
        bodyStatus.LimbAlerts[limbId].Add(blood.BleedingAlert);

        /*
         * due to bleeding being hardcoded to be from 0 to 10 which represents its severity,
         * we need to convert totalPartBleeds from fixedpoint to a number that isnt shit,
         * then we round the number ToZero cuz 6.5 cant be on a scale between 0 and 10
         * then we clamp this whole thing while being short
         * (no fucking idea what's short but its what LimbBleedSeverity wants idk dwarf number go ask chatgpt)
         */
        var severity = (short)Math.Clamp(Math.Round(totalPartBleeds.Double(), MidpointRounding.ToZero), 0, 10);
        bodyStatus.LimbBleedSeverity[limbId] = severity;
    }

    private void OnBodyUpdateLimbAlertsFinalize(Entity<BodyComponent> ent)
    {
        if (TryComp<BodyStatusComponent>(ent.Owner, out var bodyStatus))
            Dirty(ent.Owner, bodyStatus);
    }
}
