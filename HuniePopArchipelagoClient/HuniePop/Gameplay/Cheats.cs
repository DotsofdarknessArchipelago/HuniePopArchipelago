using HarmonyLib;

namespace HuniePopArchiepelagoClient.HuniePop.Gameplay
{
    [HarmonyPatch]
    public class Cheats
    {
        public static bool enable = false;

        /// <summary>
        /// makes all puzzles complete in 1 move
        /// used for easy development since puzzles take time
        /// </summary>
        [HarmonyPatch(typeof(PuzzleGame), "AddResourceValue")]
        [HarmonyPrefix]
        public static void puzzleautocomplete(PuzzleGame __instance)
        {
            if (enable)
            {
                __instance.SetResourceValue(PuzzleGameResourceType.AFFECTION, 9999, true);
            }
        }
    }
}
