# Crystal Weapons

Crystal Weapons creates a networked Crystal Ingot by cloning the vanilla Iron Ingot prefab and applying the Crystal material and ice-blue appearance. It adds smelting recipes for every valid forge mould product, including weapons, tools, and hammers.

## Recipes

- One Crystal Gem Blue in a smelter ore dock produces one Crystal Ingot. The smelter still needs normal fuel; Coal can be used as fuel, but is not currently an ore ingredient.
- Crystal Ingots cost the mould's normal ingot count. A mould that normally costs 5 ingots requires 5 Crystal Ingots.
- The recipe produces the mould product's native output quantity.
- The output keeps its normal item identity and receives a registered Crystal physical material.
- Crystal Ingots are registered in each valid mould's allowed-material set and each smelter upgrade's physical-material list.
- Crystal Ingot mould inputs require the corresponding registered mould. Insufficient stacks are rejected without consuming them.
- Ordinary smelter recipes continue through the game's normal recipe path.

Mould entries with a missing product, nonpositive cost, nonpositive output quantity, or missing output prefab are logged and skipped. Recipe hashes are derived from mould hashes and checked against the game's live recipe registry before registration. Existing Crystal mould recipe hashes are preserved.

The Crystal Ingot keeps the test's item hash `0x43574901`, network prefab hash `0x5001` (20481), recipe hash `0x43575201`, and legacy prefab alias `0x43575001`. Keeping these IDs lets saved test ingots resolve after the temporary test DLL is removed. Network prefab hashes for new types must fit in 16 bits.

## Material, appearance, and configuration

The Crystal material uses a damage multiplier of 0.85 and durability multiplier of 0.85, between Copper (0.7) and Iron (1.0). Other gameplay stats default to the installed game's Red Iron values. Its renderer materials and material channels use the vanilla Iron template, matching the Crystal Repair Hammer appearance setup, then receive the same ice-blue tint and emission. The client companion reapplies the color after the game assigns its atlas materials and keeps the color visible during heat updates; this does not change actual item temperature.

MelonLoader creates a `CrystalWeapons` preferences category displayed as **Crystal Weapons**. Each gameplay override uses the matching game field name plus `Override`. The damage and durability overrides default to `0.85`; other float overrides default to `-1` to inherit Red Iron. All float overrides accept finite nonnegative values. `hardnessLevelOverride` uses the same `-1` inherit value and accepts nonnegative integers. The supplied `CrystalWeapons.example.cfg` contains only this mod's settings. When upgrading from an older release, rename the preferences section to `[CrystalWeapons]` so existing custom overrides carry over.

Configurable fields:

```text
sourceThermalConductivityOverride
receiveThermalConductivityOverride
internalThermalConductivityOverride
glowingStartOverride
glowingEndOverride
forgeMultiplierOverride
meltingPointOverride
maxTemperatureForParticlesOverride
nailHealthMultiplierOverride
maxCraftingDamageMultiplierOverride
damageMultiplierOverride
durabilityMultiplierOverride
densityOverride
weightMultiplierOverride
tightnessMultiplierOverride
tightnessBowImpactOverride
minimumProjectileWeightOverride
maxInvalidProjectileVelocityMultiplierOverride
noiseMultiplierOverride
hardnessLevelOverride
```

Invalid values are logged and fall back to the Red Iron field value.

## Client companion

Dedicated servers use `CrystalWeapons.dll` for ingot and mould recipes, forge behavior, material registration, and output assignment. Player clients use `CrystalWeapons.Client.dll`; it registers the matching ingot item/prefab/material, the forge material lists, and the appearance needed to render the replicated ingot and forged products. It does not add recipes or consume forge inputs. Both builds reference the shared `CustomIngots.API.dll`.

Install `CrystalWeapons.dll` in the dedicated server's `Mods` folder and `CustomIngots.API.dll` in its `UserLibs` folder. Install `CrystalWeapons.Client.dll` in each player's client `Mods` folder and the same `CustomIngots.API.dll` in each client's `UserLibs` folder. Keep the server and client mod DLLs in their respective runtimes. The client build disables itself in batch/server runtimes. `CustomRecipesAPI.dll`, `MateriaLib.dll`, and Repair Hammer are optional separate mods, not dependencies of Crystal Weapons or its API.

Keep matching `CrystalWeapons` override values on the server and clients so the registered Crystal material has consistent local stats. The server remains authoritative for recipes, crafting, damage, and durability. Remove `CrystalIngotRecipeTest.Server.dll` from server Mods and `CrystalIngotRecipeTest.Client.dll` and `CrystalIngotAppearanceTest.dll` from client Mods before starting the integrated version. The test DLLs would register the same hashes.

## API for other mods

Reference `CustomIngots.API.dll` from another mod, add `using CustomIngots.API;`, and call `IngotCatalog.Register(new IngotDefinition(...))` during that mod's `OnInitializeMelon`. The same definition must be registered on the server and every client before Crystal Weapons' late initialization. The API closes registration at that point and rejects late additions and duplicate item, prefab, recipe, or material hashes. A prefab hash must fit in 16 bits.

An `IngotDefinition` supplies a unique item name and hashes, source ingot prefab name, one or more `IngotIngredient` entries, a unique physical material hash and name, and tint/emission colors. Crystal Weapons then creates and registers the ingot prefab and material on both sides. The server adds ingredient items to its ore filter and registers smelting and mould recipes. Client and server use the same catalogue; no separate recipe or material registration through CustomRecipesAPI or MateriaLib is needed for an API ingot. Other mods may continue using those libraries for their own items, but must not register the same hashes twice. New API materials inherit Red Iron gameplay stats; Crystal uses the existing `CrystalWeapons` preferences.

To change the built-in Crystal Ingot recipe later, edit its `IngotIngredient` entries in `Api/IngotCatalog.cs` and rebuild the API and both mod DLLs. For example, a Crystal Gem plus Coal recipe would need a second entry with Coal's verified item hash and name. The server would add Coal to its ore input filter; the smelter would still need fuel in its fuel dock. Keep the item, prefab, material, and recipe IDs unchanged for saved ingots.

## Build

```bash
./build.sh
```

The script builds the API dependency and `CrystalWeapons.dll` against the installed A Township Tale managed assemblies. Release artifacts are written to `Custom Ingots API Build/CustomIngots.API.dll` and `Crystal Weapons Build/CrystalWeapons.dll`. Set `GAME_PATH` if the game files are installed elsewhere, or `DOTNET` to select a .NET SDK executable.

Run `./build-client.sh` to build the client Release artifact separately. It builds the same API DLL and writes `Crystal Weapons Client Build/CrystalWeapons.Client.dll`. `./build-api.sh` builds only the API for mods that compile against it. Run these build scripts one at a time because they share the API project's intermediate files. The client project references the API and shared registration code but excludes server recipe and consumption logic.

This project contains no Repair Hammer repair behavior and does not modify the source RepairHammer project.
