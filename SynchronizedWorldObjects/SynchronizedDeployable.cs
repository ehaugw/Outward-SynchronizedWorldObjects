namespace SynchronizedWorldObjects
{
    public class SynchronizedDeployable : SynchronizedEquipment
    {
        public SynchronizedDeployable(int itemID) : base(itemID) { }

        public override object SetupClientSide(int rpcListenerID, string instanceUID, int sceneViewID, int recursionCount, string rpcMeta)
        {
            return null;
        }

        override public Item SetupServerSide()
        {
            //Item item = Item.Instantiate(ResourcesPrefabManager.Instance.GetItemPrefab(ItemID));
            //item.ChangeParent(null, Position, Rotation.EulerToQuat());
            //item.SetForceSyncPos();
            //item.SaveType = Item.SaveTypes.NonSavable;
            Item item = base.SetupServerSide();
            item.IsPickable = false;
            item.HasPhysicsWhenWorld = false;

            Deployable deployable = item.GetComponent<Deployable>();
            deployable.StartDeployAnimation();
            
            Dropable component = item.GetComponent<Dropable>();
            if (component) component.GenerateContents();

            FueledContainer fueledContainer = item.GetComponent<FueledContainer>();
            if (fueledContainer) fueledContainer.TryKindle = true;

            return item;
        }
    }
}
