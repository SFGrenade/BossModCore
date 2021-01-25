using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using Modding;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using BossModCore.MonoBehaviours;
using BossModCore.Utils;
using UnityEngine.SceneManagement;
using UGameObject = UnityEngine.GameObject;

namespace BossModCore
{
    public class BossModCore : Mod<BmcSaveSettings, BmcGlobalSettings>
    {
        internal static BossModCore Instance;

        public SceneChanger sceneChanger { get; private set; }

        private Dictionary<string, int> numBosses;
        private Dictionary<string, List<BossDescription>> customBosses;

        private GameObject h1SM = null;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override List<ValueTuple<string, string>> GetPreloadNames()
        {
            return new List<ValueTuple<string, string>>
            {
                new ValueTuple<string, string>("White_Palace_18", "White Palace Fly"),
                new ValueTuple<string, string>("GG_Hornet_1", "Boss Scene Controller"),
                new ValueTuple<string, string>("GG_Hornet_1", "_SceneManager")
            };
        }

        public BossModCore() : base("Hall of Custom Gods")
        {
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;

            initGlobalSettings();
            sceneChanger = new SceneChanger(preloadedObjects);
            numBosses = new Dictionary<string, int>();
            customBosses = new Dictionary<string, List<BossDescription>>();
            initCallbacks();

            h1SM = GameObject.Instantiate(preloadedObjects["GG_Hornet_1"]["_SceneManager"]);
            UnityEngine.Object.Destroy(h1SM.GetComponent<PlayMakerFSM>());
            MiscCreator.ResetSceneManagerAudio(h1SM.GetComponent<SceneManager>());
            h1SM.SetActive(false);
            GameObject.DontDestroyOnLoad(h1SM);

            //GameManager.instance.StartCoroutine(DEBUG_Shade_Style());

            Log("Initialized");
        }

        private void initGlobalSettings()
        {
            // Found in a project, might help saving, don't know, but who cares
            // Global Settings
        }

        private void initSaveSettings(SaveGameData data)
        {
            // Found in a project, might help saving, don't know, but who cares
            // Save Settings
        }

        private void initCallbacks()
        {
            // Hooks
            ModHooks.Instance.GetPlayerBoolHook += OnGetPlayerBoolHook;
            ModHooks.Instance.SetPlayerBoolHook += OnSetPlayerBoolHook;
            ModHooks.Instance.GetPlayerIntHook += OnGetPlayerIntHook;
            ModHooks.Instance.SetPlayerIntHook += OnSetPlayerIntHook;
            ModHooks.Instance.GetPlayerFloatHook += OnGetPlayerFloatHook;
            ModHooks.Instance.SetPlayerFloatHook += OnSetPlayerFloatHook;
            ModHooks.Instance.GetPlayerStringHook += OnGetPlayerStringHook;
            ModHooks.Instance.SetPlayerStringHook += OnSetPlayerStringHook;
            ModHooks.Instance.GetPlayerVector3Hook += OnGetPlayerVector3Hook;
            ModHooks.Instance.SetPlayerVector3Hook += OnSetPlayerVector3Hook;
            ModHooks.Instance.GetPlayerVariableHook += OnGetPlayerVariableHook;
            ModHooks.Instance.SetPlayerVariableHook += OnSetPlayerVariableHook;

            ModHooks.Instance.AfterSavegameLoadHook += initSaveSettings;
            ModHooks.Instance.ApplicationQuitHook += SaveTotGlobalSettings;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            string sceneName = to.name;
            if (sceneName == TransitionGateNames.godhome)
            {
                sceneChanger.CR_Change_GG_Atrium(to);
            }
            else if (sceneName == "GG_Workshop")
            {
                CreateStatue("GG_Hollow_Knight", new Vector3(-10.96f, 0f, 0f));
                CreateStatue("GG_Hornet_1", new Vector3(0f, 0f, 0f));
                CreateStatue("GG_Hornet_2", new Vector3(10.96f, 0f, 0f));
            }
            else if (sceneName == "CustomBossScene")
            {
                GameObject tmpSM = GameObject.Instantiate(h1SM);
                tmpSM.SetActive(false);
                MiscCreator.ResetSceneManagerAudio(h1SM.GetComponent<SceneManager>());
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(tmpSM, to);
                tmpSM.SetActive(true);

                //HeroController.instance.StartCoroutine(SetupScene(to, "GG_Hollow_Knight"));
            }
        }

        private IEnumerator SetupScene(Scene scene, string copySceneName)
        {
            Log("!SetupScene");

            //var scene = gameObject.scene;

            UnityEngine.SceneManagement.SceneManager.LoadScene(copySceneName, LoadSceneMode.Additive);
            yield return null;
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);

            var prefabScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(copySceneName);

            foreach (var go in prefabScene.GetRootGameObjects())
            {
                go.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            }

            GameObject bsc = scene.Find("Boss Scene Controller");

            var bscMB = bsc.GetComponent<BossSceneController>();
            bsc.SetActive(false);
            BossSceneController.Instance.transitionInHoldTime = 0;

            var dreamEntryControlFsm = bsc.FindGameObjectInChildren("Dream Entry").LocateMyFSM("Control");
            var dreamEntryControlFsmVars = dreamEntryControlFsm.FsmVariables;
            dreamEntryControlFsm.RemoveAction("Pause", 0);
            dreamEntryControlFsm.AddAction("Pause", new NextFrameEvent() { sendEvent = FsmEvent.Finished });

            bsc.SetActive(true);

            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(prefabScene);

            foreach (var go in scene.GetRootGameObjects())
            {
                go.SetActive(true);
            }

            GameObject.Find("Blanker White").LocateMyFSM("Blanker Control").SendEvent("FADE OUT");
            EventRegister.SendEvent("GG TRANSITION IN");
            BossSceneController.Instance.GetType().GetProperty("HasTransitionedIn").SetValue(BossSceneController.Instance, true, null);
            HeroController.instance.transform.position = bscMB.heroSpawn.position + new Vector3(0, 5, 0);

            Log("~SetupScene");
        }

        private void CreateStatue(string prefab, Vector3 offset)
        {
            //Used 56's pale prince code here
            GameObject statue = GameObject.Instantiate(GameObject.Find("GG_Statue_ElderHu"));
            statue.name = "GG_Statue_CagneyCarnation";
            statue.transform.SetPosition3D(41.0f + offset.x, statue.transform.GetPositionY() + 0.5f + offset.y, 0f + offset.z);

            var fsm = statue.FindGameObjectInChildren("Inspect").LocateMyFSM("GG Boss UI");
            var ccsn = new CustomBossCopy.ChangeCopySceneName();
            ccsn.sceneName = prefab;
            fsm.InsertAction("Change Scene", ccsn, 5);

            var scene = ScriptableObject.CreateInstance<BossScene>();
            scene.name = "CustomBossScene";
            scene.sceneName = "CustomBossScene";

            var bs = statue.GetComponent<BossStatue>();
            bs.bossScene = scene;
            bs.statueStatePD = "statueStateFlower";
            bs.SetPlaquesVisible(bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen);

            var details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = "FLOWER_NAME";
            details.descriptionKey = details.descriptionSheet = "FLOWER_DESC";
            bs.bossDetails = details;

            foreach (Transform i in statue.transform)
            {
                if (i.name.Contains("door"))
                {
                    i.name = "door_dreamReturnGG_GG_Statue_CagneyCarnation";
                }
            }

            GameObject appearance = statue.transform.Find("Base").Find("Statue").gameObject;
            appearance.SetActive(true);
            SpriteRenderer sr = appearance.transform.Find("GG_statues_0006_5").GetComponent<SpriteRenderer>();
            sr.enabled = true;
            sr.transform.SetPosition3D(sr.transform.GetPositionX(), sr.transform.GetPositionY(), 2f);

            GameObject inspect = statue.transform.Find("Inspect").gameObject;
            var tmp = inspect.transform.Find("Prompt Marker").position;
            inspect.transform.Find("Prompt Marker").position = new Vector3(tmp.x - 0.3f, tmp.y + 1.0f, tmp.z);
            inspect.SetActive(true);

            statue.transform.Find("Spotlight").gameObject.SetActive(true);

            statue.SetActive(true);
        }

        private void SaveTotGlobalSettings()
        {
            SaveGlobalSettings();
        }

        #region Get/Set Hooks

        public const string name = "BossModCore";

        class PlayerDataCommand
        {
            public enum Commands
            {
                NumBosses,
                StatueName,
                StatueDescription,
                CustomScene,
                ScenePrefabName,
                StatueGO
            }
            public string BossModCoreId = name;
            public string SubscriberClassName = "";
            public string Command = "";
            public int BossIndex = 0;

            public static PlayerDataCommand Parse(string pdValue)
            {
                string[] parts = pdValue.Split(new string[] { " - " }, StringSplitOptions.None);
                PlayerDataCommand cmd = new PlayerDataCommand();
                cmd.BossModCoreId = parts[0];
                if (cmd.BossModCoreId != name) return null;
                cmd.SubscriberClassName = parts[1];
                cmd.Command = parts[2];
                try
                {
                    var unused = Enum.Parse(typeof(Commands), cmd.Command);
                }
                catch (OverflowException e)
                {
                    return null;
                }
                if (parts.Length > 3)
                {
                    cmd.BossIndex = int.Parse(parts[3]);
                }
                return cmd;
            }
        }

        private bool OnGetPlayerBoolHook(string target)
        {
            if (target == name)
            {
                Log("Bool requested via \"" + target + "\"");
                return true;
            }

            bool ret;
            if (Settings.BoolValues.ContainsKey(target))
            {
                ret = Settings.BoolValues[target];
            }
            else
            {
                ret = PlayerData.instance.GetBoolInternal(target);
            }
            //Log("Bool get: " + target + "=" + ret.ToString());
            return ret;
        }
        private void OnSetPlayerBoolHook(string target, bool val)
        {
            if (target.StartsWith(name))
            {
                Log("Bool recieved via \"" + target + "\" => \"" + val + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);

                if (cmd.BossIndex >= numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return;
                }

                if (cmd.Command == "customScene")
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].customScene = val;
                }
            }
            else
            {
                if (Settings.BoolValues.ContainsKey(target))
                {
                    // Other Bools
                    Settings.BoolValues[target] = val;
                }
                else
                {
                    PlayerData.instance.SetBoolInternal(target, val);
                }
            }
            //Log("Bool set: " + target + "=" + val.ToString());
        }

        private int OnGetPlayerIntHook(string target)
        {
            if (target.StartsWith(name))
            {
                Log("Int requested via \"" + target + "\"");
            }

            int ret;
            if (Settings.IntValues.ContainsKey(target))
            {
                ret = Settings.IntValues[target];
            }
            else
            {
                ret = PlayerData.instance.GetIntInternal(target);
            }
            //Log("Int  get: " + target + "=" + ret.ToString());
            return ret;
        }
        private void OnSetPlayerIntHook(string target, int val)
        {
            if (target.StartsWith(name))
            {
                Log("Int recieved via \"" + target + "\" => \"" + val + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                if (cmd.Command != PlayerDataCommand.Commands.NumBosses.ToString())
                {
                    return;
                }
                numBosses.Add(cmd.SubscriberClassName, val);
                customBosses.Add(cmd.SubscriberClassName, new List<BossDescription>());
                for (int i = 0; i < val; i++)
                {
                    customBosses[cmd.SubscriberClassName].Add(new BossDescription());
                }
            }
            else
            {
                if (Settings.IntValues.ContainsKey(target))
                {
                    Settings.IntValues[target] = val;
                }
                else
                {
                    PlayerData.instance.SetIntInternal(target, val);
                }
            }
            //Log("Int  set: " + target + "=" + val.ToString());
        }

        private float OnGetPlayerFloatHook(string target)
        {
            if (target.StartsWith(name))
            {
                Log("Float requested via \"" + target + "\"");
            }

            return PlayerData.instance.GetFloatInternal(target);
        }
        private void OnSetPlayerFloatHook(string target, float val)
        {
            if (target.StartsWith(name))
            {
                Log("Float recieved via \"" + target + "\" => \"" + val + "\"");

                //string[] parts = target.Split(new string[] { " - " }, StringSplitOptions.None);
            }
            else
                PlayerData.instance.SetFloatInternal(target, val);
        }

        private string OnGetPlayerStringHook(string target)
        {
            if (target.StartsWith(name))
            {
                Log("String requested via \"" + target + "\"");
            }

            return PlayerData.instance.GetStringInternal(target);
        }
        private void OnSetPlayerStringHook(string target, string val)
        {
            if (target.StartsWith(name))
            {
                Log("String recieved via \"" + target + "\" => \"" + val + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                if (cmd.BossIndex >= numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return;
                }

                if (cmd.Command == PlayerDataCommand.Commands.StatueName.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueName = val;
                }
                else if (cmd.Command == PlayerDataCommand.Commands.StatueDescription.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueDescription = val;
                }
                else if (cmd.Command == PlayerDataCommand.Commands.ScenePrefabName.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].scenePrefabName = val;
                }
            }
            else
                PlayerData.instance.SetStringInternal(target, val);
        }

        private Vector3 OnGetPlayerVector3Hook(string target)
        {
            if (target.StartsWith(name))
            {
                Log("Vector3 requested via \"" + target + "\"");
            }

            return PlayerData.instance.GetVector3Internal(target);
        }
        private void OnSetPlayerVector3Hook(string target, Vector3 val)
        {
            if (target.StartsWith(name))
            {
                Log("Vector3 recieved via \"" + target + "\" => \"" + val + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
            }
            else
                PlayerData.instance.SetVector3Internal(target, val);
        }

        private object OnGetPlayerVariableHook(Type type, string target, object orig)
        {
            if (target.StartsWith(name))
            {
                Log(type.Name + " requested via \"" + target + "\"");
            }

            return orig;
        }
        private object OnSetPlayerVariableHook(Type type, string target, object val)
        {
            Log(type.Name + " recieved via \"" + target + "\" => \"" + val.ToString() + "\"");
            if (target.StartsWith(name))
            {
                //Log(type.Name + " recieved via \"" + target + "\" => \"" + val.ToString() + "\"");

                if (type.FullName != (new UnityEngine.GameObject()).GetType().FullName)
                {
                    Log("Wrong type!");
                    return val;
                }

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                string[] parts = target.Split(new string[] { " - " }, StringSplitOptions.None);
                int index = int.Parse(parts[3]);
                if (cmd.BossIndex >= numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return val;
                }

                if (cmd.Command == PlayerDataCommand.Commands.StatueGO.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueGO = (val as UGameObject);
                }
                return val;
            }
            return val;
        }
        #endregion

        private void printDebugFsm(PlayMakerFSM fsm)
        {
            foreach (var state in fsm.FsmStates)
            {
                Log("State: " + state.Name);
                foreach (var trans in state.Transitions)
                {
                    Log("\t" + trans.EventName + " -> " + trans.ToState);
                }
            }
        }

        private void printDebug(GameObject go, string tabindex = "")
        {
            Log(tabindex + "DEBUG Name: " + go.name);
            foreach (var comp in go.GetComponents<Component>())
            {
                Log(tabindex + "DEBUG Component: " + comp.GetType());
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                printDebug(go.transform.GetChild(i).gameObject, tabindex + "\t");
            }
        }
    }
}
