using HarmonyLib;
using HuniePopArchiepelagoClient;
using HuniePopArchiepelagoClient.Archipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuniePopArchipelagoClient.HuniePop.Girls
{
    [HarmonyPatch]
    public class Date
    {

        public static int? junkloc;

        [HarmonyPatch(typeof(ActionMenuButton), "Init")]
        [HarmonyPostfix]
        public static void datebutton(ActionMenuButton __instance)
        {
            junkloc ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]);

            if (__instance.definition.action == ActionMenuItemType.ASK_HER_OUT && Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["progressive_dates"]))
            {
                if (ArchipelagoClient.alist.hasitemcount((long)(junkloc + 86), 4)) { return; }

                if (!ArchipelagoClient.alist.hasitemcount((long)(junkloc + 86), GameManager.System.Player.GetGirlData(GameManager.System.Location.currentGirl).relationshipLevel))
                {
                    __instance.button.Disable();
                    __instance.Refresh();
                }
            }
        }

        [HarmonyPatch(typeof(ActionMenuButton), "Refresh")]
        [HarmonyPostfix]
        public static void datebutton2(ActionMenuButton __instance)
        {
            junkloc ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]);

            switch (__instance.definition.subtitleType)
            {
                case ActionMenuSubtitleType.ASK_ON_DATE:
                    if (!__instance.button.IsEnabled())
                    {
                        if (GameManager.System.Player.GetGirlData(GameManager.System.Location.currentGirl).dayDated) { __instance.actionSubLabel.SetText("-Finished-"); }
                        else if (!ArchipelagoClient.alist.hasitemcount((long)(junkloc + 86), GameManager.System.Player.GetGirlData(GameManager.System.Location.currentGirl).relationshipLevel)) { __instance.actionSubLabel.SetText("-Missing Date Pass-"); }
                    }
                    break;
            }

        }
    }
}
