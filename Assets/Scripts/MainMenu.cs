using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MainMenu : MonoBehaviour {

	public SpriteButton spriteButton;
	public GameObject categoriesPanel;
	public GameObject spritePanel;

	public List<Globals.CategoryCollection> categories;

	public Globals.Categories previousType;
	public Globals.Categories type;

	public GameData gameData;

	void Start(){
		if (File.Exists (Application.persistentDataPath + "GameData.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "GameData.dat", FileMode.Open);
			GameData dat = (GameData)bf.Deserialize (file);
			file.Close ();
			gameData = dat;
			//If more than 24 hours passed.
			System.TimeSpan ts = gameData.dayStarted - System.DateTime.Now;
			Debug.Log ("Time passed: " + ts);
			if (ts.TotalHours > 24) {
				int totalTokens = 0;
				int dayCount = 0;
				while (ts.TotalHours > 24) {
					dayCount++;
					System.TimeSpan newSpan = new System.TimeSpan (0, 24, 0);
					ts = ts.Subtract (newSpan);
					totalTokens += 30;
				}
				int minutesPassed = (int)ts.TotalMinutes;
				int addTokens = 0;
				if (minutesPassed > 2) {
					addTokens = Mathf.FloorToInt (minutesPassed);
					addTokens -= gameData.tokensClaimed;
					addTokens = Mathf.Clamp (addTokens, 0, 30);
					if (addTokens > 0) {
						totalTokens += addTokens;
					}
				}
				System.DateTime newDay = gameData.dayStarted.AddDays (dayCount);
				gameData.dayStarted = newDay;
				gameData.tokensClaimed = addTokens;
				gameData.tokenCount += totalTokens;
			}
			//Else, calculate how many tokens have been earned and cap them at 30.
			else {
				if (gameData.tokensClaimed < 30) {
					int minutesPassed = (int)ts.TotalMinutes;
					if (minutesPassed > 2) {
						int tokenCount = Mathf.FloorToInt (minutesPassed / 2);
						tokenCount -= gameData.tokensClaimed;
						tokenCount = Mathf.Clamp (tokenCount, 0, 30);
						if (tokenCount > 0) {
							gameData.tokenCount += tokenCount;
						}
					}
				}
			}
			SaveData ();
		} else {
			//Startup data.
			GameData newDat = new GameData();
			newDat.tokenCount = 50;
			newDat.dayStarted = System.DateTime.Now;
			gameData = newDat;
			SaveData ();
		}
		categories = new List<Globals.CategoryCollection> ();
		if(!Directory.Exists(Application.persistentDataPath + "/Thumbs/")){
			Directory.CreateDirectory (Application.persistentDataPath + "/Thumbs/");
		}
		if (!Directory.Exists (Application.persistentDataPath + "/LevelData/")) {
			Directory.CreateDirectory (Application.persistentDataPath + "/LevelData/");
		}
		StartCoroutine (InitializeData ());
	}

	IEnumerator InitializeData(){
		yield return null;
		Object[] sprites = Resources.LoadAll("Sprite Objects", typeof(SpriteData));
		List<SpriteButton> allButtons = new List<SpriteButton> ();
		for (int xx = 0; xx < sprites.Length; xx++) {
			SpriteButton newButton = Instantiate (spriteButton);
			newButton.data = sprites [xx] as SpriteData;
			allButtons.Add (newButton);
			if (File.Exists (Application.persistentDataPath + "/LevelData/" + newButton.data.myName + ".lev")) {
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Open (Application.persistentDataPath + "/LevelData/" + newButton.data.myName + ".lev", FileMode.Open);
				ProgressData dat = (ProgressData)bf.Deserialize (file);
				file.Close ();
				newButton.progress = dat;
			}
			if(File.Exists(Application.persistentDataPath + "/Thumbs/" + newButton.data.myName + ".png")){
				byte[] imageData = File.ReadAllBytes (Application.persistentDataPath + "/Thumbs/" + newButton.data.myName + ".png");
				Texture2D newTex = new Texture2D (0, 0);
				newTex.LoadImage (imageData);
				newTex.filterMode = FilterMode.Point;
				newTex.wrapMode = TextureWrapMode.Clamp;
				newTex.Apply ();
				newButton.myCol.sprite = Sprite.Create (newTex, new Rect (0, 0, newTex.width, newTex.height), Vector2.zero, 100);
			}
			else{
				newButton.myCol.sprite = Sprite.Create(newButton.data.thumb, new Rect(0, 0, newButton.data.thumb.width, newButton.data.thumb.height), Vector2.zero, 100);
			}
			newButton.myButton.onClick.AddListener (delegate {
				LoadLevel (newButton.data.myName);
			});
		}
		System.Array typeNums = System.Enum.GetValues (typeof(Globals.Categories));
		Globals.Categories[] types = new Globals.Categories[typeNums.Length];
		System.Array.Copy (typeNums, types, typeNums.Length);

		for (int xx = 0; xx < types.Length; xx++) {
			Globals.CategoryCollection coll = new Globals.CategoryCollection ();
			coll.buttons = new List<SpriteButton> ();
			coll.type = types [xx];
			for (int yy = 0; yy < allButtons.Count; yy++) {
				if (coll.type != Globals.Categories.Started && coll.type != Globals.Categories.Completed) {
					if (allButtons [yy].data.type == types [xx]) {
						if (allButtons [yy].progress == null) {
							coll.buttons.Add (allButtons [yy]);
						} else if (!allButtons [yy].progress.started && !allButtons [yy].progress.completed) {
							coll.buttons.Add (allButtons [yy]);
						}
					}
				} else {
					if (coll.type == Globals.Categories.Completed) {
						if (allButtons [yy].progress != null) {
							if (allButtons [yy].progress.completed) {
								coll.buttons.Add (allButtons [yy]);
							}
						}
					} else if (coll.type == Globals.Categories.Started) {
						if (allButtons [yy].progress != null) {
							if (!allButtons [yy].progress.completed) {
								if (allButtons [yy].progress.started) {
									coll.buttons.Add (allButtons [yy]);
								}
							}
						}
					}
				}
			}
			coll.buttons = SortByDifficulty (true, coll.buttons);
			categories.Add (coll);
		}
		Debug.Log ("Finished loading assets.");
		LayoutRebuilder.MarkLayoutForRebuild(spritePanel.GetComponent<RectTransform> ());
	}

	List<SpriteButton> SortByDifficulty(bool leastToMost, List<SpriteButton> list){
		List<SpriteButton> ret = new List<SpriteButton> ();
		if (leastToMost) {
			while (list.Count > 0) {
				int currLowest = int.MaxValue;
				int index = 0;
				for (int xx = 0; xx < list.Count; xx++) {
					if (list [xx].data.difficulty < currLowest) {
						currLowest = list [xx].data.difficulty;
						index = xx;
					}
				}
				ret.Add (list [index]);
				list.RemoveAt (index);
			}
		} else {
			while (list.Count > 0) {
				int currHighest = 0;
				int index = 0;
				for (int xx = 0; xx < list.Count; xx++) {
					if (list [xx].data.difficulty > currHighest) {
						currHighest = list [xx].data.difficulty;
						index = xx;
					}
				}
				ret.Add (list [index]);
				list.RemoveAt (index);
			}
		}
		return ret;
	}

	public void OpenCategories(){
		spritePanel.SetActive (false);
		categoriesPanel.SetActive (true);
	}

	public void TriggerCategory(string cat){
		if (cat == "People") {
			SwitchCategory (Globals.Categories.People);
		} else if (cat == "Food") {
			SwitchCategory (Globals.Categories.Food);
		} else if (cat == "Animals") {
			SwitchCategory (Globals.Categories.Animals);
		} else if (cat == "Weapons") {
			SwitchCategory (Globals.Categories.Weapons);
		} else if (cat == "Vehicles") {
			SwitchCategory (Globals.Categories.Vehicles);
		} else if (cat == "Buildings") {
			SwitchCategory (Globals.Categories.Buildings);
		} else if (cat == "Plants") {
			SwitchCategory (Globals.Categories.Plants);
		} else if (cat == "Monsters") {
			SwitchCategory (Globals.Categories.Monsters);
		} else if (cat == "Dinosaurs") {
			SwitchCategory (Globals.Categories.Dinosaurs);
		} else if (cat == "Scenery") {
			SwitchCategory (Globals.Categories.Scenery);
		} else if (cat == "Misc") {
			SwitchCategory (Globals.Categories.Misc);
		} else if (cat == "Started") {
			SwitchCategory (Globals.Categories.Started);
		} else if (cat == "Completed") {
			SwitchCategory (Globals.Categories.Completed);
		}
	}

	void SwitchCategory(Globals.Categories cat){
		categoriesPanel.SetActive (false);
		previousType = type;
		type = cat;
		for (int xx = 0; xx < categories.Count; xx++) {
			if (categories [xx].type == previousType) {
				for (int yy = 0; yy < categories [xx].buttons.Count; yy++) {
					categories [xx].buttons [yy].gameObject.SetActive (false);
					categories [xx].buttons [yy].transform.SetParent (null);
				}
			}
		}
		for (int xx = 0; xx < categories.Count; xx++) {
			if (categories [xx].type == type) {
				for (int yy = 0; yy < categories [xx].buttons.Count; yy++) {
					categories [xx].buttons [yy].gameObject.SetActive (true);
					categories [xx].buttons [yy].transform.SetParent (spritePanel.transform);
				}
			}
		}
		spritePanel.SetActive (true);
		LayoutRebuilder.MarkLayoutForRebuild (spritePanel.GetComponent<RectTransform> ());
	}

	void SaveData(){
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "GameData.dat");
		bf.Serialize (file, gameData);
		file.Close ();
	}

	void LoadLevel(string level){
		SpriteData myLevel = Resources.Load<SpriteData> ("Sprite Objects/" + level);
		GameManager.Instance.level = myLevel as SpriteData;
		UnityEngine.SceneManagement.SceneManager.LoadScene (1, UnityEngine.SceneManagement.LoadSceneMode.Single);
	}

}

[System.Serializable]
public class GameData{

	public int tokenCount;
	public int tokensClaimed;
	public System.DateTime dayStarted;

}