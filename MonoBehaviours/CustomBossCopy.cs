using System;
using System.Collections;
using BossModCore.Utils;
using HutongGames.PlayMaker;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;

namespace BossModCore.MonoBehaviours
{
    public class CustomBossCopy : MonoBehaviour
    {
        public class ChangeCopySceneName : FsmStateAction
        {
            public FsmString sceneName;

            public override void Reset()
            {
                sceneName = string.Empty;

                base.Reset();
            }

            public override void OnEnter()
            {
                CustomBossCopy.copySceneName = sceneName.Value;

                Finish();
            }
        }

        public static string copySceneName = "";
        private PlayMakerFSM[] blankerControls = null;

        public CustomBossCopy()
        {
            StartCoroutine(SetupScene());
            blankerControls = new PlayMakerFSM[6];
            blankerControls[0] = GameObject.Find("2dtk Blanker").LocateMyFSM("Blanker Control");
            blankerControls[1] = GameObject.Find("Blanker White").LocateMyFSM("Blanker Control");
            blankerControls[2] = GameObject.Find("Blanker").LocateMyFSM("Blanker Control");
            blankerControls[3] = GameObject.Find("Cutscene Blanker").LocateMyFSM("Blanker Control");
            blankerControls[4] = GameObject.Find("Quit Blanker").LocateMyFSM("Blanker Control");
            blankerControls[5] = GameObject.Find("Start Blanker").LocateMyFSM("Blanker Control");
        }

        private IEnumerator SetupScene()
        {
            Log("!SetupScene");

            var scene = gameObject.scene;

            UnityEngine.SceneManagement.SceneManager.LoadScene(copySceneName, LoadSceneMode.Additive);
            yield return null;
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);

            var prefabScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(copySceneName);

            foreach (var go in prefabScene.GetRootGameObjects())
            {
                go.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            }
            //yield return null;

            GameObject bsc = scene.Find("Boss Scene Controller");

            //var bscMB = bsc.GetComponent<BossSceneController>();
            //bscMB.heroSpawn.position = new Vector3(16, 5);
            //bscMB.bosses = new HealthManager[0];
            //bscMB.doTransitionOut = false;

            var dreamEntryControlFsm = bsc.FindGameObjectInChildren("Dream Entry").LocateMyFSM("Control");
            Log($"dreamEntryControlFsm: {dreamEntryControlFsm}");
            dreamEntryControlFsm.RemoveAction("Pause", 0);
            dreamEntryControlFsm.SetState("Door Entry");
            bsc.SetActive(true);
            //yield return null;

            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(prefabScene);
            //yield return null;

            foreach (var go in scene.GetRootGameObjects())
            {
                go.SetActive(true);
            }

            GameObject.Find("Blanker White").LocateMyFSM("Blanker Control").SendEvent("FADE OUT");

            Log("~SetupScene");
        }

        public void Start()
        {
        }

        private new void Log(string message)
        {
            Logger.Log($"[{GetType().FullName?.Replace(".", "]:[")}] - {message}");
        }

        private void Log(object message)
        {
            Logger.Log($"[{GetType().FullName?.Replace(".", "]:[")}] - {message}");
        }
    }
}
