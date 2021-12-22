using MonoMod.ModInterop;
using UnityEngine;

namespace BossModCore
{
    [ModExportName("BossModCore")]
    public static class BmcApi
    {
        public static void RegisterBoss(int numBosses, string[] statueNameKeys, string[] statueDescKeys, bool[] customSceneFlags, string[] scenePrefabNames, GameObject[] stateGos)
        {
        }
    }
}
