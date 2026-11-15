using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Game
    {

        /// <summary>
        /// checks for continuing a game
        /// </summary>
        [HarmonyPatch(typeof(LoadScreenSaveFile), "OnContinueButtonPressed")]
        [HarmonyPrefix]
        public static bool continueoveride(ref int ____saveFileIndex)
        {
            //make sure we are connected to a server
            //if (!Plugin.curse.fullconnection && Plugin.tringtoconnect)
            //{
            //    ArchipelagoConsole.LogImportant("There was an error setting up the connection to the server please restart the game and try again");
            //    return false;
            //}

            //only continue a game in the bottom left slot
            if (____saveFileIndex != 3) { return false; }

            //check if we have items that need to be processed and inform player
            bool n = false;
            foreach (ArchipelagoItem i in ArchipelagoClient.alist.list)
            {
                if (!i.processed) { n = true; break; }
            }
            if (n) { ArchipelagoConsole.LogMessage("Items need to be processed please move locations to process them"); }
            return true;
        }


        /// <summary>
        /// dont know really but its there and dosent break anything
        /// </summary>
        [HarmonyPatch(typeof(GameManager), "LoadGame")]
        [HarmonyILManipulator]
        public static void loadgamereset(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Brtrue) { ctx.Instrs[i].OpCode = OpCodes.Brfalse; break; }
            }
        }
    }
}
