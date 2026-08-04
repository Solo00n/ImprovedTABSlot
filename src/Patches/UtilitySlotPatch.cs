using System;
using GameNetcodeStuff;   // PlayerControllerB
using HarmonyLib;
using ImprovedTabSlot.Networking;

namespace ImprovedTabSlot
{
    /// <summary>
    /// Client-side patch that lets our whitelisted items enter the vanilla utility
    /// (Tab / "ItemOnly") slot.
    ///
    /// How the vanilla utility slot actually works (verified against the game IL):
    ///   * <c>PlayerControllerB.FirstEmptyItemSlot(GrabbableObject)</c> decides which slot
    ///     a freshly grabbed item goes to. It returns the sentinel <c>50</c> (the
    ///     <c>ItemOnlySlot</c>) only when:
    ///         ItemOnlySlot == null            (utility slot empty)
    ///      &amp;&amp; attemptingGrab != null
    ///      &amp;&amp; !itemProperties.isScrap
    ///      &amp;&amp; !itemProperties.twoHanded
    ///      &amp;&amp; !itemProperties.disallowUtilitySlot
    ///     Those last three flags are exactly the vanilla blacklist gates.
    ///   * The Tab key (<c>UseUtilitySlot_performed</c>) merely toggles the active slot
    ///     to/from 50; it does NOT re-validate the item. So populating slot 50 via
    ///     FirstEmptyItemSlot is all that's needed for both auto-pickup and Tab swapping.
    ///
    /// This Prefix reproduces vanilla's "return 50" branch for our whitelisted items,
    /// bypassing the three gates, and only when the utility slot is empty. For every other
    /// item (or a null probe) it defers to the original method unchanged.
    /// </summary>
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class UtilitySlotPatch
    {
        /// <summary>The slot index the game reserves for the utility ("ItemOnly") slot.</summary>
        internal const int UtilitySlotIndex = 50;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerControllerB.FirstEmptyItemSlot), new[] { typeof(GrabbableObject) })]
        private static bool FirstEmptyItemSlot_Prefix(
            PlayerControllerB __instance, GrabbableObject attemptingGrab, ref int __result)
        {
            try
            {
                if (!HostSync.Enabled) return true;                  // host doesn't have the mod -> vanilla
                if (!Plugin.Cfg.Enabled.Value) return true;          // run original
                if (attemptingGrab == null) return true;             // null probe (Tab-out path)
                if (__instance.ItemOnlySlot != null) return true;    // utility slot occupied -> normal logic
                if (!ItemIdentity.IsAllowed(attemptingGrab)) return true;

                __result = UtilitySlotIndex;
                if (Plugin.Cfg.VerboseLogging.Value)
                    Plugin.Log.LogInfo($"Routing '{ItemIdentity.Name(attemptingGrab)}' to the utility slot.");
                return false;                                        // skip original
            }
            catch (Exception e)
            {
                // Never break vanilla item pickup because of us.
                Plugin.Log.LogError($"FirstEmptyItemSlot_Prefix error: {e}");
                return true;
            }
        }
    }
}
