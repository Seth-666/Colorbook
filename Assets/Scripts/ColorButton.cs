using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour {

	public Button myButton;
	public Text myText;
	public Text countText;
	public GameObject countObj;
	public Image myCol;
	public int colIndex;

	public bool inactive;

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
