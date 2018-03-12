using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class ResTipPanel : PanelBase {

	private Text Text;
	string ResName = "";
	#region 生命周期
	//初始化
	public override void Init(params object[] args)
	{
		base.Init(args);
		skinPath = "ResTipPanel";
		layer = PanelLayer.Tips;
		//参数args[1]表示提示的内容
		if (args.Length == 1)
			ResName = (string)args[0];
	}

	//显示之前
	public override void OnShowing()
	{
		base.OnShowing();
		Transform skinTrans = skin.transform;
		//文字
		Text = skinTrans.Find("ResNameText").GetComponent<Text>();

		if (ResName == "Die")
			Text.text = "You Died";
		else if (ResName == "E")
			Text.text = "Heal 50 HP";
		else if (ResName == "Start")
			Text.text = "Game Starts";
		else//buff
		{
			int size = ResName.Length;
			string buff = ResName.Remove(size-7);//(clone)
			Text.text = buff;
			switch (buff)
			{
				case "SpeedBuff":Text.color = Color.yellow;break;
				case "AttackBuff":Text.color = Color.red;break;
				case "DefenceBuff":Text.color = new Color(0.784f, 0.125f, 0.855f, 1);break;
				case "JumpForceBuff":Text.color = new Color(0.918f, 0.255f, 0.102f, 1);break;
				case "CoolDownBuff":Text.color = Color.white;break;
				case "PoisonDebuff":Text.color = Color.green;break;
			}
		}
		StartCoroutine("FadeText");
	}

	IEnumerator FadeText()
	{
		yield return new WaitForSeconds(1f);
		float curAlpha = 1f;
		Color c = Text.color;
		for (; curAlpha > -0.1f; curAlpha -= Time.deltaTime * 1.2f)
		{
			Text.color = new Color(c.r,c.g,c.b, curAlpha);
			yield return 0;
		}
		StopCoroutine("FadeText");
		Close();
	}
	
	#endregion
	//按下按钮
}
