﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Painter : MonoBehaviour {

	public Color[] colors;
	public Color selected;

	public float tileSize;

	public int[,] spriteGrid;
	public int[,] progressGrid;
	Tile[,] textGrid;
	bool[,] textActive;

	public Texture2D mainTex;
	public SpriteData level;

	public int xScope;
	public int yScope;

	void Start(){
		StartCoroutine (LateStart ());
	}

	IEnumerator LateStart(){
		yield return null;
		textGrid = new Tile[level.xSize, level.ySize];
		textActive = new bool[level.xSize, level.ySize];
		if (level != null) {
			LoadLevel ();
			RenderData ();
		} else {
			Debug.Log ("No level data loaded.");
		}
	}

	public void GenerateGrid(){
		if (!GameManager.Instance.pool.gridActive) {
			if (GameManager.Instance.cam.currZoom < GameManager.Instance.cam.textZoom) {
				Ray ray = new Ray (GameManager.Instance.cam.cam.transform.position, GameManager.Instance.cam.cam.transform.forward * 10);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, GameManager.Instance.input.mask)) {
					int startX = (Mathf.FloorToInt (hit.textureCoord.x * mainTex.width) - (xScope / 2));
					int startY = (Mathf.FloorToInt (hit.textureCoord.y * mainTex.height) - (yScope / 2));
					for (int xx = startX; xx < (startX + xScope); xx++) {
						for (int yy = startY; yy < (startY + yScope); yy++) {
							if (IsInGrid (xx, yy)) {
								if (spriteGrid [xx, yy] >= 0) {
									if (progressGrid [xx, yy] != spriteGrid [xx, yy]) {
										Tile newTile = GameManager.Instance.pool.GetTile ();
										textGrid [xx, yy] = newTile;
										textActive [xx, yy] = true;
										newTile.transform.position = PosToVector2 (xx, yy);
										newTile.myText.text = spriteGrid [xx, yy].ToString ();
									}
								}
							}
						}
					}
				}
				GameManager.Instance.pool.gridActive = true;
			}
		}
	}

	public void DisableAllText(){
		for (int xx = 0; xx < level.xSize; xx++) {
			for (int yy = 0; yy < level.ySize; yy++) {
				if (textActive [xx, yy]) {
					GameManager.Instance.pool.ReturnTile (textGrid [xx, yy]);
					textGrid [xx, yy] = null;
					textActive[xx, yy] = false;
				}
			}
		}
	}

	public void AdjustGrid(){
		if (GameManager.Instance.pool.gridActive) {
			Ray ray = new Ray (GameManager.Instance.cam.cam.transform.position, GameManager.Instance.cam.cam.transform.forward * 10);
			Debug.DrawRay (ray.origin, ray.direction * 100, Color.green, 3);
			RaycastHit hit;
			//Get extents of grid check.
			//Check whole grid for any tiles outside of this range and disable them.
			//Any within this range need to be updated.
			//Any empty spaces within this range need to be created and set.
			if (Physics.Raycast (ray, out hit, GameManager.Instance.input.mask)) {
				int startX = (Mathf.FloorToInt (hit.textureCoord.x * mainTex.width) - (xScope / 2));
				int startY = (Mathf.FloorToInt (hit.textureCoord.y * mainTex.height) - (yScope / 2));
				for (int xx = 0; xx < level.xSize; xx++) {
					for (int yy = 0; yy < level.ySize; yy++) {
						if(IsInGrid(xx, yy)){
							//If there's an active text in the slot:
							if (textActive [xx, yy]) {
								if (IsInRange (startX, startX + xScope, startY, startY + yScope, xx, yy)) {
									//If it is in the range, update the text.
									if(spriteGrid[xx, yy] >= 0){
										//If it's not the right color, set the text.
										if (progressGrid [xx, yy] != spriteGrid [xx, yy]) {
											textGrid [xx, yy].myText.text = spriteGrid [xx, yy].ToString ();
										} else {
											//If it is, disable the object.
											GameManager.Instance.pool.ReturnTile(textGrid[xx, yy]);
											textGrid [xx, yy] = null;
											textActive[xx, yy] = false;
										}
									}
								} else {
									//If it's not in the range, disable it.
									GameManager.Instance.pool.ReturnTile(textGrid[xx, yy]);
									textGrid [xx, yy] = null;
									textActive[xx, yy] = false;
								}
							} else {
								//If there's nothing in the slot but there should be:
								if (IsInRange (startX, startX + xScope, startY, startY + yScope, xx, yy)) {
									//Grab a new tile from the pool and set the text.
									//If it's applicable:
									if (spriteGrid [xx, yy] >= 0) {
										//If it's not the right color (otherwise ignore it).
										if (progressGrid [xx, yy] != spriteGrid [xx, yy]) {
											Tile newTile = GameManager.Instance.pool.GetTile ();
											newTile.transform.position = PosToVector2 (xx, yy);
											textGrid [xx, yy] = newTile;
											newTile.myText.text = spriteGrid [xx, yy].ToString ();
											textActive [xx, yy] = true;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		else {
			GenerateGrid ();
		}
	}

	public void TryPaintTile(RaycastHit hit, int col){
		int x = Mathf.FloorToInt (hit.textureCoord.x * mainTex.width);
		int y = Mathf.FloorToInt (hit.textureCoord.y * mainTex.height);
		if (spriteGrid [x, y] != -1) {
			if (progressGrid [x, y] != col) {
				if (spriteGrid [x, y] != col) {
					//If it doesn't match the target color, paint it but not at full alpha.
					mainTex.SetPixel (x, y, new Color (colors [col].r, colors [col].g, colors [col].b, 0.5f));
					//If it was the correct color previously, set the text again.
					if (progressGrid [x, y] == spriteGrid [x, y]) {
					} 
					progressGrid [x, y] = col;
				}
				//If it IS the correct color. set the text to nothing and change the color.
				else {
					progressGrid [x, y] = col;
					mainTex.SetPixel (x, y, colors [col]);
				}
			}
			mainTex.Apply ();
		}
		AdjustGrid ();
	}

	void LoadLevel(){
		colors = level.colors;
		spriteGrid = Globals.LoadArray (level.colorData, level.xSize, level.ySize);
		if (!LoadData ()) {
			progressGrid = new int[level.xSize, level.ySize];
			for (int xx = 0; xx < level.xSize; xx++) {
				for (int yy = 0; yy < level.ySize; yy++) {
					if (spriteGrid [xx, yy] == -1) {
						progressGrid [xx, yy] = -1;
					} else {
						progressGrid [xx, yy] = 0;
					}
				}
			}
		}
	}

	void RenderData(){
		GameObject plane = Instantiate (GameManager.Instance.pool.basePlane);
		Vector3 newScale = new Vector3 (level.xSize * tileSize, 1, level.ySize * tileSize);
		plane.transform.localScale = newScale;
		plane.transform.name = "Level";
		Texture2D newTex = new Texture2D (level.xSize, level.ySize, TextureFormat.RGBA32, false);
		Color transCol = new Color (1, 1, 1, 0);
		for (int xx = 0; xx < level.xSize; xx++) {
			for (int yy = 0; yy < level.ySize; yy++) {
				if (spriteGrid [xx, yy] == -1) {
					newTex.SetPixel (xx, yy, transCol);
				}
				else if (spriteGrid [xx, yy] > 0) {
					newTex.SetPixel (xx, yy, colors [spriteGrid [xx, yy]]);
				}
			}
		}
		mainTex = newTex;
		newTex.filterMode = FilterMode.Point;
		newTex.Apply ();
		MeshRenderer render = plane.GetComponent<MeshRenderer> ();
		render.material.mainTexture = newTex;
		GameManager.Instance.cam.SetExtents (render);
		RecolorAll ();
	}

	public void RecolorAll(){
		for (int xx = 0; xx < level.xSize; xx++) {
			for (int yy = 0; yy < level.ySize; yy++) {
				//If the tile isn't a blank tile
				if (spriteGrid [xx, yy] >= 0) {
					//If the current color hasn't been set yet (0)
					if (progressGrid [xx, yy] == 0) {
						//Add case for whether or not its the current selcted color.
						if (spriteGrid [xx, yy] == GameManager.Instance.input.selectedColor) {
							mainTex.SetPixel (xx, yy, selected);
						} else {
							Color newCol = Globals.ToGrayScale(colors[spriteGrid[xx, yy]]);
							//newCol = Color.Lerp(newCol, Color.white, 0.25f);
							mainTex.SetPixel (xx, yy, newCol);
						}
					}
					//If the current color has been set, but isn't correct
					//Color is half-alpha of the selected color.
					else if (progressGrid [xx, yy] != spriteGrid [xx, yy]) {
						mainTex.SetPixel(xx, yy, new Color (colors [progressGrid [xx, yy]].r, colors [progressGrid [xx, yy]].g, colors [progressGrid [xx, yy]].b, 0.5f));
					}
					//If the current color has been set and is correct
					else if (progressGrid [xx, yy] == spriteGrid [xx, yy]) {
						mainTex.SetPixel (xx, yy, colors [progressGrid [xx, yy]]);
					}
				}
			}
		}
		mainTex.Apply ();
		AdjustGrid ();
	}

	public Vector2 PosToVector2(int xx, int yy){
		Vector2 ret = new Vector2 ((xx - (level.xSize / 2)) * (tileSize * 10), (yy - (level.ySize / 2)) * (tileSize * 10));
		if (level.xSize % 2 == 0) {
			ret.x += (tileSize * 10) / 2;
		}
		if (level.ySize % 2 == 0) {
			ret.y += (tileSize * 10) / 2;
		}
		return ret;
	}

	bool IsInGrid(int x, int y){
		bool ret = true;
		if (x < 0 || y < 0) {
			ret = false;
		}
		if (x >= level.xSize || y >= level.ySize) {
			ret = false;
		}
		return ret;
	}

	bool IsInRange(int startX, int endX, int startY, int endY, int x, int y){
		bool ret = true;
		if (x < startX || x >= endX) {
			ret = false;
		}
		if (y < startY || y >= endY) {
			ret = false;
		}
		return ret;
	}

	void SaveData(){
		ProgressData dat = new ProgressData ();
		BinaryFormatter bf = new BinaryFormatter ();
		if (File.Exists (Application.persistentDataPath + "/" + level.myName + ".lev")) {
			File.Delete(Application.persistentDataPath + "/" + level.myName + ".lev");
		}
		FileStream file = File.Create (Application.persistentDataPath + "/" + level.myName + ".lev");
		dat.progress = Globals.SaveArray (progressGrid);
		bf.Serialize (file, dat);
		file.Close ();
	}

	bool LoadData(){
		bool ret = true;
		if (File.Exists (Application.persistentDataPath + "/" + level.myName + ".lev")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/" + level.myName + ".lev", FileMode.Open);
			ProgressData dat = (ProgressData)bf.Deserialize (file);
			file.Close ();
			progressGrid = Globals.LoadArray (dat.progress, level.xSize, level.ySize);

		} else {
			ret = false;
		}
		return ret;
	}

}

[System.Serializable]
class ProgressData{

	public int[] progress;

}
