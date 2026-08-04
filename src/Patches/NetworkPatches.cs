using HarmonyLib;
using ImprovedTabSlot.Networking;
using Unity.Netcode;

namespace ImprovedTabSlot.Patches
{
    /// <summary>
    /// Wires up the host-authoritative handshake (see <see cref="HostSync"/>) into the game's
    /// network lifecycle. Method names are given as strings so Harmony resolves them at runtime
    /// regardless of their (sometimes non-public) visibility.
    /// </summary>
    [HarmonyPatch]
    internal static class NetworkPatches
    {
        // Register the named-message handler as soon as the network layer initializes.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NetworkManager), "Initialize")]
        private static void AfterNetworkInitialize() => HostSync.Register();

        // Tear it down when leaving the session.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "SetInstanceValuesBackToDefault")]
        private static void OnNetworkTearDown() => HostSync.Unregister();

        // The host enables the feature for itself when the lobby/round starts.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        private static void OnLobbyStart(StartOfRound __instance)
        {
            if (__instance.IsServer)
                HostSync.EnableAsHost();
        }

        // The host tells each connecting client "I have the mod" (server-side event).
        // __0 is the connecting client's id (first parameter of OnClientConnect).
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "OnClientConnect")]
        private static void OnClientConnected(StartOfRound __instance, ulong __0)
        {
            if (!__instance.IsServer) return;
            HostSync.SendHostPresent(new[] { __0 });
        }

        // Reset the gate on returning to the main menu.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuManager), "Start")]
        private static void OnMainMenu() => HostSync.Reset();
    }
}
