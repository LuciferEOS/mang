namespace Content.Medical.Shared.Body;

public sealed partial class BodyStatusSystem
{
     public void InitializeInky()
     {
     }

     // the code is perfect.
    private void InitializeLimbShit(Entity<BodyStatusComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        foreach (var limb in ent.Comp.Limbs)
        {
            if (!ent.Comp.LimbAlerts.ContainsKey(limb))
                ent.Comp.LimbAlerts[limb] = new HashSet<string>();
        }

    }
}
