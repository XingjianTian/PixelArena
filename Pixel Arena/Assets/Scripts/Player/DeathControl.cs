using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathControl : MonoBehaviour
{
    private string text = "You Died";
    public bool OnceAnim = true;
    public bool isgrounded;
    public bool ifdead = false;
    //public double speed;
    //public float Yangle;
    public PlayerControl pc;
    // Use this for initialization
    void Start()
    {
        pc = GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        isgrounded = pc.grounded;
        /*speed = GetComponent<Rigidbody2D>().velocity.magnitude;
        Yangle = GetComponent<Rigidbody2D>().velocity.y;
        if (speed >= 8f&&Yangle<-7f&&!ifdead)
        {
            ifdead = true;
           // if(isgrounded == true)
             //   GetComponent<PlayerControl>().enabled = false;
            //anim.SetTrigger("Die");
        }*/
        if (ifdead && isgrounded)
        {
            transform.gameObject.layer = 8;
            pc.enabled = false;
            if (OnceAnim)
            {
                pc.anim.SetTrigger("Die");
                OnceAnim = false;
                
            }
            //GetComponent<DeathControl>().enabled = false;
        }
    }
}