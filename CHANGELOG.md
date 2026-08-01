# Changelog

## 1.3.0
- Renamed to **ImprovedTABSlot+Beltbag** (Thunderstore package `ImprovedTABSlot_Beltbag`,
  assembly/plugin `Iron.ImprovedTABSlot_Beltbag`) to reflect that it covers both the utility
  slot and the belt bag. NOTE: the config file is now `Iron.ImprovedTABSlot_Beltbag.cfg`.
- Added a Russian description to the README (rendered on the mod page).

## 1.2.1
- Fix: the Maneater is no longer added to the belt bag. It's a live AI creature and the belt
  bag never runs its grab/equip path, so a bagged Maneater kept running its hidden body and
  desynced (appeared in the bag icon but stayed in the world and escaped). It is now supported
  only in the utility slot, whose grab path calls EquipItem and freezes the creature correctly.
- `[Items] Maneater` now means "utility (Tab) slot only".

## 1.2.0
- The Maneater (baby cave dweller) can now be enabled for the **utility slot** too, not just
  the belt bag, via a single `[Items] Maneater` toggle (default off) that governs both.
- Removed the separate `[BeltBag] AllowManeater` (superseded by `[Items] Maneater`).

## 1.1.0
- Belt Bag support: normally-blocked items (Shotgun and Kitchen knife — both scrap) can now
  be stored in the belt bag. Which items are allowed reuses the `[Items]` toggles.
- Added `[BeltBag] Enabled` (default on) and `[BeltBag] AllowManeater` (default off).
- Documented that, besides scrap, the only item-type the vanilla belt bag rejects is the
  Maneater / baby cave dweller (itemId 123984 / 819501).
- Postfix on `BeltBagItem.ItemInteractLeftRight`; startup log now confirms both patches.

## 1.0.0
- Initial release.
- Allows Shovel, Ammo (shotgun shells), Clipboard, Sticky note, Key, Shotgun and Kitchen knife into the v80 vanilla utility slot.
- Per-item toggles via BepInEx config.
- Runtime auto-detection of the utility-slot validator method, with a config override
  (`ValidatorMethodOverride`) and a diagnostic member dump (`DumpUtilityMembers`) as fallbacks.
