using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.UI;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class DepartLocation
    {

        /// <summary>
        /// process archipelago item list and check for game completion
        /// </summary>
        [HarmonyPatch(typeof(LocationManager), "DepartLocation")]
        [HarmonyPrefix]
        public static void archcheck(LocationManager __instance, ref LocationDefinition ____destinationLocation)
        {
            //dont do anything if going to a date location
            if (____destinationLocation.type == LocationType.DATE) { return; }

            PlayerManager player = GameManager.System.Player;

            //process archipelago list items
            processarch();

            //remove any shop location item that is in the inventory
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].itemDefinition == null) { continue; }
                if (player.inventory[i].itemDefinition.name.StartsWith("ARCH ITEM:"))
                {
                    player.inventory[i].itemDefinition = null;
                    player.inventory[i].presentDefinition = null;
                }
            }
            for (int j = 0; j < player.gifts.Length; j++)
            {
                if (player.gifts[j].itemDefinition == null) { continue; }
                if (player.gifts[j].itemDefinition.name.StartsWith("ARCH ITEM:"))
                {
                    player.gifts[j].itemDefinition = null;
                    player.gifts[j].presentDefinition = null;
                }
            }
            for (int j = 0; j < player.dateGifts.Length; j++)
            {
                if (player.dateGifts[j].itemDefinition == null) { continue; }
                if (player.dateGifts[j].itemDefinition.name.StartsWith("ARCH ITEM:"))
                {
                    player.dateGifts[j].itemDefinition = null;
                    player.dateGifts[j].presentDefinition = null;
                }
            }

            //make sure we are not comming back from a date and roll everything for a new day
            if (__instance.currentLocation.type != LocationType.DATE)
            {
                GameManager.System.Player.RollNewDay();
            }

            //check for if goal is completed
            if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["goal"]))
            {
                //if goal is give kyu all panties
                if (player.alphaModeActive)
                {
                    ArchipelagoClient.sendCompletion();
                }
            }
            else
            {
                //if goal is to have sex with all girls
                bool goalreached = true;

                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["tiffany_enabled"])) { if (!player.girls[0].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["aiko_enabled"])) { if (!player.girls[1].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["kyanna_enabled"])) { if (!player.girls[2].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["audrey_enabled"])) { if (!player.girls[3].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["lola_enabled"])) { if (!player.girls[4].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["nikki_enabled"])) { if (!player.girls[5].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["jessie_enabled"])) { if (!player.girls[6].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["beli_enabled"])) { if (!player.girls[7].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["kyu_enabled"])) { if (!player.girls[8].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["momo_enabled"])) { if (!player.girls[9].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["celeste_enabled"])) { if (!player.girls[10].gotPanties) { goalreached = false; } }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["venus_enabled"])) { if (!player.girls[11].gotPanties) { goalreached = false; } }

                if (goalreached) { ArchipelagoClient.sendCompletion(); }
            }


            HuniePopArchipelago.ArchipelagoClient.session.DataStorage[Scope.Slot, "savefile"] = JsonConvert.SerializeObject(GameManager.System.SaveFile);
            HuniePopArchipelago.ArchipelagoClient.session.DataStorage[Scope.Slot, "archdata"] = JsonConvert.SerializeObject(ArchipelagoClient.alist);


            ////save archdata
            //using (StreamWriter archfile = File.CreateText(Application.persistentDataPath + $"/archdata{Menu.slot}"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(archfile, ArchipelagoClient.alist);
            //}

        }


        public static int? pantiesloc;
        public static int? girlloc;
        public static int? giftloc;
        public static int? tokenloc;
        public static int? junkloc;

        /// <summary>
        /// process archipelago item list
        /// </summary>
        public static void processarch()
        {
            pantiesloc ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["panties_item_start"]);
            girlloc ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["girl_unlock_start"]);
            giftloc ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["gift_item_start"]);
            tokenloc ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["token_item_start"]);
            junkloc  ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]);

            PlayerManager player = GameManager.System.Player;
            ArchipelagoConsole.LogMessage("processing items");


            for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
            {
                ArchipelagoItem item = ArchipelagoClient.alist.list[i];

                //skip item if it has been processed
                if (item.processed) { continue; }

                //process Panties item
                if (item.Id > pantiesloc && item.Id <= girlloc)
                {
                    //skip item and flag it as processed if player has the panties in inventory or has already turned it in
                    int offset = (int)(item.Id - pantiesloc);
                    if (player.pantiesTurnedIn.Contains(IDs.pantiesoffsettoid(offset)))
                    {
                        ArchipelagoConsole.LogMessage(item.itemname + " already turned in skipping");
                        item.processed = true;
                        continue;
                    }
                    if (player.HasItem(GameManager.Data.Items.Get(IDs.pantiesoffsettoid(offset))))
                    {
                        ArchipelagoConsole.LogMessage(item.itemname + " already in inventory skipping");
                        item.processed = true;
                        continue;
                    }

                    //only process panties if player has inventory for it
                    if (!player.IsInventoryFull())
                    {
                        ArchipelagoConsole.LogMessage(item.itemname + " recieved");
                        player.AddItem(GameManager.Data.Items.Get(IDs.pantiesoffsettoid(offset)), player.inventory, false, false);
                        item.processed = true;
                    }
                }
                //process Girl Unlock
                else if (item.Id > girlloc && item.Id <= giftloc)
                {
                    //find girl and set metStatus as GirlMetStatus.MET
                    int offset = (int)(item.Id - girlloc);
                    for (int j = 0; j < player.girls.Count; j++)
                    {
                        if (player.girls[j].GetGirlDefinition().id == offset)
                        {
                            if (player.girls[j].metStatus == GirlMetStatus.MET)
                            {
                                ArchipelagoConsole.LogMessage(player.girls[j].GetGirlDefinition().firstName + " already unlocked skipping");
                                item.processed = true;
                                break;
                            }
                            ArchipelagoConsole.LogMessage(player.girls[j].GetGirlDefinition().firstName + " unlocked");
                            player.girls[j].metStatus = GirlMetStatus.MET;
                            item.processed = true;
                            break;
                        }
                    }
                }
                //if item id is between 42069024 and 42069097 process it as a Gift Item
                else if (item.Id > giftloc && item.Id <= tokenloc)
                {
                    //Make sure players inventory isnt full and give the relevent item
                    int offset = (int)(item.Id - giftloc);
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get(IDs.giftoffsettoid(offset)), player.inventory, false, false);
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(IDs.giftoffsettoid(offset)).name + " recieved and can now be bought in the shop");
                        item.processed = true;
                    }
                }
                //if item id is between 42069168 and 42069177 process it as a Trait Upgrade Item
                else if (item.Id > tokenloc && item.Id <= junkloc)
                {
                    ArchipelagoConsole.LogMessage($"{item.itemname} recieved");
                    int offset = (int)(item.Id - tokenloc);
                    player.UpgradeTraitLevel((PlayerTraitType)(offset-1));
                    item.processed = true;
                }
                //process Filler Item
                else
                {
                    //make sure that the players inventory isnt full then give the relevent item
                    int offset = (int)(item.Id - junkloc);
                    if (IDs.filleroffsettoid(offset) == -1)
                    {
                        item.processed = true;
                        continue;
                    }
                    if (!player.IsInventoryFull())
                    {
                        player.AddItem(GameManager.Data.Items.Get(IDs.filleroffsettoid(offset)), player.inventory, false, false);
                        ArchipelagoConsole.LogMessage(GameManager.Data.Items.Get(IDs.filleroffsettoid(offset)).name + " recieved");
                        item.processed = true;
                    }
                }
            }
        }
    }
}
