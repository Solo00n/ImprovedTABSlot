using System;

namespace ImprovedTabSlot
{
    /// <summary>
    /// Decides whether a grabbed item is one of our target items AND enabled in config.
    /// Matched by runtime class name (walking the inheritance chain) where a dedicated
    /// class exists, and always cross-checked against the item's display name
    /// (itemProperties.itemName) for items that share a class or have no class of their own.
    ///
    /// Game types (GrabbableObject, Item) live in the global namespace, so they are
    /// referenced here without a using directive.
    /// </summary>
    internal static class ItemIdentity
    {
        // The Maneater / baby cave dweller. These itemIds appear only inside CaveDwellerAI,
        // and are exactly the two the belt bag hard-codes as blocked.
        internal const int ManeaterItemIdA = 123984;
        internal const int ManeaterItemIdB = 819501;

        /// <summary>True if the object is the Maneater (baby cave dweller), by itemId.</summary>
        public static bool IsManeater(GrabbableObject o)
        {
            Item p = o?.itemProperties;
            return p != null && (p.itemId == ManeaterItemIdA || p.itemId == ManeaterItemIdB);
        }

        /// <summary>Best-effort readable name for logging.</summary>
        public static string Name(GrabbableObject o)
        {
            if (o == null) return "<null>";
            if (o.itemProperties != null && !string.IsNullOrEmpty(o.itemProperties.itemName))
                return o.itemProperties.itemName;
            return o.GetType().Name;
        }

        /// <summary>True if this item is whitelisted and its per-item config toggle is on.</summary>
        public static bool IsAllowed(GrabbableObject o)
        {
            if (o == null) return false;
            var c = Plugin.Cfg;

            string n = (o.itemProperties != null && o.itemProperties.itemName != null)
                ? o.itemProperties.itemName.Trim().ToLowerInvariant()
                : "";

            // Shovel + its reskins (Stop sign / Yield sign reuse the Shovel class).
            if (c.AllowShovel.Value && (HasClass(o, "Shovel")
                || n == "shovel" || n == "stop sign" || n == "yield sign")) return true;

            // Shotgun (two-handed — see README note).
            if (c.AllowShotgun.Value && (HasClass(o, "ShotgunItem") || n == "shotgun")) return true;

            // Kitchen knife (scrap).
            if (c.AllowKnife.Value && (HasClass(o, "KnifeItem") || n.Contains("knife"))) return true;

            // Keys.
            if (c.AllowKey.Value && (HasClass(o, "KeyItem") || n == "key")) return true;

            // Clipboard and Sticky note are matched by NAME only: the sticky note has no
            // dedicated class, so class-matching could not tell them apart cleanly.
            if (c.AllowClipboard.Value && n == "clipboard") return true;
            if (c.AllowStickyNote.Value && n == "sticky note") return true;

            // Ammo (shotgun shells) — no dedicated class in vanilla.
            if (c.AllowAmmo.Value && (n == "ammo" || n.Contains("shell"))) return true;

            // Maneater (baby cave dweller) — matched by itemId, opt-in.
            if (c.AllowManeater.Value && IsManeater(o)) return true;

            // Kiwi egg ("Easter egg", laid by the Giant Kiwi) — KiwiBabyItem, opt-in.
            if (c.AllowEasterEgg.Value && (HasClass(o, "KiwiBabyItem") || n.Contains("egg"))) return true;

            return false;
        }

        /// <summary>True if the object's type, or any base up to (not including)
        /// GrabbableObject, is named <paramref name="simpleName"/>.</summary>
        private static bool HasClass(GrabbableObject o, string simpleName)
        {
            for (Type t = o.GetType(); t != null && t != typeof(GrabbableObject); t = t.BaseType)
                if (t.Name == simpleName) return true;
            return false;
        }
    }
}
