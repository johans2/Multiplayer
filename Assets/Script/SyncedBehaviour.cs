using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SyncedEntity))]
public abstract class SyncedBehaviour : MonoBehaviour {
    
    /// <summary>
    /// Sending data to the server.
    /// </summary>
    public virtual byte[] Serialize() {
        return new byte[0];
    }


    /// <summary>
    /// Reading data from the server.
    /// </summary>
    public virtual void Deserialize(byte[] data) {
        
    }
}
