using UnityEngine;

public class JumpAgainstWall : MonoBehaviour {
    public bool IfOnTheWall = false;
    public float WallJumpForce = 10f;
    public bool JumpAgainst = false;
    public bool ifJumpAgainstFinished = true;
    public Animator anim;
    public PlayerControl pc;
    private Rigidbody2D rig;
    // Use this for initialization
    void Start () {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pc = GetComponent<PlayerControl>();
	}

	// Update is called once per frame
	void Update ()
    {
        if (!pc.allowable) 
            return;

        if (pc.grounded)
            IfOnTheWall = false;
        if (IfOnTheWall && pc.ps== PlayerState.Jump&&
            (pc.ifJumpAgainstWall||Input.GetKeyDown(KeyCode.Space)))
        {
            JumpAgainst = true;
            ifJumpAgainstFinished = false;
        }
        //跳
        if (JumpAgainst && !ifJumpAgainstFinished)
        {
            rig.velocity = Vector2.zero;
            bool iffacingright = pc.facingRight;
            anim.SetBool("WallRide", false);
            float x = iffacingright ? 1 : -1;
            float y = 0.9f;//pc.ifUpSideDown ? -0.9f : 0.9f;
            rig.AddForce(new Vector2(x, y)* WallJumpForce);
            anim.SetTrigger("Jump");
            pc.ifJumpAgainstWall = false;
            JumpAgainst = false;
        }
        //跳墙落地结束
        if (pc.grounded)
            ifJumpAgainstFinished = true;
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.transform.position.x < transform.position.x) //wall is left
        {
            
        }
        if (col.gameObject.CompareTag("Walls") && !pc.grounded&&!JumpAgainst)
        {
            anim.SetBool("WallRide",true);
            if (!IfOnTheWall&&col.transform.position.x < transform.position.x
                ^ pc.facingRight)
                pc.Flip();
            IfOnTheWall = true;
        }
    }
    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Walls") && !pc.grounded&&!JumpAgainst)
        {
            anim.SetBool("WallRide",true);
            if (!IfOnTheWall&&col.transform.position.x < transform.position.x
                ^ pc.facingRight)
                pc.Flip();
            IfOnTheWall = true;
            rig.gravityScale = 0.5f;
        }
    }
    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Walls") && !pc.grounded && !JumpAgainst)
        {
            IfOnTheWall = false;
        }
        anim.SetBool("WallRide", false);
        rig.gravityScale = 1f;
    }
}
