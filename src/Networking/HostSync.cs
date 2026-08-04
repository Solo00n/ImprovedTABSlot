using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace ImprovedTabSlot.Networking
{
    /// <summary>
    /// Host-authoritative gate for the whole mod.
    ///
    /// <see cref="Enabled"/> starts false and is only turned on when the HOST (server) confirms
    /// it also has the mod:
    ///   * the host enables itself directly when the lobby starts;
    ///   * the host sends the <c>HostPresent</c> named message to every client that connects,
    ///     which flips <see cref="Enabled"/> true on that client.
    /// If the host is vanilla, clients never receive the message, so the utility-slot changes
    /// never apply — no client-side advantage. The message carries no payload; it is purely a
    /// "the host runs this mod" handshake, so nothing here can be spoofed for gain.
    /// </summary>
    internal static class HostSync
    {
        /// <summary>True only when the host has the mod (see class summary).</summary>
        internal static bool Enabled;

        private static readonly string HostPresentMessage = Plugin.PLUGIN_GUID + "|HostPresent";

        internal static void Register()
        {
            var nm = NetworkManager.Singleton;
            if (nm?.CustomMessagingManager == null) return;
            nm.CustomMessagingManager.RegisterNamedMessageHandler(HostPresentMessage, OnHostPresent);
        }

        internal static void Unregister()
        {
            var nm = NetworkManager.Singleton;
            if (nm?.CustomMessagingManager == null) return;
            nm.CustomMessagingManager.UnregisterNamedMessageHandler(HostPresentMessage);
        }

        internal static void EnableAsHost()
        {
            Enabled = true;
            if (Plugin.Cfg.VerboseLogging.Value)
                Plugin.Log.LogInfo("Host has the mod — utility-slot feature enabled (host).");
        }

        internal static void Reset() => Enabled = false;

        /// <summary>Server → client(s): "the host has this mod". Enables the feature client-side.</summary>
        internal static void SendHostPresent(IReadOnlyList<ulong> targets = null)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer || nm.CustomMessagingManager == null) return;

            var writer = new FastBufferWriter(0, Allocator.Temp);
            if (targets == null)
                nm.CustomMessagingManager.SendNamedMessageToAll(HostPresentMessage, writer);
            else
                nm.CustomMessagingManager.SendNamedMessage(HostPresentMessage, targets, writer);
        }

        private static void OnHostPresent(ulong senderId, FastBufferReader reader)
        {
            if (senderId != NetworkManager.ServerClientId) return; // only trust the server
            Enabled = true;
            if (Plugin.Cfg.VerboseLogging.Value)
                Plugin.Log.LogInfo("Host has the mod — utility-slot feature enabled (client).");
        }
    }
}
