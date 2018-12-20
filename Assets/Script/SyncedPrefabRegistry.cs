using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncedPrefabRegistry : MonoBehaviour {

    public List<GameObject> syncablePrefabs;
    
    public GameObject GetPrefab(int id) {
        return syncablePrefabs[id];
    }

    public int GetPrefabID(GameObject prefab) {

        int id = syncablePrefabs.IndexOf(prefab);

        if(id == -1) {
            throw new System.Exception("Prefab not registered in syncable prefabs list.");
        }

        return id;
    }



}
