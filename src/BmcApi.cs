using System;
using MonoMod.ModInterop;
using UnityEngine;
using UScene = UnityEngine.SceneManagement.Scene;

namespace BossModCore;

[ModExportName("BossModCore")]
public static class BmcApi
{
    public static void RegisterBossExistingScene(string originalSceneName, string statueNameKey, string statueDescKey, Func<GameObject> statueCallback, Action<UScene> sceneHook)
    {
    }

    public static void RegisterBossNewScene(string statueNameKey, string statueDescKey, Func<GameObject> statueCallback, Action<UScene> sceneHook)
    {
    }
}