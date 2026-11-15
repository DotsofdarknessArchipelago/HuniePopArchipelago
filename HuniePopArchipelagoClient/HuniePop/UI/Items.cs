using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.UI
{
    [HarmonyPatch]
    public class Items
    {


        [HarmonyPatch(typeof(GirlProfilePreference), "Init")]
        [HarmonyPostfix]
        public static void profileoverwite(GirlDefinition girlDefinition, int preferenceIndex, ref LabelObject ___valueLabel)
        {
            switch (preferenceIndex)
            {
                case 2:
                    ___valueLabel.SetText(StringUtils.Titleize("See Collection"));
                    break;
                case 3:
                    ___valueLabel.SetText(StringUtils.Titleize("See Collection"));
                    break;
                case 4:
                    ___valueLabel.SetText(StringUtils.Titleize("See Collection"));
                    break;
            }
        }

       ///// <summary>
       ///// overwrite the tooltip for items in girls collection that you can buy
       ///// NOTE mainly just the same method as vanilla method with checks for archipelago stuff
       ///// </summary>
       //[HarmonyPatch(typeof(ItemTooltipContent), "Refresh")]
       //[HarmonyPrefix]
       //public static void tooltipoverite(ref ItemTooltipContent __instance, ref ItemDefinition ____itemDefinition, ref bool ____collectionItem, ref bool ____storeItem)
       //{
       //    if (____itemDefinition != null && ____itemDefinition.type == ItemType.GIFT)
       //    {
       //        __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.giftType.ToString()));
       //        __instance.valueLabel.SetText("Gift Value: " + ____itemDefinition.itemFunctionValue.ToString());
       //        __instance.nameLabel.SetColor(ColorUtils.HexToColor("A63E6E"));
       //        __instance.typeLabel.SetColor(ColorUtils.HexToColor("C06E94"));
       //        __instance.valueLabel.SetColor(ColorUtils.HexToColor("A16799"));
       //
       //        int hunie = -1;
       //        if (ArchipelagoClient.gifthunniecost.Keys.Contains(____itemDefinition.id)) { hunie = ArchipelagoClient.gifthunniecost[____itemDefinition.id]; }
       //        else { hunie = 99999; }
       //
       //        if (____collectionItem && GameManager.System.Player.endingSceneShown)
       //        {
       //            string text = __instance.valueLabel.label.text + "\n";
       //            string text2 = ColorUtils.ColorToHex(__instance.valueLabel.label.color);
       //            if (GameManager.System.GameState != GameState.SIM)
       //            {
       //                text += "[[Can't order while dating.]stop]";
       //            }
       //            else if (GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory))
       //            {
       //                text += "[[Your inventory is too full.]stop]";
       //            }
       //            else if (ArchipelagoClient.alist.hasitem(IDs.giftidtooffset(____itemDefinition.id) + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"])))
       //            {
       //                if (GameManager.System.Player.hunie < hunie)
       //                {
       //                    text += "[[Not enough Hunie to order.]stop]";
       //                }
       //                else
       //                {
       //                    text += $"[[Click to order ({hunie} Hunie).]go]";
       //                }
       //            }
       //            else if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["disable_girl_gifts"]))
       //            {
       //                text += "[[Girl gifts are disabled.]stop]";
       //            }
       //            else
       //            {
       //                text += "[[Item can't be ordered yet.]stop]";
       //            }
       //            __instance.valueLabel.SetColor(Color.white);
       //            __instance.valueLabel.SetText(StringUtils.FlattenColorBunches(text, text2));
       //        }
       //    }
       //    else if (____itemDefinition != null && ____itemDefinition.type == ItemType.UNIQUE_GIFT)
       //    {
       //        __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.specialGiftType.ToString()));
       //        __instance.nameLabel.SetColor(ColorUtils.HexToColor("9E6B21"));
       //        __instance.typeLabel.SetColor(ColorUtils.HexToColor("B3884F"));
       //
       //        int hunie = -1;
       //        if (ArchipelagoClient.gifthunniecost.Keys.Contains(____itemDefinition.id)) { hunie = ArchipelagoClient.gifthunniecost[____itemDefinition.id]; }
       //        else { hunie = 99999; }
       //
       //        if (____collectionItem && GameManager.System.Player.endingSceneShown)
       //        {
       //            string text = "";
       //            string text2 = ColorUtils.ColorToHex(__instance.valueLabel.label.color);
       //            if (GameManager.System.GameState != GameState.SIM)
       //            {
       //                text += "[[Can't order while dating.]stop]";
       //            }
       //            else if (GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory))
       //            {
       //                text += "[[Your inventory is too full.]stop]";
       //            }
       //            else if (ArchipelagoClient.alist.hasitem(IDs.giftidtooffset(____itemDefinition.id) + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"])))
       //            {
       //                if (GameManager.System.Player.hunie < hunie)
       //                {
       //                    text += "[[Not enough Hunie to order.]stop]";
       //                }
       //                else
       //                {
       //                    text += $"[[Click to order ({hunie} Hunie).]go]";
       //                }
       //            }
       //            else if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["disable_girl_gifts"]))
       //            {
       //                text += "[[Girl gifts are disabled.]stop]";
       //            }
       //            else
       //            {
       //                text += "[[Item can't be ordered yet.]stop]";
       //            }
       //            __instance.valueLabel.SetColor(Color.white);
       //            __instance.valueLabel.SetText(StringUtils.FlattenColorBunches(text, text2));
       //        }
       //    }
       //
       //
       //    if (____storeItem)
       //    {
       //    }
       //
       //}


        [HarmonyPatch(typeof(ItemTooltipContent), "Refresh")]
        [HarmonyPrefix]
        public static bool tooltipoverite2(ref ItemTooltipContent __instance, 
            ref ItemDefinition ____itemDefinition, 
            ref bool ____collectionItem, 
            ref bool ____storeItem,
            ref bool ____puzzleStatusItem,
            ref bool ____traitsItem,
            ref bool ____hasValue,
            ref float ____valueLabelOrigY)
        {
            if (____itemDefinition == null)
            {
                return false;
            }
            int hunie = -1;
            __instance.nameLabel.SetText(____itemDefinition.name);
            __instance.descriptionLabel.SetText(StringUtils.FlattenColorBunches(____itemDefinition.description, "545454"));
            if (____itemDefinition.type == ItemType.GIFT && ____itemDefinition.id == 106 && GameManager.System.Player.GetGirlData(GameManager.Stage.uiGirl.catGirlDef).metStatus == GirlMetStatus.LOCKED)
            {
                __instance.descriptionLabel.SetText(StringUtils.FlattenColorBunches("Please do not discard outdoors as it may attract stray cats.", "545454"));
            }
            switch (____itemDefinition.type)
            {
                case ItemType.FOOD:
                    __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.foodType.ToString()));
                    __instance.valueLabel.SetText("Nutrition Value: " + ____itemDefinition.itemFunctionValue.ToString());
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("B8562C"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("CC7962"));
                    __instance.valueLabel.SetColor(ColorUtils.HexToColor("9E6A42"));
                    break;
                case ItemType.DRINK:
                    __instance.typeLabel.SetText("Food  |  " + StringUtils.Titleize(____itemDefinition.type.ToString()));
                    __instance.valueLabel.SetText("Alcoholic Value: " + ____itemDefinition.itemFunctionValue.ToString());
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("326CB3"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("528CDB"));
                    __instance.valueLabel.SetColor(ColorUtils.HexToColor("508591"));
                    break;
                case ItemType.GIFT:
                    __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.giftType.ToString()));
                    __instance.valueLabel.SetText("Gift Value: " + ____itemDefinition.itemFunctionValue.ToString());
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("A63E6E"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("C06E94"));
                    __instance.valueLabel.SetColor(ColorUtils.HexToColor("A16799"));

                    hunie = -1;
                    if (ArchipelagoClient.gifthunniecost.Keys.Contains(____itemDefinition.id)) { hunie = ArchipelagoClient.gifthunniecost[____itemDefinition.id]; }
                    else { hunie = 99999; }

                    if (____collectionItem && GameManager.System.Player.endingSceneShown)
                    {
                        string text = __instance.valueLabel.label.text + "\n";
                        string text2 = ColorUtils.ColorToHex(__instance.valueLabel.label.color);
                        if (GameManager.System.GameState != GameState.SIM)
                        {
                            text += "[[Can't order while dating.]stop]";
                        }
                        else if (GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory))
                        {
                            text += "[[Your inventory is too full.]stop]";
                        }
                        else if (ArchipelagoClient.alist.hasitem(IDs.giftidtooffset(____itemDefinition.id) + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"])))
                        {
                            if (GameManager.System.Player.hunie < hunie)
                            {
                                text += "[[Not enough Hunie to order.]stop]";
                            }
                            else
                            {
                                text += $"[[Click to order ({hunie} Hunie).]go]";
                            }
                        }
                        else if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["disable_girl_gifts"]))
                        {
                            text += "[[Girl gifts are disabled.]stop]";
                        }
                        else
                        {
                            text += "[[Item can't be ordered yet.]stop]";
                        }
                        __instance.valueLabel.SetColor(Color.white);
                        __instance.valueLabel.SetText(StringUtils.FlattenColorBunches(text, text2));
                    }
                    break;
                case ItemType.UNIQUE_GIFT:
                
                    __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.specialGiftType.ToString()));
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("9E6B21"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("B3884F"));

                    hunie = -1;
                    if (ArchipelagoClient.gifthunniecost.Keys.Contains(____itemDefinition.id)) { hunie = ArchipelagoClient.gifthunniecost[____itemDefinition.id]; }
                    else { hunie = 99999; }

                    if (____collectionItem && GameManager.System.Player.endingSceneShown)
                    {
                        string text = "";
                        string text2 = ColorUtils.ColorToHex(__instance.valueLabel.label.color);
                        if (GameManager.System.GameState != GameState.SIM)
                        {
                            text += "[[Can't order while dating.]stop]";
                        }
                        else if (GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory))
                        {
                            text += "[[Your inventory is too full.]stop]";
                        }
                        else if (ArchipelagoClient.alist.hasitem(IDs.giftidtooffset(____itemDefinition.id) + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"])))
                        {
                            if (GameManager.System.Player.hunie < hunie)
                            {
                                text += "[[Not enough Hunie to order.]stop]";
                            }
                            else
                            {
                                text += $"[[Click to order ({hunie} Hunie).]go]";
                            }
                        }
                        else if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["disable_girl_gifts"]))
                        {
                            text += "[[Girl gifts are disabled.]stop]";
                        }
                        else
                        {
                            text += "[[Item can't be ordered yet.]stop]";
                        }
                        __instance.valueLabel.SetColor(Color.white);
                        __instance.valueLabel.SetText(StringUtils.FlattenColorBunches(text, text2));
                    }
                    break;
                case ItemType.DATE_GIFT:
                    __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.dateGiftType.ToString()));
                    __instance.valueLabel.SetText("Sentiment Needed: " + ____itemDefinition.sentimentCost.ToString());
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("337D91"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("48A1AA"));
                    __instance.valueLabel.SetColor(ColorUtils.HexToColor("3E89AF"));
                    if (____puzzleStatusItem && GameManager.System.Puzzle.Game != null && GameManager.System.Puzzle.Game.GetResourceValue(PuzzleGameResourceType.SENTIMENT) < ____itemDefinition.sentimentCost)
                    {
                        __instance.valueLabel.SetColor(ColorUtils.HexToColor("BC3939"));
                    }
                    break;
                case ItemType.ACCESSORY:
                    __instance.typeLabel.SetText(StringUtils.Titleize(____itemDefinition.type.ToString()) + "  |  " + StringUtils.Titleize(____itemDefinition.accessoryType.ToString()));
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("3A722A"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("598949"));
                    if (____traitsItem && GameManager.System.Player.GetTraitLevel(____itemDefinition.playerTraitType) < 6)
                    {
                        if (____itemDefinition.storeCost > GameManager.System.Player.hunie)
                        {
                            __instance.valueLabel.SetText("You can't afford to upgrade.");
                            __instance.valueLabel.SetColor(ColorUtils.HexToColor("BC3939"));
                        }
                        else
                        {
                            __instance.valueLabel.SetText("Click to purchase upgrade.");
                            __instance.valueLabel.SetColor(ColorUtils.HexToColor("3E89AF"));
                        }
                    }
                    else
                    {
                        global::UnityEngine.Object.Destroy(__instance.valueLabel.gameObj);
                        ____hasValue = false;
                    }
                    break;
                case ItemType.PANTIES:
                    __instance.typeLabel.SetText("Girls Underwear");
                    global::UnityEngine.Object.Destroy(__instance.valueLabel.gameObj);
                    ____hasValue = false;
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("803BA1"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("9364B3"));
                    break;
                case ItemType.PRESENT:
                    if (____itemDefinition.id != 300 && ____itemDefinition.id != 301)
                    {
                        __instance.typeLabel.SetText("Present From Sky Garden");
                        __instance.valueLabel.SetText("Click to open this present!");
                    }
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("A33434"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("B55E62"));
                    __instance.valueLabel.SetColor(ColorUtils.HexToColor("3E89AF"));
                    break;
                case ItemType.MISC:
                    __instance.typeLabel.SetText("Special Item");
                    global::UnityEngine.Object.Destroy(__instance.valueLabel.gameObj);
                    ____hasValue = false;
                    __instance.nameLabel.SetColor(ColorUtils.HexToColor("A33434"));
                    __instance.typeLabel.SetColor(ColorUtils.HexToColor("B55E62"));
                    break;
            }
            if (____storeItem)
            {
                string text = __instance.valueLabel.label.text + "\n";
                if (____itemDefinition.type == ItemType.UNIQUE_GIFT)
                {
                    text = string.Empty;
                }
                string text2 = ColorUtils.ColorToHex(__instance.valueLabel.label.color);
                if (GameManager.System.Player.IsInventoryFull(GameManager.System.Player.gifts) && GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory))
                {
                    text += "[[Your inventory is too full.]stop]";
                }
                else if (____itemDefinition.storeCost > GameManager.System.Player.money && ____itemDefinition.id != 300)
                {
                    text += "[[You can't afford this item.]stop]";
                }
                else if (____itemDefinition.storeCost > GameManager.System.Player.hunie && ____itemDefinition.id == 300)
                {
                    text += "[[You can't afford this item.]stop]";
                }
                else
                {
                    text += "[[Click to purchase this item.]go]";
                }
                __instance.valueLabel.SetColor(Color.white);
                __instance.valueLabel.SetText(StringUtils.FlattenColorBunches(text, text2));
            }
            if (____hasValue)
            {
                __instance.valueLabel.localY = ____valueLabelOrigY - (float)__instance.GetContentHeightPadding();
                if ((____storeItem && ____itemDefinition.type != ItemType.UNIQUE_GIFT) || (____collectionItem && GameManager.System.Player.endingSceneShown && ____itemDefinition.type == ItemType.GIFT))
                {
                    __instance.valueLabel.localY += 25f;
                }
            }
            return false;
        }


        ///// <summary>
        ///// NOP the vanilla instructions for the method above to overwrite
        ///// </summary>
        //[HarmonyPatch(typeof(ItemTooltipContent), "Refresh")]
        //[HarmonyILManipulator]
        //public static void tooltipreplace(ILContext ctx, MethodBase orig)
        //{
        //    for (int i = 10; i < ctx.Instrs.Count; i++)
        //    {
        //        if (ctx.Instrs[i].OpCode == OpCodes.Ldarg_0 &&
        //            ctx.Instrs[i - 1].OpCode == OpCodes.Br &&
        //            ctx.Instrs[i - 2].OpCode == OpCodes.Callvirt &&
        //            ctx.Instrs[i - 3].OpCode == OpCodes.Call &&
        //            ctx.Instrs[i - 4].OpCode == OpCodes.Ldstr &&
        //            ctx.Instrs[i - 4].Operand.ToString() == "508591")
        //        {
        //            for (int j = 0; j < (107+38); j++)
        //            {
        //                ctx.Instrs[i + j].OpCode = OpCodes.Nop;
        //            }
        //            break;
        //        }
        //    }
        //}
    }
}
