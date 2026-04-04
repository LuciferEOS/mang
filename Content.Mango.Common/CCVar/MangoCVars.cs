using Robust.Shared.Configuration;

namespace Content.Mango.Common.CCVar;

[CVarDefs]
public sealed partial class MangoCVars
{
    /// <summary>
    /// % chance for anything in <see cref="FunnyThingsSystem"/> to roll
    /// </summary>
    public static readonly CVarDef<float> FunProb =
        CVarDef.Create("mango.fun_value", 65f, CVar.SERVER); // 65%
}
