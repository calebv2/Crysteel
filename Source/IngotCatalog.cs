using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWeapons;

public static class IngotCatalog
{
    public static readonly IngotDefinition Crystal = new IngotDefinition(
        "Crystal Ingot", "Iron Ingot",
        0x43574901u, 0x5001u, 0x43575201u,
        CrystalMaterialRegistration.CrystalMaterialHash, "Crystal Red Iron",
        new[] { new IngotIngredient(45754u, "Crystal Gem Blue", 1) },
        new Color(CrystalAppearancePolicy.IceTintRed, CrystalAppearancePolicy.IceTintGreen,
            CrystalAppearancePolicy.IceTintBlue, CrystalAppearancePolicy.IceTintAlpha),
        new Color(CrystalAppearancePolicy.IceEmissionRed, CrystalAppearancePolicy.IceEmissionGreen,
            CrystalAppearancePolicy.IceEmissionBlue, 1f),
        0x43575001u);

    private static readonly IngotDefinition[] Definitions = { Crystal };
    private static readonly IReadOnlyList<IngotDefinition> ReadOnlyDefinitions = Array.AsReadOnly(Definitions);
    public static IReadOnlyList<IngotDefinition> All => ReadOnlyDefinitions;

    static IngotCatalog()
    {
        CheckUnique(definition => definition.ItemHash, "item");
        CheckUnique(definition => definition.PrefabHash, "prefab");
        CheckUnique(definition => definition.RecipeHash, "recipe");
        CheckUnique(definition => definition.MaterialHash, "material");
        var ingredientSignatures = new HashSet<string>(StringComparer.Ordinal);
        foreach (var definition in Definitions)
        {
            var signature = string.Join(",", definition.Ingredients
                .OrderBy(ingredient => ingredient.ItemHash)
                .Select(ingredient => ingredient.ItemHash.ToString()));
            if (!ingredientSignatures.Add(signature))
                throw new InvalidOperationException("Two ingot definitions have the same smelting ingredient items.");
        }
        var aliases = new HashSet<uint>(Definitions.Select(definition => definition.PrefabHash));
        foreach (var definition in Definitions)
        {
            foreach (var alias in definition.LegacyPrefabHashes)
            {
                if (alias == 0 || !aliases.Add(alias))
                    throw new InvalidOperationException("Duplicate or empty legacy ingot prefab hash " + alias + ".");
            }
        }
    }

    public static IngotDefinition? FindByMaterialHash(uint hash) => Definitions.FirstOrDefault(definition => definition.MaterialHash == hash);
    public static IngotDefinition? FindByPrefabHash(uint hash) => Definitions.FirstOrDefault(definition => definition.PrefabHash == hash);
    public static IngotDefinition? FindByItemHash(uint hash) => Definitions.FirstOrDefault(definition => definition.ItemHash == hash);

    private static void CheckUnique(Func<IngotDefinition, uint> hash, string kind)
    {
        if (Definitions.Select(hash).Distinct().Count() != Definitions.Length)
            throw new InvalidOperationException("Two ingot definitions use the same " + kind + " hash.");
    }
}
