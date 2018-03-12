using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DropDown : MonoBehaviour{
	public int maptype=0;
	public List<string> mapnames;
	//对应的显示
	private Dropdown dropDownItem;
	void Start()
	{
		//初始化
		//other_img = GetComponent<Image>();
		dropDownItem = GetComponent<Dropdown>();
		mapnames = new List<string> {"Ice Land", "Forest", "Wilderness"};
		UpdateDropDownItem(mapnames);
	}


	void UpdateDropDownItem(List<string> showNames)
	{
		dropDownItem.options.Clear();
		Dropdown.OptionData tempmap;
		for (int i = 0; i <showNames.Count; i++)
		{
			//给每一个option选项赋值
			tempmap = new Dropdown.OptionData();
			tempmap.text = showNames[i];
			//temoData.image = sprite_list[i];
			dropDownItem.options.Add(tempmap);
		}
		//初始选项的显示
		dropDownItem.captionText.text = showNames[0];
		dropDownItem.value = 0;
		//other_img.sprite = sprite_list[0];
		//dropDownItem.captionImage =other_img;

	}
	void Update()
	{
		if (maptype != dropDownItem.value)
			maptype = dropDownItem.value;
	}
}
