using System;
using System.Collections.Generic;
using System.Linq;
using Alta.Inventory;
using MelonLoader;

[assembly: MelonInfo(typeof(CrystalWeapons.Core), "Crystal Weapons", "2.1", "ATT", null)]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace CrystalWeapons;

public sealed class Core : MelonMod
{
    internal static MelonLogger.Instance Logger { get; private set; } = null!;

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        CrystalForgeConfig.Initialize();
        IngotSpawnActivation.Install(HarmonyInstance, message => Logger.Msg(message));
        Logger.Msg("Crystal Weapons initialized.");
    }

    public override void OnLateInitializeMelon()
    {
        try
        {
            var allTargets = new List<CrystalMouldTarget>();
            foreach (var definition in IngotCatalog.All)
            {
                var material = ReferenceEquals(definition, IngotCatalog.Crystal)
                    ? CrystalMaterialRegistration.CreateAndRegister()
                    : CrystalMaterialRegistration.CreateAndRegister(definition);
                var ingot = IngotRegistration.CreateAndRegister(definition, material);
                IngotForgeUnlock.Register(ingot, material);
                var ingotRecipe = IngotSmeltingRecipeRegistration.Register(definition, ingot);
                foreach (var ingredient in definition.Ingredients)
                {
                    var item = Item.All.FirstOrDefault(candidate => candidate.Hash == ingredient.ItemHash);
                    if (item != null) CrystalSmelterInputFilter.Allow(item);
                }
                CrystalSmelterInputFilter.Allow(ingot);
                allTargets.AddRange(CrystalMouldRecipeRegistration.Register(definition, ingot));
                Logger.Msg("Registered " + definition.ItemName + ": item=" + ingot.Hash
                    + ", prefab=" + ingot.Prefab.Hash + ", material=" + material.Hash
                    + ", prefabEntity=" + ingot.Prefab.Entity?.Hash
                    + ", entityPrefab=" + ingot.Prefab.Entity?.Prefab?.Hash
                    + ", smeltingRecipe=" + ingotRecipe.Hash + ", smeltingDuration=" + ingotRecipe.Duration + ".");
            }
            if (allTargets.Count == 0)
            {
                Logger.Warning("Crystal Weapons did not register any valid mould products.");
            }

            Logger.Msg("Crystal Weapons initialized with " + allTargets.Count + " custom mould recipes.");
        }
        catch (Exception exception)
        {
            Logger.Error("Crystal Weapons late initialization failed: " + exception);
        }
    }
}
