using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class ControlKeys : MonoBehaviour {
	//singelton
	public static ControlKeys Instance;
	//prefab
	public GameObject pre_Win;
	public GameObject pre_Mob;
	//panel
	public Transform panel;
	//keys
	public GameObject KeySpace;
	public GameObject KeyLeft;
	public GameObject KeyRight;
	
	public GameObject KeyE;
	public bool iffresh = false;
	public RectTransform KeyE_Mask;
	public Text KeyE_CD;
	public Image KeyE_img;
	public Sprite[] KeyE_imgs;
	private float CoolingSpeed;
	//state
	public PlayerControl pc;
	// Use this for initialization
	void Awake ()
	{
		Instance = this;
		//debug
		KeyE_imgs = Resources.LoadAll<Sprite>("Ui/HeroE");
		
	}
	private void Start()
	{
		//createPanel();
	}

	//射击(全平台),利用EasyControl
	void OnEnable()
	{
		//鼠标在屏幕上点击时，On_TouchStart响应一次，On_TouchDown至少响应一次；松开时On_TouchUp响应一次  
		
		EasyTouch.On_TouchDown += On_TouchDown;
		EasyTouch.On_TouchUp += On_TouchUp;
	}
	void On_TouchDown(Gesture gesture)
	{
		if (pc != null)
			if (!gesture.isHoverReservedArea)
				//pc.Hero.Shoot();
				{
					ProtocolBytes proto = new ProtocolBytes();
					proto.AddString("Ops");
					proto.AddInt(3);
					proto.AddInt(1);
					NetMgr.srvConn.Send(proto);
				}
				//pc.Ops[3] = 1;
	}

	void On_TouchUp(Gesture gesture)
	{
		if (pc != null)
			if (!gesture.isHoverReservedArea)
				//pc.Hero.Shoot();
			{
				ProtocolBytes proto = new ProtocolBytes();
				proto.AddString("Ops");
				proto.AddInt(3);
				proto.AddInt(0);
				NetMgr.srvConn.Send(proto);
			}
	}

	public float currentHeight;
	public bool ifiscooling = false;
	public bool ifstartcooling = false;
	private void Update()
	{
		if (ifstartcooling && !ifiscooling)
		{
			ifiscooling = true;
			StartCoroutine(CoolDown());//显示层
		}
	}

	public IEnumerator CoolDown()
	{
		CoolingSpeed = 50 / pc.Hero.coolDownTime;
		KeyE_Mask.sizeDelta = new Vector2(25, 50);
		currentHeight = 50;
		KeyE_img.enabled = false;
		for (; currentHeight > -0.5f; currentHeight -= Time.deltaTime * CoolingSpeed)
		{
			if (iffresh)
				break;
			KeyE_CD.text = Mathf.Ceil(currentHeight / CoolingSpeed).ToString();
			KeyE_Mask.sizeDelta = new Vector2(25, currentHeight);
			yield return 0;
		}
		currentHeight = 0f;
		KeyE_CD.text = "";
		KeyE_Mask.sizeDelta = new Vector2(25, 0f);
		KeyE_img.enabled = true;
		ifstartcooling = false;
		ifiscooling = false;
		iffresh = false;
		StopCoroutine("CoolDown");
	}

	public void createPanel()
	{
		pc = MultiBattle.Instance.list[GameMgr.Instance.id].Player;
		//pc = GameObject.Find("Roshan").GetComponent<PlayerControl>();
#if UNITY_STANDALONE_WIN
		panel = Instantiate(pre_Win).transform;
		panel.SetParent(transform, false);
		KeyE = panel.Find("KeyEvent").gameObject;
		KeyE_Mask = KeyE.transform.Find("Mask").Find("CoolDown").GetComponent<RectTransform>();
		KeyE_CD = KeyE.transform.Find("TextE").GetComponent<Text>();
		KeyE_img = KeyE.transform.Find("EImage").GetComponent<Image>();
#endif
#if UNITY_ANDROID
			panel = Instantiate(pre_Mob).transform;
			panel.SetParent(transform, false);
			KeyLeft = panel.Find("Keyleft").gameObject;
			KeyRight = panel.Find("Keyright").gameObject;
			KeySpace = panel.Find("KeyJump").gameObject;
			KeyE = panel.Find("KeyEvent").gameObject;
			KeyE_Mask = KeyE.transform.Find("Mask").Find("CoolDown").GetComponent<RectTransform>();
			KeyE_CD = KeyE.transform.Find("TextE").GetComponent<Text>();
			KeyE_img = KeyE.transform.Find("EImage").GetComponent<Image>();
			KeyLeft.GetComponent<EasyButton>().receiverGameObject = gameObject;
			KeyRight.GetComponent<EasyButton>().receiverGameObject = gameObject;
			KeySpace.GetComponent<EasyButton>().receiverGameObject = gameObject;
			KeyE.GetComponent<EasyButton>().receiverGameObject = gameObject;
		switch (pc.Hero.type)
		{
			case HeroType.Ninja:KeyE_img.sprite = KeyE_imgs[1];break;
			case HeroType.Solider:KeyE_img.sprite = KeyE_imgs[0];break;
			case HeroType.Roshan:KeyE_img.sprite = KeyE_imgs[2];break;
			default: break;
		}
#endif
	}

	public void DestroyPanel()
	{
		Destroy(panel.gameObject); 
	}

	#region 移动端按键客户端处理事件
#if UNITY_ANDROID
	public void Button_Left_Down()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(0);
        proto.AddInt(1);
        NetMgr.srvConn.Send(proto);
	}
	public void Button_Left_Up()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(0);
        proto.AddInt(0);
        NetMgr.srvConn.Send(proto);
	}
	public void Button_Right_Down()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(1);
        proto.AddInt(1);
        NetMgr.srvConn.Send(proto);
	}
	public void Button_Right_Up()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(1);
        proto.AddInt(0);
        NetMgr.srvConn.Send(proto);
	}
	public void Button_Jump_Down()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(2);
        proto.AddInt(1);
        NetMgr.srvConn.Send(proto);
		Debug.Log("fuck");
	}
	public void Button_Jump_Up()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(2);
        proto.AddInt(0);
        NetMgr.srvConn.Send(proto);
	}
	public void Button_E_Down()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(4);
        proto.AddInt(1);
        NetMgr.srvConn.Send(proto);
	}
	public void Button_E_Up()
	{
		ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Ops");
        proto.AddInt(4);
        proto.AddInt(0);
        NetMgr.srvConn.Send(proto);
	}
	
#endif
	#endregion

}
