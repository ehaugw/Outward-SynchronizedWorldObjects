using SideLoader;
using System;
using UnityEngine;

namespace SynchronizedWorldObjects
{
    public class SynchronizedNPCScene
    {
        public string Scene;
        public Vector3 Position;
        public Vector3 Rotation;
        public Character.Factions? Faction;
        public bool Sheathed;
        public Character.SpellCastType Pose;
        public string RPCMeta;
        public int[] DefaultEquipment;
        public int[] ModdedEquipment;

        public Func<bool> ShouldSpawnInScene;

        public SynchronizedNPCScene(
            string scene, Vector3 position, Vector3 rotation,
            Character.Factions? faction = null,
            bool sheathed = true,
            Character.SpellCastType pose = Character.SpellCastType.NONE,
            string rpcMeta = null,
            int[] defaultEquipment = null,
            int[] moddedEquipment = null,
            Func<bool> shouldSpawnInScene = null
        )
        {
            Scene = scene;
            Position = position;
            Rotation = rotation;
            Faction = faction;
            Sheathed = sheathed;
            Pose = pose;
            RPCMeta = rpcMeta;

            DefaultEquipment = defaultEquipment;
            ModdedEquipment = moddedEquipment;

            ShouldSpawnInScene = shouldSpawnInScene ?? delegate () { return true; };
        }
    }
}
