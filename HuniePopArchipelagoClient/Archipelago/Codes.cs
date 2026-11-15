using Archipelago.MultiClient.Net.Packets;
using HuniePopArchiepelagoClient;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HuniePopArchipelagoClient.Archipelago
{
    public static class Codes
    {
        public static void ProcessCode(string msg)
        {
            ArchipelagoConsole.LogMessage($"CODE ENTERED: {msg}");
            if (msg == "$archdata")
            {
                ArchipelagoConsole.LogMessage("-------DEBUG DATA-------");
                ArchipelagoConsole.LogMessage("List size: " + ArchipelagoClient.alist.list.Count.ToString());
                ArchipelagoConsole.LogMessage("------------------------");
                for (int i = 0; i < ArchipelagoClient.alist.list.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("#" + i.ToString());
                    ArchipelagoConsole.LogMessage("ID:" + ArchipelagoClient.alist.list[i].Id.ToString());
                    ArchipelagoConsole.LogMessage("NAME:" + ArchipelagoClient.alist.list[i].itemname);
                    ArchipelagoConsole.LogMessage("LOCID:" + ArchipelagoClient.alist.list[i].LocationId.ToString());
                    ArchipelagoConsole.LogMessage("LOCNAME:" + ArchipelagoClient.alist.list[i].locationname);
                    ArchipelagoConsole.LogMessage("PROCESSED:" + ArchipelagoClient.alist.list[i].processed.ToString());
                    ArchipelagoConsole.LogMessage("PUTINSHOP:" + ArchipelagoClient.alist.list[i].putinshop.ToString());
                    ArchipelagoConsole.LogMessage("------------------------");
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$resetitems")
            {
                ArchipelagoClient.alist = new ArchipelageItemList();
                HuniePopArchipelago.ArchipelagoClient.session.Socket.SendPacket(new SyncPacket());
                ArchipelagoConsole.LogMessage("Items reset please move locations to re-process them");
            }
            if (msg == "$sync")
            {
                HuniePopArchipelago.ArchipelagoClient.session.Socket.SendPacket(new SyncPacket());
            }
            if (msg == "$resync")
            {
                HuniePopArchipelago.ArchipelagoClient.session.Socket.SendPacket(new SyncPacket());
                HuniePopArchipelago.ArchipelagoClient.resync();
            }
            if (msg == "$deldata")
            {
                File.Delete(Application.persistentDataPath + "/archdata");
            }
            if (msg == "$goal")
            {
                SaveFile saveFile = SaveUtils.GetSaveFile(0);
                PlayerManager player = GameManager.System.Player;
                if (saveFile == null || !saveFile.started || GameManager.System.GameState == GameState.LOADING || GameManager.System.GameState == GameState.TITLE) { return; }

                int offset = 0;
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["tiffany_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["aiko_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["kyanna_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["audrey_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["lola_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["nikki_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["jessie_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["beli_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["kyu_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["momo_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["celeste_enabled"])) { offset++; }
                if (!Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["venus_enabled"])) { offset++; }

                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["goal"]))
                {
                    int p = 0;
                    string s = "";
                    ArchipelagoConsole.LogMessage("Your goal is to give kyu panties");

                    for (int i = 1; i < 13; i++)
                    {
                        if (player.pantiesTurnedIn.Count() == 0 || !player.pantiesTurnedIn.Contains(i + 276))
                        {
                            if (s != "") { s += ", "; }
                            s += GameManager.Data.Girls.Get(i).firstName;
                        }
                        else
                        {
                            p++;
                        }
                    }

                    ArchipelagoConsole.LogMessage($"Number of panties turned in: {p - offset}");
                    ArchipelagoConsole.LogMessage($"Girls panties left to give to kyu: {s}");

                }
                else
                {
                    int p = 0;
                    string s = "";
                    ArchipelagoConsole.LogMessage("Your goal is to have sex with all the girls");

                    foreach (GirlPlayerData girl in player.girls)
                    {
                        if (!girl.gotPanties)
                        {
                            if (s != "") { s += ", "; }
                            s += girl.GetGirlDefinition().firstName;
                        }
                        else
                        {
                            p++;
                        }
                    }

                    ArchipelagoConsole.LogMessage($"Number of girls that you have had sex with: {p - offset}");
                    ArchipelagoConsole.LogMessage($"Girls you still need to have sex with: {s}");

                    if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["progressive_dates"]))
                    {
                        ArchipelagoConsole.LogMessage($"Date Passes Obtained: {ArchipelagoClient.alist.itemcount(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]) + 86)} of 5");
                    }
                }
            }
            if (msg == "$shop")
            {
                ArchipelagoConsole.LogMessage($"You have a total of {ArchipelagoClient.ServerData.slotData["shop_slots"]} archipelago shop slots");
                if (Convert.ToInt32(ArchipelagoClient.ServerData.slotData["Shop Pass"]) > 0)
                {
                    ArchipelagoConsole.LogMessage("Shop Passes are ENABLED");
                    ArchipelagoConsole.LogMessage($"You have {ArchipelagoClient.alist.itemcount(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]) + 88)}/{Convert.ToInt32(ArchipelagoClient.ServerData.slotData["Shop Pass"])} shop passes");
                }
                if (Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["progressive_dates"]))
                {
                    ArchipelagoConsole.LogMessage("Progressive Dates are ENABLED");
                    ArchipelagoConsole.LogMessage($"You have {ArchipelagoClient.alist.itemcount(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]) + 86)}/5 date passes");
                    if (ArchipelagoClient.alist.hasitemcount((long)(Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]) + 86), 4))
                    {
                        ArchipelagoConsole.LogMessage($"Archipelago shop items are currently in LOGIC");
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage($"Archipelago shop items are currently NOT in LOGIC");
                    }
                }
            }
            if (msg == "$toggledeathlink")
            {
                ArchipelagoConsole.LogMessage($"TOGGLING DEATH LINK FROM {DeathLinkHandler.deathLinkEnabled} -> {!DeathLinkHandler.deathLinkEnabled}");
                HuniePopArchipelago.ArchipelagoClient.DeathLinkHandler.ToggleDeathLink();
            }
            if (msg == "$test")
            {
                ArchipelagoConsole.LogMessage($"Your gamestate is {GameManager.System.GameState}");
            }


            //pulling game data
            if (msg == "$girls")
            {
                List<GirlDefinition> l = GameManager.Data.Girls.GetAll();
                ArchipelagoConsole.LogMessage("-------GIRL DATA-------");
                foreach (GirlDefinition g in l)
                {
                    ArchipelagoConsole.LogMessage("GIRL ID:" + g.id + " | GIRL NAME:" + g.name);
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$items")
            {
                List<ItemDefinition> list = GameManager.Data.Items.GetAllOfType(ItemType.FOOD, false);
                List<ItemDefinition> list2 = GameManager.Data.Items.GetAllOfType(ItemType.DRINK, false);
                List<ItemDefinition> list3 = GameManager.Data.Items.GetAllOfType(ItemType.GIFT, false);
                List<ItemDefinition> list4 = GameManager.Data.Items.GetAllOfType(ItemType.UNIQUE_GIFT, false);
                List<ItemDefinition> list5 = GameManager.Data.Items.GetAllOfType(ItemType.DATE_GIFT, false);
                List<ItemDefinition> list6 = GameManager.Data.Items.GetAllOfType(ItemType.ACCESSORY, false);
                List<ItemDefinition> list7 = GameManager.Data.Items.GetAllOfType(ItemType.PANTIES, false);
                List<ItemDefinition> list8 = GameManager.Data.Items.GetAllOfType(ItemType.PRESENT, false);
                List<ItemDefinition> list9 = GameManager.Data.Items.GetAllOfType(ItemType.MISC, false);
                ArchipelagoConsole.LogMessage("-------FOOD DATA-------");
                foreach (ItemDefinition i in list)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------DRINK DATA-------");
                foreach (ItemDefinition i in list2)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------GIFT DATA-------");
                foreach (ItemDefinition i in list3)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------UNIQUE_GIFT DATA-------");
                foreach (ItemDefinition i in list4)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------DATE_GIFT DATA-------");
                foreach (ItemDefinition i in list5)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------ACCESSORY DATA-------");
                foreach (ItemDefinition i in list6)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------PANTIES DATA-------");
                foreach (ItemDefinition i in list7)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------PRESENT DATA-------");
                foreach (ItemDefinition i in list8)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------MISC DATA-------");
                foreach (ItemDefinition i in list9)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$location")
            {
                List<LocationDefinition> nor = GameManager.Data.Locations.GetLocationsByType(LocationType.NORMAL);
                List<LocationDefinition> date = GameManager.Data.Locations.GetLocationsByType(LocationType.DATE);

                ArchipelagoConsole.LogMessage("-------LOCATION DATA-------");
                ArchipelagoConsole.LogMessage("-------NORMAL LOCATION DATA-------");
                foreach (LocationDefinition l1 in nor)
                {
                    ArchipelagoConsole.LogMessage($"LOC ID: {l1.id} | NAME: {l1.name} | {l1.fullName}");
                }
                ArchipelagoConsole.LogMessage("-------END NORMAL LOCATION DATA-------");
                ArchipelagoConsole.LogMessage("-------DATE LOCATION DATA-------");
                foreach (LocationDefinition l2 in date)
                {
                    ArchipelagoConsole.LogMessage($"LOC ID: {l2.id} | NAME: {l2.name} | {l2.fullName}");
                }
                ArchipelagoConsole.LogMessage("-------END DATE LOCATION DATA-------");
            }
            if (msg == "$msg")
            {
                int i = 0;
                bool c = true;
                MessageDefinition data = GameManager.Data.Messages.Get(i);
                ArchipelagoConsole.LogMessage("-------MESSAGE DATA-------");
                while (c)
                {
                    data = GameManager.Data.Messages.Get(i);
                    if (data != null)
                    {
                        ArchipelagoConsole.LogMessage($"MSG ID: {data.id} | sender: {data.sender.name} | text: {data.messageText}");
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage($"MSG ID: {i} | NULL DATA");
                        if (i > 43) { c = false; }
                    }
                    i++;
                }
                ArchipelagoConsole.LogMessage("-------END MESSAGE DATA-------");
            }
        }

    }
}
