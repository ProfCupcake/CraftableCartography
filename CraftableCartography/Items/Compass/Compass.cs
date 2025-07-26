using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CraftableCartography.Items.Shared;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace CraftableCartography.Items.Compass
{
    public class Compass : Item
    {
        HudElementNavReading gui;

        float heading;
        float headingDelta;

        float damping = 0.92f;
        float accelerationMult = 4f;

        long lastUpdate;

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (byEntity.Api.Side == EnumAppSide.Client)
            {
                gui ??= new((ICoreClientAPI)byEntity.Api);
                gui.TryOpen();
                gui.SetText(GetText());
            }
            
            handling = EnumHandHandling.PreventDefault;
        }

        private void DoMoveStep(Entity byEntity)
        {
            if (lastUpdate == 0) lastUpdate = byEntity.World.ElapsedMilliseconds;

            float dt = (byEntity.World.ElapsedMilliseconds - lastUpdate) / 1000f;
            
            float yawDeg = 180 - byEntity.SidedPos.Yaw * (180 / GameMath.PI);

            float angleDiff = GameMath.AngleDegDistance(heading, yawDeg);

            headingDelta += (angleDiff * accelerationMult) * dt;
            headingDelta *= damping;

            heading += headingDelta * dt;

            while (heading < 0) heading += 360;
            while (heading > 360) heading -= 360;

            lastUpdate = byEntity.World.ElapsedMilliseconds;
        }

        private string GetText()
        {
            string word = "";
            if (heading < 67.5 || heading > 292.5)
            {
                word += "N";
            }
            else if (heading > 112.5 && heading < 247.5)
            {
                word += "S";
            }

            if (heading > 22.5 && heading < 157.5)
            {
                word += "E";
            }
            else if (heading > 202.5 && heading < 337.5)
            {
                word += "W";
            }

            string text = "";

            if (Code.FirstCodePart() == "compass") text += Math.Round(heading).ToString() + "°\n";
            text += word;

            return text;
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.Api.Side == EnumAppSide.Client)
            {
                DoMoveStep(byEntity);

                gui.SetText(GetText());
            }

            return true;
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.Api.Side == EnumAppSide.Client)
                gui.TryClose();
        }

        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            DoMoveStep(byEntity);
        }

        public override bool ConsumeCraftingIngredients(ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe)
        {
            if (Code.FirstCodePart() == "compassprimitive")
            {
                foreach (ItemSlot slot in slots)
                {
                    slot.TakeOut(1);
                }
            }
            
            return true;
        }
    }
}
