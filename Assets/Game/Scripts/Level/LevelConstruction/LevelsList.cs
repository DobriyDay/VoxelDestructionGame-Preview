using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelList", menuName = "Data/New Level List", order = 0)]
public class LevelsList : ScriptableObject
{
        public LevelReferenceById[] levelsByID;

#if UNITY_EDITOR
        [ContextMenu("Validate")]
        private void OnValidate()
        {
                var usedLevels = new HashSet<LevelReferenceById>();
                var usedIds = new HashSet<int>();
                int nextId = 0;

                for (int i = 0; i < levelsByID.Length; i++)
                {
                        if (usedLevels.Contains(levelsByID[i]))
                        {
                                Debug.LogError($"{levelsByID[i]} already in list. Error Index: {i}");
                                continue;
                        }
                        
                        if (usedIds.Contains(levelsByID[i].id))
                        {
                                while (usedIds.Contains(nextId))
                                        nextId++;

                                levelsByID[i].id = nextId;
                        }

                        usedIds.Add(levelsByID[i].id);
                }
        }
#endif
}