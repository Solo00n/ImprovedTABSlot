using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;   // PlayerControllerB
using HarmonyLib;

namespace ImprovedTabSlot
{
    /// <summary>
    /// Main BepInEx plugin entry point.
    ///
    /// Networking model: host-authoritative. The utility-slot feature is inert on a client
    /// until the HOST tells it the host has the mod (see <see cref="Networking.HostSync"/>).
    /// A lone modded client in a vanilla lobby therefore gets no gameplay change — which is
    /// what the Lethal Company community requires (no client-side advantage). The host still
    /// controls everything through its own config.
    /// </summary>
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "Iron.ImprovedTABSlot";
        public const string PLUGIN_NAME = "ImprovedTABSlot";
        public const string PLUGIN_VERSION = "2.0.0";

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
        /// Confirms the utility-slot Prefix attached, so a silent no-op (e.g. after a game
        /// update renames the method) shows up clearly in the log.
        /// </summary>
        private void VerifyPatched()
        {
            var target = AccessTools.Method(
                typeof(PlayerControllerB), "FirstEmptyItemSlot", new[] { typeof(GrabbableObject) });

            if (target == null)
            {
                Log.LogError("PlayerControllerB.FirstEmptyItemSlot(GrabbableObject) was not found. " +
                             "The game version may have changed it; the utility-slot patch is INACTIVE.");
                return;
            }

            var info = Harmony.GetPatchInfo(target);
            bool attached = info?.Prefixes != null && info.Prefixes.Any(p => p.owner == PLUGIN_GUID);
            Log.LogInfo(attached
                ? "Utility-slot patch active on PlayerControllerB.FirstEmptyItemSlot."
                : "WARNING: utility-slot Prefix did not attach — mod is loaded but inactive.");
        }
    }
}
