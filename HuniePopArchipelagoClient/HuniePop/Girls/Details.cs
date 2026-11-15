using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HuniePopArchiepelagoClient.HuniePop.Girls
{
    [HarmonyPatch]
    public class Details
    {
        /// <summary>
        /// send an archipelago location for learning a girls detail
        /// </summary>
        [HarmonyPatch(typeof(GirlPlayerData), "KnowDetail")]
        [HarmonyPrefix]
        public static void favlocation(GirlDetailType type, GirlPlayerData __instance)
        {
            HuniePopArchipelago.ArchipelagoClient.sendLoc($"{__instance.GetGirlDefinition().firstName.ToLower()}_question_loc_start", (int)type +1);
        }

        /// <summary>
        /// overwite talk options to pritoise asking girl details
        /// </summary>
        [HarmonyPatch(typeof(GirlManager), "TalkWithHer")]
        [HarmonyPrefix]
        public static bool favlocation(GirlManager __instance)
        {
            Girl activeGirl = null;

            if (GameManager.System.Location.currentGirl != null && GameManager.System.Location.IsLocationSettled())
            {
                activeGirl = GameManager.Stage.girl;
            }

            if (activeGirl == null)
            {
                return false;
            }
            GirlPlayerData girlData = GameManager.System.Player.GetGirlData(activeGirl.definition);
            if (girlData.appetite >= 2)
            {
                girlData.appetite -= 2;

                List<DialogSceneDefinition> list = new List<DialogSceneDefinition>();

                //Questions about Girl details
                for (int i = 0; i < activeGirl.definition.talkQueries.Count; i++)
                {
                    if (GameManager.System.GameLogic.GameConditionsMet(activeGirl.definition.talkQueries[i].conditions, false))
                    {
                        list.Add(activeGirl.definition.talkQueries[i]);
                        list.Add(activeGirl.definition.talkQueries[i]);
                        list.Add(activeGirl.definition.talkQueries[i]);
                        if (girlData.gotPanties && GameManager.System.Player.endingSceneShown)
                        {
                            list.Add(activeGirl.definition.talkQueries[i]);
                            list.Add(activeGirl.definition.talkQueries[i]);
                            list.Add(activeGirl.definition.talkQueries[i]);
                        }
                    }
                }

                //50% chance to only ask girl detail questions unless you know them all
                if (GameManager.System.Player.GetGirlData(activeGirl.definition).DetailKnownCount()>11 || UnityEngine.Random.Range(0, 101) > 50)
                {
                    //Questions girl will ask about her details
                    for (int j = 0; j < activeGirl.definition.talkQuizzes.Count; j++)
                    {
                        if (GameManager.System.GameLogic.GameConditionsMet(activeGirl.definition.talkQuizzes[j].conditions, true) && !girlData.IsRecentQuiz(j))
                        {
                            list.Add(activeGirl.definition.talkQuizzes[j]);
                        }
                    }

                    //random Questions girl will ask you
                    for (int k = 0; k < activeGirl.definition.talkQuestions.Count; k++)
                    {
                        if (GameManager.System.GameLogic.GameConditionsMet(activeGirl.definition.talkQuestions[k].conditions, true) && !girlData.IsRecentQuestion(k))
                        {
                            list.Add(activeGirl.definition.talkQuestions[k]);
                        }
                    }
                }

                DialogSceneDefinition dialogSceneDefinition = list[UnityEngine.Random.Range(0, list.Count)];

                if (activeGirl.definition.talkQuizzes.Contains(dialogSceneDefinition))
                {
                    girlData.AddRecentQuiz(activeGirl.definition.talkQuizzes.IndexOf(dialogSceneDefinition));
                }
                if (activeGirl.definition.talkQuestions.Contains(dialogSceneDefinition))
                {
                    girlData.AddRecentQuestion(activeGirl.definition.talkQuestions.IndexOf(dialogSceneDefinition));
                }
                GameManager.System.Dialog.PlayDialogScene(dialogSceneDefinition);
                int num = Mathf.RoundToInt(50f * (1f + (float)girlData.UniqueGiftCount() * 0.5f) * (1f + (float)girlData.inebriation * 0.1f));
                GameManager.System.Player.hunie += num;
                EnergyTrail component = new GameObject("EnergyTrail", new Type[] { typeof(EnergyTrail) }).GetComponent<EnergyTrail>();
                component.Init(GameManager.Stage.uiGirl.itemGiftZone.gameObj.transform.position, GameManager.Stage.uiGirl.itemGiveGiftEnergyTrail, "+" + num + " Hunie", EnergyTrailFormat.END, null);
                GameManager.System.Audio.Play(AudioCategory.SOUND, GameManager.Stage.uiGirl.extraResourceFlourishSound, false, 1f, true);
                if (girlData.appetite < 2)
                {
                    GameManager.System.Player.AddMessage(GameManager.Data.Messages.Get(40));
                }
            }
            else
            {
                __instance.TriggerDialog(GameManager.Stage.uiGirl.isHungryDialogTrigger, 0, false, -1);
                GameManager.Stage.uiWindows.ResetActiveWindow();
            }
            GameManager.System.Player.chatSessionCount++;
            return false;
        }
    }
}
