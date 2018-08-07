using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Painter : MonoBehaviour {

	public bool updateColors;

	public Color[] colors;
	public Color selected;

	public float tileSize;

	public int[,] spriteGrid;
	public int[,] progressGrid;

	public Texture2D mainTex;

	public SpriteData level;

	void Start(){
		if (level != null) {
			LoadLevel ();
			RenderData ();
		} else {
			Debug.Log ("No level data loaded.");
		}
		if (level.xSize > level.ySize) {
			GameManager.Instance.cam.maxZoom = level.xSize * 2;
		} else {
			GameManager.Instance.cam.maxZoom = level.ySize * 2;
		}
	}

	void Update(){
		if (updateColors) {
			updateColors = false;
			RecolorAll ();
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
						//tileGrid [tile.pos.x, tile.pos.y].myText.text = (spriteGrid [tile.pos.x, tile.pos.y]).ToString ();
						//tileGrid [tile.pos.x, tile.pos.y].render.sprite = borderedTile;
					} 
					progressGrid [x, y] = col;
				} 
				//If it IS the correct color. set the text to nothing and change the color.
				else {
					//tileGrid [tile.pos.x, tile.pos.y].myText.text = "";
					progressGrid [x, y] = col;
					mainTex.SetPixel (x, y, colors [col]);
					//tileGrid [tile.pos.x, tile.pos.y].render.color = colors [col];
					//tileGrid [tile.pos.x, tile.pos.y].render.sprite = openTile;
				}
			}
			mainTex.Apply ();
		}
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
		//tileGrid = new Tile[level.xSize, level.ySize];
		GameObject parentObj = new GameObject ();
		GameObject plane = Instantiate (GameManager.Instance.pool.basePlane);
		Vector3 newScale = new Vector3 (level.xSize * 0.1f, 1, level.ySize * 0.1f);
		plane.transform.localScale = newScale;
		plane.transform.name = "Level";
		Texture2D newTex = new Texture2D (level.xSize, level.ySize);
		Color transCol = new Color (1, 1, 1, 0);
		parentObj.name = "Tiles";
		for (int xx = 0; xx < level.xSize; xx++) {
			for (int yy = 0; yy < level.ySize; yy++) {
				if (spriteGrid [xx, yy] == -1) {
					newTex.SetPixel (xx, yy, transCol);
				}
				else if (spriteGrid [xx, yy] >= 0) {
					newTex.SetPixel (xx, yy, colors [spriteGrid [xx, yy]]);
					//Tile newTile = GameManager.Instance.pool.GetTile ();
					//newTile.gameObject.SetActive (true);
					//newTile.transform.position = PosToVector2 (xx, yy);
					//tileGrid [xx, yy] = newTile;
					//newTile.pos.x = xx;
					//newTile.pos.y = yy;
					//newTile.transform.SetParent (parentObj.transform);
					//newTile.myText.text = (spriteGrid [xx, yy]).ToString ();
				}
			}
		}
		mainTex = newTex;
		newTex.filterMode = FilterMode.Point;
		newTex.Apply ();
		plane.GetComponent<MeshRenderer> ().material.mainTexture = newTex;
		RecolorAll ();
	}

	void RecolorAll(){
		for (int xx = 0; xx < level.xSize; xx++) {
			for (int yy = 0; yy < level.ySize; yy++) {
				//If the tile isn't a blank tile
				if (spriteGrid [xx, yy] >= 0) {
					//If the current color hasn't been set yet (0)
					if (progressGrid [xx, yy] == 0) {
						//Add case for whether or not its the current selcted color.
						if (spriteGrid [xx, yy] == GameManager.Instance.input.selectedColor) {
							mainTex.SetPixel (xx, yy, selected);
							//tileGrid [xx, yy].render.color = selected;
							//tileGrid [xx, yy].myText.text = (spriteGrid [xx, yy]).ToString ();
						} else {
							Color newCol = ToGrayScale(colors[spriteGrid[xx, yy]]);
							newCol = Color.Lerp(newCol, Color.white, 0.5f);
							mainTex.SetPixel (xx, yy, newCol);
							//tileGrid [xx, yy].render.color = newCol;
							//tileGrid [xx, yy].myText.text = (spriteGrid [xx, yy]).ToString ();
						}
					}
					//If the current color has been set, but isn't correct
					//Color is half-alpha of the selected color.
					else if (progressGrid [xx, yy] != spriteGrid [xx, yy]) {
						mainTex.SetPixel(xx, yy, new Color (colors [progressGrid [xx, yy]].r, colors [progressGrid [xx, yy]].g, colors [progressGrid [xx, yy]].b, 0.5f));
						//tileGrid [xx, yy].render.color = new Color (colors [progressGrid [xx, yy]].r, colors [progressGrid [xx, yy]].g, colors [progressGrid [xx, yy]].b, 0.5f);
						//tileGrid [xx, yy].myText.text = (spriteGrid [xx, yy]).ToString ();
					}
					//If the current color has been set and is correct
					else if (progressGrid [xx, yy] == spriteGrid [xx, yy]) {
						mainTex.SetPixel (xx, yy, colors [progressGrid [xx, yy]]);
						//tileGrid [xx, yy].render.color = colors [progressGrid [xx, yy]];
						//tileGrid [xx, yy].myText.text = "";
					}
				}
			}
		}
		mainTex.Apply ();
	}

	Color ToGrayScale(Color orig){
		Color ret = new Color ();
		orig.r += (orig.r * 0.5f);
		orig.g += (orig.g * 0.5f);
		orig.b += (orig.b * 0.5f);
		Color32 col = new Color (orig.r, orig.g, orig.b, 255);
		int p = ((256 * 256 + col.r) * 256 + col.b) * 256 + col.g;
		int b = p % 256;
		p = Mathf.FloorToInt (p / 256);
		int g = p % 256;
		p = Mathf.FloorToInt(p / 256);
		int r = p % 256;
		float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
		ret.r = l;
		ret.g = l;
		ret.b = l;
		ret.a = 1;
		return ret;
	}

	public Vector2 PosToVector2(int xx, int yy){
		Vector2 ret = new Vector2 ((xx - (level.xSize / 2)) * tileSize, (yy - (level.ySize / 2)) * tileSize);
		if (level.xSize % 2 == 0) {
			ret.x += tileSize / 2;
		}
		if (level.ySize % 2 == 0) {
			ret.y += tileSize / 2;
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
