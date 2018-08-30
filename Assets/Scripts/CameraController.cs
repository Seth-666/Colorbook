using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public float minZoom, maxZoom;
	public float minX, maxX, minY, maxY;

	public float zoomSpeed;
	public float panSpeed;

	public float currZoom;
	public float panMulti;
	public float zoomMulti;
	public int textZoom;

	public Camera cam;

	void Awake(){
		cam = Camera.main;
		currZoom = cam.orthographicSize;
	}

	void Start(){
		currZoom = cam.orthographicSize;
		if (GameManager.Instance.input.isMobile) {
			panSpeed = (panMulti * currZoom);
			zoomSpeed = (zoomMulti * currZoom);
		}
		GetCameraExtents ();
	}

	public bool ShowGrid(){
		bool ret = false;
		if (currZoom < textZoom) {
			ret = true;
		}
		return ret;
	}

	public void SetExtents(MeshRenderer render){
		minX = -(render.bounds.extents.x - 1);
		maxX = (render.bounds.extents.x - 1);
		minY = -(render.bounds.extents.y - 1);
		maxY = (render.bounds.extents.y - 1);
	}

	void GetCameraExtents(){
		if (GameManager.Instance.painter.level.xSize > GameManager.Instance.painter.level.ySize) {
			maxZoom = GameManager.Instance.painter.level.xSize;
		} 
		else {
			maxZoom = GameManager.Instance.painter.level.ySize;
		}
		cam.orthographicSize = maxZoom;
		currZoom = maxZoom;
	}

	public void Zoom(float amount){
		float lastCamSetting = cam.orthographicSize;
		float camSetting = cam.orthographicSize += (amount * zoomSpeed);
		camSetting = Mathf.Clamp (camSetting, minZoom, maxZoom);
		cam.orthographicSize = camSetting;
		currZoom = camSetting;
		if (GameManager.Instance.input.isMobile) {
			panSpeed = (panMulti * currZoom);
			zoomSpeed = (zoomMulti * currZoom);
		}
		if (lastCamSetting >= textZoom && camSetting < textZoom) {
			GameManager.Instance.painter.AdjustGrid();
		} else if (lastCamSetting < textZoom && camSetting >= textZoom) {
			GameManager.Instance.painter.DisableAllText ();
		}
	}

	public void Pan(Vector3 dir){
		if (currZoom >= textZoom) {
			if (GameManager.Instance.pool.gridActive) {
				GameManager.Instance.painter.DisableAllText ();
			}
		}
		Vector3 movement = new Vector3 (dir.x * (panSpeed * Time.deltaTime), dir.y * (panSpeed * Time.deltaTime), 0);
		Vector3 targetPos = cam.transform.position - movement;
		targetPos.x = Mathf.Clamp (targetPos.x, minX, maxX);
		targetPos.y = Mathf.Clamp (targetPos.y, minY, maxY);
		cam.transform.position = targetPos;
		if (currZoom < textZoom) {
			GameManager.Instance.painter.AdjustGrid ();
		}
	}
}
