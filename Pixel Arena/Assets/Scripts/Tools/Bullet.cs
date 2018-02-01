using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	//攻击方
	public GameObject attackPlayer;
	public float power = 40f;//less better
    public float speed = 10f;
    public GameObject explode;
    public float maxLifeTime = 10f;
    public float instantiateTime = 0f;
	public bool ifright = true;
	// Use this for initialization
	void Start () {
        instantiateTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        //前进
		if(ifright)
        	transform.position += transform.right * speed * Time.deltaTime;
		else
		{
			GetComponent<SpriteRenderer>().flipX = true;
			transform.position -= transform.right * speed * Time.deltaTime;
		}
        //摧毁
        if (Time.time - instantiateTime > maxLifeTime)
            Destroy(gameObject);
	}
    //碰撞
    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
	    //打到自身
	    if (collisionInfo.gameObject == attackPlayer)
		    return;
        //爆炸效果
	    Vector2 pos = new Vector2(transform.position.x+(ifright?0.1f:-0.1f),transform.position.y);
        Instantiate(explode, pos, transform.rotation);
        //摧毁自身
        Destroy(gameObject);
	    //击中角色
	    PlayerControl pc = collisionInfo.gameObject.GetComponent<PlayerControl>();
	    if (pc != null&&attackPlayer.name==GameMgr.Instance.id)
	    {
		    float att = GetAtt();
		    attackPlayer.GetComponent<PlayerControl>().SendHit(pc.name, att);//name在generate player时设置
	    }
    }
	//计算攻击力
	private float GetAtt()
	{
		float att = 100 - (Time.time - instantiateTime) * power;
		if (att < 1)
			att = 1;
		return att;
	}
}
