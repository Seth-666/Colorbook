﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour {

	public void AnimationFinished(){
		GameManager.Instance.ui.AnimFinished ();
	}

	public void MainAnimationFinished(){
		Camera.main.GetComponent<MainMenu> ().AnimationFinished ();
	}

}
