using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using Modding;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using BossModCore.MonoBehaviours;
using SFCore.Generics;
using UnityEngine.SceneManagement;
using UGameObject = UnityEngine.GameObject;
using SFCore.Utils;

namespace BossModCore
{
    public class BossModCore : FullSettingsMod<BmcSaveSettings, BmcGlobalSettings>
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
            h1SM.GetComponent<SceneManager>().enabled = false;
            GameObject.DontDestroyOnLoad(h1SM);

            //GameManager.instance.StartCoroutine(DEBUG_Shade_Style());

            BossSceneController.Instance = null;

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
            ModHooks.GetPlayerBoolHook += OnGetPlayerBoolHook;
            ModHooks.SetPlayerBoolHook += OnSetPlayerBoolHook;
            ModHooks.GetPlayerIntHook += OnGetPlayerVarHook<int>;
            ModHooks.SetPlayerIntHook += OnSetPlayerIntHook;
            ModHooks.GetPlayerFloatHook += OnGetPlayerVarHook<float>;
            ModHooks.SetPlayerFloatHook += OnSetPlayerFloatHook;
            ModHooks.GetPlayerStringHook += OnGetPlayerVarHook<string>;
            ModHooks.SetPlayerStringHook += OnSetPlayerStringHook;
            ModHooks.GetPlayerVector3Hook += OnGetPlayerVarHook<Vector3>;
            ModHooks.SetPlayerVector3Hook += OnSetPlayerVector3Hook;
            ModHooks.GetPlayerVariableHook += OnGetPlayerVariableHook;
            ModHooks.SetPlayerVariableHook += OnSetPlayerVariableHook;

            ModHooks.AfterSavegameLoadHook += initSaveSettings;
            ModHooks.ApplicationQuitHook += SaveTotGlobalSettings;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            string sceneName = to.name;
            if (sceneName == TransitionGateNames.godhome)
            {
                sceneChanger.Change_GG_Atrium(to);
            }
            else if (sceneName == "GG_Workshop")
            {
                CreateStatue("GG_Hollow_Knight", new Vector3(-10.96f, 0f, 0f));
                CreateStatue("GG_Hornet_1", new Vector3(0f, 0f, 0f));
                CreateStatue("GG_Hornet_2", new Vector3(10.96f, 0f, 0f));
            }
            else if (sceneName == "CustomBossScene")
            {
                h1SM.GetComponent<SceneManager>().enabled = true;
                GameObject tmpSM = GameObject.Instantiate(h1SM);
                h1SM.GetComponent<SceneManager>().enabled = false;
                tmpSM.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(tmpSM, to);
                tmpSM.SetActive(true);

                //HeroController.instance.StartCoroutine(SetupScene(to, "GG_Hollow_Knight"));
            }
        }

        // Shamelessly stole jngos code
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

        private bool HasGetSettingsValue<T>(string target)
        {
            var tmpField = ReflectionHelper.GetFieldInfo(typeof(BmcSaveSettings), target);
            return tmpField != null && tmpField.FieldType == typeof(T);
        }
        private T GetSettingsValue<T>(string target)
        {
            return ReflectionHelper.GetField<BmcSaveSettings, T>(target);
        }
        private void SetSettingsValue<T>(string target, T val)
        {
            ReflectionHelper.SetField<BmcSaveSettings, T>(target, val);
        }

        private T OnGetPlayerVarHook<T>(string target, T orig)
        {
            Log($"Requested '{typeof(T)}' via '{target}'!");
            if (HasGetSettingsValue<T>(target))
            {
                return GetSettingsValue<T>(target);
            }
            return orig;
        }

        private bool OnGetPlayerBoolHook(string target, bool orig)
        {
            if (target == name)
            {
                return true;
            }
            if (HasGetSettingsValue<bool>(target))
            {
                return GetSettingsValue<bool>(target);
            }
            return orig;
        }
        private bool OnSetPlayerBoolHook(string target, bool orig)
        {
            if (target.StartsWith(name))
            {
                Log("Bool recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);

                if (cmd.BossIndex >= numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return orig;
                }

                if (cmd.Command == "customScene")
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].customScene = orig;
                }
            }
            else
            {
                if (HasGetSettingsValue<bool>(target))
                {
                    SetSettingsValue<bool>(target, orig);
                }
            }
            return orig;
        }

        private int OnSetPlayerIntHook(string target, int orig)
        {
            if (target.StartsWith(name))
            {
                Log("Int recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                if (cmd.Command != PlayerDataCommand.Commands.NumBosses.ToString())
                {
                    return orig;
                }
                numBosses.Add(cmd.SubscriberClassName, orig);
                customBosses.Add(cmd.SubscriberClassName, new List<BossDescription>());
                for (int i = 0; i < orig; i++)
                {
                    customBosses[cmd.SubscriberClassName].Add(new BossDescription());
                }
            }
            else
            {
                if (HasGetSettingsValue<int>(target))
                {
                    SetSettingsValue<int>(target, orig);
                }
            }
            return orig;
        }

        private float OnSetPlayerFloatHook(string target, float orig)
        {
            if (target.StartsWith(name))
            {
                Log("Float recieved via \"" + target + "\" => \"" + orig + "\"");

                //string[] parts = target.Split(new string[] { " - " }, StringSplitOptions.None);
            }
            else
            {
                if (HasGetSettingsValue<float>(target))
                {
                    SetSettingsValue<float>(target, orig);
                }
            }
            return orig;
        }

        private string OnSetPlayerStringHook(string target, string orig)
        {
            if (target.StartsWith(name))
            {
                Log("String recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                if (cmd.BossIndex >= numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return orig;
                }

                if (cmd.Command == PlayerDataCommand.Commands.StatueName.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueName = orig;
                }
                else if (cmd.Command == PlayerDataCommand.Commands.StatueDescription.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueDescription = orig;
                }
                else if (cmd.Command == PlayerDataCommand.Commands.ScenePrefabName.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].scenePrefabName = orig;
                }
            }
            else
            {
                if (HasGetSettingsValue<string>(target))
                {
                    SetSettingsValue<string>(target, orig);
                }
            }
            return orig;
        }

        private Vector3 OnSetPlayerVector3Hook(string target, Vector3 orig)
        {
            if (target.StartsWith(name))
            {
                Log("Vector3 recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
            }
            else
            {
                if (HasGetSettingsValue<Vector3>(target))
                {
                    SetSettingsValue<Vector3>(target, orig);
                }
            }
            return orig;
        }

        private object OnGetPlayerVariableHook(Type type, string target, object orig)
        {
            if (target.StartsWith(name))
            {
                Log(type.Name + " requested via \"" + target + "\"");
            }

            var tmpField = ReflectionHelper.GetFieldInfo(typeof(BmcSaveSettings), target);
            if (tmpField != null && tmpField.FieldType == type)
            {
                return tmpField.GetValue(_saveSettings);
            }
            return orig;
        }
        private object OnSetPlayerVariableHook(Type type, string target, object orig)
        {
            Log(type.Name + " recieved via \"" + target + "\" => \"" + orig.ToString() + "\"");
            if (target.StartsWith(name))
            {
                //Log(type.Name + " recieved via \"" + target + "\" => \"" + val.ToString() + "\"");

                if (type.FullName != (new UnityEngine.GameObject()).GetType().FullName)
                {
                    Log("Wrong type!");
                    return returnOnSetPlayerVariableHook(type, target, orig);
                }

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                string[] parts = target.Split(new string[] { " - " }, StringSplitOptions.None);
                int index = int.Parse(parts[3]);
                if (cmd.BossIndex >= numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return returnOnSetPlayerVariableHook(type, target, orig);
                }

                if (cmd.Command == PlayerDataCommand.Commands.StatueGO.ToString())
                {
                    customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueGO = (UGameObject) orig;
                }
                return returnOnSetPlayerVariableHook(type, target, orig);
            }
            return returnOnSetPlayerVariableHook(type, target, orig);
        }
        private object returnOnSetPlayerVariableHook(Type type, string target, object orig)
        {
            var tmpField = ReflectionHelper.GetFieldInfo(typeof(BmcSaveSettings), target);
            if (tmpField != null && tmpField.FieldType == type)
            {
                tmpField.SetValue(_saveSettings, orig);
            }
            return orig;
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
