using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;
using System.Linq;

namespace HuniePopArchiepelagoClient.Archipelago
{
    public class ArchipelagoItem
    {
        public long Id;
        public long LocationId;
        public int playerslot;
        public string playername;
        public string itemname;
        public string locationname;
        public bool processed = false;
        public bool putinshop = false;

        public ArchipelagoItem(ItemInfo item)
        {
            this.Id = item.ItemId;
            this.LocationId = item.LocationId;
            this.playerslot = item.Player.Slot;
            this.itemname = item.ItemName;
            if (item.Player.Slot <= 0)
            {
                this.playername = "SERVER";
                this.locationname = "SERVER";
                return;
            }
            string playername = item.Player.Name;
            string game = item.LocationGame;

            this.playername = playername;
            if (this.LocationId <= 0) { this.locationname = "SERVER"; }
            else { this.locationname = item.LocationName; }
        }

        public ArchipelagoItem()
        {
            this.Id = -1;
            this.playerslot = -1;
            this.LocationId = -1;
        }

        public bool equals(ArchipelagoItem i2)
        {
            if (i2 == null) return false;
            if (this.Id != i2.Id) return false;
            if (this.LocationId != i2.LocationId) return false;
            if (this.playerslot != i2.playerslot) return false;
            return true;
        }

    }

    public class ArchipelageItemList
    {
        public string host = "";
        public string user = "";
        public string pass = "";
        public string seed = "";
        public bool needtoreset = false;
        public int listversion = 2;
        public List<ArchipelagoItem> list = new List<ArchipelagoItem>();

        public void add(ItemInfo netitem)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == netitem.ItemId && list[i].playerslot == netitem.Player.Slot && list[i].LocationId == netitem.LocationId)
                {
                    HuniePopArchipelago.BepinLogger.LogMessage($"-item({netitem.ItemId}:{netitem.ItemName} from loc:{netitem.LocationId} total items={list.Count}) already in list skipping");
                    return;
                }
            }
            ArchipelagoItem item = new ArchipelagoItem(netitem);
            list.Add(item);
            HuniePopArchipelago.BepinLogger.LogMessage($"+item({netitem.ItemId}:{netitem.ItemName} from loc:{netitem.LocationId} total items={list.Count}) not in list adding");
        }

        public void add(ArchipelagoItem ai)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == ai.Id && list[i].playername == ai.playername && list[i].LocationId == ai.LocationId)
                {
                    list[i].processed = ai.processed;
                    list[i].putinshop = ai.putinshop;
                    return;
                }
            }
            list.Add(ai);
        }

        public bool merge(List<ArchipelagoItem> oldlist)
        {

            if (oldlist == null || oldlist.Count == 0) return false;
            for (int i = 0; i < oldlist.Count; i++)
            {
                add(oldlist[i]);
            }
            return false;

            if (oldlist.Count == 0) { return true; }
            if (list.Count == 0) { list = oldlist; return false; }

            for (int i = 0; i < oldlist.Count; i++)
            {
                bool f = true;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].equals(oldlist[i]))
                    {
                        list[j] = oldlist[i];
                        f = false;
                        break;
                    }
                }
                if (f) { return true; }
            }

            return false;
        }

        public bool hasitem(long flag)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == flag) { return true; }
            }
            return false;
        }

        public int itemcount(long flag)
        {
            int c = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == flag) { c++; }
            }
            return c;
        }

        public bool hasitemcount(long flag, int count)
        {
            int c = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == flag) { c++; }
                if (c >= count) { return true; }
            }
            return false;
        }

        public string listprint()
        {
            string output = "";
            output += "-------------\n";
            for (int i = 0; i < list.Count; i++)
            {
                output += $"I:{i}";
                output += $"ID:{list[i].Id} PLAYER:{list[i].playerslot} LOC:{list[i].LocationId}\n";
                output += $"PROCESSED:{list[i].processed} PUTINSHOP:{list[i].putinshop}\n";
            }
            return output;
        }

        public bool needprocessing()
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].processed) { return true; }
            }
            return false;
        }
    }
}
