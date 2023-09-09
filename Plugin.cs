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

    public class ChrData {
        public Vector3 position;
        public void setPosition(float x, float y) {
            position.x = x;
            position.y = y;
        }
    }

    static class GameObjects {
        //public static EnemyInformation enemyInformation;
        public static ButtonControll buttonControl; // 为什么两个l (
        public static Plugin notArknightsMod;
        public static ChrData chrData = new ChrData();
        public static Enemies enemies = new Enemies();
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
        }
        private void Start()
        {
            GameObjects.notArknightsMod = this;
            // Plugin startup logic
            //Logger.LogInfo(new Amiya_Controller().event_Roll_End);
            //GameManager.money=1000000;
            this.DoPatching();
        }
        public void Update()
        {
            if (Input.GetKeyDown (KeyCode.P))
            {
                GameObjects.enemies.getEnemies();
                GameObjects.enemies.printEnemies();
            }
            if (Input.GetKeyDown (KeyCode.C))
            {
                displayingWindow = !displayingWindow;
            }
        }
        private void renderPosition() {
            string coordinateText = "X: " + GameObjects.chrData.position.x.ToString("F2") + " Y: " + GameObjects.chrData.position.y.ToString("F2");

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(Screen.width / 12, Screen.height / 5, Screen.width, Screen.height / 5 + 10), coordinateText, style);
        }
        private void OnGUI()
        {
            if(
                GameObjects.chrData.position.x != 0.0f &&
                GameObjects.chrData.position.y != 0.0f
            ) this.renderPosition();
            if (this.displayingWindow)
            {

                
                Rect windowRect = new Rect(0f, 0f,Screen.width/3, Screen.height/3);

                windowRect = GUI.Window(31510001, windowRect, doWindow, "面板");

                GUILayout.BeginArea(new Rect(0f, 0f,Screen.width/3, Screen.height/3));

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

                if (GUILayout.Button("挑战")) {
                    NAHelper.Warn("欸嘿~");
                    GameObject.Find("EnemyInformation").GetComponent<EnemyInformation>().automatic_challenge();
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
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(Amiya_Controller __instance) {
            
        } 
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
        private static void Update(Amiya_Controller __instance)
	    {
            GameObjects.chrData.position = __instance.transform.localPosition;
            // 以下为作弊
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
    [HarmonyPatch(typeof(ButtonControll))]
    public static class ButtonControll_Patch {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(ButtonControll __instance)
	    {
            GameObjects.buttonControl = __instance;
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
    /*
    [HarmonyPatch(typeof(Nbkght_AI))]
    public static class Nbkght_AI_Patch {
        [HarmonyPatch("DataReceive")]
        [HarmonyPostfix]
        private static void DataReceive(Nbkght_AI __instance)
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
        private static void Update_SuperArmor(Nbkght_AI __instance)
	    {
            if (!Settings.HardMode) return;
		    if (__instance.currentState == "hurt"){
                __instance.SetCharacterState("idle");
                Traverse.Create(__instance).Field("moveLock").SetValue(false);      
            }
	    }
    }
    */
    [HarmonyPatch(typeof(AxeSoldier_AI))]
    public static class AxeSoldier_Patch {
        [HarmonyPatch("DataReceive")]
        [HarmonyPostfix]
        private static void DataRecieve(AxeSoldier_AI __instance)
	    {
            if (Settings.HardMode) {
                Traverse.Create(__instance).Field("max_HP").SetValue(((int)Traverse.Create(__instance).Field("max_HP").GetValue())*5);
                __instance.HP *= 5;
                Traverse.Create(__instance).Field("attackSpeed").SetValue(((float)Traverse.Create(__instance).Field("attackSpeed").GetValue())*2.5);
                Traverse.Create(__instance).Field("moveSpeed").SetValue(((float)Traverse.Create(__instance).Field("moveSpeed").GetValue())*6);
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

    // enemies
    public class Enemies {
        public Nbkght_AI[] nbkghts;
        public void getEnemies() {
            this.nbkghts = GameObject.FindObjectsOfType<Nbkght_AI>();
        }
        public void printEnemies() {
            foreach (Nbkght_AI obj in this.nbkghts)
            {
                // 在这里处理找到的对象
                GameObjects.notArknightsMod.log("Nbkght: " + obj.gameObject.name);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInformation))]
    public static class PlayerInformation_Patch {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void Start(PlayerInformation __instance) {
            GameObjects.notArknightsMod.log("skadi bwb");
        }
        [HarmonyPatch("LoadCharacter")]
        [HarmonyPrefix]
        private static void LoadCharacter(PlayerInformation __instance)
	    {
            GameObjects.notArknightsMod.log("Skadi awa");
            __instance.ChoiceCharacter = "Skadi";
        }
    }
}
