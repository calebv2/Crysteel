using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CrystalWeapons.Core), "Crystal Weapons", "2.1", "ATT", null)]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace CrystalWeapons;

public sealed class Core : MelonMod
{
    internal static MelonLogger.Instance Logger { get; private set; } = null!;
    private bool clientRuntimeEnabled;

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        if (IsServerRuntime())
        {
            Logger.Warning("Crystal Weapons is client-only and will remain disabled in this server runtime.");
            return;
        }

        CrystalForgeConfig.Initialize();
        var crystalHarmony = new HarmonyLib.Harmony("ATT.CrystalWeapons.Client");
        CrystalClientAppearancePatch.Install(crystalHarmony);
        IngotSpawnActivation.Install(crystalHarmony, message => Logger.Msg(message));
        clientRuntimeEnabled = true;
        Logger.Msg("Crystal Weapons initialized in client mode; ingot registration and renderer appearance are enabled.");
    }

    public override void OnLateInitializeMelon()
    {
        if (!clientRuntimeEnabled || IsServerRuntime()) return;

        try
        {
            foreach (var definition in IngotCatalog.All)
            {
                var material = ReferenceEquals(definition, IngotCatalog.Crystal)
                    ? CrystalMaterialRegistration.CreateAndRegister()
                    : CrystalMaterialRegistration.CreateAndRegister(definition);
                var ingot = IngotRegistration.CreateAndRegister(definition, material);
                IngotForgeUnlock.Register(ingot, material);
                Logger.Msg("Crystal Weapons registered " + definition.ItemName + " on the client: material="
                    + material.Hash + ", item=" + ingot.Hash + ", prefab=" + ingot.Prefab.Hash
                    + ", prefabEntity=" + ingot.Prefab.Entity?.Hash
                    + ", entityPrefab=" + ingot.Prefab.Entity?.Prefab?.Hash + ".");
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Crystal Weapons ingot/material registration failed on the client: " + exception);
        }
    }

    public override void OnUpdate()
    {
        if (clientRuntimeEnabled) IngotClientAppearance.Update();
    }

    private static bool IsServerRuntime()
    {
        return Application.isBatchMode || NetworkSceneManager.IsServer || ServerAssemblyIsLoaded();
    }

    private static bool ServerAssemblyIsLoaded()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (string.Equals(assembly.GetName().Name, "CrystalWeapons", StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }
}
