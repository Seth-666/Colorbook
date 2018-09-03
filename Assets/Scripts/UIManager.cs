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
	public Button menuButton;
	public Button mainMenu;
	public Button reset;
	public Button back;

	public GameObject colorPanel;
	public GameObject brushPanel;

	public Button brush1;
	public Button brush2;
	public Button brush3;

	public ColorButton[] colors;

	public Animator anim;

	public Sprite colorIcon;
	public Sprite tickIcon;

	public void Update(){
		if(Input.GetKey(KeyCode.Escape)){
			if (state == Globals.UIState.Idle) {
				ToggleMenu ();
			} else if (state == Globals.UIState.BrushPicker) {
				ToggleBrushSelect ();
			} else if (state == Globals.UIState.ColorPicker) {
				ToggleColorSelect ();
			} else if (state == Globals.UIState.Menu) {
				ToggleMenu ();
			}
		}
	}

	public void InitializeUI(){
		SetupColors ();
		ToggleColorSelect ();
	}

	public void MainMenu(){
		GameManager.Instance.painter.SaveData ();
		UnityEngine.SceneManagement.SceneManager.LoadScene (0, UnityEngine.SceneManagement.LoadSceneMode.Single);
	}

	public void ResetProgress(){
		GameManager.Instance.painter.ResetProgress ();
		ToggleMenu ();
	}

	public void Back(){
		ToggleMenu ();
	}

	public void ToggleMenu(){
		if (state == Globals.UIState.Idle) {
			GameManager.Instance.input.lastState = GameManager.Instance.input.state;
			GameManager.Instance.input.state = Globals.InputState.Busy;
			state = Globals.UIState.Busy;
			targetState = Globals.UIState.Menu;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			menuButton.interactable = false;
			anim.SetTrigger ("Menu");
		} else if (state == Globals.UIState.Menu) {
			state = Globals.UIState.Busy;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			menuButton.interactable = false;
			targetState = Globals.UIState.Idle;
			anim.SetTrigger ("Menu");
		}
	}

	public void ToggleColorSelect(){
		if (state == Globals.UIState.Idle) {
			GameManager.Instance.input.lastState = GameManager.Instance.input.state;
			GameManager.Instance.input.state = Globals.InputState.Busy;
			state = Globals.UIState.Busy;
			targetState = Globals.UIState.ColorPicker;
			for (int xx = 1; xx < colors.Length; xx++) {
				colors [xx].countText.text = GameManager.Instance.painter.colorCount [xx].ToString ();
			}
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			menuButton.interactable = false;
			anim.SetTrigger ("ColorPicker");
		} 
		else if (state == Globals.UIState.ColorPicker) {
			state = Globals.UIState.Busy;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			menuButton.interactable = false;
			targetState = Globals.UIState.Idle;
			for (int xx = 1; xx < colors.Length; xx++) {
				colors [xx].myButton.interactable = false;
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
			menuButton.interactable = false;
			anim.SetTrigger ("BrushPicker");
		} 
		else if (state == Globals.UIState.BrushPicker) {
			state = Globals.UIState.Busy;
			targetState = Globals.UIState.Idle;
			colorPicker.interactable = false;
			brushPicker.interactable = false;
			menuButton.interactable = false;
			brush1.interactable = false;
			brush2.interactable = false;
			brush3.interactable = false;
			anim.SetTrigger ("BrushPicker");
		}
	}

	public void AnimFinished(){
		if (targetState == Globals.UIState.Idle) {
			colorPicker.interactable = true;
			brushPicker.interactable = true;
			menuButton.interactable = true;
			GameManager.Instance.input.state = GameManager.Instance.input.lastState;
		} else if (targetState == Globals.UIState.BrushPicker) {
			brushPicker.interactable = true;
			if (GameManager.Instance.input.brush == Globals.Brush.Small) {
				brush1.interactable = false;
				brush2.interactable = true;
				brush3.interactable = true;
			} else if (GameManager.Instance.input.brush == Globals.Brush.Medium) {
				brush1.interactable = true;
				brush2.interactable = false;
				brush3.interactable = true;
			} else if (GameManager.Instance.input.brush == Globals.Brush.Large) {
				brush1.interactable = true;
				brush2.interactable = true;
				brush3.interactable = false;
			}
		} else if (targetState == Globals.UIState.ColorPicker) {
			colorPicker.interactable = true;
			for (int xx = 1; xx < colors.Length; xx++) {
				if (xx != GameManager.Instance.input.selectedColor) {
					colors [xx].countText.text = GameManager.Instance.painter.colorCount [xx].ToString ();
					if (GameManager.Instance.painter.colorCount [xx] > 0) {
						colors [xx].myButton.interactable = true;
						if (colors [xx].inactive) {
							colors [xx].myCol.sprite = colorIcon;
							colors [xx].countObj.SetActive (true);
							colors [xx].myText.gameObject.SetActive (true);
							colors [xx].inactive = false;
							colors [xx].myButton.transition = Selectable.Transition.ColorTint;
							colors [xx].myCol.color = GameManager.Instance.painter.colors [colors [xx].colIndex];
						}
					} else {
						colors [xx].myButton.interactable = false;
						if (!colors [xx].inactive) {
							colors [xx].myCol.sprite = tickIcon;
							colors [xx].countObj.SetActive (false);
							colors [xx].myText.gameObject.SetActive (false);
							colors [xx].inactive = true;
							colors [xx].myButton.transition = Selectable.Transition.None;
							colors [xx].myCol.color = Color.white;
						}

					}
				} else {
					colors [xx].myButton.interactable = false;
				}
			}
		} else if (targetState == Globals.UIState.Menu) {
			mainMenu.interactable = true;
			reset.interactable = true;
			back.interactable = true;
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
		colors = new ColorButton[GameManager.Instance.painter.colors.Length];
		for (int xx = 1; xx < GameManager.Instance.painter.colors.Length; xx++) {
			ColorButton button = Instantiate (newButton);
			colors [xx] = button;
			button.transform.SetParent (colorButtonParent.transform);
			button.myText.text = xx.ToString ();
			button.myCol.color = GameManager.Instance.painter.colors [xx];
			button.colIndex = xx;
			button.countText.text = GameManager.Instance.painter.colorCount [xx].ToString ();
			button.SetupSelf ();
		}
		LayoutRebuilder.MarkLayoutForRebuild(colorButtonParent.GetComponent<RectTransform> ());
	}

	public void SwitchColor(int col){
		if (col != GameManager.Instance.input.selectedColor) {
			for (int xx = 1; xx < colors.Length; xx++) {
				if (xx != col) {
					if (GameManager.Instance.painter.colorCount [xx] > 0) {
						colors [xx].myButton.interactable = true;
						if (colors [xx].inactive) {
							colors [xx].myCol.sprite = colorIcon;
							colors [xx].countObj.SetActive (true);
							colors [xx].myText.gameObject.SetActive (true);
							colors [xx].inactive = false;
							colors [xx].myButton.transition = Selectable.Transition.ColorTint;
							colors [xx].myCol.color = GameManager.Instance.painter.colors [colors [xx].colIndex];
						}
					} else {
						colors [xx].myButton.interactable = false;
						if (!colors [xx].inactive) {
							colors [xx].myCol.sprite = tickIcon;
							colors [xx].countObj.SetActive (false);
							colors [xx].myText.gameObject.SetActive (false);
							colors [xx].inactive = true;
							colors [xx].myButton.transition = Selectable.Transition.None;
							colors [xx].myCol.color = Color.white;
						}
					}
				} else {
					colors [xx].myButton.interactable = false;
				}
			}
			GameManager.Instance.input.selectedColor = col;
			GameManager.Instance.painter.RecolorAll ();
		}
	}

}
