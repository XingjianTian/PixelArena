using System.Collections;
using UnityEngine;
using System;
using Slider = UnityEngine.UI.Slider;

public enum PlayerState
{
    Stay,
    Jump,
    DoubleJump,
}
public enum CtrlType
{
    Player,
    Net,
}

public sealed class PlayerControl : MonoBehaviour
{
    //英雄特性
    public Heroes Hero;
    //公用状态及量
    public bool allowable = true;//可控制
    public bool facingRight = true;//左右
    public int h; //控制速度

    public float realh;
    //检查是否在地面上
    public Transform groundCheck;
    public Transform groundCheck1;
    public bool grounded = false;
    public PlayerState ps;//state
    private SpriteRenderer sp;//flip
    public Rigidbody2D rig;
    
    //忍者跳墙
    public JumpAgainstWall jaw{get { return Hero.jaw; }}
    public bool ifJumpAgainstWall = false;
    //血条
    public GameObject CanvasHealth;
    public float currenthp;
    public Slider healthSlider;
    //死亡
    public bool OnceDieAnim = true;
    public bool ifdead = false;
    
    void Awake()
    {
        Hero = gameObject.GetComponent<Heroes>();
        ps = PlayerState.Stay;
        
    }
    void Start()
    {
        CanvasHealth = transform.Find("CanvasHealth").gameObject;
        CanvasHealth.transform.GetChild(0).gameObject.SetActive(true);
        healthSlider = CanvasHealth.GetComponentInChildren<Slider>();
        healthSlider.maxValue = Hero.maxHp;
        healthSlider.value = Hero.maxHp;
        currenthp = Hero.maxHp;
        rig = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        //检查是否在地面
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        grounded = (grounded || (Physics2D.Linecast(transform.position, groundCheck1.position, 1 << LayerMask.NameToLayer("Ground"))));
        //已死亡
        if (ifdead && grounded)
        {
            transform.gameObject.layer = 8;
            allowable = false;
            if (OnceDieAnim)
            {
                Hero.anim.SetTrigger("Die");
                if (ctrlType == CtrlType.Player) //本机
                {
                    //AudioSource.PlayClipAtPoint(Volume.instance.Events[2], transform.position);
                    PanelMgr.instance.OpenPanel<ResTipPanel>("", "Die");
                }
                OnceDieAnim = false;
            }
            return;
        }
        
        //只发送自己
        if (ctrlType != CtrlType.Player)
            return;
        
#if UNITY_STANDALONE_WIN||UNITY_EDITOR
        //left
        #region ops
        if (Input.GetKeyDown(KeyCode.A))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(0);
            proto.AddInt(1);
            NetMgr.srvConn.Send(proto);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(0);
            proto.AddInt(0);
            NetMgr.srvConn.Send(proto);
        }
        //right
        if (Input.GetKeyDown(KeyCode.D))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(1);
            proto.AddInt(1);
            NetMgr.srvConn.Send(proto);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(1);
            proto.AddInt(0);
            NetMgr.srvConn.Send(proto);
        }
        //jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(2);
            proto.AddInt(1);
            NetMgr.srvConn.Send(proto);
            //Debug.Log(GameMgr.Instance.id+" Press jump");
            
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(2);
            proto.AddInt(0);
            NetMgr.srvConn.Send(proto);
        }
        //射击在外部改变op3
        if (Input.GetKeyDown(KeyCode.E))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(4);
            proto.AddInt(1);
            NetMgr.srvConn.Send(proto);
            
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("Ops");
            proto.AddInt(4);
            proto.AddInt(0);
            NetMgr.srvConn.Send(proto);
           
        }
        #endregion
#endif
    }
    void FixedUpdate()
    {
        realh = h;
        #region 对h的输出
        Hero.anim.SetFloat("Speed", Math.Abs(realh));
        //翻身
        if (realh!=0&&realh>0 ^ facingRight)
        {
            if (Hero.type == HeroType.Ninja)
            {
                if (!jaw.IfOnTheWall)
                    Flip();
            }
            else
                Flip();
        }
        if (realh * rig.velocity.x < (float)Hero.maxSpeed) // && grounded)
        {
            rig.AddForce(Vector2.right * realh * Hero.moveForce);
        }
        //最大速度
        if (Mathf.Abs(rig.velocity.x) > (float)Hero.maxSpeed)
            rig.velocity = new Vector2(Mathf.Sign(rig.velocity.x) * (float)Hero.maxSpeed, rig.velocity.y);
        #endregion
    }
    public void Flip()
    {
        if (!allowable) 
            return;
        facingRight = !facingRight;
        sp.flipX = !sp.flipX;
    }
    #region 帧同步
    
    IEnumerator JudgeJumpFinshed()
    {
        yield return new WaitForSeconds(0.2f);
        while(true)
        {
            if (grounded && Math.Abs(rig.velocity.y) < 0.01f)
                break;
            yield return 0;
        }
        ps = PlayerState.Stay;
        StopCoroutine("JudgeJumpFinshed");
        
    }
    public void ProcessOps(int[]ops)
    {
        if (ops[0] == 0 && ops[1] == 0)
            h =0;
        else
        {
            if (ops[0] == 1&&allowable) //left
            {
                if (jaw)
                {
                    if (jaw.IfOnTheWall)
                        h = 0;
                    else if (jaw.ifJumpAgainstFinished)
                        //h = (VInt)(realh + (-1 - realh) * 0.5f);
                        h = -1;
                }
                else
                    //h = (VInt)(realh + (-1 - realh) * 0.5f);
                    h = -1;
            }
            if (ops[1] == 1&&allowable) //right
            {
                if (jaw)
                {
                    if (jaw.IfOnTheWall)
                        h = 0;
                    else if (jaw.ifJumpAgainstFinished)
                        //h = (VInt)(realh + (1 - realh) * 0.5f);
                        h = 1;
                }
                else
                    //h = (VInt)(realh + (1 - realh) * 0.5f);
                    h = 1;
            }
        }
        
        if (ops[2] == 1&&allowable)//jump
        {
            if (Time.time - Hero.lastJumpTime < Hero.JumpInterval)
                return;
            
            if (grounded&&ps== PlayerState.Stay)
            {
                ps = PlayerState.Jump;
                StartCoroutine("JudgeJumpFinshed");
                Hero.Jump();
            }
            //二段跳
            else if (ps == PlayerState.Jump && !grounded &&Hero.type == HeroType.Ninja
                     &&!jaw.IfOnTheWall && jaw.ifJumpAgainstFinished)
            {
                ps = PlayerState.DoubleJump;
                StartCoroutine("JudgeJumpFinshed");
                Hero.Jump();
            }
        }
        if (ops[3] == 1&&allowable) //shoot
        {
            Hero.Shoot();
        }
        if (ops[4] == 1&&allowable)//e
        {
            Hero.ESkill();
        }
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)//捡道具
    {
        if (other.CompareTag("Res"))//tag res
        {
            if (ctrlType == CtrlType.Player) //本机显示
            {
                AudioSource.PlayClipAtPoint(Volume.instance.Events[1], transform.position);
                PanelMgr.instance.OpenPanel<ResTipPanel>("",other.name);
            }
            Destroy(other.gameObject);//摧毁buff石头
            Hero.GetBuff(other.name);
        }
    }
    #region 网络同步某些信息
    //控制类型                                            
    public CtrlType ctrlType;
    //网络同步
    public void SendKill()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("AddKillNum");
        NetMgr.srvConn.Send(proto);
    }
    public void SendKilled()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("AddKilledNum");
        NetMgr.srvConn.Send(proto);
    }
    //发送hit信息
    public void SendHit(string id, float damage)
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Hit");
        proto.AddString(id); //敌人id
        proto.AddFloat(damage);
        NetMgr.srvConn.Send(proto);
    }
    //受到伤害
    public void BeAttacked(float att, PlayerControl attackpc)
    {
        if (currenthp <= 0)
            return;
        if (currenthp > 0)
        {
            if (Hero.type == HeroType.Roshan && Hero.ifShieldOn == true)
                return;
            currenthp -= att;
            healthSlider.value = currenthp;
            if(GameMgr.Instance.id == name)
                DeathCameraFade.Instance.ifflash = true;
        }
        if (currenthp <= 0)
        {
            healthSlider.enabled = false;
            ifdead = true;
            //击杀提示
            if (attackpc != null)// && attackpc.ctrlType == CtrlType.player)
            {
                attackpc.SendKill();
                this.SendKilled();
                //击杀提示
            }
        }
    }
    #endregion

}
