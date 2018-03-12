using UnityEngine;

public class Bullet : MonoBehaviour
{
	//攻击方
	public GameObject attackPlayer;
	public float maxdamage;
	public float WeakenSpeed;//less better
    public float speed;
    public GameObject explode;
    public float maxLifeTime;
    public float instantiateTime = 0f;
	public bool ifright = true;
	public bool absorbed = false;
	
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
		//不影响子弹
		if (collisionInfo.gameObject.CompareTag("Bullet")
		||collisionInfo.gameObject == attackPlayer)
			return;
		//打到角色外的东西
		if (!collisionInfo.gameObject.CompareTag("Player"))
		{
			Vector2 pos1 = new Vector2(transform.position.x + (ifright ? 0.1f : -0.1f), transform.position.y);
			Instantiate(explode, pos1, transform.rotation);
			//摧毁自身
			Destroy(gameObject);
			return;
		}

	    //抵消队友伤害
	    PlayerControl pc = collisionInfo.gameObject.GetComponent<PlayerControl>();
		if (!pc)
			return;
	    if (MultiBattle.Instance.IfSameCamp(pc.gameObject, attackPlayer.gameObject))
	    { 
		    Destroy(gameObject);
		    Debug.Log("关闭队友伤害");
		    return;
	    }
		//打中敌人
	    if (pc.Hero.type == HeroType.Roshan)
	    {
		    if (pc.Hero.ifShieldOn)
		    {
			    Destroy(gameObject);
			    //音效
			    pc.Hero.dl.Rebuild();
			    return;
		    }
	    }
        //爆炸效果
	    Vector2 pos = new Vector2(transform.position.x+(ifright?0.1f:-0.1f),transform.position.y);
        Instantiate(explode, pos, transform.rotation);
        //摧毁自身
        Destroy(gameObject);
	    
	    if (!absorbed&&attackPlayer.name==GameMgr.Instance.id)//client attack send
	    {
		    float att = GetAtt();
		    //defence buff
		    if (pc.Hero.defence > 0)
			    att -= pc.Hero.defence;
		    attackPlayer.GetComponent<PlayerControl>().SendHit(pc.name, att);//name在generate player时设置
	    }
    }
	//计算攻击力
	private float GetAtt()
	{
		float att = maxdamage - (Time.time - instantiateTime) * WeakenSpeed;
		if (att < 1)
			att = 1;
		return att;
	}
}
