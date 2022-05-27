using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SynchronizedWorldObjects
{
    abstract public class SynchronizedWorldObject
    {
        public string IdentifierName;
        public string SceneIdentifierName;
        public int RPCListenerID;
        public string Uid;

        public SynchronizedWorldObject(string identifierName, int rpcListenerID)
        {
            IdentifierName = identifierName;
            RPCListenerID = rpcListenerID;

            SynchronizedWorldObjectManager.SyncedWorldObjects.Add(this);
        }

        virtual public bool ShouldBeSpawned()
        {
            return SceneManagerHelper.ActiveSceneName == SceneIdentifierName;
        }
        abstract public bool OnSceneLoaded();

        abstract public bool OnGuestJoined(string guestUID);

        //abstract public object SetupServerSide();
        abstract public object SetupClientSide(int rpcListenerID, string instanceUID, int sceneViewID, int recursionCount, string rpcMeta);
    }
}
