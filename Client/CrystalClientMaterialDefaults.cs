using System;
using System.Reflection;

namespace CrystalWeapons;

// Client materials provide the replicated item's appearance. Gameplay
// overrides are owned by the server and are never read from client preferences.
internal static class CrystalForgeConfig
{
    private const float CrystalDamageMultiplier = 0.85f;
    private const float CrystalDurabilityMultiplier = 0.85f;
    private static readonly BindingFlags MaterialFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    internal static void ApplyOverrides(PhysicalMaterial target, PhysicalMaterial redIron)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (redIron == null) throw new ArgumentNullException(nameof(redIron));

        // The target is already a Red Iron clone; only the built-in Crystal
        // damage and durability defaults differ from that source material.
        SetFloatField(target, "damageMultiplier", CrystalDamageMultiplier);
        SetFloatField(target, "durabilityMultiplier", CrystalDurabilityMultiplier);
    }

    private static void SetFloatField(PhysicalMaterial material, string name, float value)
    {
        var field = typeof(PhysicalMaterial).GetField(name, MaterialFields);
        if (field == null || field.FieldType != typeof(float))
            throw new MissingFieldException(typeof(PhysicalMaterial).FullName, name + " (expected Single)");
        field.SetValue(material, value);
    }
}
