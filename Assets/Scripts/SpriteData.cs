using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "SpriteData", menuName = "Data/SpriteData", order = 1)]
public class SpriteData : ScriptableObject {

	public string myName;

	public int xSize;
	public int ySize;

	public int[] colorData;
	public Color[] colors;

	public int tileCount;
	public int colorCount;

	public string tag;

}
