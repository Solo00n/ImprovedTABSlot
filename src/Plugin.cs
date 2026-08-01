using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;   // PlayerControllerB
using HarmonyLib;

namespace ImprovedTabSlot
{
    /// <summary>
    /// Main BepInEx plugin entry point. Client-side only: it only changes what YOUR
    /// client lets into YOUR own utility slot, so it is safe in lobbies where other
    /// players do not have the mod.
    /// </summary>
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "Iron.ImprovedTabSlot";
        public const string PLUGIN_NAME = "Improved TAB Slot";
        public const string PLUGIN_VERSION = "1.2.1";

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }
        internal static PluginConfig Cfg { get; private set; }

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Cfg = new PluginConfig(Config);

            _harmony = new Harmony(PLUGIN_GUID);

            try
            {
                _harmony.PatchAll(typeof(Plugin).Assembly);
                VerifyPatched();
            }
            catch (Exception e)
            {
                Log.LogError($"Harmony patching failed; the mod is inactive: {e}");
            }

            Log.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded.");
        }

        /// <summary>
        /// Confirms each patch actually attached, so a silent no-op (e.g. after a game update
        /// renames a method) shows up clearly in the log.
        /// </summary>
        private void VerifyPatched()
        {
            VerifyOne("utility-slot",
                AccessTools.Method(typeof(PlayerControllerB), "FirstEmptyItemSlot", new[] { typeof(GrabbableObject) }),
                "PlayerControllerB.FirstEmptyItemSlot(GrabbableObject)");

            VerifyOne("belt-bag",
                AccessTools.Method(typeof(BeltBagItem), "ItemInteractLeftRight", new[] { typeof(bool) }),
                "BeltBagItem.ItemInteractLeftRight(bool)");
        }

        private void VerifyOne(string label, System.Reflection.MethodBase target, string desc)
        {
            if (target == null)
            {
                Log.LogError($"{desc} was not found. The game version may have changed it; " +
                             $"the {label} patch is INACTIVE.");
                return;
            }

            var info = Harmony.GetPatchInfo(target);
            bool attached =
                (info?.Prefixes != null && info.Prefixes.Any(p => p.owner == PLUGIN_GUID)) ||
                (info?.Postfixes != null && info.Postfixes.Any(p => p.owner == PLUGIN_GUID));

            if (attached)
                Log.LogInfo($"{char.ToUpper(label[0]) + label.Substring(1)} patch active on {desc}.");
            else
                Log.LogWarning($"{label} patch did not attach — mod is loaded but that feature is inactive.");
        }
    }
}
