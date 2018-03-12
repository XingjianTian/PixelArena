using System;
using System.Collections.Generic;
using UnityEngine;

public enum PanelLayer
{
    //面板
    Panel,
    //提示
    Tips,
}


public class PanelMgr : MonoBehaviour {
    //单例
    public static PanelMgr instance;
    //画板
    private GameObject canvas;

    //面板
    public Dictionary<string, PanelBase> dict;
    //层级
    private Dictionary<PanelLayer, Transform> layerDict;

    public void Awake()
    {
        instance = this;
        InitLayer();
        dict = new Dictionary<string, PanelBase>();
    }

    //初始化层
    public void InitLayer()
    {
        //画布
        canvas = GameObject.Find("Canvas");
        if (canvas == null)
            Debug.Log("PanelMgr.InitLayer fail,canvas is null");
        layerDict = new Dictionary<PanelLayer, Transform>();
        foreach (PanelLayer pl in Enum.GetValues(typeof(PanelLayer)))
        {
            string name = pl.ToString();
            Transform transform = canvas.transform.Find(name);
            layerDict.Add(pl, transform);
        }
        
        
    }

    //泛型打开面板，T表示面板类型
    public void OpenPanel<T>(string skinPath,params object[] args)where T:PanelBase
    {
        //已经打开
        string name = typeof(T).ToString();
        if (dict.ContainsKey(name))
            return;
        //给Canvas添加面板脚本
        PanelBase panel = canvas.AddComponent<T>();
        panel.Init(args);
        dict.Add(name, panel);

        //加载皮肤
        skinPath = (skinPath != "" ? skinPath : panel.skinPath);
        GameObject skin = Resources.Load<GameObject>(skinPath);
        if (skin == null)
            Debug.LogError("PanelMgr.OpenPanel fail,skin is null,skinpath = " + skinPath);
        panel.skin = Instantiate(skin);
        //坐标
        Transform skinTrans = panel.skin.transform;
        PanelLayer layer = panel.layer;
        Transform parent = layerDict[layer];
        skinTrans.SetParent(parent, false);
        //Panel的生命周期
        panel.OnShowing();
        //anm预留动画实现
        panel.OnShowed();
    }

    //关闭面板
    public void ClosePanel(string name)
    {
        PanelBase panel = dict[name];
        if (panel == null)//如果没有打开不存在关闭情况
            return;
        panel.OnClosing();
        dict.Remove(name);
        panel.OnClosed();
        Destroy(panel.skin);//销毁皮肤
        Destroy(panel);//销毁面板组件
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
