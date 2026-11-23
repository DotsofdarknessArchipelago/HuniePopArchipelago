using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Boomlagoon.JSON;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Puzzle
    {
        public static bool deathprocessing = false;


        /// <summary>
        /// process deathlinks
        /// </summary>
        [HarmonyPatch(typeof(PuzzleGame), "OnUpdate")]
        [HarmonyPrefix]
        public static void puzzledeathlinks(PuzzleGame __instance)
        {
            if (__instance.isBonusRound) { return; }
            if (__instance.puzzleGameState == PuzzleGameState.WAITING && HuniePopArchipelago.ArchipelagoClient.DeathLinkHandler.HasDeathLinks())
            {
                DeathLink death = HuniePopArchipelago.ArchipelagoClient.DeathLinkHandler.GetDeathLinkDeath();
                if (death.Cause != null)
                {
                    ArchipelagoConsole.LogMessage($"Recieved DEATH from {death.Source} cause {death.Cause}");
                }
                else
                {
                    ArchipelagoConsole.LogMessage($"Recieved DEATH from {death.Source}");
                }
                __instance.puzzleGameState = PuzzleGameState.FINISHED;
                deathprocessing = true;
            }
        }

        /// <summary>
        /// send archipelago location for completing a date/having sex with a girl
        /// </summary>
        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyPrefix]
        public static void datefin(PuzzleManager __instance, ref PuzzleGame ____activePuzzleGame)
        {
            GirlPlayerData girlData = GameManager.System.Player.GetGirlData(GameManager.System.Location.currentGirl);
            if (____activePuzzleGame.isVictorious)
            {
                if (girlData.relationshipLevel >= 1) { HuniePopArchipelago.ArchipelagoClient.sendLoc($"{girlData.GetGirlDefinition().firstName.ToLower()}_date_loc_start", 1); }
                if (girlData.relationshipLevel >= 2) { HuniePopArchipelago.ArchipelagoClient.sendLoc($"{girlData.GetGirlDefinition().firstName.ToLower()}_date_loc_start", 2); }
                if (girlData.relationshipLevel >= 3) { HuniePopArchipelago.ArchipelagoClient.sendLoc($"{girlData.GetGirlDefinition().firstName.ToLower()}_date_loc_start", 3); }
                if (girlData.relationshipLevel >= 4) { HuniePopArchipelago.ArchipelagoClient.sendLoc($"{girlData.GetGirlDefinition().firstName.ToLower()}_date_loc_start", 4); }
            }
            if (____activePuzzleGame.isBonusRound && !girlData.gotPanties)
            {
                girlData.gotPanties = true;
                girlData.AddPhotoEarned(3);
                HuniePopArchipelago.ArchipelagoClient.sendLoc("sleep_loc_start", (girlData.GetGirlDefinition().id));
            }
            if (HuniePopArchipelago.ArchipelagoClient.DeathLinkHandler.deathLinkOptionVal > 0 && !deathprocessing && !____activePuzzleGame.isVictorious)
            {
                HuniePopArchipelago.ArchipelagoClient.DeathLinkHandler.SendDeathLink($"Failed date with {GameManager.System.Location.currentGirl.firstName}");
            }
            deathprocessing = false;
        }


        public static int? junkloc;

        /// <summary>
        /// disable bonus round when you dont have date pass for it when playing with progressive dates
        /// </summary>
        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyPostfix]
        public static void datefin2(PuzzleManager __instance, ref bool ____goToBonusRound)
        {
            junkloc ??= Convert.ToInt32(ArchipelagoClient.ServerData.slotData["junk_item_start"]);

            if (____goToBonusRound && Convert.ToBoolean(ArchipelagoClient.ServerData.slotData["progressive_dates"]))
            {
                if (!ArchipelagoClient.alist.hasitemcount((long)(junkloc + 86), 5)) { ____goToBonusRound = false; }
            }
        }

        /// <summary>
        /// NOP instructions for when completeing a puzzle for getting panties
        /// </summary>
        [HarmonyPatch(typeof(PuzzleManager), "OnPuzzleGameComplete")]
        [HarmonyILManipulator]
        public static void removepantiesgift(ILContext ctx, MethodBase orig)
        {
            for (int i = 0; i < ctx.Instrs.Count; i++)
            {
                if (i < 10) { continue; }
                if (ctx.Instrs[i - 1].OpCode == OpCodes.Br && ctx.Instrs[i].OpCode == OpCodes.Ldloc_0 && ctx.Instrs[i + 1].OpCode == OpCodes.Callvirt)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 1].OpCode = OpCodes.Ldc_I4_1;
                }

                if (ctx.Instrs[i - 2].OpCode == OpCodes.Callvirt && ctx.Instrs[i - 1].OpCode == OpCodes.Br && ctx.Instrs[i].OpCode == OpCodes.Call && ctx.Instrs[i + 1].OpCode == OpCodes.Callvirt)
                {
                    ctx.Instrs[i].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 1].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 2].OpCode = OpCodes.Nop;
                    ctx.Instrs[i + 3].OpCode = OpCodes.Ldc_I4_1;
                }
            }
        }

        /// <summary>
        /// set affection goal for a puzzle besed on what we want caped at 999999
        /// </summary>
        [HarmonyPatch(typeof(PuzzleManager), "GetAffectionGoal")]
        [HarmonyPrefix]
        public static bool puzzlegoalmodding(ref int __result)
        {
            int goal = Convert.ToInt32(ArchipelagoClient.ServerData.slotData["puzzle_affection_base"]);
            int mod = 0;

            List<GirlPlayerData> girls = GameManager.System.Player.girls;
            foreach (GirlPlayerData g in girls)
            {
                mod += (g.relationshipLevel - 1);
            }
            __result = goal + (Convert.ToInt32(ArchipelagoClient.ServerData.slotData["puzzle_affection_add"]) * mod);
            if (__result > 999999) { __result = 999999; }
            return false;
        }

        /// <summary>
        /// set number of moves when begining a puzzle game
        /// </summary>
        [HarmonyPatch(typeof(PuzzleGame), "Begin")]
        [HarmonyPrefix]
        public static void puzzlemodding(PuzzleGame __instance, ref int ____maxMoves)
        {
            ____maxMoves = 99;
            if (__instance.isBonusRound)
            {
                //ArchipelagoConsole.LogMessage("BONUS");
            }
            else
            {
                //ArchipelagoConsole.LogMessage("NOT BONUS");
                __instance.SetResourceValue(PuzzleGameResourceType.MOVES, Convert.ToInt32(ArchipelagoClient.ServerData.slotData["puzzle_moves"]), false);
            }
        }
    }
}
