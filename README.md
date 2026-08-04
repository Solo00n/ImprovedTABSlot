# ImprovedTABSlot

**Language / Язык:** [English](#improvedtabslot) · [Русский](#russian)

A BepInEx mod for Lethal Company (v80+, "The Blooming Update", which added the utility
slot). It lets items that the vanilla utility-slot blacklist normally rejects be carried
in the **Tab** slot:

- **Shovel** (and its reskins: Stop sign / Yield sign)
- **Ammo** (shotgun shells)
- **Clipboard**
- **Sticky note**
- **Key**
- **Shotgun** *(two-handed — see caveat)*
- **Kitchen knife**
- **Maneater** (baby cave dweller) — *opt-in, off by default*

Each item can be toggled individually.

## Multiplayer (host-authoritative)

Because a client-side "extra slot" can be a competitive advantage, the feature is
**host-authoritative**: it stays inert on a client until the host confirms it also has the
mod (via a Netcode named-message handshake). A lone modded client in a vanilla lobby gets
no change; when the host has the mod, the utility slot uses the vanilla, fully-networked
slot path (`ItemOnlySlot`, index 50) so all players stay in sync.

## How it works

The blacklist lives in `PlayerControllerB.FirstEmptyItemSlot`, which routes a grabbed item
to slot `50` only when `!isScrap && !twoHanded && !disallowUtilitySlot`. A Harmony Prefix
reproduces that "return 50" branch for enabled items (when the slot is empty and the host
has the mod), bypassing those gates. The **Tab** handler only toggles the active slot and
never re-validates, so this covers both auto-pickup and Tab swapping.

**Shotgun caveat:** it's two-handed, so vanilla's two-handed switching lock still applies
once it's in your hands (you must drop it to switch away).

## Install
1. Install **BepInEx 5** (BepInExPack for Lethal Company).
2. Put `Iron.ImprovedTABSlot.dll` in `Lethal Company/BepInEx/plugins/`.
3. Launch once to generate `BepInEx/config/Iron.ImprovedTABSlot.cfg`.

## Config (`Iron.ImprovedTABSlot.cfg`)
| Section | Key | Default | Meaning |
|---------|-----|---------|---------|
| General | `Enabled` | `true` | Master switch. |
| Items | `Shovel` … `Knife` | `true` | Per-item toggles. |
| Items | `Maneater` | `false` | Allow the Maneater (off by design). |
| Debug | `VerboseLogging` | `false` | Log routed items + networking state. |

## Build
`dotnet build -c Release` → `bin/Release/Iron.ImprovedTABSlot.dll`. Game references come
from the `LethalCompany.GameLibs.Steam` NuGet package (match the version to your build).

## Compatibility
Only patches vanilla `FirstEmptyItemSlot`; doesn't touch ReservedItemSlotCore. If you run
another utility-slot manager, watch for a Tab-keybind conflict.

---

<a id="russian"></a>

# ImprovedTABSlot — Русское описание

**[⤴ English](#improvedtabslot)**

Мод на BepInEx для Lethal Company (v80+), позволяющий носить в **Tab-слоте** предметы,
которые ваниль туда не пускает:

- **Лопата** (и рероллы: Stop/Yield sign)
- **Патроны** (для дробовика)
- **Планшет**, **Стикер**, **Ключ**
- **Дробовик** *(двуручный — см. оговорку)*
- **Кухонный нож**
- **Манчер** (детёныш cave dweller) — *опция, по умолчанию выкл.*

Каждый предмет включается отдельно.

## Мультиплеер (host-authoritative)
Так как «лишний слот» на клиенте — это преимущество, фича **управляется хостом**: на
клиенте она выключена, пока хост named-message'ем не подтвердит, что мод есть и у него.
Одиночный клиент в ванильном лобби ничего не получает; когда мод у хоста — используется
штатный сетевой путь слота (`ItemOnlySlot`, индекс 50), и все синхронизированы. Это и есть
фикс правила Lethal Company про клиентское преимущество.

## Как работает
«Чёрный список» — в `PlayerControllerB.FirstEmptyItemSlot`: предмет уходит в слот `50`
только если `!isScrap && !twoHanded && !disallowUtilitySlot`. Harmony-Prefix повторяет
ветку «вернуть 50» для включённых предметов (когда слот пуст и мод у хоста), обходя эти
проверки. Клавиша **Tab** предмет не перепроверяет — покрыт и авто-подбор, и обмен по Tab.

**Оговорка про дробовик:** он двуручный, поэтому ванильная блокировка смены слота для
двуручных сохраняется (чтобы переключиться назад — выбросить).

## Установка
1. Поставь **BepInEx 5** (BepInExPack for Lethal Company).
2. Положи `Iron.ImprovedTABSlot.dll` в `Lethal Company/BepInEx/plugins/`.
3. Запусти раз — создастся `BepInEx/config/Iron.ImprovedTABSlot.cfg`.

## Совместимость
Патчит только ванильный `FirstEmptyItemSlot`, ReservedItemSlotCore не трогает. Если стоит
другой менеджер utility-слота — следи за конфликтом бинда Tab.
