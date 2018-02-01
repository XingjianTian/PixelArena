using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Application.runInBackground = true;//后台运行
        PanelMgr.instance.OpenPanel<LoginPanel>("");
    }
	
	// Update is called once per frame
	void Update () {
        NetMgr.Update();
    }
}
