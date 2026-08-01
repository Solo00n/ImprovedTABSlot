using System;
using GameNetcodeStuff;   // PlayerControllerB
using HarmonyLib;
using UnityEngine;

namespace ImprovedTabSlot
{
    /// <summary>
    /// Client-side patch that lets normally-blocked items be stored in the Belt Bag.
    ///
    /// How the vanilla belt bag decides what it accepts (verified against the game IL of
    /// <c>BeltBagItem.ItemInteractLeftRight(bool)</c>): while holding the bag you LEFT-interact
    /// while aiming at a world item within ~4 m; it is added unless it is
    ///   * scrap (<c>itemProperties.isScrap</c>),
    ///   * currently held (<c>isHeld</c> / <c>isHeldByEnemy</c>), or
    ///   * one of two hard-coded itemIds — 123984 and 819501 — which are the Maneater / baby
    ///     cave dweller (those constants appear only inside <c>CaveDwellerAI</c>).
    /// So besides scrap, the Maneater is the only item-type the belt bag rejects. The Shotgun
    /// and Kitchen knife are blocked simply because they are scrap.
    ///
    /// We only re-enable the SCRAP items here. The Maneater is deliberately NOT added: it is a
    /// live AI creature (CaveDwellerAI). Storing it in the bag never runs its grab/equip path
    /// (CaveDwellerPhysicsProp.EquipItem -> PickUpBabyLocalClient), which is what disables its
    /// NavMeshAgent — so a bagged Maneater keeps running the (hidden) body and desyncs. Vanilla
    /// forbids it for this reason. The Maneater is only supported in the utility slot, whose grab
    /// path DOES call EquipItem and therefore freezes the creature correctly.
    ///
    /// We use a Postfix (not a Prefix that skips the original): vanilla runs fully and — for a
    /// blocked item — simply does nothing, leaving <c>tryingAddToBag == false</c>. We then repeat
    /// the exact same raycast and, if the aimed item is one we're configured to allow, add it via
    /// the game's own <c>TryAddObjectToBag</c>. The <c>tryingAddToBag</c> guard prevents any
    /// double-add when vanilla already accepted the item.
    /// </summary>
    [HarmonyPatch(typeof(BeltBagItem))]
    internal static class BeltBagPatch
    {
        // These MUST match BeltBagItem.ItemInteractLeftRight's own raycast so we resolve the
        // same target the game would: 4 m, layer mask 1073742144, triggers ignored.
        private const float ReachDistance = 4f;
        private const int RaycastMask = 1073742144;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BeltBagItem.ItemInteractLeftRight), new[] { typeof(bool) })]
        private static void ItemInteractLeftRight_Postfix(BeltBagItem __instance, bool right)
        {
            try
            {
                if (!Plugin.Cfg.Enabled.Value || !Plugin.Cfg.BeltBagEnabled.Value) return;
                if (right) return;                        // only LEFT interact adds (vanilla rule)
                if (__instance.tryingAddToBag) return;    // vanilla already added something this press

                PlayerControllerB player = __instance.playerHeldBy;
                if (player == null) return;
                if (__instance.objectsInBag == null || __instance.objectsInBag.Count >= 15) return;

                Transform cam = player.gameplayCamera.transform;
                if (!Physics.Raycast(cam.position, cam.forward, out RaycastHit hit,
                        ReachDistance, RaycastMask, QueryTriggerInteraction.Ignore))
                    return;

                GrabbableObject target = hit.collider.gameObject.GetComponent<GrabbableObject>();
                if (target == null || target == __instance) return;
                if (target.isHeld || target.isHeldByEnemy) return;   // keep vanilla's safety guards

                Item props = target.itemProperties;
                if (props == null) return;

                // Never bag the Maneater: it's a live AI the bag can't freeze (see class summary).
                if (ItemIdentity.IsManeater(target)) return;

                // Scrap is the only other thing the belt bag rejects. Non-scrap items are added by
                // vanilla already, so there is nothing for us to do for them.
                if (!props.isScrap) return;
                if (!ItemIdentity.IsAllowed(target)) return;   // not one of our enabled items

                if (Plugin.Cfg.VerboseLogging.Value)
                    Plugin.Log.LogInfo($"Belt bag: adding normally-blocked '{ItemIdentity.Name(target)}'.");

                __instance.TryAddObjectToBag(target);
            }
            catch (Exception e)
            {
                // Never break vanilla belt-bag interaction because of us.
                Plugin.Log.LogError($"BeltBag ItemInteractLeftRight_Postfix error: {e}");
            }
        }
    }
}
