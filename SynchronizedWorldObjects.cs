using BepInEx;

namespace SynchronizedWorldObjects
{
    using TinyHelper;
    using UnityEngine;
    using SideLoader;
    using HarmonyLib;
    using System.Collections.Generic;

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SideLoader",    BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(TinyHelper.GUID,           TinyHelper.VERSION)]

    public class SynchronizedWorldObjects : BaseUnityPlugin
    {
        public const string GUID = "com.ehaugw.synchronizedworldobjects";
        public const string VERSION = "1.0.0";
        public const string NAME = "Synchronized World Objects";


        internal void Awake()
        {
            var rpcGameObject = new GameObject("SynchronizedWorldObjectsRPC");
            DontDestroyOnLoad(rpcGameObject);
            rpcGameObject.AddComponent<SynchronizedWorldObjectManager>();

            SynchronizedWorldObjectManager.SyncedWorldObjects = new List<SynchronizedWorldObject>();

            SL.OnPacksLoaded += OnPackLoaded;
            SL.OnSceneLoaded += OnSceneLoaded;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void OnPackLoaded()
        {
        }

        private void OnSceneLoaded()
        {
            SynchronizedWorldObjectManager.OnSceneLoaded();
        }
    }
}