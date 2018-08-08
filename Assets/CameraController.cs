﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public float minZoom, maxZoom;
	public float minX, maxX, minY, maxY;

	public float zoomSpeed;
	public float panSpeed;

	public float currZoom;
	public AnimationCurve panRamp;

	public Camera cam;

	void Awake(){
		cam = Camera.main;
		currZoom = cam.orthographicSize;
	}

	void Start(){
		currZoom = cam.orthographicSize;
		if (GameManager.Instance.input.isMobile) {
			panSpeed = panRamp.Evaluate (currZoom);
		}
		GetCameraExtents ();
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
		} else {
			maxZoom = GameManager.Instance.painter.level.ySize;
		}
		cam.orthographicSize = maxZoom;
		currZoom = maxZoom;
	}

	public void Zoom(float amount){
		float camSetting = cam.orthographicSize += (amount * zoomSpeed);
		camSetting = Mathf.Clamp (camSetting, minZoom, maxZoom);
		cam.orthographicSize = camSetting;
		currZoom = camSetting;
		if (GameManager.Instance.input.isMobile) {
			panSpeed = panRamp.Evaluate (currZoom);
		}
		if (currZoom < 20 && !GameManager.Instance.pool.gridActive) {
			GameManager.Instance.painter.GenerateGrid ();
		} else if (currZoom >= 20 && GameManager.Instance.pool.gridActive) {
			GameManager.Instance.pool.DisableAll ();
		}
	}

	public void Pan(Vector3 dir){
		if (currZoom >= 20) {
			if (GameManager.Instance.pool.gridActive) {
				GameManager.Instance.pool.DisableAll ();
			}
		}
		Vector3 movement = new Vector3 (dir.x * (panSpeed * Time.deltaTime), dir.y * (panSpeed * Time.deltaTime), 0);
		Vector3 targetPos = cam.transform.position - movement;
		targetPos.x = Mathf.Clamp (targetPos.x, minX, maxX);
		targetPos.y = Mathf.Clamp (targetPos.y, minY, maxY);
		cam.transform.position = targetPos;
		if (currZoom < 20) {
			GameManager.Instance.painter.AdjustGrid ();
		}
	}
}
