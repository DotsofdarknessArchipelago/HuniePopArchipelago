using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Data
    {
        [HarmonyPatch(typeof(GirlData), "Get")]
        [HarmonyPrefix]
        public static bool girldataoverrite1(int id, ref Dictionary<int, GirlDefinition> ____definitions, ref GirlDefinition __result)
        {
            if (ArchipelagoClient.girldata == null)
            {
                return true; 
            }

            if (ArchipelagoClient.girldata.ContainsKey(id))
            {
                __result = ArchipelagoClient.girldata[id];
                return false;
                //__result.mostDesiredTrait = (GameManager.Data.Traits.Get(Convert.ToInt32(Plugin.curse.connected.slot_data["girldata"][__result.name]["loves"])));
                //__result.leastDesiredTrait = (GameManager.Data.Traits.Get(Convert.ToInt32(Plugin.curse.connected.slot_data["girldata"][__result.name]["hates"])));

                //__result.collection = 
            }
            __result = null;
            return false;
        }
        [HarmonyPatch(typeof(GirlData), "GetAll")]
        [HarmonyPrefix]
        public static bool girldataoverrite2(GirlData __instance, ref Dictionary<int, GirlDefinition> ____definitions, ref List<GirlDefinition> __result)
        {
            if (ArchipelagoClient.girldata == null) return true;

            List<GirlDefinition> list = new List<GirlDefinition>();
            for (int i = 1; i <= ArchipelagoClient.girldata.Count; i++)
            {
                list.Add(ArchipelagoClient.girldata[i]);
            }
            __result = list;
            return false;
        }

    }
}
