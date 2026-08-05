using BepInEx.Configuration;

namespace ImprovedTabSlot
{
    /// <summary>
    /// All BepInEx config bindings. Written to
    /// BepInEx/config/Iron.ImprovedTABSlot.cfg on first launch.
    ///
    /// These settings are the HOST's rules. In multiplayer the feature only activates when
    /// the host has the mod; each client's own toggles decide what THEY may place, but the
    /// host stays authoritative over the shared game state.
    /// </summary>
    public class PluginConfig
    {
        public readonly ConfigEntry<bool> Enabled;

        public readonly ConfigEntry<bool> AllowShovel;
        public readonly ConfigEntry<bool> AllowAmmo;
        public readonly ConfigEntry<bool> AllowClipboard;
        public readonly ConfigEntry<bool> AllowStickyNote;
        public readonly ConfigEntry<bool> AllowKey;
        public readonly ConfigEntry<bool> AllowShotgun;
        public readonly ConfigEntry<bool> AllowKnife;
        public readonly ConfigEntry<bool> AllowManeater;
        public readonly ConfigEntry<bool> AllowEasterEgg;

        public readonly ConfigEntry<bool> VerboseLogging;

        public PluginConfig(ConfigFile file)
        {
            Enabled = file.Bind(
                "General", "Enabled", true,
                "Master switch. When false the mod does nothing and the vanilla utility-slot blacklist is left untouched.");

            AllowShovel = file.Bind(
                "Items", "Shovel", true,
                "Allow the Shovel into the utility slot. (Stop sign / Yield sign reuse the Shovel class and are included.)");
            AllowAmmo = file.Bind(
                "Items", "Ammo", true,
                "Allow Ammo (shotgun shells) into the utility slot.");
            AllowClipboard = file.Bind(
                "Items", "Clipboard", true,
                "Allow the Clipboard into the utility slot.");
            AllowStickyNote = file.Bind(
                "Items", "StickyNote", true,
                "Allow the Sticky note into the utility slot.");
            AllowKey = file.Bind(
                "Items", "Key", true,
                "Allow Keys into the utility slot.");
            AllowShotgun = file.Bind(
                "Items", "Shotgun", true,
                "Allow the Shotgun into the utility slot. NOTE: the Shotgun is two-handed, so vanilla's " +
                "two-handed switching lock still applies once it is in your hands (see README).");
            AllowKnife = file.Bind(
                "Items", "Knife", true,
                "Allow the Kitchen knife into the utility slot.");
            AllowManeater = file.Bind(
                "Items", "Maneater", false,
                "Allow the Maneater (baby cave dweller) into the utility (Tab) slot. The utility slot uses the " +
                "real grab path (EquipItem) which freezes the creature correctly. WARNING: off by design — it's a " +
                "balance cheese. Set true only if you specifically want it.");
            AllowEasterEgg = file.Bind(
                "Items", "EasterEgg", false,
                "Allow the Kiwi egg (the \"Easter egg\" laid by the Giant Kiwi) into the utility slot. It is a " +
                "normal grabbable (no AI of its own), so it behaves like any other item here. Off by default.");

            VerboseLogging = file.Bind(
                "Debug", "VerboseLogging", false,
                "Log each time an item is routed to the utility slot, and networking state changes.");
        }
    }
}
