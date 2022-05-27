using MapMagic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SynchronizedWorldObjects
{
    public class SynchronizedEquipment : SynchronizedWorldObject
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public int ItemID;
        public bool IsPickable = false;

        public SynchronizedEquipment(int itemID) : base("-1", -1)
        {
            ItemID = itemID;
        }
        public void AddToScene(string scene, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            SceneIdentifierName = scene;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public override bool OnGuestJoined(string guestUID)
        {
            return false;
        }

        public override bool OnSceneLoaded()
        {
            if (ShouldBeSpawned())
            {
                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    SetupServerSide();
                }
                return true;
            }
            return false;
        }

        public override object SetupClientSide(int rpcListenerID, string instanceUID, int sceneViewID, int recursionCount, string rpcMeta)
        {
            return null;
        }

        virtual public Item SetupServerSide()
        {
            Item item = ItemManager.Instance.GenerateItemNetwork(ItemID);
            item.ChangeParent(null, Position, Rotation.EulerToQuat());

            item.SetForceSyncPos();
            item.SaveType = Item.SaveTypes.NonSavable;
            item.IsPickable = false;
            item.HasPhysicsWhenWorld = true;
            return item;
        }
    }
}
