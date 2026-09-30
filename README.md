# Crysteel

Crysteel creates and Crysteel Ingots. It adds smelting recipes for every valid forge mould product, including weapons, tools, and hammers.

## Recipes

- One Darksteel Ingot and 20 Crystal Gems in the smelter one Crysteel Ingot.
- Cost the mould's normal ingot count. A mould that normally costs 5 ingots requires 5 Crystal or 5 

## Client companion

Dedicated servers use `Crysteel.dll` for ingot and mould recipes, forge behavior, material registration, and output assignment. Player clients use `Crysteel.Client.dll`; it registers the matching ingot item/prefab/material, the forge material lists, and the appearance needed to render the replicated ingot and forged products. It does not add recipes or consume forge inputs. Both builds need `CustomIngots.API.dll`.

Install `Crysteel.dll` in the dedicated server's `Mods` folder and [`CustomIngots.API.dll`](https://github.com/calebv2/Custom-Ingots/releases/latest/download/CustomIngots.API.dll) in its `UserLibs` folder. Install `Crysteel.Client.dll` in each player's client `Mods` folder and the same `CustomIngots.API.dll` in each client's `UserLibs` folder. Keep the server and client mod DLLs in their respective runtimes. The client build disables itself in batch/server runtimes. The basic Crystal recipe has no dependency on other recipe mods. Crafting Crysteel requires an obtainable Darksteel Ingot; [UsableDarksteel](https://github.com/Circl-NoE/UsableDarksteel) provides that and requires `CustomRecipesAPI.dll` and `MateriaLib.dll` on server and clients.
