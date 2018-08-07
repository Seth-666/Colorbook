using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

	public Tile[] pool;
	public Tile tile;
	public GameObject basePlane;

	bool poolEmpty = false;

	public Tile GetTile(){
		Tile ret = null;
		if (!poolEmpty) {
			for (int xx = 0; xx < pool.Length; xx++) {
				if (!pool [xx].gameObject.activeSelf) {
					ret = pool [xx];
					break;
				}
			}
			if (ret == null) {
				poolEmpty = true;
				ret = Instantiate (tile);
			}
		}
		else{
			ret = Instantiate (tile);
		}
		return ret;
	}
}
