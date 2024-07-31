using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using TinyHelper;
using UnityEngine;

namespace SynchronizedWorldObjects
{
    public class SynchronizedNPC : SynchronizedWorldObject
    {
        public enum HairColors
        {
            BrownMedium = 0,
            BrownBright = 1,
            BrownDark = 2,
            Black = 3,
            Blonde = 4,
            White = 5,
            Red = 6,
            Blue = 7,
            Green = 8,
            Orange = 9,
            Purple = 10
        }

        public enum HairStyles
        {
            Bald = 0,
            Basic = 1,
            PonyTail = 2,
            Wild = 3,
            CombedBack = 4,
            PonyTailBraids = 5,
            BraidsBack = 6,
            Bun = 7,
            MaleShort = 8,
            MaleMedium = 9,
            MaleLong = 10,
            CornrowsMedium = 11,
            CornrowsLong = 12,
            CornrowsShort = 13,
            Ball = 14,

        }

        public int[] DefaultEquipment = { };
        public int[] ModdedEquipment = { };
        public Vector3 Scale;

        public string RPCMeta;
        
        public Character.Factions Faction;
        private Character localCharacter;
        private AIRoot aiRoot;
        public SL_Character.VisualData VisualData;

        public List<SynchronizedNPCScene> Scenes = new List<SynchronizedNPCScene>();
        public SynchronizedNPCScene ActiveScene;

        public SynchronizedNPC(string identifierName, int rpcListenerID, int[] defaultEquipment = null, int[] moddedEquipment = null, Vector3? scale = null, Character.Factions? faction = null, SL_Character.VisualData visualData = null) : base(identifierName, rpcListenerID)
        {
            DefaultEquipment = defaultEquipment ?? new int[] { };
            ModdedEquipment = moddedEquipment ?? new int[] { };
            Scale = scale ?? Vector3.one;
            Faction = faction ?? Character.Factions.NONE;
            VisualData = visualData;
        }

        public void AddToScene(SynchronizedNPCScene scene)
        {
            Scenes.Add(scene);
        }

        public void AssignAI(AIRoot aiRoot)
        {
            this.aiRoot = aiRoot;
        }

        public GameObject GetGameObject()
        {
            return GameObject.Find("UNPC_" + IdentifierName);
        }
        public object GetSynchronizedObject(string instanceUID)
        {
            return CharacterManager.Instance.GetCharacter(instanceUID);
        }


        public override bool ShouldBeSpawned()
        {
            foreach (var scene in Scenes)
            {
                if (scene.ShouldSpawnInScene() && scene.Scene == SceneManagerHelper.ActiveSceneName)
                {
                    ActiveScene = scene;
                    return true;
                }
            }
            return false;
        }

        override public bool OnSceneLoaded()
        {
            if (ShouldBeSpawned())
            {
                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    var viewID = PhotonNetwork.AllocateSceneViewID();
                    var instanceGameObject = GetGameObject();

                    if (instanceGameObject == null)
                    {
                        SetupServerSide();
                        SynchronizedWorldObjectManager.Instance.photonView.RPC("SetupClientSide", PhotonTargets.All, new object[] { RPCListenerID, this.Uid, viewID, 0, ActiveScene.RPCMeta});
                        return true;
                    }
                }
            } else
            {
                if (GetGameObject() is GameObject obj) UnityEngine.Object.DestroyImmediate(obj);
            }
            return false;

        }

        public override bool OnGuestJoined(string guestUID)
        {
            if (ShouldBeSpawned())
            {
                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    var viewID = PhotonNetwork.AllocateSceneViewID();
                    var instanceGameObject = GetGameObject();

                    if (instanceGameObject == null)
                    {
                        SynchronizedWorldObjectManager.Instance.photonView.RPC("SetupClientSide", PhotonTargets.All, new object[] { RPCListenerID, Uid, viewID, 0, ActiveScene.RPCMeta });
                        return true;
                    }
                }
            }
            else
            {
                if (GetGameObject() is GameObject obj) UnityEngine.Object.DestroyImmediate(obj);
            }
            return false;
        }

        override public object SetupClientSide(int rpcListenerID, string instanceUID, int sceneViewID, int recursionCount, string rpcMeta)
        {
            const object failReturn = null;

            if (RPCListenerID != rpcListenerID) return failReturn;

            ActiveScene = Scenes.FirstOrDefault(x => x.RPCMeta == rpcMeta);

            int millisecondDelay = 200;

            Character instanceCharacter = GetSynchronizedObject(instanceUID) as Character;

            if (instanceCharacter == null)
            {
                if (recursionCount * millisecondDelay < 20000)
                {
                    TinyHelper.DelayedTask.GetTask(millisecondDelay).ContinueWith(_ => SetupClientSide(rpcListenerID, instanceUID, sceneViewID, recursionCount + 1, rpcMeta));
                    Console.Read();
                    return failReturn;
                }
                else
                {
                    Debug.Log("SynchronizedNPC with UID " + instanceUID + " could not fetched from server");
                    return failReturn;
                }
            }

            var obj = new GameObject("UNPC_" + IdentifierName);

            obj.transform.position = ActiveScene.Position;
            obj.transform.rotation = Quaternion.Euler(ActiveScene.Rotation);

            GameObject instanceGameObject = instanceCharacter.gameObject;

            if (VisualData != null)
            {
                TinyHelper.TinyHelper.Instance.StartCoroutine(SL_Character.SetVisuals(instanceCharacter, VisualData.ToString()));
            }

            instanceGameObject.transform.parent = obj.transform;
            instanceGameObject.transform.position = obj.transform.position;
            instanceGameObject.transform.rotation = obj.transform.rotation;

            instanceGameObject.transform.localScale = Scale;

            UnityEngine.Object.DestroyImmediate(instanceGameObject.GetComponent<StartingEquipment>());

            //Character instanceCharacter = instanceGameObject.GetComponent<Character>(); 
            instanceCharacter.Stats.enabled = false;

            if (instanceCharacter.CurrentWeapon?.TwoHanded ?? false)
            {
                instanceCharacter.LeftHandEquipment = instanceCharacter.CurrentWeapon;
                instanceCharacter.LeftHandChanged();
            }
            if (instanceCharacter.CurrentWeapon != null)
            {
                instanceCharacter.Sheathed = ActiveScene.Sheathed;
            }

            //instanceCharacter.gameObject.SetActive(false);
            //instanceCharacter.gameObject.SetActive(true);

            localCharacter = instanceCharacter;

            return instanceCharacter;
        }

        public void AITick()
        {
            TinyHelper.DelayedTask.GetTask(1000).ContinueWith(_ => AITick());
            Console.Read();

            if (localCharacter != null /*&& localCharacter.InCombat*/)
            {
                var validPoses = new Character.SpellCastType[] { Character.SpellCastType.EnterInnBed, Character.SpellCastType.Sit };
                if (validPoses.Contains(ActiveScene.Pose) && localCharacter.InLocomotion)
                {
                    localCharacter.Animator.SetInteger("SpellType", (int)ActiveScene.Pose);
                    localCharacter.Animator.SetTrigger("Spell");
                    //localCharacter?.SpellCastAnim(Pose, Character.SpellCastModifier.Immobilized, 0);
                }
            }
        }

        public object SetupServerSide()
        {
            //Debug.Log("SynchronizedNPC " + IdentifierName + " started SetupServerSide");
            GameObject instanceGameObject;

            // ============= setup base NPC object ================
            var uid = UID.Generate().ToString();
            instanceGameObject = TinyCharacterManager.SpawnCharacter(uid, ActiveScene.Position, ActiveScene.Rotation).gameObject;
            //instanceGameObject = CustomCharacters.SpawnCharacter(new SL_Character() { SpawnPosition = ActiveScene.Position, CharacterVisualsData = VisualData}, ActiveScene.Position, uid, IdentifierName);
            instanceGameObject.transform.rotation = Quaternion.Euler(ActiveScene.Rotation);

            Character instanceCharacter = instanceGameObject.GetComponent<Character>();

            SideLoader.At.SetField<Character>(instanceCharacter, "m_instantiationType", CharacterManager.CharacterInstantiationTypes.Item);

            foreach (int itemID in ActiveScene.DefaultEquipment ?? DefaultEquipment)
                instanceCharacter.Inventory.Equipment.EquipInstantiate(ResourcesPrefabManager.Instance.GetItemPrefab(itemID) as Equipment);

            foreach (int itemID in ActiveScene.ModdedEquipment ?? ModdedEquipment)
            {
                // setup custom weapon (using SideLoader, requires slightly different method to equip)            
                if (ResourcesPrefabManager.Instance.ContainsItemPrefab(itemID.ToString()))
                {
                    var item = ItemManager.Instance.GenerateItemNetwork(itemID) as Equipment;
                    instanceCharacter.Inventory.TakeItem(item.UID);
                    SideLoader.At.Invoke<CharacterEquipment>(instanceCharacter.Inventory.Equipment, "EquipWithoutAssociating", new Type[] { typeof(Equipment), typeof(bool) }, new object[] { item, false });
                }
            }

            if (instanceCharacter.CurrentWeapon?.TwoHanded ?? false)
            {
                instanceCharacter.LeftHandEquipment = instanceCharacter.CurrentWeapon;
                instanceCharacter.LeftHandChanged();
            }
            if (instanceCharacter.CurrentWeapon != null)
            {
                instanceCharacter.Sheathed = ActiveScene.Sheathed;
            }

            instanceCharacter.ChangeFaction(ActiveScene.Faction ?? Faction);

            instanceGameObject.SetActive(true);

            //Relevant when I wish to implement AI again
            //if (aiRoot != null) InquisitionAI.AssignInquisitionAI(instanceCharacter, aiRoot);

            TinyHelper.DelayedTask.GetTask(1000).ContinueWith(_ => AITick());
            Console.Read();

            this.Uid = uid;

            return uid;
        }
    }
}
