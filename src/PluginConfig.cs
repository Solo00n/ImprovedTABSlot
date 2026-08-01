using BepInEx.Configuration;

namespace ImprovedTabSlot
{
    /// <summary>
    /// All BepInEx config bindings. Written to
    /// BepInEx/config/Iron.ImprovedTabSlot.cfg on first launch.
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

        public readonly ConfigEntry<bool> BeltBagEnabled;

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
                "Allow the Maneater (baby cave dweller) into the UTILITY (Tab) SLOT. It is intentionally NOT " +
                "allowed in the belt bag: the bag never runs the creature's grab/equip path, so a bagged Maneater " +
                "would keep running its (hidden) body and desync. The utility slot uses the real grab path and " +
                "freezes it correctly. WARNING: still off by design — it's a balance cheese. Set true only if you " +
                "want it.");

            BeltBagEnabled = file.Bind(
                "BeltBag", "Enabled", true,
                "Also let normally-blocked items be stored in the Belt Bag. Vanilla's belt bag rejects all " +
                "scrap items (so the Shotgun and Kitchen knife — both scrap — cannot go in). With this on, any " +
                "scrap item enabled in the [Items] section above can be added to the belt bag too. (The Maneater " +
                "is never added to the belt bag — see the [Items] Maneater note.)");

            VerboseLogging = file.Bind(
                "Debug", "VerboseLogging", false,
                "Log each time an item is routed to the utility slot or added to the belt bag by this mod.");
        }
    }
}
