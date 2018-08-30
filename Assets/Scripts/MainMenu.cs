using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MainMenu : MonoBehaviour {

	public ColorButton spriteButton;
	public GameObject spritePanel;

	void Start(){
		if(!Directory.Exists(Application.persistentDataPath + "/Thumbs/")){
			Directory.CreateDirectory (Application.persistentDataPath + "/Thumbs/");
		}
		if (!Directory.Exists (Application.persistentDataPath + "/LevelData/")) {
			Directory.CreateDirectory (Application.persistentDataPath + "/LevelData/");
		}
		Object[] sprites = Resources.LoadAll("Sprite Objects", typeof(SpriteData));
		for (int xx = 0; xx < sprites.Length; xx++) {
			SpriteData current = sprites [xx] as SpriteData;
			ColorButton newButton = Instantiate (spriteButton);
			newButton.transform.SetParent (spritePanel.transform);
			if(File.Exists(Application.persistentDataPath + "/Thumbs/" + current.myName + ".png")){
				byte[] imageData = File.ReadAllBytes (Application.persistentDataPath + "/Thumbs/" + current.myName + ".png");
				Texture2D newTex = new Texture2D (0, 0);
				newTex.LoadImage (imageData);
				newTex.filterMode = FilterMode.Point;
				newTex.wrapMode = TextureWrapMode.Clamp;
				newTex.Apply ();
				newButton.myCol.sprite = Sprite.Create (newTex, new Rect (0, 0, newTex.width, newTex.height), Vector2.zero, 100);
			}
			else{
				newButton.myCol.sprite = Sprite.Create(current.thumb, new Rect(0, 0, current.thumb.width, current.thumb.height), Vector2.zero, 100);
			}
			newButton.myButton.onClick.AddListener (delegate {
				LoadLevel (current.myName);
			});
		}
		LayoutRebuilder.MarkLayoutForRebuild(spritePanel.GetComponent<RectTransform> ());
	}

	void LoadLevel(string level){
		SpriteData myLevel = Resources.Load<SpriteData> ("Sprite Objects/" + level);
		GameManager.Instance.level = myLevel as SpriteData;
		UnityEngine.SceneManagement.SceneManager.LoadScene (1, UnityEngine.SceneManagement.LoadSceneMode.Single);
	}

}
