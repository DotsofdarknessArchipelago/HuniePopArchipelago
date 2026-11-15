using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.UI;
using HuniePopArchipelagoClient.Archipelago;
using UnityEngine;

namespace HuniePopArchiepelagoClient
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class HuniePopArchipelago : BaseUnityPlugin
    {
        public const string PluginGUID = "Dots.Archipelago.huniepop";
        public const string PluginName = "Hunie Pop";
        public const string PluginVersion = "3.0.0";
        public static int compatworldmajor = 3;
        public static int compatworldminor = 0;
        public static int compatworldbuild = 0;


        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        public const string APDisplayInfo = $"Archipelago v{PluginVersion}";
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;

        public static bool showwindow = false;

        private static Texture2D SolidBoxTex;

        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            ArchipelagoClient = new ArchipelagoClient();
            ArchipelagoConsole.Awake();

            new Harmony(PluginGUID).PatchAll();

            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded! - Press F8 to Toggle Console");

        }
        void Update()
        {
            if (ArchipelagoClient.Authenticated)
            {
                if (GameManager.System.GameState != GameState.PUZZLE && ArchipelagoClient.DeathLinkHandler.deathLinkOptionVal != 3 && ArchipelagoClient.DeathLinkHandler.HasDeathLinks())
                {
                    BepinLogger.LogMessage("RESETTING DEATLINK LIST SINCE OUTSIDE OF PUZZLE");
                    ArchipelagoClient.DeathLinkHandler.ClearDeathLink();
                }
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                ArchipelagoConsole.toggle();
            }
            //if (Input.GetKeyDown(KeyCode.F7))
            //{
            //    ArchipelagoConsole.LogMessage($"gamestate: {GameManager.System.GameState}");
            //    ArchipelagoConsole.LogMessage($"puzzlestate: {GameManager.System.Puzzle.Game.puzzleGameState}");
            //}
            if (Input.GetKeyDown(KeyCode.Escape) && !ArchipelagoClient.Authenticated)
            {
                Menu.newgame = false;
            }
            if (Input.GetKeyDown(KeyCode.Return) && !ArchipelagoConsole.Hidden && !ArchipelagoConsole.CommandText.IsNullOrWhiteSpace())
            {
                if (ArchipelagoConsole.CommandText.StartsWith("$")) { Codes.ProcessCode(ArchipelagoConsole.CommandText); }
                else { HuniePopArchipelago.ArchipelagoClient.SendMessage(ArchipelagoConsole.CommandText); }
                ArchipelagoConsole.CommandText = "";
            }
        }

        private void OnGUI()
        {
            GUI.depth = -1;
            // show the mod is currently loaded in the corner
            ArchipelagoConsole.OnGUI();

            GUI.backgroundColor = Color.black;

            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;
                GUI.Box(new Rect(Screen.width - 300, 40, 300, 20), "");
                if (ArchipelagoClient.worldver == null)
                {
                    GUI.Label(new Rect(Screen.width - 295, 40, 300, 20), "Client V(" + PluginVersion + "), World V(ERROR VERSION NOT SUPPORTED): Status: Connected");
                }
                else
                {
                    GUI.Label(new Rect(Screen.width - 295, 40, 300, 20), "Client V(" + PluginVersion + "), World V(" + ArchipelagoClient.worldver.Major + "." + ArchipelagoClient.worldver.Minor + "." + ArchipelagoClient.worldver.Build + "): Status: Connected");
                }

            }

            if (showwindow)
            {
                Menu.archwindowrect.Set((float)(Screen.width * 0.1), (float)(Screen.height * 0.1), (float)(Screen.width * 0.8), (float)(Screen.height * 0.8));

                Menu.archwindow();
                //GUI.Window(61, Menu.archwindowrect, Menu.archwindow, "");
            }
            // this is a good place to create and add a bunch of debug buttons
        }


        public static void DrawSolidBox(Rect boxRect)
        {
            if (SolidBoxTex == null)
            {
                var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                windowBackground.SetPixel(0, 0, new Color(0, 0, 0));
                windowBackground.Apply();
                SolidBoxTex = windowBackground;
            }

            // It's necessary to make a new GUIStyle here or the texture doesn't show up
            GUI.Box(boxRect, "", new GUIStyle { normal = new GUIStyleState { background = SolidBoxTex } });
        }

    }
}