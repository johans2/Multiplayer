using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationSyncing : SyncedBehaviour {

    public float speed = 10;

    public void Update() {
        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));

        if(Input.GetKeyDown(KeyCode.Space)) {
            speed += 10;
        }
    }

    public override byte[] Serialize() {
        //Debug.Log("Serializing object..");

        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteFloat(speed);
        return buffer.ToArray();
    }

    public override void Deserialize(byte[] data) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        float receivedSpeed = buffer.ReadFloat();
        speed = receivedSpeed;

        //Debug.Log(string.Format("Deserialized object , speed = {0}", receivedSpeed));
    }



}
