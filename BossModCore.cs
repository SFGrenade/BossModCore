using System;
using System.Reflection;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using BossModCore.MonoBehaviours;
using SFCore.Generics;
using UGameObject = UnityEngine.GameObject;
using SFCore.Utils;
using Object = UnityEngine.Object;

namespace BossModCore
{
    public class BossModCore : FullSettingsMod<BmcSaveSettings, BmcGlobalSettings>
    {
        internal static BossModCore Instance;

        public SceneChanger SceneChanger { get; private set; }

        private Dictionary<string, int> _numBosses;
        private Dictionary<string, List<BossDescription>> _customBosses;

        private GameObject _h1Sm;

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

            InitGlobalSettings();
            SceneChanger = new SceneChanger(preloadedObjects);
            _numBosses = new Dictionary<string, int>();
            _customBosses = new Dictionary<string, List<BossDescription>>();
            InitCallbacks();

            _h1Sm = Object.Instantiate(preloadedObjects["GG_Hornet_1"]["_SceneManager"]);
            Object.Destroy(_h1Sm.GetComponent<PlayMakerFSM>());
            MiscCreator.ResetSceneManagerAudio(_h1Sm.GetComponent<SceneManager>());
            _h1Sm.SetActive(false);
            _h1Sm.GetComponent<SceneManager>().enabled = false;
            Object.DontDestroyOnLoad(_h1Sm);

            //GameManager.instance.StartCoroutine(DEBUG_Shade_Style());

            BossSceneController.Instance = null;

            Log("Initialized");
        }

        private void InitGlobalSettings()
        {
            // Found in a project, might help saving, don't know, but who cares
            // Global Settings
        }

        private void InitSaveSettings(SaveGameData data)
        {
            // Found in a project, might help saving, don't know, but who cares
            // Save Settings
        }

        private void InitCallbacks()
        {
            // Hooks
            ModHooks.GetPlayerBoolHook += OnGetPlayerBoolHook;
            ModHooks.SetPlayerBoolHook += OnSetPlayerBoolHook;
            ModHooks.GetPlayerIntHook += OnGetPlayerVarHook;
            ModHooks.SetPlayerIntHook += OnSetPlayerIntHook;
            ModHooks.GetPlayerFloatHook += OnGetPlayerVarHook;
            ModHooks.SetPlayerFloatHook += OnSetPlayerFloatHook;
            ModHooks.GetPlayerStringHook += OnGetPlayerVarHook;
            ModHooks.SetPlayerStringHook += OnSetPlayerStringHook;
            ModHooks.GetPlayerVector3Hook += OnGetPlayerVarHook;
            ModHooks.SetPlayerVector3Hook += OnSetPlayerVector3Hook;
            ModHooks.GetPlayerVariableHook += OnGetPlayerVariableHook;
            ModHooks.SetPlayerVariableHook += OnSetPlayerVariableHook;

            ModHooks.AfterSavegameLoadHook += InitSaveSettings;
            ModHooks.ApplicationQuitHook += SaveTotGlobalSettings;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            string sceneName = to.name;
            if (sceneName == TransitionGateNames.Godhome)
            {
                SceneChanger.Change_GG_Atrium(to);
            }
            else if (sceneName == "GG_Workshop")
            {
                CreateStatue("GG_Hollow_Knight", new Vector3(-10.96f, 0f, 0f));
                CreateStatue("GG_Hornet_1", new Vector3(0f, 0f, 0f));
                CreateStatue("GG_Hornet_2", new Vector3(10.96f, 0f, 0f));
            }
            else if (sceneName == "CustomBossScene")
            {
                _h1Sm.GetComponent<SceneManager>().enabled = true;
                GameObject tmpSm = Object.Instantiate(_h1Sm);
                _h1Sm.GetComponent<SceneManager>().enabled = false;
                tmpSm.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(tmpSm, to);
                tmpSm.SetActive(true);

                //HeroController.instance.StartCoroutine(SetupScene(to, "GG_Hollow_Knight"));
            }
        }

        // Shamelessly stole jngos code
        private void CreateStatue(string prefab, Vector3 offset)
        {
            //Used 56's pale prince code here
            GameObject statue = Object.Instantiate(GameObject.Find("GG_Statue_ElderHu"));
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

        public const string CommandName = "BossModCore";

        class PlayerDataCommand
        {
            public enum Commands
            {
                NumBosses,
                StatueName,
                StatueDescription,
                CustomScene,
                ScenePrefabName,
                STATUE_GO
            }
            public string BossModCoreId = CommandName;
            public string SubscriberClassName = "";
            public string Command = "";
            public int BossIndex;

            public static PlayerDataCommand Parse(string pdValue)
            {
                string[] parts = pdValue.Split(new[] { " - " }, StringSplitOptions.None);
                PlayerDataCommand cmd = new PlayerDataCommand();
                cmd.BossModCoreId = parts[0];
                if (cmd.BossModCoreId != CommandName) return null;
                cmd.SubscriberClassName = parts[1];
                cmd.Command = parts[2];
                try
                {
                    var unused = Enum.Parse(typeof(Commands), cmd.Command);
                }
                catch (OverflowException)
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
            if (target == CommandName)
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
            if (target.StartsWith(CommandName))
            {
                Log("Bool recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);

                if (cmd.BossIndex >= _numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return orig;
                }

                if (cmd.Command == "customScene")
                {
                    _customBosses[cmd.SubscriberClassName][cmd.BossIndex].customScene = orig;
                }
            }
            else
            {
                if (HasGetSettingsValue<bool>(target))
                {
                    SetSettingsValue(target, orig);
                }
            }
            return orig;
        }

        private int OnSetPlayerIntHook(string target, int orig)
        {
            if (target.StartsWith(CommandName))
            {
                Log("Int recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                if (cmd.Command != PlayerDataCommand.Commands.NumBosses.ToString())
                {
                    return orig;
                }
                _numBosses.Add(cmd.SubscriberClassName, orig);
                _customBosses.Add(cmd.SubscriberClassName, new List<BossDescription>());
                for (int i = 0; i < orig; i++)
                {
                    _customBosses[cmd.SubscriberClassName].Add(new BossDescription());
                }
            }
            else
            {
                if (HasGetSettingsValue<int>(target))
                {
                    SetSettingsValue(target, orig);
                }
            }
            return orig;
        }

        private float OnSetPlayerFloatHook(string target, float orig)
        {
            if (target.StartsWith(CommandName))
            {
                Log("Float recieved via \"" + target + "\" => \"" + orig + "\"");

                //string[] parts = target.Split(new string[] { " - " }, StringSplitOptions.None);
            }
            else
            {
                if (HasGetSettingsValue<float>(target))
                {
                    SetSettingsValue(target, orig);
                }
            }
            return orig;
        }

        private string OnSetPlayerStringHook(string target, string orig)
        {
            if (target.StartsWith(CommandName))
            {
                Log("String recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                if (cmd.BossIndex >= _numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return orig;
                }

                if (cmd.Command == PlayerDataCommand.Commands.StatueName.ToString())
                {
                    _customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueName = orig;
                }
                else if (cmd.Command == PlayerDataCommand.Commands.StatueDescription.ToString())
                {
                    _customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueDescription = orig;
                }
                else if (cmd.Command == PlayerDataCommand.Commands.ScenePrefabName.ToString())
                {
                    _customBosses[cmd.SubscriberClassName][cmd.BossIndex].scenePrefabName = orig;
                }
            }
            else
            {
                if (HasGetSettingsValue<string>(target))
                {
                    SetSettingsValue(target, orig);
                }
            }
            return orig;
        }

        private Vector3 OnSetPlayerVector3Hook(string target, Vector3 orig)
        {
            if (target.StartsWith(CommandName))
            {
                Log("Vector3 recieved via \"" + target + "\" => \"" + orig + "\"");

                PlayerDataCommand.Parse(target);
            }
            else
            {
                if (HasGetSettingsValue<Vector3>(target))
                {
                    SetSettingsValue(target, orig);
                }
            }
            return orig;
        }

        private object OnGetPlayerVariableHook(Type type, string target, object orig)
        {
            if (target.StartsWith(CommandName))
            {
                Log(type.Name + " requested via \"" + target + "\"");
            }

            var tmpField = ReflectionHelper.GetFieldInfo(typeof(BmcSaveSettings), target);
            if (tmpField != null && tmpField.FieldType == type)
            {
                return tmpField.GetValue(SaveSettings);
            }
            return orig;
        }
        private object OnSetPlayerVariableHook(Type type, string target, object orig)
        {
            Log(type.Name + " recieved via \"" + target + "\" => \"" + orig + "\"");
            if (target.StartsWith(CommandName))
            {
                //Log(type.CommandName + " recieved via \"" + target + "\" => \"" + val.ToString() + "\"");

                if (type.FullName != (new GameObject()).GetType().FullName)
                {
                    Log("Wrong type!");
                    return returnOnSetPlayerVariableHook(type, target, orig);
                }

                PlayerDataCommand cmd = PlayerDataCommand.Parse(target);
                if (cmd.BossIndex >= _numBosses[cmd.SubscriberClassName])
                {
                    Log("Index too large!");
                    return returnOnSetPlayerVariableHook(type, target, orig);
                }

                if (cmd.Command == PlayerDataCommand.Commands.STATUE_GO.ToString())
                {
                    _customBosses[cmd.SubscriberClassName][cmd.BossIndex].statueGo = (UGameObject) orig;
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
                tmpField.SetValue(SaveSettings, orig);
            }
            return orig;
        }
        #endregion
    }
}
