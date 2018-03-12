using UnityEngine;

public class Health : MonoBehaviour
{
	public float xOffset = -350f;//1920
	public float yOffset = 90f;//1080
	//主角对象
	public GameObject hero;

	//主摄像机对象
	
	private Camera camera;
	public RectTransform HealthrecTransform;
	public Vector2 player2DPosition;
	// Use this for initialization
	void Start()
	{
		camera = Camera.main;
		//hero = MultiBattle.Instance.list[GameMgr.Instance.id].Player.gameObject;
		
		HealthrecTransform = GetComponent<RectTransform>();
		hero = transform.parent.parent.gameObject;
		if (hero.GetComponent<Heroes>().type == HeroType.Roshan)
		{
			xOffset = -360f;
			yOffset = 130f;
		}
	}
  	
	void Update ()  
	{  
		
		player2DPosition = camera.WorldToScreenPoint(hero.transform.position);
		//player2DPosition = transform.position;
		HealthrecTransform.position = player2DPosition + new Vector2(xOffset, yOffset);  
  
		//血条超出屏幕就不显示  
		if (player2DPosition.x > Screen.width || player2DPosition.x < 0 || player2DPosition.y > Screen.height || player2DPosition.y < 0)  
		{  
			transform.GetChild(0).gameObject.SetActive(false);
		}  
		else  
		{  
			transform.GetChild(0).gameObject.SetActive(true); 
		} 
		/*
		transform.position = new Vector2(hero.transform.position.x,
			hero.transform.position.y + 0.3f);*/
	}  

}
