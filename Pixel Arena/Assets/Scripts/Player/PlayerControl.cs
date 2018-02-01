using UnityEngine;
using UnityEngine.UI;
public enum PlayerState
{
    stay,
    Jump,
    DoubleJump,
}
public enum CtrlType
{
    none,
    player,
    computer,
    net,
}

public sealed class PlayerControl : MonoBehaviour
{
    public ControlKeys ck;
    public GameObject Gun;
    public GameObject HealthUI;

    //生命值
    private float maxHp = 100;
    public float currenthp = 100;
    public Slider healthSlider;
    //子弹预设
    public GameObject bullet;
    public float lastShootTime = 0f; //上一次开炮的时间
    public float shootInterval = 0.2f; //开炮的时间间隔

    public void Shoot() //开炮
    {
        //发射间隔
        if (Time.time - lastShootTime < shootInterval)
            return;
        if (bullet == null) //子弹
            return;
        //发射
        Vector2 pos = new Vector2(transform.position.x + (facingRight ? 0.2f : -0.2f), transform.position.y);
        bullet.GetComponent<Bullet>().ifright = facingRight;
        GameObject bulletObj = Instantiate(bullet, pos, transform.rotation);
        Bullet b = bulletObj.GetComponent<Bullet>();
        if (b != null)
            b.attackPlayer = this.gameObject;
        lastShootTime = Time.time;
        //如果是玩家操控，发送shoot同步信息
        if (ctrlType == CtrlType.player)
            SendShootInfo(bullet.GetComponent<Bullet>());
    }

    #region 网络同步

    //控制类型                                            
    public CtrlType ctrlType;

    Vector2 pos;

    //网络同步
    private float lastSendInfoTime = float.MinValue;

    //上次的位置信息
    Vector2 lPos;

    Vector3 lRot;

    //预测的位置信息
    Vector2 fPos;

    Vector3 fRot;

    //时间间隔
    float delta = 1;

    //上次接受的时间
    float lastRecvInfoTime = float.MinValue;

    //发送信息
    public void SendUnitInfo()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateUnitInfo");
        //位置和旋转
        Vector2 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        //转身
        int IfFlip = gameObject.GetComponent<SpriteRenderer>().flipX ? 1 : 0;

        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(rot.x);
        proto.AddFloat(rot.y);
        proto.AddFloat(rot.z);
        proto.AddInt(IfFlip);
        proto.AddFloat(h);
        NetMgr.srvConn.Send(proto);
    }

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

    //发送动画信息
    public void SendAnimInfo(string animname)
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Anim");
        proto.AddString(animname);
        NetMgr.srvConn.Send(proto);
    }

    //发送射击信息
    public void SendShootInfo(Bullet bulletTrans)
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("Shooting");
        //位置和旋转
        Vector2 pos = bulletTrans.transform.position;
        Vector3 rot = bulletTrans.transform.eulerAngles;
        //朝向
        int IfFlip = bulletTrans.ifright ? 0 : 1;
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(rot.x);
        proto.AddFloat(rot.y);
        proto.AddFloat(rot.z);
        proto.AddInt(IfFlip);
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

    //预测目标位置
    public void NetForecastInfo(Vector2 nPos, Vector3 nRot)
    {
        //预测的位置
        /*fPos = lPos + (nPos - lPos) * 2;
        fRot = lRot + (nRot - lRot) * 2;
        if(Time.time-lastRecvInfoTime>0.3f)
        {
            fPos = nPos;
            fRot = nRot;
        }
        //时间
        delta = Time.time - lastRecvInfoTime;*/
        //更新
        lPos = nPos;
        lRot = nRot;
        //lastRecvInfoTime = Time.time;
    } //

    //初始化位置预测数据
    public void InitNetCtrl()
    {
        lPos = transform.position;
        lRot = transform.eulerAngles;
        //fPos = transform.position;
        //fRot = transform.eulerAngles;
        //GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }

    //同步位置
    public void NetUpdate()
    {
        //当前位置
        //Vector2 pos = transform.position;
        //Vector3 rot = transform.eulerAngles;
        //更新位置

        if ((Vector2) transform.position != lPos)
            transform.position = lPos;

        if (transform.rotation != Quaternion.Euler(lRot))
            transform.rotation = Quaternion.Euler(lRot);

        /*if(delta>0)
        {
            transform.position = Vector2.Lerp(pos, fPos, delta);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(rot), Quaternion.Euler(fRot), delta);
        }*/
    }

    //同步翻转Flip
    public void NetFlipUpdate(int IfFlip)
    {
        int current = GetComponent<SpriteRenderer>().flipX ? 1 : 0;
        if (current != IfFlip)
            Flip();
    }

    //同步速度动画
    public void NetSpeedUpdate(float speed)
    {
        anim.SetFloat("Speed", Mathf.Abs(speed));
    }

    //同步Shooting
    public void NetShoot(Vector2 npos, Vector3 nrot, int ifflip)
    {
        bullet.GetComponent<Bullet>().ifright = (ifflip <= 0);
        GameObject bulletObj = Instantiate(bullet, pos, transform.rotation);
        Bullet b = bulletObj.GetComponent<Bullet>();
        if (b != null)
            b.attackPlayer = gameObject;
    }

    //同步Anim
    public void NetAnim(string animname)
    {
        anim.SetTrigger(animname);
        Debug.Log("netanim");
    }

    //受到伤害
    public void NetBeAttacked(float att, GameObject attackPlayer)
    {
        if (currenthp <= 0)
            return;
        if (currenthp > 0)
        {
            currenthp -= att;
            if (name == GameMgr.Instance.id)
                healthSlider.value = currenthp;
        }
        if (currenthp <= 0)
        {
            gameObject.GetComponent<DeathControl>().ifdead = true;
            ctrlType = CtrlType.none;
            //击杀提示
            if (attackPlayer != null)
            {
                PlayerControl attackpc = attackPlayer.GetComponent<PlayerControl>();
                if (attackpc != null && attackpc.ctrlType == CtrlType.player)
                {
                    attackpc.SendKill();
                    this.SendKilled();
                    //击杀提示
                }
            }
        }
    }

    #endregion

    public float h; //控制速度
    public bool allowable = true;

    //状态
    public bool ifJumpAgainstWall = false;
    public bool ifUpSideDown = false;
    public bool facingRight = true;
    public bool jump = false;
    public Transform groundCheck;
    public bool grounded = false;

    public PlayerState ps;

    //数值
    private float moveForce = 20f;
    private float maxSpeed = 2f;
    public float jumpForce = 150f;

    //音效
    public AudioClip[] jumpClips;
    public AudioClip[] Events;

    //动画
    public Animator anim;
    //类对象
    public GameObject KeyE;
    public JumpAgainstWall jaw;
    public Volume volume;
    public Rigidbody2D rig;
    //public bool ifButtonPress = false;

    void Awake()
    {
        ps = PlayerState.stay;
    }

    void Start()
    {
        HealthUI = GameObject.Find("Health");
        HealthUI.transform.GetChild(0).gameObject.SetActive(true);
        healthSlider = HealthUI.GetComponentInChildren<Slider>();
        healthSlider.value = 100;
        volume = GameObject.Find("Volume").GetComponent<Volume>();
        pos = (Vector2) gameObject.transform.position;
        rig = GetComponent<Rigidbody2D>();
        jaw = GetComponent<JumpAgainstWall>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //是否可控制
        if (!allowable) return;
        //是否由网络同步控制
        if (ctrlType == CtrlType.net)
        {
            NetUpdate();
            return;
        }
        //移动/pc端各自的controls
        ck = ControlKeys.Instance;
        ck.ifexist = true;
        //死亡
        if (currenthp <= 0) gameObject.GetComponent<DeathControl>().ifdead = true;

        #region pc端射击

#if UNITY_STANDALONE_WIN
        if (Input.GetMouseButton(0))
            Shoot();
#endif

        #endregion

        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground")) |
                   Physics2D.Linecast(transform.position, groundCheck.position,
                       1 << LayerMask.NameToLayer("YellowGround"));

        #region pc端跳跃

#if UNITY_STANDALONE_WIN ||UNITY_EDITOR
        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                jump = true;
                ps = PlayerState.Jump;
            }
            else if (ps == PlayerState.Jump && !grounded && !jaw.IfOnTheWall &&
                     GetComponent<ResPutUp>().Reses.Ability1.num == 1)
            {
                jump = true;
                ps = PlayerState.stay;
            }
        }
#endif

        #endregion

        //发送位置信息
        if ((Vector2) transform.position != pos)
        {
            SendUnitInfo();
            //pos = transform.position;
            //lastSendInfoTime = Time.time;
        }

    }

    void FixedUpdate()
    {
        if (!allowable || ctrlType != CtrlType.player)
            return;

#if UNITY_STANDALONE_WIN
       h = Input.GetAxis("Horizontal");
#endif
        if (ifUpSideDown)
            h = -h;
        anim.SetFloat("Speed", Mathf.Abs(h));
        if (h * rig.velocity.x < maxSpeed) // && grounded)
        {
            rig.AddForce(Vector2.right * h * moveForce);
        }
        //最大速度
        if (Mathf.Abs(rig.velocity.x) > maxSpeed)
            rig.velocity = new Vector2(Mathf.Sign(rig.velocity.x) * maxSpeed, rig.velocity.y);
        //翻身
        if (!ifUpSideDown)
        {
            if (h > 0 && !facingRight && !jaw.IfOnTheWall)
                Flip();
            else if (h < 0 && facingRight && !jaw.IfOnTheWall)
                Flip();
        }
        else
        {
            if (h > 0 && facingRight && !jaw.IfOnTheWall)
                Flip();
            else if (h < 0 && !facingRight && !jaw.IfOnTheWall)
                Flip();
        }
        //跳跃
        if (jump)
        {
            anim.SetTrigger("Jump");
            SendAnimInfo("Jump");
            int i = UnityEngine.Random.Range(0, jumpClips.Length);
            if (volume.ifvolumeon)
                AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);
            if (jaw.IfOnTheWall == false)
                if (Physics2D.gravity.y < 0)
                    rig.AddForce(new Vector2(0f, jumpForce));
                else
                    rig.AddForce(new Vector2(0f, -jumpForce));
            jump = false;
        }
    }

    public void Flip()
    {
        facingRight = !facingRight;
        gameObject.GetComponent<SpriteRenderer>().flipX = !gameObject.GetComponent<SpriteRenderer>().flipX;
    }
}
