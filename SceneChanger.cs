using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using GlobalEnums;
using On;
using Logger = Modding.Logger;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using ModCommon.Util;
using HutongGames.PlayMaker.Actions;

namespace BossModCore
{

    enum EnviromentType
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
        private static bool debug = false;
        private static string abPath = "E:\\Github_Projects\\DreamKing Assets\\Assets\\AssetBundles\\";

        public AssetBundle ab_bmc_scene { get; private set; } = null;
        public AssetBundle ab_bmc_mat { get; private set; } = null;
        public Mesh gg_atrium_newmesh { get; private set; } = null;

        private GameObject wpFlyPrefab;

        /*
    new ValueTuple<string, string>("White_Palace_18", "White Palace Fly")
        */

        public SceneChanger(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            On.GameManager.RefreshTilemapInfo += OnGameManagerRefreshTilemapInfo;

            wpFlyPrefab = GameObject.Instantiate(preloadedObjects["White_Palace_18"]["White Palace Fly"]);
            SetInactive(wpFlyPrefab);

            Assembly _asm = Assembly.GetExecutingAssembly();
            #region Load AssetBundles
            //if (debug)
            //    Log("Loading AssetBundles");
            //if (ab_bmc_scene == null)
            //{
            //    if (!debug)
            //    {
            //        using (Stream s = _asm.GetManifestResourceStream("BossModCore.Resources.bossmodcore_scenes"))
            //        {
            //            if (s != null)
            //            {
            //                ab_bmc_scene = AssetBundle.LoadFromStream(s);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        ab_bmc_scene = AssetBundle.LoadFromFile(abPath + "bossmodcore_scenes");
            //    }
            //}
            //if (ab_bmc_mat == null)
            //{
            //    if (!debug)
            //    {
            //        using (Stream s = _asm.GetManifestResourceStream("BossModCore.Resources.bossmodcore_materials"))
            //        {
            //            if (s != null)
            //            {
            //                ab_bmc_mat = AssetBundle.LoadFromStream(s);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        ab_bmc_mat = AssetBundle.LoadFromFile(abPath + "bossmodcore_materials");
            //    }
            //}
            //if (debug)
            //    Log("Finished loading AssetBundles");
            #endregion

            #region Load misc
            if (debug)
                Log("Loading Meshes");
            
            gg_atrium_newmesh = ObjImporter.ImportFile(@"E:\Github_Projects\HollowKnightMods\BossModCore\Resources\GG_Atrium_NewMesh.obj");
            
            if (debug)
                Log("Finished loading Meshes");
            #endregion
        }

        private void OnGameManagerRefreshTilemapInfo(On.GameManager.orig_RefreshTilemapInfo orig, GameManager self, string targetScene)
        {
            orig(self, targetScene);
            if (targetScene == TransitionGateNames.customWorkshop)
            {
                self.tilemap.width = 192;
                self.tilemap.height = 64;
                self.sceneWidth = 192;
                self.sceneHeight = 64;
            }
        }

        public void CR_Change_GG_Atrium(Scene scene)
        {
            if (scene.name != TransitionGateNames.godhome)
                return;

            if (debug)
                Log("CR_Change_GG_Atrium()");

            CreateGateway(TransitionGateNames.gh_cws, new Vector2(50.5f, 8), new Vector2(1, 4), TransitionGateNames.customWorkshop, TransitionGateNames.cws_gh,
                          new Vector2(-3, 0), false, true, false, GameManager.SceneLoadVisualizations.Default);

            #region Edit GG_Atrium for new Transition
            GameObject sceneMap = null;
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

            MeshFilter meshFilter = chunk02.GetComponent<MeshFilter>();
            meshFilter.mesh = gg_atrium_newmesh;
            #endregion
            #endregion

            if (debug)
                Log("CR_Change_GG_Atrium Done");
        }

        private void PatchMisc(Scene scene)
        {
            if (debug)
                Log("!Misc");
            #region Area Title Controller
            //GameObject tmpPMU2D = GameObject.Instantiate(popPmU2dPrefab, scene.GetRootGameObjects()[6].transform);
            //tmpPMU2D.SetActive(true);
            //tmpPMU2D.name = "PlayMaker Unity 2D";
            //if (scene.name == TransitionGateNames.tot01)
            //{
            //    GameObject atc = GameObject.Instantiate(popAreaTitleCtrlPrefab);
            //    atc.SetActive(true);
            //    atc.transform.localPosition = Vector3.zero;
            //    atc.transform.localEulerAngles = Vector3.zero;
            //    atc.transform.localScale = Vector3.one;

            //    PlayMakerFSM atcFsm = atc.GetComponent<PlayMakerFSM>();
            //    atcFsm.FsmVariables.GetFsmFloat("Unvisited Pause").Value = 3f;
            //    atcFsm.FsmVariables.GetFsmFloat("Visited Pause").Value = 3f;

            //    atcFsm.FsmVariables.GetFsmBool("Always Visited").Value = false;
            //    atcFsm.FsmVariables.GetFsmBool("Display Right").Value = false;
            //    atcFsm.FsmVariables.GetFsmBool("Only On Revisit").Value = false;
            //    atcFsm.FsmVariables.GetFsmBool("Sub Area").Value = true;
            //    atcFsm.FsmVariables.GetFsmBool("Visited Area").Value = BossModCore.Instance.Settings.SFGrenadeBossModCore_VisitedBossModCore;
            //    atcFsm.FsmVariables.GetFsmBool("Wait for Trigger").Value = false;

            //    atcFsm.FsmVariables.GetFsmString("Area Event").Value = LanguageStrings.TotAreaTitle_Event;
            //    atcFsm.FsmVariables.GetFsmString("Visited Bool").Value = nameof(BossModCore.Instance.Settings.SFGrenadeBossModCore_VisitedBossModCore);

            //    atcFsm.FsmVariables.GetFsmGameObject("Area Title").Value = GameObject.Find("Area Title");

            //    atcFsm.SendEvent("DISPLAY");

            //    atc.AddComponent<NonBouncer>();

            //    BossModCore.Instance.Settings.SFGrenadeBossModCore_VisitedBossModCore = true;
            //}
            #endregion

            #region Scene Manager
            //if (scene.name == TransitionGateNames.tot01)
            //{
            //    GameObject tmp = GameObject.Instantiate(popSceneManagerPrefab);
            //    tmp.name = "_SceneManager";
            //    tmp.SetActive(true);
            //}
            #endregion
            if (debug)
                Log("~Misc");
        }

        private IEnumerator PatchBlurPlane(Scene scene)
        {
            yield return null;
            if (debug)
                Log("!BlurPlane");

            GameObject plane = GameObject.Find("BlurPlane");
            //plane.GetComponent<MeshRenderer>().material = new Material(Shader.Find("UI/Blur/UIBlur"));
            var bp = plane.AddComponent<BlurPlane>();
            var mr = bp.gameObject.GetComponent<MeshRenderer>();
            Material[] tmp = new Material[1];
            var tmpShader = Shader.Find("UI/Blur/UIBlur");
            tmp[0] = new Material(tmpShader);
            tmp[0].SetColor(Shader.PropertyToID("_TintColor"), new Color(1.0f, 1.0f, 1.0f, 0.2f));
            tmp[0].SetFloat(Shader.PropertyToID("_Size"), 53.7f);
            tmp[0].SetFloat(Shader.PropertyToID("_Vibrancy"), 0.2f);
            tmp[0].SetFloat(Shader.PropertyToID("_StencilComp"), 8.0f);
            tmp[0].SetFloat(Shader.PropertyToID("_Stencil"), 0.0f);
            tmp[0].SetFloat(Shader.PropertyToID("_StencilOp"), 0.0f);
            tmp[0].SetFloat(Shader.PropertyToID("_StencilWriteMask"), 255.0f);
            tmp[0].SetFloat(Shader.PropertyToID("_StencilReadMask"), 255.0f);
            mr.materials = tmp;
            bp.SetPlaneMaterial(mr.materials[0]);
            bp.SetPlaneVisibility(true);

            Material mat = new Material(Shader.Find("tk2d/BlendVertexColor"));
            mat.SetTexture(Shader.PropertyToID("_MainTex"), ab_bmc_mat.LoadAsset<Texture2D>("Black Tile tot") as Texture);
            GameObject scenemap = GameObject.Find("Scenemap");
            for (int i = 0; i < scenemap.transform.childCount; i++)
            {
                GameObject tmpGo = scenemap.transform.GetChild(i).gameObject;
                if (tmpGo.activeInHierarchy)
                {
                    tmpGo.GetComponent<MeshRenderer>().material = mat;
                }
            }

            if (debug)
                Log("~BlurPlane");
        }

        private IEnumerator PatchLitSpriteMaterials(Scene scene, float scale)
        {
            yield return null;
            if (debug)
                Log("!Lit Sprites");

            string[] sprites_lit = new string[] {
                "Wall_Middle",
                "Wall_Decoration",
                "Seal_Of_Binding_Wall"
            };
            Material mat = new Material(Shader.Find("Sprites/Lit"));
            mat.SetColor(Shader.PropertyToID("_Color"), new Color(1.0f, 1.0f, 1.0f, 1.0f));
            mat.SetFloat(Shader.PropertyToID("PixelSnap"), 0.0f);
            mat.SetFloat(Shader.PropertyToID("_EnableExternalAlpha"), 0.0f);
            int i = 0;
            foreach (var str_lit in sprites_lit)
            {
                GameObject parent = GameObject.Find(str_lit);
                if ((parent != null) && parent.activeInHierarchy)
                {
                    foreach (SpriteRenderer sr in parent.GetComponentsInChildren<SpriteRenderer>())
                    {
                        sr.gameObject.transform.localScale = sr.gameObject.transform.localScale * scale;
                        if (sr.gameObject.name.Contains("deepnest_fog_02"))
                        {
                            sr.gameObject.transform.localScale = sr.gameObject.transform.localScale * 4;
                        }
                        sr.material = mat;
                        if ((++i) >= 100)
                        {
                            i = 0;
                            yield return null;
                        }
                    }
                }
            }
            yield return null;
            if (debug)
                Log("~Lit Sprites");
        }
        private IEnumerator PatchLitSpriteMaterials(Scene scene)
        {
            return PatchLitSpriteMaterials(scene, 1.0f);
        }
        private IEnumerator PatchDefaultSpriteMaterials(Scene scene, float scale)
        {
            yield return null;
            if (debug)
                Log("!Default Sprites");

            string[] sprites_default = new string[] {
                "Clouds",
                "level370_Clouds",
                "Damage Colliders",
                "Hornet Pickup Colliders",
                "Organ_Wall",
                "Thorns"
            };
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.SetColor(Shader.PropertyToID("_Color"), new Color(1.0f, 1.0f, 1.0f, 1.0f));
            mat.SetFloat(Shader.PropertyToID("PixelSnap"), 0.0f);
            mat.SetFloat(Shader.PropertyToID("_EnableExternalAlpha"), 0.0f);
            int i = 0;
            foreach (var str_def in sprites_default)
            {
                GameObject parent = GameObject.Find(str_def);
                if ((parent != null) && parent.activeInHierarchy)
                {
                    foreach (SpriteRenderer sr in parent.GetComponentsInChildren<SpriteRenderer>())
                    {
                        sr.gameObject.transform.localScale = sr.gameObject.transform.localScale * scale;
                        sr.material = mat;
                        if ((++i) >= 100)
                        {
                            i = 0;
                            yield return null;
                        }
                    }
                }
            }
            yield return null;
            if (debug)
                Log("~Default Sprites");
        }
        private IEnumerator PatchDefaultSpriteMaterials(Scene scene)
        {
            return PatchDefaultSpriteMaterials(scene, 1.0f);
        }

        private IEnumerator PatchDamageResetColliders(Scene scene)
        {
            yield return null;
            if (debug)
                Log("!Damage Colliders");

            GameObject parent = GameObject.Find("Damage Colliders");
            Transform ch;
            for (int a = 0; a < parent.transform.childCount; a++)
            {
                ch = parent.transform.GetChild(a);

                if ((ch != null) && ch.gameObject.activeInHierarchy)
                {
                    try
                    {
                        var dh = ch.gameObject.AddComponent<DamageHero>();
                        dh.damageDealt = 1;
                        dh.shadowDashHazard = false;
                        dh.resetOnEnable = false;
                        dh.hazardType = (int)HazardType.ACID;

                        string name = ch.gameObject.name.ToLower();
                        if (name.Contains("thorn"))
                        {
                            ch.gameObject.AddComponent<NonBouncer>();
                        }
                        else if (name.Contains("pit"))
                        {
                            //dh.hazardType = (int)HazardType.PIT;
                            ch.gameObject.AddComponent<NonBouncer>();
                        }
                        else if (name.Contains("spike"))
                        {
                        }
                        else if (name.Contains("saw"))
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("PatchDamageResetColliders - " + ex.ToString());
                    }
                }
            }
            if (debug)
                Log("~Damage Colliders");
        }

        private IEnumerator PatchHazardRespawnTrigger(Scene scene)
        {
            yield return new WaitWhile(() => !GameObject.Find("Hazard Respawn Trigger List"));
            if (debug)
                Log("!Hazard Respawn Triggers");

            GameObject markers = GameObject.Find("Hazard Respawn Trigger List");
            if (markers == null)
            {
                yield break;
            }
            Transform tf;
            GameObject go;
            HazardRespawnMarker hrm;
            HazardRespawnTrigger hrt;
            for (int i = 0; i < markers.transform.childCount; i++)
            {
                tf = markers.transform.GetChild(i);
                go = tf.gameObject;
                if (go.name.Contains("Hazard Respawn Trigger v2"))
                {
                    #region Add HazardRespawnMarker to Child: Hazard Respawn Marker
                    hrm = go.transform.GetChild(0).gameObject.AddComponent<HazardRespawnMarker>();
                    hrm = go.transform.GetChild(0).gameObject.GetComponent<HazardRespawnMarker>();
                    if (go.name.Contains("left"))
                        hrm.respawnFacingRight = false;
                    else
                        hrm.respawnFacingRight = true;
                    #endregion
                    #region Add HazardRespawnTrigger to Parent: Hazard Respawn Trigger v2
                    hrt = go.AddComponent<HazardRespawnTrigger>();
                    hrt = go.GetComponent<HazardRespawnTrigger>();
                    hrt.respawnMarker = hrm;
                    hrt.fireOnce = false;
                    #endregion
                }
            }

            if (debug)
                Log("~Hazard Respawn Triggers");
        }

        private IEnumerator PatchCameraLockAreas(Scene scene)
        {
            yield return new WaitWhile(() => !GameObject.Find("_Camera Lock Zones"));
            if (debug)
                Log("!Camera Lock Areas");

            GameObject areas = GameObject.Find("_Camera Lock Zones");
            if (areas == null)
            {
                yield break;
            }
            Transform tf;
            GameObject go;
            BoxCollider2D bc2d;
            CameraLockArea cla;
            for (int i = 0; i < areas.transform.childCount; i++)
            {
                tf = areas.transform.GetChild(i);
                go = tf.gameObject;
                cla = go.AddComponent<CameraLockArea>();
                cla = go.GetComponent<CameraLockArea>();
                bc2d = go.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>();
                cla.cameraXMin = bc2d.bounds.min.x + 14.6f;
                cla.cameraYMin = bc2d.bounds.min.y + 8.3f;
                cla.cameraXMax = bc2d.bounds.max.x - 14.6f;
                cla.cameraYMax = bc2d.bounds.max.y - 8.3f;
                cla.preventLookUp = go.name.Contains("nlu") || go.name.Contains("nldu");
                cla.preventLookDown = go.name.Contains("nld") || go.name.Contains("nlud");
                cla.maxPriority = false;
            }

            if (debug)
                Log("~Camera Lock Areas");
        }

        private void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate, Vector2 respawnPoint,
                                   bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
        {
            if (debug)
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

            if (debug)
                Log("~Gateway");
        }

        private void printDebug(GameObject go, string tabindex = "")
        {
            Log(tabindex + "Name: " + go.name);
            foreach (var comp in go.GetComponents<Component>())
            {
                Log(tabindex + "Component: " + comp.GetType());
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                printDebug(go.transform.GetChild(i).gameObject, tabindex + "\t");
            }
        }

        private void Log(String message)
        {
            Logger.Log("[BossModCore]:[SceneChanger] - " + message.ToString());
        }
        private void Log(System.Object message)
        {
            Logger.Log("[BossModCore]:[SceneChanger] - " + message.ToString());
        }

        private static void SetInactive(GameObject go)
        {
            if (go != null)
            {
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.SetActive(false);
            }
        }
        private static void SetInactive(UnityEngine.Object go)
        {
            if (go != null)
            {
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
        }
    }
}
