using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattle : MonoBehaviour {

    //单例
    public static MultiBattle Instance;
    /*
    {
        get { return Nested.instance; }
    }

    class Nested
    {
        static Nested(){ }
        internal static readonly MultiBattle instance = new MultiBattle();
    }*/
    //角色预设
    public GameObject[] playerPrefabs;
    //场内的所有角色
    public Dictionary<string, BattlePlayer> list = new Dictionary<string, BattlePlayer>();
	// Use this for initialization
	void Start ()
	{
	    Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //获取阵营，0表示错误
    public int GetCamp(GameObject playerObj)
    {
        foreach(BattlePlayer p in list.Values)
        {
            if (p.Player.gameObject == playerObj)
                return p.camp;
        }
        return 0;
    }
    //是否同一阵营
    public bool IfSameCamp(GameObject p1,GameObject p2)
    {
        return GetCamp(p1) == GetCamp(p2);
    }

    //清理场景，初始化角色列表
    public void ClearBattle()
    {
        list.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0;i<players.Length;i++)
        {
            Destroy(players[i]);
        }
        Destroy(GameObject.FindGameObjectWithTag("Controls"));
    }

    //开始战斗
    public void StartBattle(ProtocolBytes proto)
    {
        //解析协议
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        if (protoName != "Fight")
            return;
        //玩家总数
        int count = proto.GetInt(start, ref start);
        //清理场景
        ClearBattle();
        for(int i= 0;i<count;i++)
        {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            int bornPosID = proto.GetInt(start, ref start);
            GeneratePlayer(id, team, bornPosID);
        }
        //还有oncelistenner已添加过
        NetMgr.srvConn.msgDist.AddListener("UpdateUnitInfo",RecvUpdateUnitInfo);
        NetMgr.srvConn.msgDist.AddListener("Shooting",RecvShooting);
        NetMgr.srvConn.msgDist.AddListener("Anim",Recvanim);
        NetMgr.srvConn.msgDist.AddListener("Hit",RecvHit);
        NetMgr.srvConn.msgDist.AddListener("Result",RecvResult);

    }
    
    //收到更新角色单位协议，都是委托
    public void RecvUpdateUnitInfo(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        Vector2 nPos;
        Vector3 nRot;
        int IfFlip;
        float speed;
        nPos.x = proto.GetFloat(start, ref start);
        nPos.y = proto.GetFloat(start, ref start);
        nRot.x = proto.GetFloat(start, ref start);
        nRot.y = proto.GetFloat(start, ref start);
        nRot.z = proto.GetFloat(start, ref start);
        IfFlip = proto.GetInt(start, ref start);
        speed = proto.GetFloat(start, ref start);
        //处理
        //Debug.Log("RecvUpdateUnitInfo " + id);
        if (!list.ContainsKey(id))
        {
            //Debug.Log("RecvUpdateUnitInfo bt == null");
            return;
        }
        BattlePlayer Bp = list[id];
        if (id == GameMgr.Instance.id)
            return;
        Bp.Player.NetForecastInfo(nPos, nRot);
        Bp.Player.NetFlipUpdate(IfFlip);
        Bp.Player.NetSpeedUpdate(speed);
    
    }
    //收到hit协议
    public void RecvHit(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        string attID = proto.GetString(start, ref start);
        string defID = proto.GetString(start, ref start);
        float damage = proto.GetFloat(start, ref start);
        //获取BattlePlayer
        if (!list.ContainsKey(attID))
        {
            //Debug.Log("RecvHit attplayer==null"+attID);
            return;
        }
        BattlePlayer attBp = list[attID];
        if (!list.ContainsKey(defID))
        {
            //Debug.Log("RecvHit defplayer==null"+defID);
            return;
        }
        BattlePlayer defBp = list[defID];
        //被击中的玩家
        defBp.Player.NetBeAttacked(damage,attBp.Player.gameObject);
    }
    //收到anim协议
    public void Recvanim(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        string animname = proto.GetString(start, ref start);
        if (!list.ContainsKey(id))
        {
            //Debug.Log("RecvUpdateUnitInfo bt == null");
            return;
        }
        BattlePlayer Bp = list[id];
        if (id == GameMgr.Instance.id)//跳过自己的同步信息
            return;
        Bp.Player.NetAnim(animname);
    }
    //收到Shooting协议
    public void RecvShooting(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        Vector2 nPos;
        Vector3 nRot;
        nPos.x = proto.GetFloat(start, ref start);
        nPos.y = proto.GetFloat(start, ref start);
        nRot.x = proto.GetFloat(start, ref start);
        nRot.y = proto.GetFloat(start, ref start);
        nRot.z = proto.GetFloat(start, ref start);
        int ifflip = proto.GetInt(start, ref start);
        //处理
        //Debug.Log("RecvUpdateUnitInfo " + id);
        if (!list.ContainsKey(id))
        {
            //Debug.Log("RecvUpdateUnitInfo bt == null");
            return;
        }
        BattlePlayer Bp= list[id];
        if (id == GameMgr.Instance.id)//跳过自己的同步信息
            return;
        Bp.Player.NetShoot(nPos,nRot,ifflip);
        
    }
    //收到Result协议
    public void RecvResult(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        int winteam = proto.GetInt(start, ref start);
        
        //弹出胜负面板
        string id = GameMgr.Instance.id;
        BattlePlayer Bp = list[id];
        StartCoroutine(WaitAndOpenTip(Bp, winteam));
        //取消监听
        NetMgr.srvConn.msgDist.DelListener("UpdateUnitInfo",RecvUpdateUnitInfo);
        NetMgr.srvConn.msgDist.DelListener("Shooting",RecvShooting);
        NetMgr.srvConn.msgDist.DelListener("Anim",Recvanim);
        NetMgr.srvConn.msgDist.DelListener("Hit",RecvHit);
        NetMgr.srvConn.msgDist.DelListener("Result",RecvResult);
    }
    IEnumerator WaitAndOpenTip(BattlePlayer Bp,int winteam)
    {
        yield return new WaitForSeconds(3f);
        if(Bp.camp == winteam)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "你胜利了！");
            Bp.Player.HealthUI.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "你失败了！");
            Bp.Player.HealthUI.transform.GetChild(0).gameObject.SetActive(false);
        }
    
        
    }
    public void GeneratePlayer(string id, int team, int bornPosID)
    {
        //获取出生点
        Transform bornPoints = GameObject.Find("BornPoints").transform;
        Transform bornTrans;
        if(team==1)
        {
            Transform teamBorn = bornPoints.GetChild(0);
            bornTrans = teamBorn.GetChild(bornPosID - 1);
        }
        else
        {
            Transform teamBorn = bornPoints.GetChild(1);
            bornTrans = teamBorn.GetChild(bornPosID - 1);
        }
        if(bornTrans==null)
        {
            Debug.LogError("GeneratePlayer 出生点错误!");
            return;
        }
        //预设
        if(playerPrefabs.Length<2)
        {
            Debug.LogError("角色预设数量不够！");
            return;
        }
        //产生角色
        GameObject playerObj = (GameObject)Instantiate(playerPrefabs[team - 1]);
        playerObj.name = id;
        playerObj.GetComponentInChildren<TextMesh>().text = id;
        playerObj.transform.position = bornTrans.position;

        //列表处理
        BattlePlayer Bp = new BattlePlayer();
        Bp.Player = playerObj.GetComponent<PlayerControl>();
        Bp.camp = team;
        list.Add(id, Bp);

        //玩家处理
        if(id==GameMgr.Instance.id)
        {
            Bp.Player.ctrlType = CtrlType.player;
            CameraMoveWithPlayer cmwp = Camera.main.gameObject.GetComponent<CameraMoveWithPlayer>();
            cmwp.SetCharaterTarget(Bp.Player.gameObject);
        }
        else
        {
            Bp.Player.ctrlType = CtrlType.net;
            Bp.Player.InitNetCtrl();
        }
    }

}
