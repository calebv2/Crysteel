# Crystal Fabric Design

## Summary

Add a distinct item named Crystal Fabric to Crystal Weapons. Build it from the vanilla Soft Fabric Large Roll item, hash 6960, tint it with the Crystal ice-blue appearance, and add it as a separate candidate in generated world loot.

The server and client must register the same item prefab and stable network hash so a server-spawned Crystal Fabric can be resolved by connected clients.

## Goals

- Register Crystal Fabric as its own item and network prefab, separate from Soft Fabric Large Roll.
- Preserve the source roll's size, pickup behavior, stack settings, weight, and loot category/value.
- Use an ice-blue appearance on cloned renderer materials, without modifying the vanilla prefab or its shared materials.
- Add Crystal Fabric alongside Soft Fabric Large Roll in generated-loot tables that already include the vanilla roll.
- Copy the source entry's rarity and minimum/maximum quantity.
- Keep the original Soft Fabric Large Roll item and loot entries unchanged.
- Register the same stable prefab hash on the server and client companion.

## Considered approaches

1. **Distinct item with an additional loot entry — selected.** Register a Crystal Fabric clone and append a separate loot entry beside each matching Soft Fabric Large Roll entry, copying its rarity and quantity range. The original item remains unchanged and both items remain possible loot outcomes.
2. **Add Crystal Fabric to every biome table.** This would make it appear in places where Soft Fabric Large Roll does not currently appear and would require separate rarity and quantity tuning for each table.
3. **Tint the vanilla item.** This would change the appearance of ordinary Soft Fabric Large Rolls and would not give Crystal Fabric its own identity.

## Item and network registration

Resolve item hash 6960 and verify its name is Soft Fabric Large Roll and that it has a NetworkPrefab. Clone both the item definition and prefab at runtime. Rename the clones to Crystal Fabric while keeping source pickup, stack, size, weight, category, loot value, and other item settings.

Assign the clone a stable Crystal Weapons network prefab hash from a dedicated constant. Check the live prefab and item registries before registration; fail this feature cleanly if the hash is already owned. Register the cloned prefab and item with the game's runtime registries on both server and client. Do not add a dependency on SyncLib.

Crystal Fabric registration must not modify the original item, prefab, or material objects. A missing source item, missing prefab, or hash collision should be logged and should not prevent Crystal weapon recipes or material registration from initializing.

## Appearance

Clone each source renderer material before changing it. For materials that expose these shader properties, set _Color, _Color1, and _Color2 to the existing Crystal ice tint. Keep the source texture and bump map.

Use the material's emission properties only when present: set the vector property _EmissionColor to the established Crystal ice emission, set float property _Emission to 0.2, set _UseEmission to enabled when available, and enable the _EMISSION keyword. This matches the provided fabric shader property types while keeping its woven texture visible.

## Generated world loot

The installed game selects generated loot from LootTable entries before LootHelper spawns the selected item. During server late initialization, find the world-generated loot tables that contain Soft Fabric Large Roll and append a separate Crystal Fabric entry to the same rarity group. Mirror entries in seasonal lists when the source roll is present there.

For each matched vanilla entry, copy its minimum and maximum quantity into a new entry that points to Crystal Fabric. Leave every existing entry untouched. Make registration idempotent so reinitialization cannot add duplicate Crystal Fabric entries. This adds Crystal Fabric as a separate possible result in the same generated-loot locations and rarity groups; it does not replace or recolor Soft Fabric Large Roll.

The game selects uniformly from the eligible entries after category weighting. Adding a candidate keeps the original Soft Fabric entry intact and the number of results per roll unchanged, while slightly shifting relative odds among items in the same category.

This applies only to generated loot using loot tables; it does not alter preset loot, vendors, recipes, or manually spawned items. Log the resolved source item, registered Crystal Fabric hash, matched world tables, and number of new entries. If no source entries are found, report that clearly instead of silently claiming the drop is active.

## Server and client behavior

The server build registers the cloned item and adds its entries to generated world loot tables. The client companion registers the same clone with the same network hash and appearance so replicated spawns resolve locally. No client loot-table mutation is needed.

Both DLLs remain separate: CrystalWeapons.dll stays on the server and CrystalWeapons.Client.dll is installed by players. Update the README to document the Crystal Fabric drop and the matching server/client requirement. The example config remains unchanged because the initial appearance and loot placement use copied game values without new preferences.

## Validation

- Build the server and client projects against the installed game assemblies.
- Confirm both builds use the same item name, stable prefab hash, and appearance code.
- Confirm source item validation, registry collision checks, and initialization failures are logged without stopping existing Crystal Weapons setup.
- Confirm every new loot entry points to Crystal Fabric, copies the source rarity and quantity range, and leaves the original hash 6960 entry untouched.
- Confirm duplicate registration is skipped and preset loot remains untouched.

Live multiplayer verification should confirm that a dropped Crystal Fabric appears blue on clients and remains the distinct Crystal Fabric item after pickup and save/load.

## Out of scope

- Changing vanilla Soft Fabric Large Roll properties or existing loot locations.
- Adding Crystal Fabric recipes, vendor stock, or a new physical-material gameplay stat.
- Making the loot distribution or appearance configurable in this first version.
