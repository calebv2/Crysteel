using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using MelonLoader;

namespace CrystalWeapons;

public static class CrystalForgeConfig
{
    private const string CategoryName = "CrystalWeapons";
    private const float InheritValue = -1f;
    private static readonly string[] FloatFieldNames =
    {
        "sourceThermalConductivity",
        "receiveThermalConductivity",
        "internalThermalConductivity",
        "glowingStart",
        "glowingEnd",
        "forgeMultiplier",
        "meltingPoint",
        "maxTemperatureForParticles",
        "nailHealthMultiplier",
        "maxCraftingDamageMultiplier",
        "damageMultiplier",
        "durabilityMultiplier",
        "density",
        "weightMultiplier",
        "tightnessMultiplier",
        "tightnessBowImpact",
        "minimumProjectileWeight",
        "maxInvalidProjectileVelocityMultiplier",
        "noiseMultiplier"
    };

    private static readonly BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Dictionary<string, MelonPreferences_Entry<float>> FloatEntries = new Dictionary<string, MelonPreferences_Entry<float>>(StringComparer.Ordinal);
    private static MelonPreferences_Entry<int>? hardnessLevelOverride;
    private static MelonPreferences_Entry<float>? crysteelDamageScale;
    private static MelonPreferences_Entry<float>? crysteelDurabilityScale;
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized) return;

        var category = MelonPreferences.CreateCategory(CategoryName, "Crystal Weapons");
        foreach (var fieldName in FloatFieldNames)
        {
            var identifier = fieldName + "Override";
            FloatEntries.Add(identifier, category.CreateEntry<float>(
                identifier,
                GetDefaultOverride(fieldName),
                fieldName + " override",
                "Set to -1 to inherit the Red Iron value; use a finite, nonnegative value to override. Damage and durability default to 0.85."));
        }

        hardnessLevelOverride = category.CreateEntry<int>(
            "hardnessLevelOverride",
            -1,
            "hardnessLevel override",
            "Set to -1 to inherit the Red Iron value; use a nonnegative integer to override.");
        crysteelDamageScale = category.CreateEntry<float>(
            "crysteelDamageScale", 1.10f, "Crysteel damage scale",
            "Multiply the live Darksteel damage multiplier by this positive finite value.");
        crysteelDurabilityScale = category.CreateEntry<float>(
            "crysteelDurabilityScale", 0.90f, "Crysteel durability scale",
            "Multiply the live Darksteel durability multiplier by this positive finite value.");
        initialized = true;
    }

    public static void ApplyCrysteelScaling(PhysicalMaterial target, PhysicalMaterial darksteel)
    {
        if (!initialized) Initialize();
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (darksteel == null) throw new ArgumentNullException(nameof(darksteel));

        ApplyScale(target, darksteel, "damageMultiplier", crysteelDamageScale!.Value, 1.10f);
        ApplyScale(target, darksteel, "durabilityMultiplier", crysteelDurabilityScale!.Value, 0.90f);
    }

    private static void ApplyScale(PhysicalMaterial target, PhysicalMaterial source, string fieldName, float configuredScale, float defaultScale)
    {
        var scale = configuredScale;
        if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
        {
            Core.Logger.Warning("Invalid Crysteel scale for " + fieldName + "="
                + scale.ToString(CultureInfo.InvariantCulture) + "; using "
                + defaultScale.ToString(CultureInfo.InvariantCulture) + ".");
            scale = defaultScale;
        }

        var field = GetMaterialField(fieldName, typeof(float));
        var sourceValue = (float)(field.GetValue(source) ?? 0f);
        var scaled = sourceValue * scale;
        if (float.IsNaN(scaled) || float.IsInfinity(scaled) || scaled <= 0f)
            throw new InvalidOperationException("Crysteel " + fieldName + " is invalid after scaling Darksteel.");
        field.SetValue(target, scaled);
    }

    public static void ApplyOverrides(PhysicalMaterial target, PhysicalMaterial redIron)
    {
        if (!initialized) Initialize();
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (redIron == null) throw new ArgumentNullException(nameof(redIron));

        foreach (var fieldName in FloatFieldNames)
        {
            var field = GetMaterialField(fieldName, typeof(float));
            var redIronValue = (float)(field.GetValue(redIron) ?? 0f);
            var configuredValue = FloatEntries[fieldName + "Override"].Value;
            if (configuredValue == InheritValue)
            {
                field.SetValue(target, redIronValue);
                continue;
            }

            if (float.IsNaN(configuredValue) || float.IsInfinity(configuredValue) || configuredValue < 0f)
            {
                Core.Logger.Warning("Invalid config value for " + fieldName + "Override="
                    + configuredValue.ToString(CultureInfo.InvariantCulture) + "; using Red Iron value "
                    + redIronValue.ToString("0.###", CultureInfo.InvariantCulture) + ".");
                field.SetValue(target, redIronValue);
                continue;
            }

            field.SetValue(target, configuredValue);
        }

        var hardnessField = GetMaterialField("hardnessLevel", typeof(uint));
        var redIronHardness = (uint)(hardnessField.GetValue(redIron) ?? 0u);
        var configuredHardness = hardnessLevelOverride!.Value;
        if (configuredHardness == -1)
        {
            hardnessField.SetValue(target, redIronHardness);
        }
        else if (configuredHardness < 0)
        {
            Core.Logger.Warning("Invalid config value for hardnessLevelOverride=" + configuredHardness
                + "; using Red Iron value " + redIronHardness + ".");
            hardnessField.SetValue(target, redIronHardness);
        }
        else
        {
            hardnessField.SetValue(target, (uint)configuredHardness);
        }

        var effectiveValues = new List<string>();
        foreach (var fieldName in FloatFieldNames)
        {
            var field = GetMaterialField(fieldName, typeof(float));
            effectiveValues.Add(fieldName + "=" + ((float)(field.GetValue(target) ?? 0f)).ToString("0.###", CultureInfo.InvariantCulture));
        }
        effectiveValues.Add("hardnessLevel=" + hardnessField.GetValue(target));
        Core.Logger.Msg("Crystal Weapons effective material stats: " + string.Join(", ", effectiveValues) + ".");
    }

    private static float GetDefaultOverride(string fieldName)
    {
        return fieldName == "damageMultiplier" || fieldName == "durabilityMultiplier"
            ? 0.85f
            : InheritValue;
    }

    private static FieldInfo GetMaterialField(string fieldName, Type expectedType)
    {
        var field = typeof(PhysicalMaterial).GetField(fieldName, InstanceFields);
        if (field == null || field.FieldType != expectedType)
        {
            throw new MissingFieldException(typeof(PhysicalMaterial).FullName, fieldName + " (expected " + expectedType.Name + ")");
        }

        return field;
    }
}
