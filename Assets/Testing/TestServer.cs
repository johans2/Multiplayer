using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestServer : MonoBehaviour {
    
	void Start () {
        ServerHandleNetworkData.InitializePackageHandlers();
        ServerTCP.SetupServer();
	}
}
