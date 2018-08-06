using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {

	public enum InputState{
		Waiting,
		Painting,
		Dragging
	}

	public struct Coord{
		public int x;
		public int y;
		public Coord(int xx, int yy){
			x = xx;
			y = yy;
		}
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

}
