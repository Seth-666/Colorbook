using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

	//Try queue

	public Tile[] allTiles;

	Queue<Tile> inactiveTiles;

	public Tile tile;
	public GameObject basePlane;
	public GameObject tileParent;

	public bool gridActive = false;

	void Awake(){
		inactiveTiles = new Queue<Tile> ();
		for (int xx = 0; xx < allTiles.Length; xx++) {
			inactiveTiles.Enqueue (allTiles [xx]);
		}
	}

	public void ReturnTile(Tile tile){
		tile.myText.text = "";
		inactiveTiles.Enqueue (tile);
	}

	public Tile GetTile(){
		Tile ret = null;
		if (inactiveTiles.Count > 0) {
			ret = inactiveTiles.Dequeue ();
		}
		else{
			ret = Instantiate (tile);
			ret.transform.SetParent (tileParent.transform);
		}
		return ret;
	}
}
