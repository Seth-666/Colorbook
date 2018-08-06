using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public float minZoom, maxZoom;
	public float minX, maxX, minY, maxY;

	public float zoomSpeed;
	public float panSpeed;

	public float currZoom;
	public AnimationCurve panRamp;

	Camera cam;

	void Start(){
		cam = Camera.main;
		currZoom = cam.orthographicSize;
		panSpeed = panRamp.Evaluate (currZoom);
		GetCameraExtents ();
	}

	void GetCameraExtents(){
		Vector2 dl = GameManager.Instance.painter.PosToVector2 (0, 0);
		Vector2 ur = GameManager.Instance.painter.PosToVector2 (GameManager.Instance.painter.level.xSize - 1, GameManager.Instance.painter.level.ySize - 1);
		if (dl.x < ur.x) {
			minX = dl.x;
			maxX = ur.x;
		} else {
			minX = ur.x;
			maxX = dl.x;
		}
		if (dl.y < ur.y) {
			minY = dl.y;
			maxY = ur.y;
		} else {
			minY = ur.y;
			maxY = dl.y;
		}
	}

	public void Zoom(float amount){
		float camSetting = cam.orthographicSize += (amount * zoomSpeed);
		camSetting = Mathf.Clamp (camSetting, minZoom, maxZoom);
		cam.orthographicSize = camSetting;
		currZoom = camSetting;
		panSpeed = panRamp.Evaluate (currZoom);
	}

	public void Pan(Vector3 dir){
		Vector3 movement = new Vector3 (dir.x * (panSpeed * Time.deltaTime), dir.y * (panSpeed * Time.deltaTime), 0);
		Vector3 targetPos = cam.transform.position - movement;
		targetPos.x = Mathf.Clamp (targetPos.x, minX, maxX);
		targetPos.y = Mathf.Clamp (targetPos.y, minY, maxY);
		cam.transform.position = targetPos;
	}
}
