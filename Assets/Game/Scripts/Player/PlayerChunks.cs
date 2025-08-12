using UnityEngine;


public class PlayerChunks : ChunksContainer
{
        [field: SerializeField] public Transform CameraFollowPoint { get; private set; }
        [field: SerializeField] public HandsItem[] HandsItems { get; private set; }

        private void Awake()
        {
                for (int i = 0; i < chunks.Length; i++)
                {
                        GameObjectsServiceLocator.Register<PlayerChunks>(chunks[i].gameObject, this);
                }
        }

        public override void Dispose()
        {
                base.Dispose();
                for (int i = 0; i < chunks.Length; i++)
                {
                        GameObjectsServiceLocator.Unregister<PlayerChunks>(chunks[i].gameObject);
                }
        }
}