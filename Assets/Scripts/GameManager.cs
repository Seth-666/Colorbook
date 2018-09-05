using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;

	public Painter painter;
	public InputManager input;
	public UIManager ui;
	public CameraController cam;
	public ObjectPool pool;
	public AdManager ads;

	public SpriteData level;

	void Awake(){
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (this.gameObject);
		}
	}

	public void Initialize(){
		painter = Camera.main.GetComponent<Painter> ();
		painter.level = level;
		input = Camera.main.GetComponent<InputManager> ();
		ui = Camera.main.GetComponent<UIManager> ();
		cam = Camera.main.GetComponent<CameraController>();
		pool = Camera.main.GetComponent<ObjectPool> ();
	}

}
