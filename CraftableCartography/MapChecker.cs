using CraftableCartography.Config;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using static CraftableCartography.Lib.ItemChecks;

namespace CraftableCartography
{
    public class MapChecker : ModSystem
    {
        ICoreClientAPI capi;

        CraftableCartographyModConfig ModConfig
        {
            get
            {
                return capi.ModLoader.GetModSystem<CraftableCartographyModSystem>().Config.modConfig;
            }
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            capi = api;

            api.Event.RegisterGameTickListener(MapCheckTick, 1000);
        }

        public void MapCheckTick(float dt)
        {
            GuiDialogWorldMap map = capi.ModLoader.GetModSystem<WorldMapManager>().worldMapDlg;
            if (map != null)
            {
                if (map.DialogType == EnumDialogType.HUD)
                {
                    if (!IsMinimapAllowed())
                    {
                        map.TryClose();
                    }
                }
                else if (map.DialogType == EnumDialogType.Dialog)
                {
                    if (!IsMapAllowed())
                    {
                        map.TryClose();
                    }
                }
            }
        }

        public bool IsMinimapAllowed()
        {
            return ModConfig.AllowMapOpeningWithoutTool || HasJPS(capi.World.Player);
        }

        public bool IsMapAllowed()
        {
            return ModConfig.AllowMapOpeningWithoutTool || HasMap(capi.World.Player) || HasJPS(capi.World.Player);
        }
    }
}
