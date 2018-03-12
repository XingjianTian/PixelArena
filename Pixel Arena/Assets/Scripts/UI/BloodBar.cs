using UnityEngine;

public class BloodBar : MonoBehaviour
{

	public GameObject c;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = new Vector2(c.transform.position.x,
			c.transform.position.y + 1);
	}
}
