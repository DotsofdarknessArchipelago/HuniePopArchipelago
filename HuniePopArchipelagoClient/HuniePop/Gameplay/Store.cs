using Boomlagoon.JSON;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Store
    {
        /// <summary>
        /// when buying an item in the shopif its an archipelago location sent it otherwise if its an item make sure it dosent get put back in the shop
        /// </summary>
        [HarmonyPatch(typeof(StoreCellApp), "OnStoreItemSlotPressed")]
        [HarmonyPrefix]
        public static bool storepurchase(StoreItemSlot storeItemSlot, StoreCellApp __instance, ref int ____currentStoreTab)
        {
            if (storeItemSlot.itemDefinition.id == 300)
            {
                StoreItemPlayerData storeItemPlayerData = StoreCellApp.STORE_TABS[____currentStoreTab] switch
                {
                    "cell_app_shop_header_food" => GameManager.System.Player.storeFood[storeItemSlot.itemSlotIndex],
                    "cell_app_shop_header_unique" => GameManager.System.Player.storeUnique[storeItemSlot.itemSlotIndex],
                    "cell_app_shop_header_drinks" => GameManager.System.Player.storeDrinks[storeItemSlot.itemSlotIndex],
                    _ => GameManager.System.Player.storeGifts[storeItemSlot.itemSlotIndex],
                };

                storeItemPlayerData.soldOut = true;
                GameManager.System.Player.money += 1000;
                GameManager.System.Player.hunie -= (int)(1000 * Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hunie_to_money"]));
                GameManager.System.Audio.Play(AudioCategory.SOUND, __instance.purchaseItemSound, false, 1f, true);

                __instance.Refresh();
                GameManager.Stage.uiGirl.RefreshItemSlots();
                return false;
            }
            else if (storeItemSlot.itemDefinition.id == 301)
            {
                StoreItemPlayerData storeItemPlayerData = StoreCellApp.STORE_TABS[____currentStoreTab] switch
                {
                    "cell_app_shop_header_food" => GameManager.System.Player.storeFood[storeItemSlot.itemSlotIndex],
                    "cell_app_shop_header_unique" => GameManager.System.Player.storeUnique[storeItemSlot.itemSlotIndex],
                    "cell_app_shop_header_drinks" => GameManager.System.Player.storeDrinks[storeItemSlot.itemSlotIndex],
                    _ => GameManager.System.Player.storeGifts[storeItemSlot.itemSlotIndex],
                };
                storeItemPlayerData.soldOut = true;
                GameManager.System.Player.money -= (int)(1000 / Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hunie_to_money"]));
                GameManager.System.Player.hunie += 1000;
                GameManager.System.Audio.Play(AudioCategory.SOUND, __instance.purchaseItemSound, false, 1f, true);
                __instance.Refresh();
                GameManager.Stage.uiGirl.RefreshItemSlots();
                return false;
            }


            if (storeItemSlot.itemDefinition.id > Convert.ToInt32(ArchipelagoClient.ServerData.slotData["shop_loc_start"]))
            {
                ArchipelagoConsole.LogMessage($"PURCHASED ITEM:{storeItemSlot.itemDefinition.name}");
                ArchipelagoConsole.LogMessage($"SENDING LOCATION {storeItemSlot.itemDefinition.id}");
                HuniePopArchipelago.ArchipelagoClient.sendLoc(storeItemSlot.itemDefinition.id);
                return true;
            }
            return true;
        }

        public static tk2dSpriteCollectionData uicellphone = null;
        public static tk2dSpriteCollectionData itemicons = null;

        [HarmonyPatch(typeof(StoreItemSlot), "PopulateItem")]
        [HarmonyPrefix]
        public static bool PopulateItem(ItemDefinition item, StoreItemSlot __instance)
        {
            __instance.itemDefinition = item;
            if (__instance.itemDefinition != null)
            {
                if (__instance.itemDefinition.id == 300 || __instance.itemDefinition.id == 301)
                {
                    if (uicellphone == null)
                    {
                        __instance.itemIcon.sprite.SetSprite("item_blank");
                        return false;
                    }
                    if(itemicons == null)
                    {
                        itemicons = __instance.itemIcon.sprite.Collection;
                    }

                    //HuniePopArchipelago.BepinLogger.LogMessage($"Setting sprite for item ID {__instance.itemDefinition.id}: {__instance.itemDefinition.iconName}");
                    __instance.itemIcon.sprite.SetSprite(uicellphone, __instance.itemDefinition.iconName);
                    __instance.itemIcon.sprite.scale = new Vector3(2, 2, 2);
                }
                else
                {
                    //HuniePopArchipelago.BepinLogger.LogMessage($"Setting sprite for item {__instance.itemDefinition.name}: {__instance.itemDefinition.iconName}");
                    if (__instance.itemIcon.sprite.Collection.spriteCollectionName != "ItemIconsSpriteCollection")
                    {
                        __instance.itemIcon.sprite.SetSprite(itemicons,__instance.itemDefinition.iconName);
                        __instance.itemIcon.sprite.scale = new Vector3(1, 1, 1);
                    }
                    else
                    {
                        __instance.itemIcon.sprite.SetSprite(__instance.itemDefinition.iconName);
                    }

                }
            }
            else
            {
                __instance.itemIcon.sprite.SetSprite("item_blank");
            }
            return false;
        }



        [HarmonyPatch(typeof(StoreItem), "Init")]
        [HarmonyPostfix]
        public static void test(StoreItem __instance)
        {
            if (uicellphone == null)
            {
                uicellphone = __instance.moneyIcon.sprite.Collection;
            }
            //ArchipelagoConsole.LogMessage($"Money sprite name: {__instance.moneyIcon.sprite.GetCurrentSpriteDef().name}");
            //ArchipelagoConsole.LogMessage($"Money sprite name: {__instance.moneyIcon.sprite.Collection.spriteCollectionGUID}");
            //ArchipelagoConsole.LogMessage($"Money sprite name: {__instance.moneyIcon.sprite.Collection.loadable}");

            //ArchipelagoConsole.LogMessage($"hunie sprite name: {__instance.hunieIcon.sprite.GetCurrentSpriteDef().name}");
            //ArchipelagoConsole.LogMessage($"hunie collection name: {__instance.hunieIcon.sprite.Collection.spriteCollectionName}");


        }

        /// <summary>
        /// populate the store with stuff we want
        /// </summary>
        [HarmonyPatch(typeof(PlayerManager), "RollNewStoreList")]
        [HarmonyPrefix]
        public static bool storeoverite(StoreItemPlayerData[] storeList, ItemType itemType, PlayerManager __instance)
        {
            //populate the gift tab of the shop
            if (itemType == ItemType.GIFT)
            {
                List<ItemDefinition> list = GameManager.Data.Items.GetAllOfType(ItemType.DATE_GIFT, false);

                ListUtils.Shuffle<ItemDefinition>(list);

                ItemDefinition hunnietomoney = ScriptableObject.CreateInstance<ItemDefinition>();
                hunnietomoney.type = ItemType.PRESENT;
                hunnietomoney.hidden = false;
                hunnietomoney.iconName = "cell_app_shop_money_icon";

                hunnietomoney.name = "1000 Money";
                hunnietomoney.id = 300;
                hunnietomoney.description = $"Convert {1000 * Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hunie_to_money"])} Hunie to Money";

                ItemDefinition moneytohunie = ScriptableObject.CreateInstance<ItemDefinition>();
                moneytohunie.type = ItemType.PRESENT;
                moneytohunie.hidden = false;
                moneytohunie.iconName = "cell_app_upgrades_hunie_icon";
                
                moneytohunie.name = "1000 Hunie";
                moneytohunie.id = 301;
                moneytohunie.description = $"Convert {(int)(1000 / Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hunie_to_money"]))} Money to Hunie";


                if (list.Count > 12)
                {
                    list.RemoveRange(12, list.Count - 12);
                }


                for (int l = 0; l < 12; l++)
                {
                    if (Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hunie_to_money"]) > 0)
                    {
                        if (l == 0)
                        {
                            storeList[l].itemDefinition = hunnietomoney;
                            storeList[l].soldOut = false;
                            continue;
                        }
                        else if (l == 1)
                        {
                            storeList[l].itemDefinition = moneytohunie;
                            storeList[l].soldOut = false;
                            continue;
                        }
                    }
                    if (list.Count > l)
                    {
                        storeList[l].itemDefinition = list[l];
                        storeList[l].soldOut = false;
                    }
                    else
                    {
                        storeList[l].itemDefinition = null;
                        storeList[l].soldOut = true;
                    }
                }

                return false;

            }
            //populate the Unique Gift tab for the shop
            else if (itemType == ItemType.UNIQUE_GIFT)
            {

                List<ItemDefinition> arch = new List<ItemDefinition>();

                //get all archipelago shop locations to put in the store
                double shopslots = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["number_of_shop_items"]);
                double shoppass = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["Shop Pass"]);
                double shoppassid = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]) + 88;

                HuniePopArchipelago.BepinLogger.LogMessage($"Shop slots: {shopslots}, Shop pass: {shoppass}, Shop pass ID: {shoppassid}, Number of Shop Passes Obtained: {ArchipelagoClient.alist.itemcount((long)shoppassid)}");

                int[] pre = [293, 294, 295, 296, 297, 298];
                for (int j = 0; j < shopslots; j++)
                {

                    if (shoppass > 0 && ArchipelagoClient.alist.itemcount((long)shoppassid) < Math.Ceiling(((j + 1) * (shoppass / shopslots)))) { continue; }

                    if (ArchipelagoClient.ServerData.CheckedLocations != null)
                    {
                        if (ArchipelagoClient.ServerData.CheckedLocations.Contains(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["shop_loc_start"]) + j + 1)) { continue; }
                    }
                    ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
                    item.type = ItemType.PRESENT;
                    item.hidden = false;
                    item.iconName = GameManager.Data.Items.Get(pre[UnityEngine.Random.Range(0, pre.Count() - 1)]).iconName;

                    item.name = "ARCH ITEM:" + (j + 1).ToString();
                    item.id = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["shop_loc_start"]) + j + 1;
                    item.description = "LOCATION CHECK FOR ARCHIPELAGO WILL BE REMOVED FROM INVENTORY WHEN MOVING LOCATIONS";
                    arch.Add(item);
                }

                //get all gift items that have been recieved by the client
                List<ItemDefinition> gifts = new List<ItemDefinition>();
                List<ItemDefinition> prigifts = new List<ItemDefinition>();
                foreach (int id in IDs.giftids)
                {
                    if (ArchipelagoClient.alist.hasitem(IDs.giftidtooffset(id) + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"])))
                    {
                        bool pri = false;
                        foreach (var g in __instance.girls)
                        {
                            if (g.metStatus == GirlMetStatus.MET)
                            {
                                foreach (var c in g.GetGirlDefinition().collection)
                                {
                                    if (c.id == id)
                                    {
                                        if (!g.IsItemInCollection(GameManager.Data.Items.Get(id)))
                                        {
                                            prigifts.Add(GameManager.Data.Items.Get(id));
                                            pri = true;
                                        }
                                        break;
                                    }
                                }
                                if (pri) break;
                            }
                        }
                        if (!pri) prigifts.Add(GameManager.Data.Items.Get(id));
                    }
                }

                ListUtils.Shuffle<ItemDefinition>(arch);
                ListUtils.Shuffle<ItemDefinition>(gifts);
                ListUtils.Shuffle<ItemDefinition>(prigifts);
                List<ItemDefinition> storelist = new List<ItemDefinition>();

                if ((gifts.Count + prigifts.Count + arch.Count) <= 12)
                {
                    storelist.AddRange(gifts);
                    storelist.AddRange(prigifts);
                    storelist.AddRange(arch);
                }
                else
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (prigifts.Count > 0 && (i < 6 || (arch.Count == 0 && gifts.Count == 0)))
                        {
                            storelist.Add(prigifts.Pop());
                        }
                        else if (gifts.Count > 0 && (i < 6 || (arch.Count == 0)))
                        {
                            storelist.Add(gifts.Pop());
                        }
                        else if (arch.Count > 0)
                        {
                            storelist.Add(arch.Pop());
                        }
                    }
                }

                ListUtils.Shuffle<ItemDefinition>(storelist);

                for (int l = 0; l < 12; l++)
                {
                    if (storelist.Count > l)
                    {
                        storeList[l].itemDefinition = storelist[l];
                        storeList[l].soldOut = false;
                    }
                    else
                    {
                        storeList[l].itemDefinition = null;
                        storeList[l].soldOut = true;
                    }
                }

                return false;
            }
            return true;
        }

        /// <summary>
        /// set store cost for items what we want them to be
        /// </summary>
        [HarmonyPatch(typeof(ItemDefinition), "storeCost", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool itemcost(ref int __result, ItemDefinition __instance)
        {
            if (__instance.type == ItemType.PRESENT)
            {
                if (__instance.id == 300)
                {
                    __result = 1000* Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hunie_to_money"]);
                    return false;
                }
                else if (__instance.id == 301)
                {
                    __result = 1000/ Convert.ToInt32(ArchipelagoClient.ServerData.slotData["hunie_to_money"]);
                    return false;
                }
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_item_cost"]);
                int i = __instance.id - Convert.ToInt32(ArchipelagoClient.ServerData.slotData["shop_loc_start"]);
                if (ArchipelagoClient.archshopcost.Keys.Contains($"shop{i}")) { __result = ArchipelagoClient.archshopcost[$"shop{i}"]; }
                else { __result = 50000; }
                return false;
            }
            else if (__instance.type == ItemType.GIFT)
            {
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_gift_cost"]);
                if (ArchipelagoClient.giftshopcost.Keys.Contains(__instance.id)) { __result = ArchipelagoClient.giftshopcost[__instance.id]; }
                else { __result = 50000; }
                return false;
            }
            else if (__instance.type == ItemType.UNIQUE_GIFT)
            {
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_gift_cost"]);
                if (ArchipelagoClient.giftshopcost.Keys.Contains(__instance.id)) { __result = ArchipelagoClient.giftshopcost[__instance.id]; }
                else { __result = 50000; }
                return false;
            }
            else if (__instance.type == ItemType.DATE_GIFT)
            {
                //__result = Convert.ToInt32(Plugin.curse.connected.slot_data["shop_date_gift_cost"]);
                if (ArchipelagoClient.dateshopcost.Keys.Contains(__instance.id)) { __result = ArchipelagoClient.dateshopcost[__instance.id]; }
                else { __result = 50000; }
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(StoreItem), "PopulateItem")]
        [HarmonyPostfix]
        public static void itemcost(StoreItemPlayerData storeItemPlayerData, StoreItem __instance)
        {
            if (__instance.itemSlot.itemDefinition == null) return;
            if (storeItemPlayerData.soldOut) return;


            if (__instance.itemSlot.itemDefinition.id == 300)
            {
                __instance.moneyIcon.sprite.SetSprite("cell_app_upgrades_hunie_icon");
                if (storeItemPlayerData.itemDefinition.storeCost <= GameManager.System.Player.hunie)
                {
                    __instance.itemSlot.button.Enable();
                }                 
            }
            else
            {
                __instance.moneyIcon.sprite.SetSprite("cell_app_shop_money_icon");
            }


            bool fav = false;
            bool curfav = false;
            bool mg = false;
            if ((storeItemPlayerData.itemDefinition.type == ItemType.GIFT || storeItemPlayerData.itemDefinition.type == ItemType.UNIQUE_GIFT))
            {
                foreach (var g in GameManager.System.Player.girls)
                {
                    if (GameManager.System.Location.currentGirl.id == g.GetGirlDefinition().id) { mg = true; }
                    else if (fav) { continue; }

                    if (g.metStatus == GirlMetStatus.MET)
                    {
                        foreach (var c in g.GetGirlDefinition().collection)
                        {
                            if (c.id == storeItemPlayerData.itemDefinition.id)
                            {
                                if (!g.IsItemInCollection(storeItemPlayerData.itemDefinition))
                                {
                                    if (GameManager.System.Location.currentGirl.id == g.GetGirlDefinition().id) curfav = true;
                                    fav = true;
                                }
                                break;
                            }
                        }
                        if (curfav || (mg && fav)) break;
                    }
                }
            }

            if (curfav)
            {
                __instance.starIcon.SetAlpha(1f, 0f);
                __instance.starIcon.sprite.SetSprite("cell_app_shop_heart");
            }
            else if (fav)
            {
                __instance.starIcon.SetAlpha(1f, 0f);
                __instance.starIcon.sprite.SetSprite("cell_app_shop_star");
            }
            else
            {
                __instance.starIcon.SetAlpha(0f, 0f);
            }
        }


        [HarmonyPatch(typeof(ActionMenuButton), "Refresh")]
        [HarmonyPrefix]
        public static bool stroelabel(ActionMenuButton __instance)
        {
            if (__instance.definition.subtitleType == ActionMenuSubtitleType.SHOP_FOR_HER)
            {
                int num3 = 0;
                for (int k = 0; k < GameManager.System.Player.storeUnique.Length; k++)
                {
                    if (!GameManager.System.Player.storeUnique[k].soldOut)
                    {
                        num3++;
                    }
                }
                if (num3 > 0)
                {
                    __instance.actionSubLabel.SetText(num3.ToString() + " in Stock");
                }
                else
                {
                    __instance.actionSubLabel.SetText("Out of Stock");
                }
                return false;
            }
            return true;
        }
    }
}
