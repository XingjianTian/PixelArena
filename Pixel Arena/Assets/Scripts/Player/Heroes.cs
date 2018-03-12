using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum HeroType
{
	Ninja,
	Solider,
	Roshan
}
public class Heroes : MonoBehaviour{

	public HeroType type;

	private PlayerControl pc;
	//数值
	public int moveForce;
	public VInt maxSpeed;
	public int jumpForce;
	public int maxHp;//生命值
	public float shootInterval;//射击的时间间隔
	public float coolDownTime;

	//defence buff
	public int defence = 0;
	
	//子弹预设
	public GameObject bullet;
	//动画
	public Animator anim;
	//音效
	public AudioClip[] heroClips;
	//职业特殊
	//ninja
	public JumpAgainstWall jaw;//跳墙
	public RaycastHit2D hit;//检测擦过
	List<string> checkhit = new List<string>();
	//roshan
	public bool ifShieldOn = false;//护盾
	public GameObject shield;
	public DynamicLight dl;
	
	//开枪间隔
	public float lastShootTime =0f;
	public float lastETime =0f;
	
	//跳跃间隔
	public float lastJumpTime =0f;
	public float JumpInterval =0.5f;
	//子弹发射偏移位置
	public Vector2 offset;
	
	//buff camera
	public CameraFilterPack_AAA_SuperComputer came3A;
	public int buffcount = 0;
	private void Awake()
	{
		
		//职业特殊object
		jaw = type == HeroType.Ninja ? GetComponent<JumpAgainstWall>() : null;
		
		shield = type == HeroType.Roshan ? transform.Find("Shield").gameObject:null;
		dl = shield!=null ?shield.GetComponent<DynamicLight>():null;
		switch (type)
		{
			case HeroType.Ninja:
			{
				moveForce = 80;
				maxSpeed = (VInt)1.2f;
				jumpForce = 250;//避免跳太高
				maxHp = 100;//生命值
				shootInterval = 2f;//射击的时间间隔
				coolDownTime = 8f;
				offset.x = 0.5f;
				offset.y = 0f;
			}
				break;
			case HeroType.Solider:
			{
				moveForce = 80;
				maxSpeed = (VInt)1f;
				jumpForce = 250;
				maxHp = 150;//生命值
				shootInterval = 0.3f;//射击的时间间隔
				coolDownTime = 10f;
				offset.x = 0.35f;
				offset.y = 0.1f;
			}
				break;
			case HeroType.Roshan:
			{
				moveForce = 60;
				maxSpeed = (VInt)0.8f;
				jumpForce = 250;
				maxHp = 250;//生命值
				shootInterval = 0.15f;//射击的时间间隔
				coolDownTime = 15f;
				//dl.InsideFieldOfViewEvent += iSaw;
				offset.x = 0.45f;
				offset.y = -0.15f;

			}
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		
	}
	private void Start()
	{
		pc = gameObject.GetComponent<PlayerControl>();
		anim = GetComponent<Animator>();
		lastETime = -coolDownTime;
		came3A = Camera.main.GetComponent<CameraFilterPack_AAA_SuperComputer>();
	}
	//shoot
	public void Shoot()
	{
		//发射间隔
		if (Time.time - (float)lastShootTime < (float)shootInterval)
			return;
		if (bullet == null) //子弹
			return;
		//发射
		//枪声
		AudioSource.PlayClipAtPoint(heroClips[4], transform.position);
		switch (type)
		{
			case HeroType.Ninja:
			{
				anim.SetTrigger("Shoot");
				StartCoroutine("NinjaShoot");
				lastShootTime = Time.time;
			}
				break;
			case HeroType.Solider:
			{
				Vector2 pos = new Vector2(transform.position.x + (pc.facingRight ? offset.x : -offset.x), transform.position.y+offset.y);
				bullet.GetComponent<Bullet>().ifright = pc.facingRight;
				GameObject bulletObj = Instantiate(bullet, pos, transform.rotation);
				Bullet b = bulletObj.GetComponent<Bullet>();
				if (b != null)
					b.attackPlayer = gameObject;
				lastShootTime = Time.time;
			}
				break;
			case HeroType.Roshan:
			{
				Vector2 pos = new Vector2(transform.position.x + (pc.facingRight ? offset.x : -offset.x), transform.position.y+offset.y);
				bullet.GetComponent<Bullet>().ifright = pc.facingRight;
				float yoffset = Random.Range(-0.1f, 0.1f);
				pos.y += yoffset;
				GameObject bulletObj = Instantiate(bullet, pos, transform.rotation);
				Bullet b = bulletObj.GetComponent<Bullet>();
				if (b != null)
					b.attackPlayer = gameObject;
				lastShootTime = Time.time;
			}
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	//jump
	public void Jump()
	{
		anim.SetTrigger("Jump");
		int i = Random.Range(0, 3);
		AudioSource.PlayClipAtPoint(heroClips[i], transform.position);
		if (type == HeroType.Ninja)
		{
			if (!jaw.IfOnTheWall&& jaw.ifJumpAgainstFinished)
			{ 
				pc.rig.AddForce(Physics2D.gravity.y < 0
					? new Vector2(0f, jumpForce)
					: new Vector2(0f, -jumpForce));
				lastJumpTime = Time.time;
			}
		}
		else
		{
			pc.rig.AddForce(Physics2D.gravity.y < 0 
				? new Vector2(0f, jumpForce) 
				: new Vector2(0f, -jumpForce));
			lastJumpTime = Time.time;
		}
	}
	
	//E技能
	private IEnumerator ShiledStart()//显示层应该没关系
	{
		float zoomspeed = 1f;
		while (dl.LightRadius <= 0.5)
		{
			dl.LightRadius += Time.deltaTime*zoomspeed;
			yield return 0;
		}
		yield return  new WaitForSeconds(5f);
		while (dl.LightRadius >= 0)
		{
			dl.LightRadius -= Time.deltaTime*zoomspeed;
			yield return 0;
		}
		ifShieldOn = false;
		yield return 0;
	}
	void OnCollisionEnter2D(Collision2D collisionInfo)
	{
		
		if (collisionInfo.gameObject.CompareTag("Walls"))
		{
			checkhit.Clear();
			pc.allowable = true;
			Camera.main.GetComponent<CameraFilterPack_Blur_Focus>().enabled = false;
			StopCoroutine("Rush");
		}
	}

	private IEnumerator NinjaShoot()
	{
		int count = 0;
		while (count < 3)
		{
			Vector2 NinjaDartPos = new Vector2(transform.position.x + (pc.facingRight ? offset.x : -offset.x),
				transform.position.y + offset.y);
			bullet.GetComponent<Bullet>().ifright = pc.facingRight;
			GameObject bulletObj1 = Instantiate(bullet, NinjaDartPos, transform.rotation);
			Bullet b1 = bulletObj1.GetComponent<Bullet>();
			if (b1 != null)
				b1.attackPlayer = gameObject;
			count++;
			yield return new WaitForSeconds(0.05f);
		}
		StopCoroutine("NinjaShoot");
	}
	private IEnumerator Rush()
	{
		//聚焦
		if (pc.ctrlType == CtrlType.Player) //本机shader
		{
			var bf = Camera.main.GetComponent<CameraFilterPack_Blur_Focus>();	
			bf.enabled = true;
		}

	bool dir = pc.facingRight;
		float endposx = transform.position.x + (dir ? 2f : -2f);
		/*
		Vector3 endpos = new Vector3(transform.position.x+(pc.facingRight?2.5f:-2.5f),
			transform.position.y,transform.position.z);*/
		while (Math.Abs(transform.position.x - endposx) > 0.1f)  
		{  
			/*
			transform.position = Vector3.MoveTowards
				(transform.position, endpos, 8 * Time.deltaTime);*/
			transform.position = new Vector2(transform.position.x+
			 (pc.facingRight ? 5*Time.deltaTime:-5*Time.deltaTime),transform.position.y);
			//检测擦过
			Vector2 origin = new Vector2(transform.position.x+(dir?0.15f:-0.15f),transform.position.y);
			hit = Physics2D.Raycast(origin,dir ? 
					Vector2.right : Vector2.left, 0.1f,
				1 << (LayerMask.NameToLayer("Player")));
			if (hit.collider&&!checkhit.Contains(hit.collider.gameObject.name))
			{
				
				checkhit.Add(hit.collider.gameObject.name);
				hit.collider.GetComponent<PlayerControl>().BeAttacked(50, gameObject.GetComponent<PlayerControl>());
			}
			yield return 0;  
		}
		pc.allowable = true;
		checkhit.Clear();
		Camera.main.GetComponent<CameraFilterPack_Blur_Focus>().enabled = false;
		StopCoroutine("Rush");
	}
	public IEnumerator DrinkEnergy()
	{
		//霓虹
			PanelMgr.instance.OpenPanel<ResTipPanel>("", "E");
			Debug.Log(pc.name+" start drink");
			Camera.main.GetComponent<CameraFilterPack_Color_Chromatic_Aberration>().enabled = true;
			yield return new WaitForSeconds(1);
			Camera.main.GetComponent<CameraFilterPack_Color_Chromatic_Aberration>().enabled = false;
			StopCoroutine("DrinkEnergy");
	}
	
	public void ESkill()
	{
		//发射间隔
		//if (Time.time - lastETime < coolDownTime)
			//return;
		if (ControlKeys.Instance.ifiscooling)
			return;
		if(pc.ctrlType== CtrlType.Player)//仅本机cooldown界面
			ControlKeys.Instance.ifstartcooling = true;
		lastETime = Time.time;
		AudioSource.PlayClipAtPoint(heroClips[3], transform.position);
		switch (type)
		{
			case HeroType.Ninja:
			{
				if (pc.allowable)
				{
					pc.allowable = false;
					pc.h = 0;
					pc.rig.velocity = new Vector2(0f, 0f);
					//rush
					StartCoroutine("Rush");
					//anim
                    anim.SetTrigger("E");
				}
			}
				break;
			case HeroType.Solider:
			{
				float afterhealinghp = pc.currenthp + 60;
				pc.currenthp = Math.Min(afterhealinghp,maxHp);
				pc.healthSlider.value = pc.currenthp;
				ProtocolBytes proto = new ProtocolBytes();
				proto.AddString("ESoilder");
				NetMgr.srvConn.Send(proto);
				
				anim.SetTrigger("E");
				if (pc.ctrlType == CtrlType.Player)
				{
					Debug.Log(pc.name+" Drink Energy");
					StartCoroutine("DrinkEnergy");
					
				}
			}
				break;
			case HeroType.Roshan:
			{
				if (!ifShieldOn)
				{
					shield.GetComponent<MeshRenderer>().enabled = true;
					shield.GetComponent<DynamicLight>().enabled = true;
					ifShieldOn = true;
					anim.SetTrigger("E");
					StartCoroutine(ShiledStart());
				}
			}
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		
	}
	//getbuff
	public void GetBuff(string buffname)
	{
		int size = buffname.Length;
		string buff = buffname.Remove(size-7);//(clone)
		StartCoroutine("Get"+buff);
	}
	//buffs
	IEnumerator GetSpeedBuff() //黄
	{
		maxSpeed *= 2;
		float lastingtime = 8f;
		//camerashader
		if (pc.ctrlType == CtrlType.Player)
		{
			came3A._BorderColor = Color.yellow;
			came3A.enabled = true;
			++buffcount;
		}
		while (lastingtime > 0f)
		{
			lastingtime -= Time.deltaTime;
			yield return 0;
			
		}
		--buffcount;
		if(buffcount==0)
			came3A.enabled = false;
		maxSpeed /= 2;
		StopCoroutine("GetSpeedBuff");
	}
	IEnumerator GetAttackBuff() //红
	{
		Bullet b = bullet.GetComponent<Bullet>();
		b.WeakenSpeed /= 2;
		float lastingtime = 8f;
		//camerashader
		if (pc.ctrlType == CtrlType.Player)
		{
			came3A._BorderColor = Color.red;
			came3A.enabled = true;
			++buffcount;
		}
	while (lastingtime > 0f)
		{
			lastingtime -= Time.deltaTime;
			yield return 0;
		}
		--buffcount;
		if(buffcount==0)
			came3A.enabled = false;
		b.WeakenSpeed *= 2;
		StopCoroutine("GetAttackBuff");
	}
	IEnumerator GetDefenceBuff()//紫
	{
		defence = 5;
		float lastingtime = 8f;
		//camerashader
		if (pc.ctrlType == CtrlType.Player)
		{
			came3A._BorderColor = new Color(0.784f, 0.125f, 0.855f, 1);
			came3A.enabled = true;
			++buffcount;
		}
		//血条颜色
		while (lastingtime > 0f)
		{
			lastingtime -= Time.deltaTime;
			yield return 0;
		}
		--buffcount;
		if(buffcount==0)
			came3A.enabled = false;
		defence = 0;
		StopCoroutine("GetDefenceBuff");
	}
	IEnumerator GetCoolDownBuff()//白
	{
		if(ControlKeys.Instance.ifiscooling)
			ControlKeys.Instance.iffresh = true;
		coolDownTime /= 2f;
		float lastingtime = 8f;
		if (pc.ctrlType == CtrlType.Player)
		{
			came3A._BorderColor = Color.white;
			came3A.enabled = true;
			++buffcount;
		}
		while (lastingtime > 0f)
		{
			lastingtime -= Time.deltaTime;
			yield return 0;
		}
		--buffcount;
		if(buffcount==0)
			came3A.enabled = false;
		coolDownTime *= 2;
		StopCoroutine("GetSpeedBuff");
	}
	IEnumerator GetPoisonDebuff()//绿
	{
		float att = 5f;
		int lastingtime = 8;
		if (pc.ctrlType == CtrlType.Player)
		{
			came3A._BorderColor = Color.green;
			came3A.enabled = true;
			//洞穴
			Camera.main.GetComponent<CameraFilterPack_Blur_BlurHole>().enabled = true;
			++buffcount;
		}
		float timecount = 0f;
		while (lastingtime > 0f)
		{
			timecount += Time.deltaTime;
			if (timecount > 1f)
			{
				Debug.Log(timecount);
				timecount = 0f;
				--lastingtime;
				pc.SendHit(gameObject.name, 5);
			}
			yield return 0;
		}
		--buffcount;
		Camera.main.GetComponent<CameraFilterPack_Blur_BlurHole>().enabled =false;
		if(buffcount==0)
			came3A.enabled = false;
		StopCoroutine("GetPoisonDebuff");
	}
}
