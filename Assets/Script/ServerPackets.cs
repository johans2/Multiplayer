﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sent from server to client. Clients listen for these.
public enum ServerPackets {
    SConnectionOK = 1,
}

// Send from client to server. Server listen for these.
public enum ClientPackets {
    CThankYou = 1,
}