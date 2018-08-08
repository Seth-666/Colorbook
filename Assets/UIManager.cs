using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public Slider zoomLevel;
	public Text zoomText;

	public void SetZoom(){
		zoomText.text = zoomLevel.value.ToString ();
		GameManager.Instance.cam.zoomSpeed = zoomLevel.value;
	}

	public void PrevColor(){
		int selCol = GameManager.Instance.input.selectedColor - 1;
		if (selCol < 1) {
			selCol = GameManager.Instance.painter.colors.Length - 1;
		}
		GameManager.Instance.input.selectedColor = selCol;
		GameManager.Instance.painter.RecolorAll ();
	}

	public void NextColor(){
		int selCol = GameManager.Instance.input.selectedColor + 1;
		if (selCol >= GameManager.Instance.painter.colors.Length) {
			selCol = 1;
		}
		GameManager.Instance.input.selectedColor = selCol;
		GameManager.Instance.painter.RecolorAll ();
	}

}
