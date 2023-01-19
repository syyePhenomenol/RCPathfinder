using System.Diagnostics;
using System.Diagnostics.Contracts;
using Modding;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;

namespace RMPathfinder
{
    public class RMPathfinder : Mod
    {
        public override string GetVersion() => "test";

        public static RMPathfinder? Instance { get; private set; }

        public static RMSearchSettings? Rmss { get; private set; }

        public RMPathfinder()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            On.GameManager.StartNewGame += GameManager_StartNewGame;
            On.GameManager.ContinueGame += GameManager_ContinueGame;
            On.QuitToMenu.Start += QuitToMenu_Start;
        }

        private static void GameManager_ContinueGame(On.GameManager.orig_ContinueGame orig, GameManager self)
        {
            orig(self);
            OnEnterGame();
        }

        private static void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
        {
            orig(self, permadeathMode, bossRushMode);
            OnEnterGame();
        }

        private static void OnEnterGame()
        {
            if (!RandomizerMod.RandomizerMod.IsRandoSave) return;

            Rmss = new();

            Testing.GlobalTest();
        }

        private System.Collections.IEnumerator QuitToMenu_Start(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            Rmss = null;

            return orig(self);
        }
    }
}
