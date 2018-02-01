using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour {

	public static GameMgr Instance
	{
		get{return Nested.instance;}
	}

	private class Nested
	{
		static Nested() { }
		internal static readonly GameMgr instance = new GameMgr();
	}
    public string id = "PixelArenaGame";

    private void Awake()
    {

    }
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
