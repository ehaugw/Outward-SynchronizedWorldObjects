using InstanceIDs;
using System.Collections.Generic;
using UnityEngine;

namespace SynchronizedWorldObjects
{
    class SynchronizedWorldObjectManager : Photon.MonoBehaviour
    {
        public static SynchronizedWorldObjectManager Instance;
        public static List<SynchronizedWorldObject> SyncedWorldObjects;

        public static void OnSceneLoaded()
        {
            foreach (var worldObject in SyncedWorldObjects)
            {
                worldObject.OnSceneLoaded();
            }
        }
        internal void Start()
        {
            Instance = this;
            
            var view = this.gameObject.AddComponent<PhotonView>();
            view.viewID = IDs.SynchronizedWorldObjectsRPCPhotonID;
            Debug.Log("Registered SynchronizedWorldObjectManager with ViewID " + this.photonView.viewID);
        }

        [PunRPC]
        public void SetupClientSide(int rpcListenerID, string instanceUID, int sceneViewID, int recursionCount, string rpcMeta)
        {
            foreach (SynchronizedWorldObject worldObject in SyncedWorldObjects)
                worldObject.SetupClientSide(rpcListenerID, instanceUID, sceneViewID, recursionCount, rpcMeta);
        }
    }
}
