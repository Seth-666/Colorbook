using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour {

	public Button myButton;
	public Text myText;
	public Image myCol;
	public int colIndex;

	void Start(){
		this.transform.localScale = Vector3.one;
	}

	public void SetupSelf(){
		myButton.onClick.AddListener (delegate {
			GameManager.Instance.ui.SwitchColor (colIndex);
		});
		myButton.interactable = false;
	}

}
