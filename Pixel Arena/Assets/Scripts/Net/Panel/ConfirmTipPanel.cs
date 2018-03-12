using UnityEngine;
using UnityEngine.UI;
public class ConfirmTipPanel : PanelBase {

	private Text Text;
	private Button CloseBtn;
	private Button ConfirmBtn;
	string str = "";

	#region 生命周期
	//初始化
	public override void Init(params object[] args)
	{
		base.Init(args);
		skinPath = "ConfirmTipPanel";
		layer = PanelLayer.Tips;
		//参数args[1]表示提示的内容
		if (args.Length == 1)
			str = (string)args[0];
	}

	//显示之前
	public override void OnShowing()
	{
		base.OnShowing();
		Transform skinTrans = skin.transform;
		//文字
		Text = skinTrans.Find("Text").GetComponent<Text>();
		Text.text = str;
		//关闭按钮
		CloseBtn = skinTrans.Find("CloseButton").GetComponent<Button>();
		CloseBtn.onClick.AddListener(Close);
		//确认按钮
		ConfirmBtn = skinTrans.Find("ConfirmButton").GetComponent<Button>();
		ConfirmBtn.onClick.AddListener(OnConfirmButtonClick);
        
	}

	#endregion
	//按下确认按钮
	public void OnConfirmButtonClick()
	{
		AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
		ProtocolBytes protocol = new ProtocolBytes();
		protocol.AddString("Logout");
		NetMgr.srvConn.Send(protocol, (ProtocolBase) =>NetMgr.srvConn.Close());
		Application.Quit();
	}
}
