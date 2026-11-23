using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Girls
{
    [HarmonyPatch]
    public class Gifts
    {
        /// <summary>
        /// send an archipelago location for gifting a gift item to a girl
        /// </summary>
        [HarmonyPatch(typeof(GirlPlayerData), "AddItemToCollection")]
        [HarmonyPostfix]
        public static void cgiftloc(ItemDefinition item, GirlPlayerData __instance, ref bool __result)
        {
            if (__result)
            {
                HuniePopArchipelago.ArchipelagoClient.sendLoc($"{__instance.GetGirlDefinition().firstName.ToLower()}_gift_loc_start", IDs.giftidtooffset(item.id));
            }
        }



        [HarmonyPatch(typeof(GirlManager), "GiveItem")]
        [HarmonyPrefix]
        public static bool releaseallitems1(ItemDefinition item, GirlManager __instance, ref bool __result)
        {
            Girl ag = null;

            if (GameManager.System.Location.currentGirl != null && GameManager.System.Location.IsLocationSettled())
            {
                ag = GameManager.Stage.girl;
            }

            if (ag == null || (item.type != ItemType.FOOD && item.type != ItemType.DRINK && item.type != ItemType.GIFT && item.type != ItemType.UNIQUE_GIFT && item.type != ItemType.PANTIES && item.type != ItemType.MISC))
            {
                __result = false;
                return false;
            }

            if (item.type != ItemType.GIFT && item.type != ItemType.UNIQUE_GIFT && item.type != ItemType.PANTIES)
            {
                return true;
            }

            GirlPlayerData girlData = GameManager.System.Player.GetGirlData(ag.definition);
            DialogTriggerDefinition dialogTriggerDefinition = null;
            int num = 0;
            bool flag = true;
            switch (item.type)
            {

                case ItemType.UNIQUE_GIFT:
                case ItemType.GIFT:
                    dialogTriggerDefinition = GameManager.Stage.uiGirl.givenGiftDialogTrigger;
                    if (girlData.appetite == 0)
                    {
                        dialogTriggerDefinition = GameManager.Stage.uiGirl.isHungryDialogTrigger;
                        num = 0;
                        flag = false;
                        break;
                    }

                    num = 0;
                    flag = false;

                    for (int i = 0; i < 24; i++)
                    {
                        if (item.id == ArchipelagoClient.girldata[ag.definition.id].collection[i].id)
                        {
                            flag = true;
                            num = i / 6;
                            break;
                        }
                    }

                    if (!flag) num = 4;

                    switch (num)
                    {
                        case 0:
                        case 1:
                        case 2:

                            int num4 = Mathf.RoundToInt((float)(item.itemFunctionValue * 100) * (1f + (float)girlData.inebriation * 0.1f));
                            GameManager.System.Player.hunie += num4;
                            girlData.AddItemToCollection(item);
                            EnergyTrail energyTrail = new GameObject("EnergyTrail", new Type[] { typeof(EnergyTrail) }).GetComponent<EnergyTrail>();
                            energyTrail.Init(GameManager.System.Cursor.GetMousePosition(), GameManager.Stage.uiGirl.itemGiveGiftEnergyTrail, "+" + num4.ToString() + " Hunie", EnergyTrailFormat.END, null);

                            break;
                        case 3:

                            girlData.AddItemToUniqueGifts(item);
                            girlData.AddItemToCollection(item);
                            EnergyTrail energyTrail2 = new GameObject("EnergyTrail", new Type[] { typeof(EnergyTrail) }).GetComponent<EnergyTrail>();
                            energyTrail2.Init(GameManager.System.Cursor.GetMousePosition(), GameManager.Stage.uiGirl.itemGiveUniqueEnergyTrail, null, EnergyTrailFormat.END, null);

                            break;
                    }

                    break;

                case ItemType.PANTIES:
                    dialogTriggerDefinition = GameManager.Stage.uiGirl.kyuSpecialDialogTrigger;
                    if (ag.definition != GameManager.Stage.uiGirl.fairyGirlDef && !Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["kyu_enabled"]))
                    {
                        dialogTriggerDefinition = GameManager.Stage.uiGirl.givenGiftDialogTrigger;

                        if (ag.definition.pantiesItem.id == item.id)
                        {
                            num = 0;
                            flag = true;
                        }
                        else
                        {
                            num = 4;
                            flag = false;
                        }
                    }
                    else
                    {
                        num = item.id - 277;
                        if (!GameManager.System.Player.pantiesTurnedIn.Contains(item.id)) GameManager.System.Player.pantiesTurnedIn.Add(item.id);

                        if (GameManager.System.Player.pantiesTurnedIn.Count == 12)
                        {
                            num = 12;
                            GameManager.System.Player.alphaModeActive = true;
                            GameManager.System.Player.settingsDifficulty = SettingsDifficulty.MEDIUM;
                            GameManager.Stage.cellNotifications.Notify(CellNotificationType.MESSAGE, "Alpha Mode Activated!");
                            SteamUtils.UnlockAchievement("alpha", true);

                            if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["goal"])) 
                            {
                                HuniePopArchipelago.ArchipelagoClient.sendLoc("goal_loc_start", 1);
                                ArchipelagoClient.sendCompletion();
                            }

                        }

                        if (flag)
                        {
                            EnergyTrail energyTrail = new GameObject("EnergyTrail", new Type[] { typeof(EnergyTrail) }).GetComponent<EnergyTrail>();
                            energyTrail.Init(GameManager.System.Cursor.GetMousePosition(), GameManager.Stage.uiGirl.itemGiveGiftEnergyTrail, null, EnergyTrailFormat.END, null);

                            HuniePopArchipelago.ArchipelagoClient.sendLoc("kyu_panties_loc_start", IDs.pantiesidtooffset(item.id));
                        }
                    }
                    break;
            }
            __instance.TriggerDialog(dialogTriggerDefinition, num, false, -1);
            if (flag)
            {
                GameManager.System.Audio.Play(AudioCategory.SOUND, GameManager.Stage.uiGirl.itemGiveSuccessSound, false, 1f, true);
                GameManager.Stage.uiWindows.RefreshActiveWindow();
            }
            else
            {
                GameManager.System.Audio.Play(AudioCategory.SOUND, GameManager.Stage.uiGirl.itemGiveFailureSound, false, 1f, true);
            }
            GameManager.Stage.uiWindows.RefreshActiveWindow();
            __result = flag;
            return false;
        }


        /// <summary>
        /// allow for buying gits in the gift collection for girls if allowed by archipelago logic
        /// </summary>
        [HarmonyPatch(typeof(GirlProfileCellApp), "OnCollectionSlotPressed")]
        [HarmonyPrefix]
        public static bool collectionoverite(GirlProfileCollectionSlot collectionSlot, ref GirlProfileCellApp __instance)
        {
            if (collectionSlot.itemDefinition == null) { return true; }
            if (!(collectionSlot.itemDefinition.type == ItemType.GIFT || collectionSlot.itemDefinition.type == ItemType.UNIQUE_GIFT)) { return true; }

            //int hunie = Convert.ToInt32(Plugin.curse.connected.slot_data["hunie_gift_cost"]);
            int hunie = -1;
            if (ArchipelagoClient.gifthunniecost.Keys.Contains(collectionSlot.itemDefinition.id)) { hunie = ArchipelagoClient.gifthunniecost[collectionSlot.itemDefinition.id]; }
            else { hunie = 99999; }

            //only allow buying if not in date, have inventory slots, has enougth hunie and if the item has already been recieved by the client
            if ((collectionSlot.itemDefinition.type == ItemType.GIFT || collectionSlot.itemDefinition.type == ItemType.UNIQUE_GIFT) &&
                GameManager.System.GameState == GameState.SIM &&
                !GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory) &&
                GameManager.System.Player.hunie >= hunie &&
                ArchipelagoClient.alist.hasitem(IDs.giftidtooffset(collectionSlot.itemDefinition.id) + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"])))
            {
                GameManager.System.Player.AddItem(collectionSlot.itemDefinition, GameManager.System.Player.inventory, true, false);
                GameManager.System.Player.hunie -= hunie;
                GameManager.System.Audio.Play(AudioCategory.SOUND, __instance.orderItemSound, false, 1f, true);
                GameManager.Stage.cellNotifications.Notify(CellNotificationType.INVENTORY, GirlManager.FAIRY_PRESENT_NOTIFICATIONS[global::UnityEngine.Random.Range(0, GirlManager.FAIRY_PRESENT_NOTIFICATIONS.Length)]);
                __instance.Refresh();
                GameManager.Stage.tooltip.Refresh();
            }
            return false;
        }

        /// <summary>
        /// enable or disable the button ability for buying a gift through the girls collection
        /// </summary>
        [HarmonyPatch(typeof(GirlProfileCollectionSlot), "Refresh")]
        [HarmonyPrefix]
        public static bool collectionoverite2(GirlProfileCollectionSlot __instance)
        {

            if (__instance.itemDefinition == null) { return true; }
            if (!(__instance.itemDefinition.type == ItemType.GIFT || __instance.itemDefinition.type == ItemType.UNIQUE_GIFT)) { return true; }

            int hunie = -1;
            if (ArchipelagoClient.gifthunniecost.Keys.Contains(__instance.itemDefinition.id)) { hunie = ArchipelagoClient.gifthunniecost[__instance.itemDefinition.id]; }
            else { hunie = 99999; }

            //only enable button if not in date, have inventory slots, has enougth hunie and if the item has already been recieved by the client
            if (__instance.itemDefinition != null &&
                (__instance.itemDefinition.type == ItemType.GIFT || __instance.itemDefinition.type == ItemType.UNIQUE_GIFT) &&
                GameManager.System.GameState == GameState.SIM &&
                GameManager.System.Player.endingSceneShown &&
                !GameManager.System.Player.IsInventoryFull(GameManager.System.Player.inventory) &&
                GameManager.System.Player.hunie >= hunie &&
                ArchipelagoClient.alist.hasitem(IDs.giftidtooffset(__instance.itemDefinition.id) + Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"])))
            {
                __instance.button.Enable();
            }
            else
            {
                __instance.button.Disable();
            }
            return false;
        }
    }
}
