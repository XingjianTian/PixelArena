using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Volume : MonoBehaviour {

    public AudioSource BGM;
    private GameObject came;
    private AudioListener cameAL;
    public bool ifvolumeon;
    public Sprite VolumeOn;
    public Sprite VolumeOff;
    public Image img;
    void Start ()
    {
        if(GameObject.Find("BGM_Audio")!=null)
            BGM = GameObject.Find("BGM_Audio").GetComponent<AudioSource>();
        img = GetComponent<Image>();
        ifvolumeon = true;
	}

    private void Update()
    {
        if(BGM!=null)
            if(BGM.volume<=0.2f)//&&!scenedoor.ifAudioFade)
                BGM.volume += Time.deltaTime / 6;
    }

    public void OnChangeVolumeSet()
    {
        if(ifvolumeon==true)
        {
            ifvolumeon = false;
            img.sprite = VolumeOff;
            AudioListener.pause = true;
        }
        else if(ifvolumeon == false)
        {
            ifvolumeon = true;
            img.sprite = VolumeOn;
            AudioListener.pause = false;
        }
        
    }

    public void Button_Close_Down()
    {
        Application.Quit();
    }
        

}
