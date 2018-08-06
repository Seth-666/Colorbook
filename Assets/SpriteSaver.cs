using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSaver : MonoBehaviour {
	#if UNITY_EDITOR
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
		GameObject[] tiles = GameObject.FindGameObjectsWithTag ("Tile");
		tileGrid = new Tile[xSize, ySize];
		GameObject parentObj = new GameObject ();
		parentObj.name = "Tiles";
		for (int xx = 0; xx < tiles.Length; xx++) {
			Destroy (tiles [xx]);
		}
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (spriteGrid [xx, yy] >= 0) {
					Tile newTile = Instantiate (tile);
					newTile.transform.position = PosToVector2 (xx, yy);
					tileGrid [xx, yy] = newTile;
					newTile.transform.SetParent (parentObj.transform);
					newTile.myText.text = (spriteGrid [xx, yy]).ToString ();
				}
			}
		}
		RecolorAll ();
	}

	void RecolorAll(){
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				if (spriteGrid [xx, yy] >= 0) {
					//float lerpVal = Mathf.InverseLerp (0, colors.Count - 1, spriteGrid [xx, yy]);
					//Color newCol = Color.Lerp (startCol, endCol, lerpVal);
					Color greyCol = ToGrayScale(colors[spriteGrid[xx, yy]]);//newCol;
					greyCol = Color.Lerp(greyCol, Color.white, 0.75f);
					tileGrid [xx, yy].render.color = greyCol;
				}
			}
		}
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
