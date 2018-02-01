using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ControlKeys : MonoBehaviour {
	//singelton
	public static ControlKeys Instance;
	//prefab
	public GameObject pre_Win;
	public GameObject pre_Mob;
	//keys
	public GameObject KeySpace;
	public GameObject KeyLeft;
	public GameObject KeyRight;
	public GameObject KeyE;
	//state
	public bool ifexist = false;
	public bool ifcreated = false;
	
	public float k = 0f;
	public PlayerControl pc;
	// Use this for initialization
	void Start ()
	{
		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		if (ifexist&&!ifcreated)
		{
#if UNITY_STANDALONE_WIN
			GameObject panel = Instantiate(pre_Win);
			panel.transform.SetParent(transform, false);
#endif
#if UNITY_ANDROID
			pc = MultiBattle.Instance.list[GameMgr.Instance.id].Player;
			//pc = GameObject.Find("NewHero").GetComponent<PlayerControl>();
			Transform panel = Instantiate(pre_Mob).transform;
			Transform control = panel.Find("Controls");
			panel.transform.SetParent(transform, false);
			KeyLeft = control.Find("Keyleft").gameObject;
			KeyRight = control.Find("Keyright").gameObject;
			KeySpace = control.Find("KeyJump").gameObject;
			KeyE = control.Find("KeyEvent").gameObject;

			KeyLeft.GetComponent<EasyButton>().receiverGameObject = gameObject;
			KeyRight.GetComponent<EasyButton>().receiverGameObject = gameObject;
			KeySpace.GetComponent<EasyButton>().receiverGameObject = gameObject;
			KeyE.GetComponent<EasyButton>().receiverGameObject = gameObject;
#endif
			ifcreated = true;
		}
	}

	private void FixedUpdate()
	{
#if UNITY_ANDROID
		if (pc!=null&&Input.touchCount == 0)
			pc.h *= 0.5f;
#endif
	}

	#region 移动端按键事件和射击
#if UNITY_ANDROID
	public void Button_Left_Down()
	{
		if (pc.jaw.ifJumpAgainstFinished) {
			//h = h + (-1 - h) * 0.5f;
			pc.h = pc.h + (-1 - pc.h)*0.5f;
		}
	}
	public void Button_Left_Press()
	{
		if (pc.jaw.ifJumpAgainstFinished) {
			//h = h + (-1 - h) * 0.5f;
			pc.h = pc.h + (-1 - pc.h)*0.5f;
		}
	}
	public void Button_Right_Down()
	{
		if (pc.jaw.ifJumpAgainstFinished) {
			//h = h + (-1 - h) * 0.5f;
			pc.h = pc.h + (1 - pc.h)*0.5f;
		}
	}
	public void Button_Right_Press()
	{
		if (pc.jaw.ifJumpAgainstFinished) {
			//h = h + (-1 - h) * 0.5f;
			pc.h = pc.h+ (1 - pc.h)*0.5f;
		}
	}

	public void Button_Jump_Down()
	{
		if (pc.jaw.IfOnTheWall)
			pc.ifJumpAgainstWall = true;
		if (pc.grounded)
		{
			pc.jump = true;
			pc.ps = PlayerState.Jump;

			if (pc.ps == PlayerState.Jump && !pc.grounded && !pc.jaw.IfOnTheWall
			    && pc.gameObject.GetComponent<ResPutUp>().Reses.Ability1.num == 1)
			{
				pc.jump = true;
				pc.ps = PlayerState.stay;
			}
		}
	}
	
#endif
	void OnEnable()
	{
		//鼠标在屏幕上点击时，On_TouchStart响应一次，On_TouchDown至少响应一次；松开时On_TouchUp响应一次  
		EasyTouch.On_TouchDown += On_TouchDown;
	}
	void On_TouchDown(Gesture gesture)
	{
		if (pc != null)
		{
			if (!gesture.isHoverReservedArea)
			{
				pc.Shoot();
			}
		}
	}
	#endregion
}
