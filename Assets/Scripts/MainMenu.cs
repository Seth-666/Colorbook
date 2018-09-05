using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MainMenu : MonoBehaviour {

	public Animator anim;

	public SpriteButton spriteButton;
	public GameObject categoriesPanel;
	public GameObject spritePanel;

	public List<Globals.CategoryCollection> categories;

	public Globals.Categories previousType;
	public Globals.Categories type;

	public GameData gameData;

	public Globals.MainMenu lastState;
	public Globals.MainMenu currState;

	public Globals.MainMenu targetState;

	public Globals.MainLoadState state;
	bool initialized = false;
	float timer;

	public Text dialogText;
	public Button yesButton;
	public Button noButton;

	public SpriteButton selectedArt;

	void Awake(){
		if(!Directory.Exists(Application.persistentDataPath + "/MainData/")){
			Directory.CreateDirectory (Application.persistentDataPath + "/MainData/");
		}
		if(!Directory.Exists(Application.persistentDataPath + "/Thumbs/")){
			Directory.CreateDirectory (Application.persistentDataPath + "/Thumbs/");
		}
		if (!Directory.Exists (Application.persistentDataPath + "/LevelData/")) {
			Directory.CreateDirectory (Application.persistentDataPath + "/LevelData/");
		}
	}

	void Update(){
		timer += Time.deltaTime;
		if (timer > 120) {
			timer = 0;
			CheckTimeDiff ();
		}
		if (Input.GetKey (KeyCode.Escape)) {
			if (currState == Globals.MainMenu.Sprites) {
				OpenCategories ();
			} else if (currState == Globals.MainMenu.Dialog) {
				CancelDialog ();
			}
		}
	}

	void OnApplicationQuit(){
		SaveData ();
	}

	void OnApplicationFocus(bool focus){
		if (initialized) {
			if (!focus) {
				SaveData ();
			} else {
				CheckTimeDiff ();
			}
		}
	}

	void OnApplicationPause(bool pause){
		if (initialized) {
			if (pause) {
				SaveData ();
			} else {
				CheckTimeDiff ();
			}
		}
	}

	public void AnimationFinished(){
		if (targetState == Globals.MainMenu.Dialog) {
			yesButton.interactable = true;
			noButton.interactable = true;
		}
		currState = targetState;
	}

	public void OpenDialog(){
		dialogText.text = "Would you like to unlock this art for 10 tokens?";
		yesButton.onClick.RemoveAllListeners ();
		yesButton.onClick.AddListener(delegate {
			UnlockArt();
		});
		noButton.onClick.RemoveAllListeners ();
		noButton.onClick.AddListener(delegate {
			CancelDialog();
		});
		lastState = currState;
		currState = Globals.MainMenu.Busy;
		targetState = Globals.MainMenu.Dialog;
		anim.SetTrigger ("Dialog");
	}

	public void CancelDialog(){
		yesButton.interactable = false;
		noButton.interactable = false;
		targetState = lastState;
		currState = Globals.MainMenu.Busy;
		anim.SetTrigger ("Dialog");
	}

	public void VideoDialog(){
		dialogText.text = "You don't have enough tokens to unlock this art. Would you like to watch an ad for 15 tokens?";
		yesButton.onClick.RemoveAllListeners ();
		yesButton.onClick.AddListener(delegate {
			WatchVideo();
		});
		noButton.onClick.RemoveAllListeners ();
		noButton.onClick.AddListener(delegate {
			CancelDialog();
		});
	}

	public void WatchVideo(){
		GameManager.Instance.ads.ShowRewardedVideo ();
	}

	public void VideoComplete(){
		gameData.tokenCount += 15;
		CancelDialog ();
		SaveData ();
	}

	public void VideoCanceled(){
		CancelDialog ();
		SaveData ();
	}

	public void UnlockArt(){
		if (gameData.tokenCount >= 10) {
			UnlockSingular ();
			CancelDialog ();
			SaveData ();
		} else {
			if (GameManager.Instance.ads.adReady) {
				VideoDialog ();
			} else {
				dialogText.text = "You don't have enough tokens to unlock this art.";
			}
		}
	}

	void UnlockSingular(){
		gameData.tokenCount -= 10;
		if (!gameData.unlocked.Contains (selectedArt.data.myName)) {
			gameData.unlocked.Add (selectedArt.data.myName);
		}
		selectedArt.locked.enabled = false;
	}

	void Start(){
		lastState = Globals.MainMenu.Categories;
		currState = Globals.MainMenu.Categories;
		bool initialData = false;
		if (File.Exists (Application.persistentDataPath + "/MainData/GameData.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/MainData/GameData.dat", FileMode.Open);
			GameData dat = (GameData)bf.Deserialize (file);
			file.Close ();
			gameData = dat;
		} else {
			GameData newDat = new GameData();
			newDat.tokenCount = 50;
			System.DateTime currTime = System.DateTime.Now;
			newDat.dayStarted = currTime;
			newDat.lastCheck = currTime;
			newDat.unlocked = new List<string> ();
			gameData = newDat;
			initialData = true;
		}
		categories = new List<Globals.CategoryCollection> ();
		StartCoroutine(InitializeData ());
		if (initialData) {
			StartCoroutine(UnlockInitialData ());
		} else {
			state = Globals.MainLoadState.UnlockingSprites;
		}
		StartCoroutine(SetLockedState ());
		StartCoroutine(CheckTimeDiff ());
	}

	IEnumerator SetLockedState(){
		while (state != Globals.MainLoadState.UnlockingSprites) {
			yield return null;
		}
		yield return null;
		for (int xx = 0; xx < categories.Count; xx++) {
			for (int yy = 0; yy < categories [xx].buttons.Count; yy++) {
				if (gameData.unlocked.Contains (categories [xx].buttons [yy].data.myName)) {
					categories [xx].buttons [yy].locked.enabled = false;
				}
			}
		}
		state = Globals.MainLoadState.Complete;
	}

	IEnumerator UnlockInitialData(){
		while (state != Globals.MainLoadState.InitialUnlock) {
			yield return null;
		}
		yield return null;
		for (int xx = 0; xx < categories.Count; xx++) {
			int unlockCount = 0;
			if (categories [xx].type == Globals.Categories.Buildings || categories [xx].type == Globals.Categories.Scenery) {
				unlockCount = 5;
			} else if (categories [xx].type != Globals.Categories.Completed || categories [xx].type != Globals.Categories.Started) {
				unlockCount = 10;
			}
			unlockCount = Mathf.Clamp (unlockCount, 0, (categories [xx].buttons.Count));
			for(int yy = 0; yy < unlockCount; yy++){
				if(!gameData.unlocked.Contains(categories[xx].buttons[yy].data.myName)){
					gameData.unlocked.Add (categories [xx].buttons [yy].data.myName);
				}
			}
		}
		state = Globals.MainLoadState.UnlockingSprites;
	}

	IEnumerator CheckTimeDiff(){
		if (state != Globals.MainLoadState.Complete) {
			yield return null;
		}
		yield return null;
		if (initialized) {
			System.TimeSpan dayCheck = System.DateTime.Now - gameData.dayStarted;
			if (dayCheck.TotalHours > 24) {
				int totalDays = Mathf.FloorToInt ((float)dayCheck.TotalHours / 24);
				int totalTokens = (totalDays * 30) - gameData.tokensClaimed;
				gameData.tokensClaimed = 0;
				gameData.dayStarted = gameData.dayStarted.AddDays (totalDays);
				gameData.lastCheck = gameData.dayStarted;
				gameData.tokenCount += totalTokens;
			}
			System.TimeSpan currCheck = System.DateTime.Now - gameData.lastCheck;
			if (currCheck.TotalMinutes > 2) {
				if (gameData.tokensClaimed < 30) {
					int totalMinutes = Mathf.FloorToInt ((float)currCheck.TotalMinutes);
					int totalTokens = Mathf.Clamp (Mathf.FloorToInt(totalMinutes / 2), 0, 30);
					totalTokens -= gameData.tokensClaimed;
					gameData.tokensClaimed += totalTokens;
					gameData.lastCheck = gameData.lastCheck.AddMinutes (totalMinutes * 2);
					gameData.tokenCount += totalTokens;
				}
			}
			SaveData ();
		}
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
				LoadLevel (newButton);
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
		LayoutRebuilder.MarkLayoutForRebuild(spritePanel.GetComponent<RectTransform> ());
		initialized = true;
		state = Globals.MainLoadState.InitialUnlock;
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
		if (currState != Globals.MainMenu.Dialog && currState != Globals.MainMenu.Busy) {
			spritePanel.SetActive (false);
			categoriesPanel.SetActive (true);
			lastState = currState;
			currState = Globals.MainMenu.Categories;
		}
	}

	public void TriggerCategory(string cat){
		if (currState != Globals.MainMenu.Dialog && currState != Globals.MainMenu.Dialog) {
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
			} else if (cat == "Nature") {
				SwitchCategory (Globals.Categories.Nature);
			} else if (cat == "Monsters") {
				SwitchCategory (Globals.Categories.Monsters);
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
	}

	void SwitchCategory(Globals.Categories cat){
		if (currState != Globals.MainMenu.Dialog && currState != Globals.MainMenu.Dialog) {
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
			lastState = currState;
			currState = Globals.MainMenu.Sprites;
		}
	}

	void SaveData(){
		if (initialized) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (Application.persistentDataPath + "/MainData/GameData.dat");
			bf.Serialize (file, gameData);
			file.Close ();
		}
	}

	void LoadLevel(SpriteButton button){
		SaveData ();
		if (gameData.unlocked.Contains (button.data.myName)) {
			SpriteData myLevel = Resources.Load<SpriteData> ("Sprite Objects/" + button.data.myName);
			GameManager.Instance.level = myLevel as SpriteData;
			UnityEngine.SceneManagement.SceneManager.LoadScene (1, UnityEngine.SceneManagement.LoadSceneMode.Single);
		} else {
			selectedArt = button;
			OpenDialog ();
		}
	}

}

[System.Serializable]
public class GameData{

	public int tokenCount;
	public int tokensClaimed;
	public System.DateTime dayStarted;
	public System.DateTime lastCheck;
	public List<string> unlocked;

}