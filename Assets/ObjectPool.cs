using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

	//Try queue or stack instead.

	public List<Tile> pool;
	public List<bool> activeState;
	public Tile tile;
	public GameObject basePlane;
	public GameObject tileParent;

	public bool gridActive = false;

	int listCount;

	void Awake(){
		listCount = pool.Count;
		for (int xx = 0; xx < listCount; xx++) {
			pool [xx].myIndex = xx;
			activeState.Add (false);
		}
	}

	public void DisableAll(){
		for (int xx = 0; xx < listCount; xx++) {
			pool [xx].myText.text = "";	
		}
	}

	public void ReturnTile(Tile tile){
		activeState [tile.myIndex] = false;
		tile.myText.text = "";
	}

	public Tile GetTile(){
		Tile ret = null;
		bool found = false;
		int index = 0;
		for (int xx = 0; xx < listCount; xx++) {
			if(!activeState[xx]){
				found = true;
				index = xx;
				break;
			}
		}
		if (!found) {
			ret = Instantiate (tile);
			ret.myIndex = listCount;
			pool.Add (ret);
			listCount = pool.Count;
			activeState.Add (true);
			ret.transform.SetParent (tileParent.transform);
		} else {
			ret = pool [index];
			activeState [index] = true;
		}
		return ret;
	}
}
