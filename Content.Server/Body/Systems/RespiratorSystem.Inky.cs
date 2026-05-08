using Content.Medical.Common._Inky.Events;
using Content.Medical.Shared.Body;
using Content.Server.Body.Components;
using Content.Shared.Body.Components;
using Content.Trauma.Common.Body;

namespace Content.Server.Body.Systems;

public sealed partial class RespiratorSystem
{
    private void InitializeInky()
    {
        SubscribeLocalEvent<RespiratorComponent, SuffocationAlertUpdateEvent>(OnSuffocationAlertUpdate);
    }

    private void OnSuffocationAlertUpdate(Entity<RespiratorComponent> ent, ref SuffocationAlertUpdateEvent args)
        => UpdateAsphyxiationAlert(ent.Owner);

    private void UpdateAsphyxiationAlert(EntityUid uid)
    {
        if (!TryGetLungs(uid, out var lungComp)
            || lungComp == null)
            return;

        if (!TryComp<BodyStatusComponent>(uid, out var bodyStat))
            return;

        var allDamage = _damageableSys.GetAllDamage((uid, null));
        allDamage.DamageDict.TryGetValue("Asphyxiation", out var currentAsphyxDamage); // not rewkirking the whole dmg sys just for inkymed btw
        var totalDmg = currentAsphyxDamage;

        var alerts = bodyStat.LimbAlerts["Torso"];

        alerts.Remove(lungComp.HypoxiaAlertId);
        alerts.Remove(lungComp.AsphyxationAlertId);
        alerts.Remove(lungComp.LungFailAlertId);

        // there is a better way to do it for a FACT that i just do not know of
        if (totalDmg >= lungComp.LungFailThreshold)
            alerts.Add(lungComp.LungFailAlertId);
        else if (totalDmg >= lungComp.AsphyxationThreshold)
            alerts.Add(lungComp.AsphyxationAlertId);
        else if (totalDmg > 0)
            alerts.Add(lungComp.HypoxiaAlertId);

        Dirty(uid, bodyStat);
    }

    /// <summary>
    /// Gets the oxygen supply modifier for the brain based on current asphyx damage
    /// </summary>
    /// <param name="uid">The entity to check</param>
    /// <returns>the oxygen supply modifier (default 1.0f if no lungs)</returns>
    public float GetOxygenSupplyModifier(EntityUid uid) // todo
    {
        if (!TryGetLungs(uid, out var lungComp)
            || lungComp == null)
            return 1.0f;

        var allDamage = _damageableSys.GetAllDamage((uid, null));
        allDamage.DamageDict.TryGetValue("Asphyxiation", out var currentAsphyxDamage);
        var totalDmg = currentAsphyxDamage;

        // TODO INKYMED: enum these bitches
        if (totalDmg >= lungComp.LungFailThreshold)
            return lungComp.ModifierThresholds.GetValueOrDefault("ResArrest", 0.0f);

        if (totalDmg >= lungComp.AsphyxationThreshold)
            return lungComp.ModifierThresholds.GetValueOrDefault("Asphyxia", 0.5f);

        if (totalDmg > 0)
            return lungComp.ModifierThresholds.GetValueOrDefault("Hypoxia", 0.8f);

        return lungComp.ModifierThresholds.GetValueOrDefault("Normal", 1.0f);
    }

    public bool TryGetLungs(EntityUid uid, out LungComponent? lungComp)
    {
        lungComp = null;
        var lungOrgan = _body.GetOrgan(uid, LungsCategory);
        return lungOrgan != null && TryComp<LungComponent>(lungOrgan.Value, out lungComp);
    }
}
