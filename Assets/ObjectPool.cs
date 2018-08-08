using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

	public List<Tile> pool;
	public List<bool> activeState;
	public Tile tile;
	public GameObject basePlane;
	public GameObject tileParent;

	public bool gridActive = false;

	void Awake(){
		for (int xx = 0; xx < pool.Count; xx++) {
			pool [xx].myIndex = xx;
			activeState.Add (false);
		}
	}

	public void DisableAll(){
		GameManager.Instance.painter.ClearText ();
		if(gridActive){
			for (int xx = 0; xx < pool.Count; xx++) {
				pool[xx].transform.position = new Vector2(10000, 10000);
				activeState [xx] = false;
			}
			gridActive = false;
		}
	}

	public void ReturnTile(Tile tile){
		activeState [tile.myIndex] = false;
		tile.transform.position = new Vector2(10000, 10000);
	}

	public Tile GetTile(){
		Tile ret = null;
		bool found = false;
		int index = 0;
		for (int xx = 0; xx < pool.Count; xx++) {
			if(!activeState[xx]){
				found = true;
				index = xx;
				break;
			}
		}
		if (!found) {
			ret = Instantiate (tile);
			ret.myIndex = pool.Count;
			pool.Add (ret);
			activeState.Add (true);
			ret.transform.SetParent (tileParent.transform);
		} else {
			ret = pool [index];
			activeState [index] = true;
		}
		return ret;
	}
}
