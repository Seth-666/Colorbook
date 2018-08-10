using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public int selectedColor;
	public Globals.Brush brush;

	public Globals.InputState state;
	public Globals.InputState lastState;

	public LayerMask mask;
	//Current touch duration without moving beyond the threshold.
	public float touchTime;
	//Maximum duration before touch becomes paint-dragging.
	public float touchMax;
	//Maximum distance before touch turns into drag.
	public float distMax;

	Vector3 lastTouch;
	public Vector3 currTouch;

	public bool isMobile = false;

	public bool inputOn = false;

	void Start(){
		if (Application.isMobilePlatform) {
			isMobile = true;
		} else {
			GameManager.Instance.cam.zoomSpeed = 10;
			GameManager.Instance.cam.panSpeed = 10;
		}
	}

	void Update(){
		if (state != Globals.InputState.Busy) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null) {				
				if (!isMobile) {
					if (Input.GetAxis ("Mouse ScrollWheel") != 0) {
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
							currTouch = GameManager.Instance.cam.cam.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, GameManager.Instance.cam.cam.transform.position.z));
							touchTime += Time.deltaTime;
						}
					}
					if (Input.GetMouseButton (0)) {
						if (inputOn) {
							lastTouch = currTouch;
							currTouch = GameManager.Instance.cam.cam.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, GameManager.Instance.cam.cam.transform.position.z));
							if (state == Globals.InputState.Waiting) {
								touchTime += Time.deltaTime;
								if (touchTime >= touchMax) {
									state = Globals.InputState.Painting;
									Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.mousePosition);
									RaycastHit hit;
									if (Physics.Raycast (ray, out hit, mask)) {
										GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
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
							} else if (state == Globals.InputState.Painting) {
								Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.mousePosition);
								RaycastHit hit;
								if (Physics.Raycast (ray, out hit, mask)) {
									GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
								}
							}
						}
					}
					if (Input.GetMouseButtonUp (0)) {
						if (inputOn) {
							lastTouch = currTouch;
							currTouch = GameManager.Instance.cam.cam.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, GameManager.Instance.cam.cam.transform.position.z));
							inputOn = false;
							touchTime = 0;
							if (state == Globals.InputState.Waiting) {
								//Try to paint pixel here.
								Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.mousePosition);
								RaycastHit hit;
								if (Physics.Raycast (ray, out hit, mask)) {
									GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
								}
							} else if (state == Globals.InputState.Painting) {
								//Try to paint last pixel here.
								Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.mousePosition);
								RaycastHit hit;
								if (Physics.Raycast (ray, out hit, mask)) {
									GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
								}
							}
							state = Globals.InputState.Waiting;
						}
					}
				} else {
					if (Input.touchCount == 1) {
						if (Input.GetTouch (0).phase == TouchPhase.Began) {
							if (!inputOn) {
								inputOn = true;
								currTouch = GameManager.Instance.cam.cam.ScreenToWorldPoint (new Vector3 (Input.GetTouch (0).position.x, Input.GetTouch (0).position.y, GameManager.Instance.cam.cam.transform.position.z));
								touchTime += Time.deltaTime;
							}
						}
						if (Input.GetTouch (0).phase == TouchPhase.Moved || Input.GetTouch (0).phase == TouchPhase.Stationary) {
							if (inputOn) {
								lastTouch = currTouch;
								currTouch = GameManager.Instance.cam.cam.ScreenToWorldPoint (new Vector3 (Input.GetTouch (0).position.x, Input.GetTouch (0).position.y, GameManager.Instance.cam.cam.transform.position.z));
								if (state == Globals.InputState.Waiting) {
									touchTime += Time.deltaTime;
									if (touchTime >= touchMax) {
										state = Globals.InputState.Painting;
										Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.GetTouch (0).position);
										RaycastHit hit;
										if (Physics.Raycast (ray, out hit, mask)) {
											GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
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
									GameManager.Instance.cam.Pan (Input.GetTouch (0).deltaPosition);
								} else if (state == Globals.InputState.Painting) {
									Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.GetTouch (0).position);
									RaycastHit hit;
									if (Physics.Raycast (ray, out hit, mask)) {
										GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
									}
								}
							}
						}
						if (Input.GetTouch (0).phase == TouchPhase.Ended || Input.GetTouch (0).phase == TouchPhase.Canceled) {
							if (inputOn) {
								lastTouch = currTouch;
								currTouch = GameManager.Instance.cam.cam.ScreenToWorldPoint (new Vector3 (Input.GetTouch (0).position.x, Input.GetTouch (0).position.y, GameManager.Instance.cam.cam.transform.position.z));
								inputOn = false;
								touchTime = 0;
								if (state == Globals.InputState.Waiting) {
									//Try to paint pixel here.
									Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.GetTouch (0).position);
									RaycastHit hit;
									if (Physics.Raycast (ray, out hit, mask)) {
										GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
									}
								} else if (state == Globals.InputState.Painting) {
									//Try to paint last pixel here.
									Ray ray = GameManager.Instance.cam.cam.ScreenPointToRay (Input.GetTouch (0).position);
									RaycastHit hit;
									if (Physics.Raycast (ray, out hit, mask)) {
										GameManager.Instance.painter.TryPaintTile (hit, selectedColor);
									}
								}
								state = Globals.InputState.Waiting;
							}
						}
					} else if (Input.touchCount == 2) {
						Touch touch1 = Input.GetTouch (0);
						Touch touch2 = Input.GetTouch (1);

						Vector2 firstPrevPos = touch1.position - touch1.deltaPosition;
						Vector2 secondPrevPos = touch2.position - touch2.deltaPosition;

						float prevDeltaMag = (firstPrevPos - secondPrevPos).magnitude;
						float currDeltaMag = (touch1.position - touch2.position).magnitude;

						float deltaMagDiff = prevDeltaMag - currDeltaMag;

						GameManager.Instance.cam.Zoom (deltaMagDiff);
					}

				}
			}
		}
	}

}
