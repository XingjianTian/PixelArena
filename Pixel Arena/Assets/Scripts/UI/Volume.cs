using UnityEngine;

public class Volume : MonoBehaviour
{

    public static Volume instance;
    public AudioSource BGM;
    public bool ifvolumeon;
    public Texture2D volumeoff;
    public Texture2D volumeon;
    public EasyButton vb;
    public AudioClip[] Events;//0 按钮 1捡东西
    void Start ()
    {
        instance = this;
        
        vb = GameObject.Find("VolumeBtn").GetComponent<EasyButton>();
        if(GameObject.Find("BGM_Audio")!=null)
            BGM = GameObject.Find("BGM_Audio").GetComponent<AudioSource>();
        ifvolumeon = true;
	}

    private void Update()
    {
        if(BGM!=null)
            if(BGM.volume<=0.15f)
                BGM.volume += Time.deltaTime / 6;
    }

    public void OnChangeVolumeSet()
    {
        if(ifvolumeon==true)
        {
            ifvolumeon = false;
            vb.NormalTexture = volumeoff;
            AudioListener.pause = true;
        }
        else if(ifvolumeon == false)
        {
            ifvolumeon = true;
            vb.NormalTexture = volumeon;
            AudioListener.pause = false;
        }
        
    }
    public void Button_Close_Down()
    {
        PanelMgr.instance.OpenPanel<ConfirmTipPanel>("","Want to quit ?");
    }
        

}
