using System.Collections;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using SFCore.Utils;

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
                copySceneName = sceneName.Value;

                Finish();
            }
        }

        public static string copySceneName = "";

        public IEnumerator Start()
        {
            Log("!ctor");

            yield return SetupScene();

            Log("~ctor");
        }

        private IEnumerator SetupScene()
        {
            Log("!SetupScene");

            var scene = gameObject.scene;

            Log(1);

            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(copySceneName, LoadSceneMode.Additive);
            
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);

            Log(2);

            var prefabScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(copySceneName);

            Log(3);

            foreach (var go in prefabScene.GetRootGameObjects())
            {
                go.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            }

            Log(4);

            GameObject bsc = scene.Find("Boss Scene Controller");

            Log(5);

            var bscMb = bsc.GetComponent<BossSceneController>();
            bsc.SetActive(false);
            BossSceneController.Instance.transitionInHoldTime = 0;

            Log(6);

            var dreamEntryControlFsm = bsc.FindGameObjectInChildren("Dream Entry").LocateMyFSM("Control");
            dreamEntryControlFsm.RemoveAction("Pause", 0);
            dreamEntryControlFsm.AddAction("Pause", new NextFrameEvent() { sendEvent = FsmEvent.Finished });

            Log(7);

            bsc.SetActive(true);

            Log(8);

            StartCoroutine(UnloadScene(prefabScene));

            Log(9);

            foreach (var go in scene.GetRootGameObjects())
            {
                go.SetActive(true);
            }

            Log(10);

            GameObject.Find("Blanker White").LocateMyFSM("Blanker Control").SendEvent("FADE OUT");
            EventRegister.SendEvent("GG TRANSITION IN");
            BossSceneController.Instance.GetType().GetProperty("HasTransitionedIn")?.SetValue(BossSceneController.Instance, true, null);
            HeroController.instance.transform.position = bscMb.heroSpawn.GetComponent<TransitionPoint>().respawnMarker.transform.position;

            Log("~SetupScene");
        }

        private void EnterLevel()
        {

        }

        private static IEnumerator UnloadScene(Scene scene)
        {
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }

        private void Log(string message)
        {
            Logger.Log($"[{GetType().FullName?.Replace(".", "]:[")}] - {message}");
        }

        private void Log(object message)
        {
            Logger.Log($"[{GetType().FullName?.Replace(".", "]:[")}] - {message}");
        }
    }
}
