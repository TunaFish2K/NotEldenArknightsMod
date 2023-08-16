using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace NotArknightsMod
{
    public static class NAHelper {
        public static UIEffect ui;
        public static void Warn (string content) {
            (new Plugin()).log(NAHelper.ui.ToString());
            if (!NAHelper.ui) return;
            NAHelper.ui.Warn(content);
        }
    }
    public static class Settings {
        public static bool Invulnerable = false;
        public static bool SuperArmor = false;
        public static bool LockMP = false;
        public static bool Fly = false;
        public static bool IgnoreMerchantDeath = false;
        public static bool HardMode = false;
    }
    [BepInPlugin("tunafish2k.notarknightsmod", "Not Arknights Mod", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public bool displayingWindow = false;
        public void log(string content) {
            Logger.LogInfo(content);
        }
        public void DoPatching()
        {
            var harmony = new Harmony("tunafish2k.notarknightsmod");
            harmony.PatchAll();
            Logger.LogInfo("awa");
        }
        private void Start()
        {
            // Plugin startup logic
            //Logger.LogInfo(new Amiya_Controller().event_Roll_End);
            //GameManager.money=1000000;
            this.DoPatching();
        }
        public void Update()
        {
            if (Input.GetKeyDown (KeyCode.C))
            {
                displayingWindow = !displayingWindow;
            }
        }
        private void OnGUI()
        {
            if (this.displayingWindow)
            {

                
                Rect windowRect = new Rect(10, 600, 400, 1000);

                windowRect = GUI.Window(31510001, windowRect, doWindow, "面板");

                GUILayout.BeginArea(new Rect(10, 600, 400, 1000));

                GUILayout.Label("作弊");
                Settings.Invulnerable = (GUILayout.Toggle(Settings.Invulnerable,"无敌"));
                Settings.SuperArmor = (GUILayout.Toggle(Settings.SuperArmor,"霸体"));
                Settings.LockMP = (GUILayout.Toggle(Settings.LockMP,"无限MP"));
                Settings.Fly = (GUILayout.Toggle(Settings.Fly,"腾空跳"));
                Settings.HardMode = (GUILayout.Toggle(Settings.HardMode,"困难模式"));
                Settings.IgnoreMerchantDeath = (GUILayout.Toggle(Settings.IgnoreMerchantDeath,"永远生成商人"));

                if (GUILayout.Button("重置商人击杀进度")) {
                    PlayerPrefs.SetInt("BOSS", 0);
                    NAHelper.Warn("成功重置进度，休息一下吧。");
                }

                GUILayout.EndArea();
            }
        }

        public void doWindow(int winId)
        {
        }
    }
    [HarmonyPatch(typeof(Amiya_Controller))]
    public static class Amiya_Controller_Patch {
        [HarmonyPatch("DataReceive")]
        [HarmonyPostfix]
        private static void DataReceive(Amiya_Controller __instance)
	    {
            if (Settings.Invulnerable){
               	__instance.HP = (int)Traverse.Create(__instance).Field("max_HP").GetValue();
                __instance.basicData.HP = __instance.HP;
                __instance.gameManager.HP = __instance.HP; 
            }
            if (Settings.LockMP){
                __instance.MP = (int)Traverse.Create(__instance).Field("max_MP").GetValue();
                __instance.basicData.MP = __instance.MP;
                __instance.gameManager.MP = __instance.MP; 
            }

	    }
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update_SuperArmor(Amiya_Controller __instance)
	    {
            if (!Settings.SuperArmor) return;
		    if (__instance.currentState == "hurt"){
                __instance.SetCharacterState("idle");
                Traverse.Create(__instance).Field("moveLock").SetValue(false);      
            }
	    }
        [HarmonyPatch("Jump")]
        [HarmonyPrefix]
        private static bool Jump(Amiya_Controller __instance)
	    {
            if (Settings.Fly){
                if (((string)Traverse.Create(__instance).Field("actionInput").GetValue()) == "Jump")
		            {
		            	((Rigidbody2D)Traverse.Create(__instance).Field("playerRigibody").GetValue()).velocity = new Vector2(0,5.5f);
		            }
            return false;
            }
        return true;   
	    }
    }
    [HarmonyPatch(typeof(GameManager))]
    public static class GameManager_Patch {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(GameManager __instance)
	    {
            NAHelper.ui = __instance.uiEffect;
        }
    }
    [HarmonyPatch(typeof(Bearmi_AI))]
    public static class Bearmi_AI_Patch {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(GameManager __instance)
	    {
            if (Settings.IgnoreMerchantDeath) {
                __instance.gameObject.SetActive(true);
            }
            
        }
    }
    [HarmonyPatch(typeof(Arkadia_AI))]
    public static class Arkadia_AI_Patch {
        [HarmonyPatch("DataReceive")]
        [HarmonyPostfix]
        private static void DataRecieve(Arkadia_AI __instance)
	    {
            if (Settings.HardMode) {
                Traverse.Create(__instance).Field("max_HP").SetValue(((int)Traverse.Create(__instance).Field("max_HP").GetValue())*5);
                __instance.HP *= 5;
                Traverse.Create(__instance).Field("attackSpeed").SetValue(((int)Traverse.Create(__instance).Field("attackSpeed").GetValue())*2);
                Traverse.Create(__instance).Field("moveSpeed").SetValue(((int)Traverse.Create(__instance).Field("moveSpeed").GetValue())*2);
            }
            
        }
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update_SuperArmor(Arkadia_AI __instance)
	    {
            if (!Settings.HardMode) return;
		    if (__instance.currentState == "hurt"){
                __instance.SetCharacterState("idle");
                Traverse.Create(__instance).Field("moveLock").SetValue(false);      
            }
	    }
    }
    [HarmonyPatch(typeof(AxeSoldier_AI))]
    public static class AxeSoldier_Patch {
        [HarmonyPatch("DataReceive")]
        [HarmonyPostfix]
        private static void DataRecieve(AxeSoldier_AI __instance)
	    {
            if (Settings.HardMode) {
                Traverse.Create(__instance).Field("max_HP").SetValue(((int)Traverse.Create(__instance).Field("max_HP").GetValue())*5);
                __instance.HP *= 5;
                Traverse.Create(__instance).Field("attackSpeed").SetValue(((float)Traverse.Create(__instance).Field("attackSpeed").GetValue())*2);
                Traverse.Create(__instance).Field("moveSpeed").SetValue(((float)Traverse.Create(__instance).Field("moveSpeed").GetValue())*2);
            }
            
        }
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update_SuperArmor(AxeSoldier_AI __instance)
	    {
            if (!Settings.HardMode) return;
		    if (__instance.currentState == "hurt"){
                __instance.SetCharacterState("idle");
                Traverse.Create(__instance).Field("moveLock").SetValue(false);      
            }
	    }
    }
}
