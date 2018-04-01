using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public static GameMgr Instance;
    /*
	{
		get{return Nested.instance;}
	}

	private class Nested
	{
		static Nested() { }
		internal static readonly GameMgr instance = new GameMgr();
	}*/
    
    public string id = "PixelArenaGame";
    private void Awake()
    {
        Instance = this;
    }
	
}
