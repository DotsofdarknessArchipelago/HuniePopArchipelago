using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using HuniePopArchiepelagoClient.HuniePop.UI;
using HuniePopArchiepelagoClient.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using UnityEngine;

namespace HuniePopArchiepelagoClient.Archipelago
{
    public class ArchipelagoClient
    {
        public const string APVersion = "0.6.0";
        private const string Game = "Hunie Pop";

        public static bool Authenticated;
        public static bool gamestarted = false;
        public static bool attemptingConnection;

        public static string connectedplayerfile = null;

        public static ArchipelagoData ServerData = new();
        public DeathLinkHandler DeathLinkHandler;
        public ArchipelagoSession session;


        public NetworkVersion worldver;
        public static Dictionary<int, GirlDefinition> girldata;
        public static Dictionary<int, int> giftshopcost;
        public static Dictionary<int, int> gifthunniecost;
        public static Dictionary<string, int> archshopcost;
        public static Dictionary<int, int> dateshopcost;

        public static ArchipelageItemList alist = new ArchipelageItemList();


        /// <summary>
        /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            if (Authenticated || attemptingConnection) return;

            attemptingConnection = true;

            ServerData.Uri = Menu.playeruri;
            ServerData.SlotName = Menu.playername;
            ServerData.Password = Menu.playerpass;

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
                SetupSession();
            }
            catch (Exception e)
            {
                HuniePopArchipelago.BepinLogger.LogError(e);
            }

            TryConnect();
        }

        /// <summary>
        /// add handlers for Archipelago events
        /// </summary>
        private void SetupSession()
        {
            session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.ErrorReceived += OnSessionErrorReceived;
            session.Socket.SocketClosed += OnSessionSocketClosed;
        }

        /// <summary>
        /// attempt to connect to the server with our connection info
        /// </summary>
        private void TryConnect()
        {
            try
            {
                // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
                ThreadPool.QueueUserWorkItem(
                    _ => HandleConnectResult(
                        session.TryConnectAndLogin(
                            Game,
                            ServerData.SlotName,
                            ItemsHandlingFlags.AllItems,
                            new Version(APVersion),
                            password: ServerData.Password,
                            requestSlotData: ServerData.NeedSlotData
                        )));
            }
            catch (Exception e)
            {
                HuniePopArchipelago.BepinLogger.LogError(e);
                HandleConnectResult(new LoginFailure(e.ToString()));
                attemptingConnection = false;
            }
        }

        /// <summary>
        /// handle the connection result and do things
        /// </summary>
        /// <param name="result"></param>
        private void HandleConnectResult(LoginResult result)
        {
            string outText;
            if (result.Successful)
            {
                var success = (LoginSuccessful)result;

                ServerData.SetupSession(success.SlotData, session.RoomState.Seed);

                List<int> testworldver = JsonConvert.DeserializeObject<List<int>>(ServerData.slotData["world_version"].ToString());
                worldver = new NetworkVersion(testworldver[0], testworldver[1], testworldver[2]);
                if (worldver.Major != HuniePopArchipelago.compatworldmajor || worldver.Minor != HuniePopArchipelago.compatworldminor || worldver.Build != HuniePopArchipelago.compatworldbuild)
                {
                    ArchipelagoConsole.LogMessage("WARNING EXPECTED APWORLD VERSION MISSMATCH");
                    ArchipelagoConsole.LogImportant($"EXPECTED V{HuniePopArchipelago.compatworldmajor}.{HuniePopArchipelago.compatworldminor}.{HuniePopArchipelago.compatworldbuild} GOT V{worldver.Major}.{worldver.Minor}.{worldver.Build}");
                }
                HuniePopArchipelago.BepinLogger.LogMessage($"APWORLD V{worldver.Major}.{worldver.Minor}.{worldver.Build}");

                setupgirldata();

                giftshopcost = JsonConvert.DeserializeObject<Dictionary<int, int>>(ServerData.slotData["shop_gift_cost"].ToString());
                gifthunniecost = JsonConvert.DeserializeObject<Dictionary<int, int>>(ServerData.slotData["hunie_gift_cost"].ToString());
                archshopcost = JsonConvert.DeserializeObject<Dictionary<string, int>>(ServerData.slotData["shop_item_cost"].ToString());
                dateshopcost = JsonConvert.DeserializeObject<Dictionary<int, int>>(ServerData.slotData["shop_date_gift_cost"].ToString());


                DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
                if (Convert.ToInt32(ServerData.slotData["deathlink"].ToString()) != 0)
                {
                    HuniePopArchipelago.BepinLogger.LogMessage($"DEATHLINK ENABLED ({Convert.ToInt32(ServerData.slotData["deathlink"].ToString())})");
                    DeathLinkHandler.deathLinkOptionVal = Convert.ToInt32(ServerData.slotData["deathlink"].ToString());
                    DeathLinkHandler.EnableDeathLink();
                }
                else
                {
                    HuniePopArchipelago.BepinLogger.LogMessage("DEATHLINK DISABLED DOING NOTHING");
                }

                gamestarted = session.DataStorage[Scope.Slot, "slotsetup"];

                string alists = session.DataStorage[Scope.Slot, "archdata"];
                if (alists.IsNullOrWhiteSpace())
                {
                    ArchipelagoConsole.LogMessage("Error loading archdata from server creatine new archlist");
                    alist.seed = session.RoomState.Seed;
                    alist.host = ServerData.SlotName;
                    alist.user = ServerData.Uri;
                    alist.pass = ServerData.Password;
                }
                else
                {
                    ArchipelageItemList alist2 = JsonConvert.DeserializeObject<ArchipelageItemList>(alists);
                    ArchipelagoConsole.LogMessage("SERVER ARCHDATA:");
                    ArchipelagoConsole.LogMessage(alists.ToString());
                    if (alist2.seed != "")
                    {
                        alist.merge(alist2.list);
                    }
                    else
                    {
                        alist.seed = session.RoomState.Seed;
                    }
                }

                ArchipelagoConsole.CommandText = "!help";

                Authenticated = true;

                connectedplayerfile = HuniePopArchipelago.ArchipelagoClient.session.DataStorage[Scope.Slot, $"savefile"];

                session.Locations.CompleteLocationChecksAsync(null, ServerData.CheckedLocations.ToArray());
                outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

                ArchipelagoConsole.LogMessage(outText);
            }
            else
            {
                var failure = (LoginFailure)result;
                outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
                outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

                HuniePopArchipelago.BepinLogger.LogError(outText);
                ArchipelagoConsole.LogImportant(outText);

                Authenticated = false;
                Disconnect();
            }

            //ArchipelagoConsole.LogMessage(outText);
            attemptingConnection = false;
        }

        /// <summary>
        /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
        /// </summary>
        private void Disconnect()
        {
            HuniePopArchipelago.BepinLogger.LogDebug("disconnecting from server...");
            session?.Socket.Disconnect();
            session = null;
            Authenticated = false;
        }

        public void SendMessage(string message)
        {
            session.Socket.SendPacketAsync(new SayPacket { Text = message });
        }

        /// <summary>
        /// we received an item so reward it here
        /// </summary>
        /// <param name="helper">item helper which we can grab our item from</param>
        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var receivedItem = helper.DequeueItem();

            if (helper.Index <= ServerData.Index) return;

            ServerData.Index++;

            alist.add(receivedItem);

            // TODO reward the item here
            // if items can be received while in an invalid state for actually handling them, they can be placed in a local
            // queue/collection to be handled later
        }

        /// <summary>
        /// something went wrong with our socket connection
        /// </summary>
        /// <param name="e">thrown exception from our socket</param>
        /// <param name="message">message received from the server</param>
        private void OnSessionErrorReceived(Exception e, string message)
        {
            HuniePopArchipelago.BepinLogger.LogError(e);
            ArchipelagoConsole.LogMessage(message);
        }

        /// <summary>
        /// something went wrong closing our connection. disconnect and clean up
        /// </summary>
        /// <param name="reason"></param>
        private void OnSessionSocketClosed(string reason)
        {
            HuniePopArchipelago.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
            ArchipelagoConsole.LogImportant($"Connection to Archipelago lost: {reason}");
            Disconnect();
        }


        public void sendLoc(int loc)
        {
            if (!ServerData.CheckedLocations.Contains(loc)) { ServerData.CheckedLocations.Add(loc); }
            session.Locations.CompleteLocationChecksAsync(null, loc);
        }

        public void sendLoc(string set, int offset)
        {
            if (offset < 1)
            {
                ArchipelagoConsole.LogImportant($"ERROR: sendLoc called with offset < 1 ({offset}) for set {set} SEND TO MOD DEV TO FIX");
                return;
            }
            HuniePopArchipelago.BepinLogger.LogMessage($"SENDING LOC:\nSET:{set}, OFFSET:{offset}");
            if (set == null)
            {
                new Thread(() => sendLoc(offset)).Start();
                return;
            }
            new Thread(() => sendLoc(Convert.ToInt32(ServerData.slotData[set]) + offset)).Start();
            //sendLoc(Convert.ToInt32(ServerData.slotData[set]) + offset);
        }
        public static void sendCompletion()
        {
            var statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            HuniePopArchipelago.ArchipelagoClient.session.Socket.SendPacket(statusUpdatePacket);
        }

        public void resync()
        {
            SaveFile saveFile = SaveUtils.GetSaveFile(Menu.slot);
            if (saveFile == null || !saveFile.started) { return; }
            ArchipelagoConsole.LogMessage($"re-sending all locations");

            for (int i = 1; i < 13; i++)
            {

                GirlPlayerData girl = GameManager.System.Player.GetGirlData(GameManager.Data.Girls.Get(i));
                ArchipelagoConsole.LogMessage($"test");
                if (girl == null || (int)girl.metStatus != 3) { continue; }

                GirlDefinition girldef = GameManager.Data.Girls.Get(i);
                ArchipelagoConsole.LogMessage($"re-sending all {girldef.firstName.ToLower()}'s locations aquired");
                if (girl.relationshipLevel > 1) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 1); }
                if (girl.relationshipLevel > 2) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 2); }
                if (girl.relationshipLevel > 3) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 3); }
                if (girl.relationshipLevel > 4) { sendLoc($"{girldef.firstName.ToLower()}_date_loc_start", 4); }
                if (girl.gotPanties) { sendLoc("sleep_loc_start", (girldef.id)); }

                foreach (var gift in girldata[i].collection)
                {
                    if (girl.IsItemInCollection(gift))
                    {
                        sendLoc($"{girldef.firstName.ToLower()}_gift_loc_start", IDs.giftidtooffset(gift.id));
                    }
                }

                for (int j = 0; j < 12; j++)
                {
                    if (girl.IsDetailKnown((GirlDetailType)j)) { sendLoc($"{girldef.firstName.ToLower()}_question_loc_start", j+1); }
                }
            }
            ArchipelagoConsole.LogMessage($"all girl location re-sent");
            ArchipelagoConsole.LogMessage($"re-sending all panties turned in locations");

            for (int i = 0; i < 12; i++)
            {
                if (GameManager.System.Player.pantiesTurnedIn.Contains(i + 277)) { sendLoc("kyu_panties_loc_start", i+1); }
            }
            ArchipelagoConsole.LogMessage($"all panties turned in locations re-sent");
            ArchipelagoConsole.LogMessage($"all locations re-sent");
        }

        public static void setupgirldata()
        {
            List<GirlDefinition> olddata = GameManager.Data.Girls.GetAll();
            girldata = new Dictionary<int, GirlDefinition>();

            Dictionary<string, Dictionary<string, int>> data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(ServerData.slotData["girldata"].ToString());

            foreach (GirlDefinition g in olddata)
            {
                g.mostDesiredTrait = GameManager.Data.Traits.Get(data[g.firstName.ToLower()]["loves"] + 1);
                g.leastDesiredTrait = GameManager.Data.Traits.Get(data[g.firstName.ToLower()]["hates"] + 1);

                g.collection =
                [
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift1"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift2"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift3"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift4"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift5"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift6"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift7"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift8"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift9"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift10"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift11"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift12"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift13"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift14"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift15"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift16"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift17"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift18"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift19"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift20"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift21"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift22"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift23"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift24"]),
                ];

                g.uniqueGiftList =
                    [
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift19"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift20"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift21"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift22"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift23"]),
                    GameManager.Data.Items.Get(data[g.firstName.ToLower()]["gift24"]),

                    ];

                girldata.Add(g.id, g);
            }

        }

    }
}