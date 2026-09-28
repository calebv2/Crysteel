# Crystal Fabric Design

## Summary

Add a distinct item named Crystal Fabric to Crystal Weapons. Build it from the vanilla Soft Fabric Large Roll item, hash 6960, tint it with the Crystal ice-blue appearance, and allow it to appear as a rare alternative in generated world loot.

The server and client must register the same item prefab and stable network hash so a server-spawned Crystal Fabric can be resolved by connected clients.

## Goals

- Register Crystal Fabric as its own item and network prefab, separate from Soft Fabric Large Roll.
- Preserve the source roll's size, pickup behavior, stack settings, weight, and loot category/value.
- Use an ice-blue appearance on cloned renderer materials, without modifying the vanilla prefab or its shared materials.
- Make Crystal Fabric a low-frequency generated-loot outcome at locations where Soft Fabric Large Roll can already be selected.
- Keep its quantity equal to the quantity the loot table selected for Soft Fabric Large Roll.
- Register the same stable prefab hash on the server and client companion.

## Considered approaches

1. **Distinct item with a rare replacement outcome — recommended.** Register a Crystal Fabric clone and replace 5% of generated Soft Fabric Large Roll results with it. This keeps existing loot locations and total cloth frequency while giving Crystal Fabric its own name, appearance, and network identity.
2. **Add an entry to every matching loot table.** This is direct, but it increases cloth availability and requires balancing each table's rarity and category entries.
3. **Tint the vanilla item.** This avoids a new network identity, but all Soft Fabric Large Rolls share the same item identity and could not be distinguished as Crystal Fabric in inventory.

## Item and network registration

Resolve item hash 6960 and verify its name is Soft Fabric Large Roll and that it has a NetworkPrefab. Clone both the item definition and prefab at runtime. Rename the clones to Crystal Fabric while keeping source pickup, stack, size, weight, category, loot value, and other item settings.

Assign the clone a stable Crystal Weapons network prefab hash from a dedicated constant. Check the live prefab and item registries before registration; fail this feature cleanly if the hash is already owned. Register the cloned prefab and item with the game's runtime registries on both server and client. Do not add a dependency on SyncLib.

Crystal Fabric registration must not modify the original item, prefab, or material objects. A missing source item, missing prefab, or hash collision should be logged and should not prevent Crystal weapon recipes or material registration from initializing.

## Appearance

Clone each source renderer material before changing it. For materials that expose these shader properties, set _Color, _Color1, and _Color2 to the existing Crystal ice tint. Keep the source texture and bump map.

Use the material's emission properties only when present: set the vector property _EmissionColor to the established Crystal ice emission, set float property _Emission to 0.2, set _UseEmission to enabled when available, and enable the _EMISSION keyword. This matches the provided fabric shader property types while keeping its woven texture visible.

## Generated world loot

The installed game routes generated loot through LootTable.GetLoot before LootHelper spawns the selected item. Install a server-side Harmony postfix on the overload used by that flow.

When the selected item is Soft Fabric Large Roll, replace it with Crystal Fabric with a fixed 5% probability. Preserve the selected count and leave rarity, location, and other loot outcomes unchanged. This applies only to generated loot using loot tables; it does not alter preset loot, vendors, recipes, or manually spawned items.

Log the resolved source item, registered Crystal Fabric hash, and active feature state at initialization. If runtime inspection shows that the source item is not available to the generated-loot flow, report that clearly instead of silently claiming the drop is active.

## Server and client behavior

The server build registers the cloned item and installs the generated-loot replacement patch. The client companion registers the same clone with the same network hash and appearance so replicated spawns resolve locally. No client loot-generation patch is needed.

Both DLLs remain separate: CrystalWeapons.dll stays on the server and CrystalWeapons.Client.dll is installed by players. Update the README to document the Crystal Fabric drop and the matching server/client requirement. The example config remains unchanged because the initial drop chance and appearance are fixed.

## Validation

- Build the server and client projects against the installed game assemblies.
- Confirm both builds use the same item name, stable prefab hash, and appearance code.
- Confirm source item validation, registry collision checks, and initialization failures are logged without stopping existing Crystal Weapons setup.
- Review the world-loot patch to ensure it only replaces hash 6960, preserves count, and leaves all other generated and preset loot untouched.

Live multiplayer verification should confirm that a dropped Crystal Fabric appears blue on clients and remains the distinct Crystal Fabric item after pickup and save/load.

## Out of scope

- Changing vanilla Soft Fabric Large Roll properties, existing loot locations, or total fabric frequency.
- Adding Crystal Fabric recipes, vendor stock, or a new physical-material gameplay stat.
- Making the drop chance or appearance configurable in this first version.
