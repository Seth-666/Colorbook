using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteButton : MonoBehaviour {

	public Button myButton;
	public Image myCol;
	public Image locked;
	public SpriteData data;
	public ProgressData progress;

	void Start(){
		this.transform.localScale = Vector3.one;
	}

}
