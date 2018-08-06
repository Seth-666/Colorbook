﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public int selectedColor;

	public Globals.InputState state;

	public LayerMask mask;
	//Current touch duration without moving beyond the threshold.
	public float touchTime;
	//Maximum duration before touch becomes paint-dragging.
	public float touchMax;
	//Maximum distance before touch turns into drag.
	public float distMax;

	Vector3 lastTouch;
	Vector3 currTouch;

	bool isMobile = false;

	public bool inputOn = false;

	void Start(){
		if (Application.isMobilePlatform) {
			isMobile = true;
		}
	}

	void Update(){
		if (!isMobile) {
			if(Input.GetAxis("Mouse ScrollWheel") != 0) {
				float zoomAmount = Input.GetAxis ("Mouse ScrollWheel");
				if (zoomAmount < 0) {
					zoomAmount = Mathf.Abs (zoomAmount);
				} else {
					zoomAmount = -zoomAmount;
				}
				GameManager.Instance.cam.Zoom (zoomAmount);
			}
			if (Input.GetMouseButtonDown (0)) {
				if (!inputOn) {
					inputOn = true;
					currTouch = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
					touchTime += Time.deltaTime;
				}
			}
			if (Input.GetMouseButton (0)) {
				if (inputOn) {
					lastTouch = currTouch;
					currTouch = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
					if (state == Globals.InputState.Waiting) {
						touchTime += Time.deltaTime;
						if (touchTime >= touchMax) {
							state = Globals.InputState.Painting;
							if (Physics2D.OverlapPoint (currTouch, mask)) {
								Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
								GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
							}
						} 
						//If the distance has been exceeded, move the camera.
						else {
							float dist = Vector3.Distance (currTouch, lastTouch);
							if (dist >= distMax) {
								state = Globals.InputState.Dragging;
							}
						}
					} else if (state == Globals.InputState.Dragging) {
						Vector3 delta = currTouch - lastTouch;
						GameManager.Instance.cam.Pan (delta);
						//lastTouch = delta;
					} else if (state == Globals.InputState.Painting) {
						if (Physics2D.OverlapPoint (currTouch, mask)) {
							Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
							GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
						}
					}
				}
			}
			if (Input.GetMouseButtonUp (0)) {
				if (inputOn) {
					lastTouch = currTouch;
					currTouch = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
					inputOn = false;
					touchTime = 0;
					if (state == Globals.InputState.Waiting) {
						//Try to paint pixel here.
						if (Physics2D.OverlapPoint (currTouch, mask)) {
							Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
							GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
						}
					} else if (state == Globals.InputState.Dragging) {
						//Put the camera to a final position.
					} else if (state == Globals.InputState.Painting) {
						//Try to paint last pixel here.
						if (Physics2D.OverlapPoint (currTouch, mask)) {
							Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
							GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
						}
					}
					state = Globals.InputState.Waiting;
				}
			}
		} 

		else {
			if (Input.touchCount == 1) {
				if (Input.GetTouch (0).phase == TouchPhase.Began) {
					if (!inputOn) {
						inputOn = true;
						currTouch = Camera.main.ScreenToWorldPoint (new Vector3 (Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, Camera.main.transform.position.z));
						touchTime += Time.deltaTime;
					}
				}
				if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) {
					if (inputOn) {
						lastTouch = currTouch;
						currTouch = Camera.main.ScreenToWorldPoint (new Vector3 (Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, Camera.main.transform.position.z));
						if (state == Globals.InputState.Waiting) {
							touchTime += Time.deltaTime;
							if (touchTime >= touchMax) {
								state = Globals.InputState.Painting;
								if (Physics2D.OverlapPoint (currTouch, mask)) {
									Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
									GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
								}
							} 
							//If the distance has been exceeded, move the camera.
							else {
								float dist = Vector3.Distance (currTouch, lastTouch);
								if (dist >= distMax) {
									state = Globals.InputState.Dragging;
								}
							}
						} else if (state == Globals.InputState.Dragging) {
							GameManager.Instance.cam.Pan(Input.GetTouch(0).deltaPosition);
						} else if (state == Globals.InputState.Painting) {
							if (Physics2D.OverlapPoint (currTouch, mask)) {
								Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
								GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
							}
						}
					}
				}
				if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled) {
					if (inputOn) {
						lastTouch = currTouch;
						currTouch = Camera.main.ScreenToWorldPoint (new Vector3 (Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, Camera.main.transform.position.z));
						inputOn = false;
						touchTime = 0;
						if (state == Globals.InputState.Waiting) {
							//Try to paint pixel here.
							if (Physics2D.OverlapPoint (currTouch, mask)) {
								Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
								GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
							}
						} else if (state == Globals.InputState.Dragging) {
							//Put the camera to a final position.
						} else if (state == Globals.InputState.Painting) {
							//Try to paint last pixel here.
							if (Physics2D.OverlapPoint (currTouch, mask)) {
								Tile theTile = Physics2D.OverlapPoint (currTouch, mask).GetComponent<Tile> ();
								GameManager.Instance.painter.TryPaintTile (theTile, selectedColor);
							}
						}
						state = Globals.InputState.Waiting;
					}
				}
			}

			else if(Input.touchCount == 2){
				Touch touch1 = Input.GetTouch(0);
				Touch touch2 = Input.GetTouch(1);

				Vector2 firstPrevPos = touch1.position - touch1.deltaPosition;
				Vector2 secondPrevPos = touch2.position - touch2.deltaPosition;

				float prevDeltaMag = (firstPrevPos - secondPrevPos).magnitude;
				float currDeltaMag = (touch1.position - touch2.position).magnitude;

				float deltaMagDiff = prevDeltaMag - currDeltaMag;

				GameManager.Instance.cam.Zoom(deltaMagDiff);
			}

		}
	}

}
