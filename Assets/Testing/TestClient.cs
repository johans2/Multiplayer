using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClient : MonoBehaviour {
    
	void Start () {
        ClientHandleNetworkData.InitializePackageHandlers();
        ClientTCP.ConnectToServer();
	}
	
}
