using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public Globals.UIState state;
	public Globals.UIState targetState;

	public GameObject colorButtonParent;
	public ColorButton newButton;

	public Button colorPicker;
	public Button brushPicker;

	public GameObject colorPanel;
	public GameObject brushPanel;

	public Button brush1;
	public Button brush2;
	public Button brush3;

	public Button[] colors;

	public Animator anim;

	public void InitializeUI(){
		SetupColors ();
	}

	public void ToggleColorSelect(){
		if (state == Globals.UIState.Idle) {
			GameManager.Instance.input.lastState = GameManager.Instance.input.state;
			GameManager.Instance.input.state = Globals.InputState.Busy;
			state = Globals.UIState.Busy;
			targetState = Globals.UIState.ColorPicker;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			colorPanel.SetActive (true);
			anim.SetTrigger ("ColorPicker");
		} 
		else if (state == Globals.UIState.ColorPicker) {
			state = Globals.UIState.Busy;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			targetState = Globals.UIState.Idle;
			for (int xx = 1; xx < colors.Length; xx++) {
				colors [xx].interactable = false;
			}
			anim.SetTrigger ("ColorPicker");
		}
	}

	public void ToggleBrushSelect(){
		if (state == Globals.UIState.Idle) {
			GameManager.Instance.input.lastState = GameManager.Instance.input.state;
			GameManager.Instance.input.state = Globals.InputState.Busy;
			state = Globals.UIState.Busy;
			targetState = Globals.UIState.BrushPicker;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			brushPanel.SetActive (true);
			anim.SetTrigger ("BrushPicker");
		} 
		else if (state == Globals.UIState.BrushPicker) {
			state = Globals.UIState.Busy;
			targetState = Globals.UIState.Idle;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			brush1.interactable = false;
			brush2.interactable = false;
			brush3.interactable = false;
			anim.SetTrigger ("BrushPicker");
		}
	}

	public void AnimFinished(){
		if (targetState == Globals.UIState.Idle) {
			colorPanel.SetActive (false);
			brushPanel.SetActive (false);
			colorPicker.interactable = true;
			brushPicker.interactable = true;
			GameManager.Instance.input.state = GameManager.Instance.input.lastState;
		} 
		else if (targetState == Globals.UIState.BrushPicker) {
			brushPicker.interactable = true;
			if (GameManager.Instance.input.brush == Globals.Brush.Small) {
				brush1.interactable = false;
				brush2.interactable = true;
				brush3.interactable = true;
			}
			else if (GameManager.Instance.input.brush == Globals.Brush.Medium) {
				brush1.interactable = true;
				brush2.interactable = false;
				brush3.interactable = true;
			}
			else if (GameManager.Instance.input.brush == Globals.Brush.Large) {
				brush1.interactable = true;
				brush2.interactable = true;
				brush3.interactable = false;
			}
		}
		else if (targetState == Globals.UIState.ColorPicker) {
			colorPicker.interactable = true;
			for (int xx = 1; xx < colors.Length; xx++) {
				if (xx != GameManager.Instance.input.selectedColor) {
					colors [xx].interactable = true;
				} else {
					colors [xx].interactable = false;
				}
			}
		}
		state = targetState;
	}

	public void ChangeBrush(int brush){
		if (brush == 1) {
			if (GameManager.Instance.input.brush != Globals.Brush.Small) {
				brush1.interactable = false;
				brush2.interactable = true;
				brush3.interactable = true;
				GameManager.Instance.input.brush = Globals.Brush.Small;
			}
		} else if (brush == 2) {
			if (GameManager.Instance.input.brush != Globals.Brush.Medium) {
				brush1.interactable = true;
				brush2.interactable = false;
				brush3.interactable = true;
				GameManager.Instance.input.brush = Globals.Brush.Medium;
			}
		} else if (brush == 3) {
			if (GameManager.Instance.input.brush != Globals.Brush.Large) {
				brush1.interactable = true;
				brush2.interactable = true;
				brush3.interactable = false;
				GameManager.Instance.input.brush = Globals.Brush.Large;
			}
		}
	}

	void SetupColors(){
		colors = new Button[GameManager.Instance.painter.colors.Length];
		for (int xx = 1; xx < GameManager.Instance.painter.colors.Length; xx++) {
			ColorButton button = Instantiate (newButton);
			colors [xx] = button.myButton;
			button.transform.SetParent (colorButtonParent.transform);
			button.myText.text = xx.ToString ();
			button.myCol.color = GameManager.Instance.painter.colors [xx];
			button.colIndex = xx;
			button.SetupSelf ();
		}
		LayoutRebuilder.MarkLayoutForRebuild(colorButtonParent.GetComponent<RectTransform> ());
	}

	public void SwitchColor(int col){
		if (col != GameManager.Instance.input.selectedColor) {
			for (int xx = 1; xx < colors.Length; xx++) {
				if (xx != col) {
					colors [xx].interactable = true;
				} else {
					colors [xx].interactable = false;
				}
			}
			GameManager.Instance.input.selectedColor = col;
			GameManager.Instance.painter.RecolorAll ();
		}
	}

}
