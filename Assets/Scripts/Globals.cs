using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {

	public enum Categories{
		People,
		Food,
		Animals,
		Weapons,
		Vehicles,
		Buildings,
		Plants,
		Monsters,
		Dinosaurs,
		Scenery,
		Misc,
		Started,
		Completed
	}

	public enum UIState{
		Idle,
		BrushPicker,
		ColorPicker,
		Menu,
		Busy,
		Paused
	}

	public enum Brush{
		Small,
		Medium,
		Large
	}

	public enum InputState{
		Waiting,
		Painting,
		Dragging,
		Busy
	}

	public struct CategoryCollection{
		public Globals.Categories type;
		public List<SpriteButton> buttons;
	}

	public struct Coord{
		public int x;
		public int y;
		public Coord(int xx, int yy){
			x = xx;
			y = yy;
		}
	}

	public static Coord[] GetMediumBrush(int x, int y){
		Coord[] ret = new Coord[5];
		ret [0] = new Coord (x, y);
		ret [1] = new Coord (x - 1, y);
		ret [2] = new Coord (x + 1, y);
		ret [3] = new Coord (x, y + 1);
		ret [4] = new Coord (x, y - 1);
		return ret;
	}

	public static Coord[] GetLargeBrush(int x, int y){
		Coord[] ret = new Coord[9];
		ret [0] = new Coord (x, y);
		ret [1] = new Coord (x - 1, y + 1);
		ret [2] = new Coord (x, y + 1);
		ret [3] = new Coord (x + 1, y + 1);
		ret [4] = new Coord (x - 1, y);
		ret [5] = new Coord (x + 1, y);
		ret [6] = new Coord (x - 1, y - 1);
		ret [7] = new Coord (x, y - 1);
		ret [8] = new Coord (x + 1, y - 1);
		return ret;
	}

	//Generic method for turning a 2D array into a 1D array.
	public static T[] SaveArray<T>(T[,] data){
		int xSize = data.GetLength (0);
		int ySize = data.GetLength (1);
		T[] arr = new T[xSize * ySize];
		int currIndex = 0;
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				arr [currIndex] = data [xx, yy];
				currIndex++;
			}
		}
		return arr;
	}

	//Generic method for returning a 1D array to a 2D array, based on dimensions provided.
	public static T[,] LoadArray<T>(T[] arr, int xSize, int ySize){
		T[,] ret = new T[xSize, ySize];
		int currIndex = 0;
		for (int xx = 0; xx < xSize; xx++) {
			for (int yy = 0; yy < ySize; yy++) {
				ret [xx, yy] = arr [currIndex];
				currIndex++;
			}
		}
		return ret;
	}

	public static Color ToGrayScale(Color orig){
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

}
