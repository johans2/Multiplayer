using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SyncedBehaviour : MonoBehaviour {

    public int ID { get; private set; }
    public int Owner { get; private set; }
    public bool IsControlledByMe { get; private set; }
    
    /// <summary>
    /// Sending data to the server.
    /// </summary>
    public virtual byte[] Serialize() {
        return new byte[0];
    }


    /// <summary>
    /// Reading data from the server.
    /// </summary>
    public virtual void Deserialize() {
        
    }


    

    void Update () {
		
	}



}
