using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour {

    private static Text logText;
    private static string text;

    private void Awake() {
        logText = GetComponent<Text>();
    }

    public static void Log(string msg) {
        text += (msg + "\n");
    }

    private void Update() {
        if(logText == null) {
            return;
        }
        logText.text = text;

    }

}
