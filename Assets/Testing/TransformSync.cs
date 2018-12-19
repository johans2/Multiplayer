using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSync : SyncedBehaviour {

    public float speed = 10;
    private bool move = true;

    public void Update() {
        if(ServerClientSwitcher.IsServer && move) {
            transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
            transform.position = new Vector3(transform.position.x, Mathf.Sin(Time.timeSinceLevelLoad * (speed / 50)),transform.position.y);
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            move = !move;
        }
    }
    
    public override byte[] Serialize() {
        PacketBuffer buffer = new PacketBuffer();

        // Write position
        buffer.WriteVector3(transform.position);

        // Write rotation
        buffer.WriteVector3(transform.rotation.eulerAngles);

        // Write Scale

        return buffer.ToArray();
    }

    public override void Deserialize(byte[] data) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);

        // Read position
        Vector3 receivedPosition = buffer.ReadVector3();
        transform.position = receivedPosition;

        // Read rotation
        Vector3 receivedRotation = buffer.ReadVector3();
        transform.rotation = Quaternion.identity * Quaternion.Euler(receivedRotation);


        //Debug.Log(string.Format("Deserialized object , rot = {0}", receivedRotation));
    }



}
