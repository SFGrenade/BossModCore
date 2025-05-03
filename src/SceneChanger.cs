using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Logger = Modding.Logger;
using UnityEngine.SceneManagement;
using SFCore.Utils;

namespace BossModCore;

internal enum EnviromentType
{
    DUST = 0,
    GRASS,
    BONE,
    SPA,
    METAL,
    NOEFFECT,
    WET
}

public class SceneChanger : MonoBehaviour
{
    private static bool _debug = true;
    private static string _abPath = "C:\\Users\\SFG\\Documents\\Projects\\Unity Projects\\CustomBossScene Assets\\Assets\\AssetBundles\\";

    public AssetBundle AbBmcScene { get; private set; }
    public AssetBundle AbBmcMat { get; private set; } = null;
    public Mesh GgAtriumNewmesh { get; private set; }

    private GameObject _wpFlyPrefab;

    public SceneChanger(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        On.GameManager.RefreshTilemapInfo += OnGameManagerRefreshTilemapInfo;

        _wpFlyPrefab = Instantiate(preloadedObjects["White_Palace_18"]["White Palace Fly"]);
        SetInactive(_wpFlyPrefab);

        Assembly asm = Assembly.GetExecutingAssembly();
        #region Load AssetBundles
        Log("Loading AssetBundles");
        if (AbBmcScene == null)
        {
            if (!_debug)
            {
                using (Stream s = asm.GetManifestResourceStream("BossModCore.Resources.custom_bosses_scene"))
                {
                    if (s != null)
                    {
                        AbBmcScene = AssetBundle.LoadFromStream(s);
                    }
                }
            }
            else
            {
                AbBmcScene = AssetBundle.LoadFromFile(_abPath + "custom_bosses_scene");
            }
        }
        //if (ab_bmc_mat == null)
        //{
        //    if (!debug)
        //    {
        //        using (Stream s = _asm.GetManifestResourceStream("BossModCore.Resources.custom_bosses_materials"))
        //        {
        //            if (s != null)
        //            {
        //                ab_bmc_mat = AssetBundle.LoadFromStream(s);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ab_bmc_mat = AssetBundle.LoadFromFile(abPath + "custom_bosses_materials");
        //    }
        //}
        Log("Finished loading AssetBundles");
        #endregion

        #region Load misc
        Log("Loading Meshes");

        GgAtriumNewmesh = ObjImporter.ImportFile(@"C:/Users/SFG/Documents/Hollow Knight Stuff\HollowKnightMods\BossModCore\Resources\GG_Atrium_NewMesh.obj");

        Log("Finished loading Meshes");
        #endregion
    }

    private static void OnGameManagerRefreshTilemapInfo(On.GameManager.orig_RefreshTilemapInfo orig, GameManager self, string targetScene)
    {
        orig(self, targetScene);
        if (targetScene == TransitionGateNames.CustomWorkshop)
        {
            self.tilemap.width = 192;
            self.tilemap.height = 64;
            self.sceneWidth = 192;
            self.sceneHeight = 64;
        }
    }

    public void Change_GG_Atrium(Scene scene)
    {
        if (scene.name != TransitionGateNames.Godhome)
            return;

        Log("Change_GG_Atrium()");

        CreateGateway(TransitionGateNames.GhCws, new Vector2(50.5f, 8), new Vector2(1, 4), TransitionGateNames.CustomWorkshop, TransitionGateNames.CwsGh,
            new Vector2(-3, 0), false, true, false, GameManager.SceneLoadVisualizations.Default);

        #region Edit GG_Atrium for new Transition
        GameObject sceneMap;
        GameObject chunk02 = null;
        foreach (GameObject rgo in scene.GetRootGameObjects())
        {
            if (rgo.name == "Template-TileMap (1) Render Data")
            {
                sceneMap = rgo.transform.Find("Scenemap").gameObject;

                chunk02 = sceneMap.transform.Find("Chunk 0 2").gameObject;
            }
        }
        #region Chunk 0 2

        if (chunk02 is not null)
        {
            EdgeCollider2D edgeCollider2D = chunk02.GetComponent<EdgeCollider2D>();
            //Log("Found " + edgeCollider2Ds.Length + " EdgeCollider2D(s)");
            Vector2[] changedPoints = new Vector2[23];
            changedPoints[0] =  new Vector2( 0,  0);
            changedPoints[1] =  new Vector2(32,  0);
            changedPoints[2] =  new Vector2(32, 13);
            changedPoints[3] =  new Vector2( 7, 13);
            changedPoints[4] =  new Vector2( 7, 21);
            changedPoints[5] =  new Vector2( 8, 21);
            changedPoints[6] =  new Vector2( 8, 22);
            changedPoints[7] =  new Vector2(16, 22);
            changedPoints[8] =  new Vector2(16, 21);
            changedPoints[9] =  new Vector2(17, 21);
            changedPoints[10] = new Vector2(17, 19);
            changedPoints[11] = new Vector2(21, 19);
            changedPoints[12] = new Vector2(21, 21);
            changedPoints[13] = new Vector2(22, 21);
            changedPoints[14] = new Vector2(22, 22);
            changedPoints[15] = new Vector2(30, 22);
            changedPoints[16] = new Vector2(30, 21);
            changedPoints[17] = new Vector2(31, 21);
            changedPoints[18] = new Vector2(31, 19);
            changedPoints[19] = new Vector2(32, 19);
            changedPoints[20] = new Vector2(32, 32);
            changedPoints[21] = new Vector2( 0, 32);
            changedPoints[22] = new Vector2( 0,  0);

            edgeCollider2D.points = changedPoints;
        }

        if (chunk02 is not null)
        {
            var meshFilter = chunk02.GetComponent<MeshFilter>();
            meshFilter.mesh = GgAtriumNewmesh;
        }

        #endregion
        #endregion

        #region Change CameraLockArea
        var claGo = scene.FindRoot("CameraLockArea (5)");
        claGo.GetComponent<BoxCollider2D>().offset = new Vector2(-6.237274f, 0f);
        claGo.GetComponent<BoxCollider2D>().size = new Vector2(43.03106f, 24.57448f);
        claGo.GetComponent<CameraLockArea>().cameraXMin = 76.34f;
        #endregion

        #region Make Door
        var doorGo = Instantiate(scene.FindRoot("Door_Workshop"), null, true);
        doorGo.name = "Door_CustomWorkshop";
        doorGo.transform.position = new Vector3(76f, 13.76f, 0.2f);

        var doorControlFsm = doorGo.LocateMyFSM("Door Control");
        var doorControlFsmVars = doorControlFsm.FsmVariables;
        doorControlFsmVars.FindFsmString("New Scene").Value = "GG_Workshop";

        var tp = doorGo.GetComponent<TransitionPoint>();
        tp.targetScene = "GG_Workshop";
        #endregion

        PrepareGgAtrium(scene);

        Log("Change_GG_Atrium Done");
    }

    private void PrepareGgAtrium(Scene scene)
    {
        Log("prepareGgAtrium()");

        #region Add Gameobjects for looks

        var tmpGo = scene.FindRoot("white_solid (36)");
        tmpGo.transform.position = new Vector3(82.31f, 16.68f, 24.09f);

        tmpGo = scene.FindRoot("side_pillar_left (1)");
        tmpGo.transform.position = new Vector3(80.78f, 16.41f, -1.3f);

        tmpGo = Instantiate(scene.FindRoot("gg_ascent_wall_0002_2"), null, true);
        tmpGo.transform.position = new Vector3(83f, 19.7f, -0.05f);

        tmpGo = Instantiate(scene.FindRoot("GG_big_door_part_small"), null, true);
        tmpGo.transform.position = new Vector3(75.7f, 15.68f, 8.13f);

        tmpGo = Instantiate(scene.FindRoot("Col_Glow_Remasker (1)"), null, true);
        tmpGo.transform.position = new Vector3(75.1f, 11.38f, 11.99f);

        tmpGo = Instantiate(scene.FindRoot("Col_Glow_Remasker (2)"), null, true);
        tmpGo.transform.position = new Vector3(77.33f, 9.96f, 18.99f);

        tmpGo = Instantiate(scene.FindRoot("white_solid (11)"), null, true);
        tmpGo.transform.position = new Vector3(75f, 14.38f, 24.09f);

        tmpGo = Instantiate(scene.FindRoot("GG_scene_arena_extra_0000_3 (21)"), null, true);
        tmpGo.transform.position = new Vector3(80.25f, 12.59f, -0.18f);

        tmpGo = Instantiate(scene.FindRoot("GG_scenery_0005_16 (61)"), null, true);
        tmpGo.transform.position = new Vector3(79.83999f, 14.25f, 19.541f);

        tmpGo = Instantiate(scene.FindRoot("GG_scenery_0005_16 (62)"), null, true);
        tmpGo.transform.position = new Vector3(74.26999f, 13.86f, 20.32f);

        tmpGo = Instantiate(scene.FindRoot("GG_scenery_0005_16 (63)"), null, true);
        tmpGo.transform.position = new Vector3(72.47999f, 14.46f, 16.05f);

        tmpGo = Instantiate(scene.FindRoot("GG_scenery_0005_16 (71)"), null, true);
        tmpGo.transform.position = new Vector3(79.22f, 17.84f, 14.49f);

        tmpGo = Instantiate(scene.FindRoot("GG_scenery_0005_16 (72)"), null, true);
        tmpGo.transform.position = new Vector3(72.22f, 14.02f, 12.41f);

        tmpGo = Instantiate(scene.FindRoot("GG_scenery_0013_8 (15)"), null, true);
        tmpGo.transform.position = new Vector3(75.8f, 12.49f, -0.18f);
        tmpGo.transform.localScale = new Vector3(0.66f, 1f, 1f);

        tmpGo = Instantiate(scene.FindRoot("GG_scenery_0013_8 (15)"), null, true);
        tmpGo.transform.position = new Vector3(83f, 12.49f, -0.18f);
        tmpGo.transform.localScale = new Vector3(0.4f, 1f, 1f);

        tmpGo = Instantiate(scene.FindRoot("white_solid (35)"), null, true);
        tmpGo.transform.position = new Vector3(81.5f, 19.33f, 8.32f);
        tmpGo.transform.eulerAngles = new Vector3(0f, 0f, -170f);
        tmpGo.transform.localScale = new Vector3(5.113138f, 10f, 8.43629f);

        tmpGo = Instantiate(scene.FindRoot("white_solid (35)"), null, true);
        tmpGo.transform.position = new Vector3(70.2f, 19.01f, 8.32f);
        tmpGo.transform.eulerAngles = new Vector3(0f, 0f, 170f);
        tmpGo.transform.localScale = new Vector3(5.113138f, 10f, 8.43629f);

        tmpGo = Instantiate(scene.FindRoot("white_solid (35)"), null, true);
        tmpGo.transform.position = new Vector3(75.65f, 23.41f, 8.32f);
        tmpGo.transform.eulerAngles = new Vector3(0f, 0f, 90f);

        var tmpParent = scene.FindRoot("gg_arch");
        tmpGo = Instantiate(tmpParent.FindGameObjectInChildren("GG_scenery_0000_2 (6)"), null, true);
        tmpGo.transform.position = new Vector3(75.924f, 21.55699f, -0.3399999f);
        tmpGo.transform.localScale = new Vector3(0.9875f, 1f, 1f);

        tmpGo = Instantiate(tmpParent.FindGameObjectInChildren("GG_scenery_0001_1 (5)"), null, true);
        tmpGo.transform.position = new Vector3(78.76799f, 21.05f, -0.04999995f);
        tmpGo.transform.localScale = new Vector3(-1.13381f, 1.0395f, 1.08f);

        tmpGo = Instantiate(tmpParent.FindGameObjectInChildren("GG_scenery_0001_1 (6)"), null, true);
        tmpGo.transform.position = new Vector3(73.15099f, 21.05f, -0.04999995f);
        tmpGo.transform.localScale = new Vector3(1.13382f, 1.0395f, 1.08f);

        tmpGo = Instantiate(tmpParent.FindGameObjectInChildren("GG_scenery_0000_2 (7)"), null, true);
        tmpGo.transform.position = new Vector3(81.3f, 19.82f, -0.3399999f);
        tmpGo.transform.localScale = new Vector3(0.9875f, 1f, 1f);

        tmpGo = Instantiate(tmpParent.FindGameObjectInChildren("GG_scenery_0001_1 (7)"), null, true);
        tmpGo.transform.position = new Vector3(72.50999f, 20.13f, 6.945433f);
        tmpGo.transform.eulerAngles = new Vector3(0f, 0f, 9.04f);
        tmpGo.transform.localScale = new Vector3(1.077833f, 1.0395f, 1.08f);

        tmpGo = Instantiate(tmpParent.FindGameObjectInChildren("GG_scenery_0000_2 (7)"), null, true);
        tmpGo.transform.position = new Vector3(84.61f, 19.82f, -0.3399999f);
        tmpGo.transform.localScale = new Vector3(0.9875f, 1, 1f);

        var tmpParent2 = scene.FindRoot("GG_gold_wall_side (18)");
        tmpGo = Instantiate(tmpParent2.FindGameObjectInChildren("GG_gold_wall"), null, true);
        tmpGo.transform.position = new Vector3(84.44885f, 18.8948f, 4.9089f);
        tmpGo.transform.localScale = new Vector3(1.285819f, 1.1124f, 1.1124f);

        tmpGo = Instantiate(tmpParent2.FindGameObjectInChildren("GG_gold_wall (1)"), null, true);
        tmpGo.transform.position = new Vector3(84.44885f, 16.052f, 4.847583f);
        tmpGo.transform.localScale = new Vector3(1.285819f, 1.1124f, 1.1124f);

        tmpGo = Instantiate(tmpParent2.FindGameObjectInChildren("GG_gold_wall (2)"), null, true);
        tmpGo.transform.position = new Vector3(84.44885f, 12.9929f, 4.775483f);
        tmpGo.transform.localScale = new Vector3(1.285819f, 1.1124f, 1.1124f);

        tmpGo = Instantiate(tmpParent2.FindGameObjectInChildren("GG_gold_wall (3)"), null, true);
        tmpGo.transform.position = new Vector3(81.22186f, 13.2092f, 4.775483f);
        tmpGo.transform.localScale = new Vector3(-1.380095f, 1.1124f, 1.1124f);

        tmpGo = Instantiate(tmpParent2.FindGameObjectInChildren("GG_gold_wall (4)"), null, true);
        tmpGo.transform.position = new Vector3(81.22186f, 16.2683f, 4.868183f);
        tmpGo.transform.localScale = new Vector3(-1.380095f, 1.1124f, 1.1124f);

        tmpGo = Instantiate(tmpParent2.FindGameObjectInChildren("GG_gold_wall (5)"), null, true);
        tmpGo.transform.position = new Vector3(81.22186f, 19.1111f, 4.9707f);
        tmpGo.transform.localScale = new Vector3(-1.380095f, 1.1124f, 1.1124f);

        var tmpParent3 = scene.FindRoot("gg_bush_gold (1)");
        tmpGo = Instantiate(tmpParent3.FindGameObjectInChildren("gg_bush_01_0001_1 (176)"), null, true);
        tmpGo.transform.position = new Vector3(77.81135f, 22.6003f, -3.05776f);

        tmpGo = Instantiate(tmpParent3.FindGameObjectInChildren("gg_bush_01_0001_1 (177)"), null, true);
        tmpGo.transform.position = new Vector3(70.70876f, 20.95042f, -3.194228f);

        tmpGo = Instantiate(tmpParent3.FindGameObjectInChildren("gg_bush_01_0001_1 (178)"), null, true);
        tmpGo.transform.position = new Vector3(71.36681f, 12.54226f, -6.781381f);

        tmpGo = Instantiate(tmpParent3.FindGameObjectInChildren("gg_bush_01_0001_1 (208)"), null, true);
        tmpGo.transform.position = new Vector3(73.74979f, 21.5849f, 6.231797f);

        tmpGo = Instantiate(tmpParent3.FindGameObjectInChildren("gg_bush_01_0001_1 (209)"), null, true);
        tmpGo.transform.position = new Vector3(79.22862f, 20.45591f, 6.709435f);

        tmpGo = Instantiate(tmpParent3.FindGameObjectInChildren("gg_bush_01_0001_1 (210)"), null, true);
        tmpGo.transform.position = new Vector3(81.13562f, 17.99367f, 6.050489f);

        var tmpParent4 = scene.FindRoot("fg_stones (4)");
        tmpGo = Instantiate(tmpParent4.FindGameObjectInChildren("GG_scenery_0016_5 (19)"), null, true);
        tmpGo.transform.position = new Vector3(80.33371f, 12.34181f, -4.88848f);

        tmpGo = Instantiate(tmpParent4.FindGameObjectInChildren("GG_scenery_0016_5 (20)"), null, true);
        tmpGo.transform.position = new Vector3(72.35446f, 12.66539f, -4.115045f);

        tmpGo = Instantiate(tmpParent4.FindGameObjectInChildren("GG_scenery_0016_5 (21)"), null, true);
        tmpGo.transform.position = new Vector3(75.70625f, 12.21284f, -5.044881f);
        #endregion

        Log("prepareGgAtrium Done");
    }

    private void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate, Vector2 respawnPoint,
        bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
    {
        Log("!Gateway");

        GameObject gate = new GameObject(gateName);
        gate.transform.SetPosition2D(pos);
        var tp = gate.AddComponent<TransitionPoint>();
        if (!onlyOut)
        {
            var bc = gate.AddComponent<BoxCollider2D>();
            bc.size = size;
            bc.isTrigger = true;
            tp.SetTargetScene(toScene);
            tp.entryPoint = entryGate;
        }
        tp.alwaysEnterLeft = left;
        tp.alwaysEnterRight = right;

        GameObject rm = new GameObject("Hazard Respawn Marker");
        rm.transform.parent = gate.transform;
        rm.transform.SetPosition2D(pos.x + respawnPoint.x, pos.y + respawnPoint.y);
        var tmp = rm.AddComponent<HazardRespawnMarker>();
        tmp.respawnFacingRight = right;
        tp.respawnMarker = rm.GetComponent<HazardRespawnMarker>();
        tp.sceneLoadVisualization = vis;

        Log("~Gateway");
    }

    private static void Log(string message)
    {
        Logger.Log("[BossModCore]:[SceneChanger] - " + message);
    }
    private static void Log(object message)
    {
        Log(message.ToString());
    }

    private static void SetInactive(GameObject go)
    {
        if (go == null) return;
        DontDestroyOnLoad(go);
        go.SetActive(false);
    }
    private static void SetInactive(UnityEngine.Object go)
    {
        if (go != null)
        {
            DontDestroyOnLoad(go);
        }
    }
}