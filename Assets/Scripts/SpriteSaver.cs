using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSaver : MonoBehaviour {
	#if UNITY_EDITOR

	public LayerMask layer;

	public Texture2D[] textures;
	public bool processData;

	public bool prescan;

	public float tileSize;

	public GameObject texObj;

	void Update(){
		if (prescan) {
			prescan = false;
			if (!Prescan ()) {
				Debug.Log ("Prescan found transparent images.");
			}
		}
		if (processData) {
			processData = false;
			if (Prescan ()) {
				StartCoroutine (ProcessData ());
			}
		}
	}

	IEnumerator ProcessData(){
		for (int xx = 0; xx < textures.Length; xx++) {
			GetData (textures [xx]);
			yield return null;
		}
	}

	bool Prescan(){
		bool ret = true;
		for (int xx = 0; xx < textures.Length; xx++) {
			Color[] allColors = textures[xx].GetPixels ();
			for (int yy = 0; yy < allColors.Length; yy++){
				if (allColors [yy].a > 0 && allColors [yy].a < 1) {
					Debug.Log ("Semi-transparent pixel found on file " + textures[xx].name);
					ret = false;
					break; 
				}
			}
		}
		return ret;
	}

	void GetData(Texture2D tex){
		Color[] allColors = tex.GetPixels ();
		List<Color> tempColors = GetColors (tex.GetPixels());
		List<Color> colors = ColorSort(tempColors);
		int xSize = Mathf.FloorToInt(tex.width);
		int ySize = Mathf.FloorToInt(tex.height);
		int[,] spriteGrid = new int[xSize, ySize];
		int currIndex = 0;
		bool transFound = false;
		for (int yy = 0; yy < ySize; yy++) {
			for (int xx = 0; xx < xSize; xx++) {
				if (allColors [currIndex].a == 1) {
					spriteGrid [xx, yy] = GetColorIndex (allColors [currIndex], colors);
				} else if (allColors [currIndex].a > 0 && allColors [currIndex].a < 1) {
					Debug.Log ("Semi-transparent pixel found on file " + tex.name);
					transFound = true;
					break;
				}
				else{
					spriteGrid [xx, yy] = -1;
				}
				currIndex++;
			}
			if (transFound) {
				break;
			}
		}
		if (!transFound) {
			SaveData (tex, xSize, ySize, colors, spriteGrid);
		}
	}

	List<Color> ColorSort(List<Color> tempColors){
		List<Color> colors = new List<Color> ();
		colors.Add (Color.white);
		int currIndex = 0;
		while (tempColors.Count > 0) {
			float shortestDist = 1000;
			int index = 0;
			Vector3 currCol = new Vector3 (colors [currIndex].r, colors [currIndex].g, colors [currIndex].b);
			for (int xx = 0; xx < tempColors.Count; xx++) {
				Vector3 newCol = new Vector3 (tempColors [xx].r, tempColors [xx].g, tempColors [xx].b);
				float dist = Vector3.Distance (currCol, newCol);
				if (dist < shortestDist) {
					shortestDist = dist;
					index = xx;
				}
			}
			colors.Add (tempColors [index]);
			currIndex++;
			tempColors.RemoveAt (index);
		}
		return colors;
	}
		
	int GetColorIndex(Color col, List<Color> colors){
		int index = 0;
		//We start at 1 because the 0 will be the default blank color.
		for (int xx = 1; xx < colors.Count; xx++) {
			if (col == colors [xx]) {
				index = xx;
				break;
			}
		}
		return index;
	}

	List<Color> GetColors(Color[] cols){
		List<Color> newCols = new List<Color> ();
		for (int xx = 0; xx < cols.Length; xx++) {
			if(cols[xx].a == 1){
				if (!newCols.Contains(cols [xx])) {
					newCols.Add (cols [xx]);
				}
			}
		}
		return newCols;
	}

	void SaveData(Texture2D tex, int xSize, int ySize, List<Color> colors, int[,] spriteGrid){
		Texture2D snapshot = RenderData (xSize, ySize, spriteGrid, colors);
		SpriteData asset = ScriptableObject.CreateInstance<SpriteData>();
		UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/Resources/Sprite Objects/" + tex.name + ".asset");
		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.CreateAsset (snapshot, "Assets/Resources/Sprite Thumbnails/" + tex.name + "_img.asset");
		UnityEditor.AssetDatabase.SaveAssets ();

		asset.xSize = xSize;
		asset.ySize = ySize;
		asset.thumb = snapshot;
		asset.myName = tex.name;
		asset.colors = new Color[colors.Count];
		asset.colorCount = colors.Count;
		int tileCount = 0;
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (spriteGrid [xx, yy] >= 0) {
					tileCount++;
				}
			}
		}
		asset.tileCount = tileCount;
		for (int xx = 0; xx < colors.Count; xx++) {
			asset.colors [xx] = colors [xx];
		}
		asset.colorData = Globals.SaveArray (spriteGrid);
		asset.difficulty = colors.Count * tileCount;
		UnityEditor.AssetDatabase.Refresh ();
		UnityEditor.EditorUtility.SetDirty (asset);
		UnityEditor.AssetDatabase.SaveAssets ();
	}

	Texture2D RenderData(int xSize, int ySize, int[,] spriteGrid, List<Color> colors){
		Texture2D tex = new Texture2D (xSize, ySize, TextureFormat.RGBA32, false);
		Color transCol = new Color (1, 1, 1, 0);
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (spriteGrid [xx, yy] == -1) {
					tex.SetPixel (xx, yy, transCol);
				} else {
					if (spriteGrid [xx, yy] > 0) {
						Color greyCol = Globals.ToGrayScale(colors[spriteGrid[xx, yy]]);
						tex.SetPixel (xx, yy, greyCol);
					}
				}
			}
		}
		tex.filterMode = FilterMode.Point;
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.Apply ();
		return tex;
	}

	public Vector2 PosToVector2(int xx, int yy, int xSize, int ySize){
		Vector2 ret = new Vector2 ((xx - (xSize / 2)) * tileSize, (yy - (ySize / 2)) * tileSize);
		if (xSize % 2 == 0) {
			ret.x += tileSize / 2;
		}
		if (ySize % 2 == 0) {
			ret.y += tileSize / 2;
		}
		return ret;
	}
	#endif
}
