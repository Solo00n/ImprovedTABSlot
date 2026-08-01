# Improved TAB Slot

A **client-side** BepInEx mod for Lethal Company (v80+, "The Blooming Update", which
added the vanilla utility slot). It lets selected items — normally kept out by the
utility-slot blacklist — be carried in the Tab slot:

- **Shovel** (and its reskins: Stop sign / Yield sign)
- **Ammo** (shotgun shells)
- **Clipboard**
- **Sticky note**
- **Key**
- **Shotgun**  *(two-handed — see caveat below)*
- **Kitchen knife**
- **Maneater** (baby cave dweller) — *utility slot only, opt-in, off by default; see note below*

Each item can be toggled on/off individually in the config file.

It also **fixes the Belt Bag** so those same items (notably the **Shotgun** and **Kitchen
knife**, which vanilla blocks for being scrap) can be stored in it — see the Belt Bag
section below.

> This mod only changes what **your** client puts into **your** utility slot. It is safe
> in lobbies where other players don't have it. Item placement uses the exact same
> vanilla code path (slot index `50`) that the flashlight and walkie-talkie use, so it is
> networked normally.

---

## How it works (verified against the game code)

The v80 utility slot's blacklist lives in **one** method:
`PlayerControllerB.FirstEmptyItemSlot(GrabbableObject)`. It chooses which inventory slot
a freshly grabbed item goes to and returns the sentinel `50` (the `ItemOnlySlot`, i.e.
the utility/Tab slot) **only** when:

```
ItemOnlySlot == null           // utility slot is empty
&& attemptingGrab != null
&& !itemProperties.isScrap        // gate 1
&& !itemProperties.twoHanded      // gate 2
&& !itemProperties.disallowUtilitySlot   // gate 3
```

Those three flags are the whole blacklist. Different target items are blocked by
different gates:

| Item        | Blocked by            |
|-------------|-----------------------|
| Shovel      | `disallowUtilitySlot` |
| Clipboard   | `disallowUtilitySlot` |
| Sticky note | `disallowUtilitySlot` |
| Ammo        | `disallowUtilitySlot` |
| Key         | `disallowUtilitySlot` |
| Kitchen knife | `isScrap`           |
| Shotgun     | `twoHanded`           |

The mod uses **HarmonyX** to add a **Prefix** to `FirstEmptyItemSlot`. When the utility
slot is empty and the grabbed item is one you enabled, the Prefix returns `50` directly
— reproducing vanilla's own "allowed" branch while skipping all three gates. For every
other item (and for the `null` probe the game uses elsewhere) it does nothing and the
original method runs unchanged.

Because the **Tab** handler (`UseUtilitySlot_performed`) only toggles the active slot
to/from `50` and never re-validates the item, filling slot `50` is all that's needed:

- **Auto-pickup:** grabbing an enabled item while the utility slot is empty stashes it
  in the slot automatically, exactly like a flashlight/walkie.
- **Tab swap:** Tab toggles your active item with whatever is in the utility slot.

### ⚠️ Shotgun caveat (two-handed)

The Shotgun is a **two-handed** item. The mod can place it in the utility slot and you
can Tab **to** it, but vanilla's two-handed rule still locks slot switching while a
two-handed item is actually in your hands — so once you Tab to the shotgun you must drop
it to switch away, just like any two-handed item. Placement works; the two-handed
handling is unchanged by design. Disable `[Items] Shotgun` if you don't want it.

---

## Belt Bag

The belt bag is a **separate** system with its own rules (verified against
`BeltBagItem.ItemInteractLeftRight`). While holding the bag you left-interact aiming at a
world item within ~4 m; it is stored **unless** the item is:

- **scrap** (`itemProperties.isScrap`) — this is why the **Shotgun** and **Kitchen knife**
  can't go in vanilla; both are scrap,
- currently held by a player or enemy, or
- the **Maneater / baby cave dweller** (hard-coded `itemId` 123984 or 819501 — those
  constants appear only inside `CaveDwellerAI`).

So besides scrap, the Maneater is the **only** item-type the belt bag rejects.

This mod adds a **Postfix** to that method: vanilla runs untouched (and, for a blocked
item, simply does nothing), then we repeat the exact same raycast and, if the aimed item
is one you enabled, store it via the game's own `TryAddObjectToBag`. A `tryingAddToBag`
guard prevents any double-add. The situational guards (held / held-by-enemy, the 15-item
limit) are preserved.

- `[BeltBag] Enabled` — master switch for this feature (default on). Which items are
  allowed reuses the `[Items]` toggles, so enabling e.g. `[Items] Knife` lets the knife go
  in **both** the utility slot and the belt bag.
- The **Maneater is never added to the belt bag**, even with `[Items] Maneater` on. It is a
  live AI creature (`CaveDwellerAI`): storing it in the bag never runs its grab/equip path
  (`CaveDwellerPhysicsProp.EquipItem` → `PickUpBabyLocalClient`), which is what disables its
  NavMeshAgent — so a bagged Maneater keeps running the hidden body and desyncs. Vanilla
  blocks it for this reason. The Maneater is supported **only in the utility slot**, whose
  grab path *does* call `EquipItem` and freezes it correctly.

Non-scrap items your list already covers (Shovel, Clipboard, Sticky note, Key, Ammo) are
**not** blocked by the belt bag in the first place, so they already work there in vanilla.

---

## Build

Requirements: **.NET SDK 6.0+** (`dotnet --version`).

```bash
dotnet build -c Release
```

Output: `bin/Release/Iron.ImprovedTabSlot.dll`.

### Game references

The `.csproj` pulls stripped game assemblies from the
[`LethalCompany.GameLibs.Steam`](https://www.nuget.org/packages/LethalCompany.GameLibs.Steam)
NuGet package. **Match the package version to your game build** — it is set to
`81.0.0-*` (newest v81 prerelease). For a strictly-v80 install use `80.0.0-*`. The mod
only touches long-stable public members, so building against 81 still runs on v80.

Prefer to reference your local install instead? The csproj has a commented-out
`Assembly-CSharp` `HintPath` block — delete the GameLibs `PackageReference`, uncomment
that block, and fix the path to your Steam install.

---

## Install (manual)

1. Install **BepInEx 5** (BepInExPack for Lethal Company) if you haven't.
2. Copy `Iron.ImprovedTabSlot.dll` into `Lethal Company/BepInEx/plugins/`.
3. Launch once to generate `BepInEx/config/Iron.ImprovedTabSlot.cfg`.

On launch you should see this line in `BepInEx/LogOutput.log`:

```
[Info : Improved TAB Slot] Utility-slot patch active on PlayerControllerB.FirstEmptyItemSlot.
```

If instead it logs that the method wasn't found or the Prefix didn't attach, the game
version changed the method — open an issue and it can be re-pointed.

---

## Configuration

File: `BepInEx/config/Iron.ImprovedTabSlot.cfg`

| Section | Key              | Default | Meaning |
|---------|------------------|---------|---------|
| General | `Enabled`        | `true`  | Master switch. |
| Items   | `Shovel`         | `true`  | Allow Shovel (+ Stop/Yield sign). |
| Items   | `Ammo`           | `true`  | Allow shotgun shells. |
| Items   | `Clipboard`      | `true`  | Allow Clipboard. |
| Items   | `StickyNote`     | `true`  | Allow Sticky note. |
| Items   | `Key`            | `true`  | Allow Keys. |
| Items   | `Shotgun`        | `true`  | Allow Shotgun (two-handed; see caveat). |
| Items   | `Knife`          | `true`  | Allow Kitchen knife. |
| Items   | `Maneater`       | `false` | Allow the Maneater into the **utility slot only** (never the belt bag; off by design). |
| BeltBag | `Enabled`        | `true`  | Also allow enabled `[Items]` into the belt bag. |
| Debug   | `VerboseLogging` | `false` | Log each routed / bagged item. |

---

## Compatibility

- **ReservedItemSlotCore / ReservedUtilitySlot (FlipMods):** this mod does **not** patch
  or call those mods — it only adds a Prefix to the vanilla `FirstEmptyItemSlot`, so it
  won't fight their slot logic. If you also run a mod that bridges the vanilla utility
  slot with ReservedItemSlotCore, watch for a Tab-keybind conflict (both may bind Tab);
  prefer running one utility-slot manager at a time.
- Nothing here is networked from the config; each client controls its own whitelist.

---

## Project layout

```
ImprovedTabSlot.csproj        # SDK-style build, game refs via NuGet
nuget.config                  # nuget.org + BepInEx feeds
manifest.json                 # Thunderstore metadata (add icon.png before packaging)
src/
  Plugin.cs                   # BaseUnityPlugin entry point, Harmony.PatchAll + verify
  PluginConfig.cs             # BepInEx config bindings
  ItemIdentity.cs             # "is this one of our items & enabled?"
  Patches/
    UtilitySlotPatch.cs       # Prefix on PlayerControllerB.FirstEmptyItemSlot
    BeltBagPatch.cs           # Postfix on BeltBagItem.ItemInteractLeftRight
```

> **Packaging for Thunderstore:** add a 256×256 `icon.png` next to `manifest.json`
> before zipping. It's intentionally omitted here.
