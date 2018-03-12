using UnityEngine;

public class BuffDestroy : MonoBehaviour {

	public float maxLifeTime = 10f;
	public float instantiateTime = 0f;
	// Use this for initialization
	void Start () {
		instantiateTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		//前进
		if (Time.time - instantiateTime > maxLifeTime)
			Destroy(gameObject);
	}
}
