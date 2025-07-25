using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using static CraftableCartography.Lib.ItemChecks;

namespace CraftableCartography.Patches
{
    [HarmonyPatch]
    class MapLayerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OwnedEntityMapLayer), nameof(OwnedEntityMapLayer.Render)]
        public static bool OwnedEntityMapLayerCheck(OwnedEntityMapLayer __instance)
        {
            ICoreClientAPI capi = Traverse.Create(__instance).Field("capi").GetValue<ICoreClientAPI>();

            return HasJPS(capi.World.Player);
        }
    }
}
