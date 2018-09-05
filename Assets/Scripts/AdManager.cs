using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour {

	public string gameID;
	public bool adReady;

	void Start(){
		GameManager.Instance.ads = this;
		Advertisement.Initialize (gameID);
	}

	void Update(){
		if(Advertisement.IsReady("rewardedVideo")){
			adReady = true;
		}
		else{
			adReady = false;
		}
	}

	public void ShowRewardedVideo (){
		ShowOptions options = new ShowOptions();
		options.resultCallback = HandleShowResult;
		Advertisement.Show("rewardedVideo", options);
	}

	void HandleShowResult (ShowResult result){
		if(result == ShowResult.Finished) {
			Debug.Log("Video completed - Offer a reward to the player");
			Camera.main.GetComponent<MainMenu> ().VideoComplete ();
		}
		else if(result == ShowResult.Skipped) {
			Debug.LogWarning("Video was skipped - Do NOT reward the player");
			Camera.main.GetComponent<MainMenu> ().VideoCanceled ();
		}
		else if(result == ShowResult.Failed) {
			Debug.LogError("Video failed to show");
			Camera.main.GetComponent<MainMenu> ().VideoCanceled ();
		}
	}
}
