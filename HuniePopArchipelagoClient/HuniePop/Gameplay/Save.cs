using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.HuniePop.Girls;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Save
    {

        /// <summary>
        /// change the save file to be HuniePopArchSaveData1.game
        /// </summary>
        [HarmonyPatch(typeof(SaveUtils), "Init")]
        [HarmonyILManipulator]
        public static void talkedit(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (ctx.Instrs[i].OpCode == OpCodes.Ldstr)
                {
                    if (ctx.Instrs[i].Operand.ToString() == "/HuniePopSaveData")
                    {
                        ctx.Instrs[i].Operand = "/HuniePopArchSaveData";
                    }
                    if (ctx.Instrs[i].Operand.ToString() == "HuniePopSaveData")
                    {
                        ctx.Instrs[i].Operand = "HuniePopArchSaveData";
                    }
                }
            }
        }


        /// <summary>
        /// make sure that when reseting a file that the inventory is clear of items and starting location/girl is -1
        /// </summary>
        [HarmonyPatch(typeof(SaveFile), "ResetFile")]
        [HarmonyPostfix]
        public static void savereset(SaveFile __instance)
        {
            __instance.currentGirl = -1;
            __instance.currentLocation = -1;

            __instance.inventory = new InventoryItemSaveData[30];
            for (int j = 0; j < __instance.inventory.Length; j++)
            {
                __instance.inventory[j] = new InventoryItemSaveData();
            }
        }


        /// <summary>
        /// make it so every time the same is saved that we save at archdata as well
        /// </summary>
        [HarmonyPatch(typeof(GameManager), "SaveGame")]
        [HarmonyPostfix]
        public static void saveflags()
        {

            HuniePopArchipelago.ArchipelagoClient.session.DataStorage[Scope.Slot, "savefile"] = JsonConvert.SerializeObject(GameManager.System.SaveFile);
            HuniePopArchipelago.ArchipelagoClient.session.DataStorage[Scope.Slot, "archdata"] = JsonConvert.SerializeObject(ArchipelagoClient.alist);

            //using (StreamWriter file = File.CreateText(Application.persistentDataPath + "/archdata"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(file, ArchipelagoClient.alist);
            //}
        }


        [HarmonyPatch(typeof(GirlSaveData), MethodType.Constructor, new Type[] { typeof(GirlDefinition) })]
        [HarmonyPrefix]
        public static bool girlsavetest(GirlDefinition girl, GirlSaveData __instance)
        {
            __instance.metStatus = 0;
            __instance.relationshipLevel = 1;
            __instance.appetite = 6;
            __instance.inebriation = 0;
            __instance.dayDated = false;
            __instance.photosEarned = new List<int>();
            __instance.uniqueGifts = new List<int>();
            __instance.collection = new List<int>();
            __instance.hairstyles = new List<int>();
            __instance.hairstyles.Add(0);
            __instance.outfits = new List<int>();
            __instance.outfits.Add(0);
            __instance.hairstyle = 0;
            __instance.outfit = 0;
            __instance.details = new bool[Enum.GetNames(typeof(GirlDetailType)).Length];
            __instance.recentQuizzes = new List<int>();
            __instance.recentQuestions = new List<int>();
            __instance.gotPanties = false;
            __instance.lovePotionUsed = false;
            return false;
        }

    }
}
