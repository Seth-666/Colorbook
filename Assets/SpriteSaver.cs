using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSaver : MonoBehaviour {
	#if UNITY_EDITOR

	public LayerMask layer;

	public Texture2D source;
	public bool getData;
	public bool saveData;
	public bool loadData;
	public bool renderData;

	public Tile tile;

	public List<Color> colors;
	public int[,] spriteGrid;
	Tile[,] tileGrid;
	public int xSize;
	public int ySize;

	public float tileSize;

	public Color selected;
	public Color startCol;
	public Color endCol;

	public SpriteData container;
	public string dataName;
	public string[] tags;

	public GameObject obj;
	public Texture2D createdTexture;

	void Update(){
		if (getData) {
			getData = false;
			GetData ();
		}
		if (saveData) {
			saveData = false;
			SaveData ();
		}
		if (loadData) {
			loadData = false;
			LoadData ();
		}
		if (renderData) {
			renderData = false;
			RenderData ();
		}
	}

	void GetData(){
		Color[] allColors = source.GetPixels ();
		List<Color> tempColors = GetColors (source.GetPixels());
		colors = new List<Color> ();
		colors.Add (Color.white);
		while (tempColors.Count > 0) {
			float currHighest = 0;
			int index = 0;
			for (int xx = 0; xx < tempColors.Count; xx++) {
				if (tempColors [xx].r > currHighest) {
					currHighest = tempColors [xx].r;
					index = xx;
				} else if (tempColors [xx].r == currHighest) {
					if (tempColors [xx].g > tempColors [index].g) {
						index = xx;
					} else if (tempColors [xx].g == tempColors [index].g) {
						if (tempColors [xx].b > tempColors [index].b) {
							index = xx;
						}
					}
				}
			}
			colors.Add (tempColors [index]);
			tempColors.RemoveAt (index);
		}

		//Sort colors.

		xSize = Mathf.FloorToInt(source.width);
		ySize = Mathf.FloorToInt(source.height);
		spriteGrid = new int[xSize, ySize];
		int currIndex = 0;
		for (int yy = 0; yy < ySize; yy++) {
			for (int xx = 0; xx < xSize; xx++) {
				if (allColors [currIndex].a == 1) {
					spriteGrid [xx, yy] = GetColorIndex (allColors [currIndex]);
				} else {
					spriteGrid [xx, yy] = -1;
				}
				currIndex++;
			}
		}
 
	}

	int GetColorIndex(Color col){
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

	void SaveData(){
		if (container == null) {
			if (dataName == string.Empty) {
				dataName = source.name;
			}
			SpriteData asset = ScriptableObject.CreateInstance<SpriteData>();

			UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/Sprite Objects/" + dataName + ".asset");
			UnityEditor.AssetDatabase.SaveAssets();
			container = asset;
		}
		container.xSize = xSize;
		container.ySize = ySize;
		container.colors = new Color[colors.Count];
		container.colorCount = colors.Count;
		int tileCount = 0;
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (spriteGrid [xx, yy] >= 0) {
					tileCount++;
				}
			}
		}
		container.tileCount = tileCount;
		for (int xx = 0; xx < colors.Count; xx++) {
			container.colors [xx] = colors [xx];
		}
		container.colorData = Globals.SaveArray (spriteGrid);
		UnityEditor.AssetDatabase.Refresh ();
		UnityEditor.EditorUtility.SetDirty (container);
		UnityEditor.AssetDatabase.SaveAssets ();
	}

	void LoadData(){
		if (container != null) {
			xSize = container.xSize;
			ySize = container.ySize;
			colors = new List<Color> ();
			for (int xx = 0; xx < container.colors.Length; xx++) {
				colors.Add (container.colors [xx]);
			}
			spriteGrid = Globals.LoadArray (container.colorData, xSize, ySize);
		}
	}

	void RenderData(){
		GameObject newObj = Instantiate (obj);
		Vector3 newScale = new Vector3 (xSize * 0.1f, 1, ySize * 0.1f);
		newObj.transform.localScale = newScale;
		newObj.transform.position = Vector3.zero;
		Texture2D tex = new Texture2D (xSize, ySize);
		Color transCol = new Color (1, 1, 1, 0);
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (spriteGrid [xx, yy] == -1) {
					tex.SetPixel (xx, yy, transCol);
				} else {
					tex.SetPixel (xx, yy, colors [spriteGrid [xx, yy]]);
				}
			}
		}
		tex.filterMode = FilterMode.Point;
		tex.Apply ();
		createdTexture = tex;
		newObj.GetComponent<MeshRenderer> ().material.mainTexture = tex;
		RecolorAll ();
	}

	void RecolorAll(){
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (spriteGrid [xx, yy] >= 0) {
					Color greyCol = Globals.ToGrayScale(colors[spriteGrid[xx, yy]]);
					greyCol = Color.Lerp(greyCol, Color.white, 0.5f);
					createdTexture.SetPixel (xx, yy, greyCol);
				}
			}
		}
		createdTexture.Apply ();
	}

	public Vector2 PosToVector2(int xx, int yy){
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
