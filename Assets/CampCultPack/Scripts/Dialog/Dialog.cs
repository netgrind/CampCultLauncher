﻿using UnityEngine;  
using System.Collections.Generic;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {  
	public List<DialogElement> dialogs = new List<DialogElement>();

	Text text;
	Image bg;
	int index = 0;
	bool dialogMode = false;
	Color bgOn = new Color (1, 1, 1, .4f);
	Color bgOff = new Color (1, 1, 1, 0);
	List<MonoBehaviour> scriptsToLock = new List<MonoBehaviour>();
	Camera cam;
	Quaternion  lookTarget;
	Quaternion  lookStart;
	Quaternion  initialLook;
	float lineHeight = 25;
	string[] newLine = new string[]{"\n"};

	void OnEnable(){
		text = FindObjectOfType<Text> ();
		text.text = "";
		bg = text.rectTransform.parent.GetComponent<Image> ();
        bg.color = bgOff;
        cam = Camera.main;
        //abstract below?
		/*MouseLook[] ml = FindObjectsOfType<MouseLook> ();
		foreach(MouseLook m in ml)
			scriptsToLock.Add (m);
		scriptsToLock.Add (FindObjectOfType<FirstPersonDrifter> ());
		scriptsToLock.Add (FindObjectOfType<MouseLook> ());
		scriptsToLock.Add (FindObjectOfType<PlayerController> ());
		scriptsToLock.Add (FindObjectOfType<HeadBob> ());*/
	}

	void Update()
    {
        if (dialogMode) {
			if (Input.GetMouseButtonDown (0)) {
				NextInput ();
			}
		}
		if (text.text.Length != 0) {
			bg.color = Color.Lerp (bg.color, bgOn, .05f);
		} else {
			bg.color = Color.Lerp (bg.color, bgOff, .05f);
		}
	}

	public void StartDialog(){
		index = 0;
		initialLook = cam.transform.rotation;
		dialogMode = true;
		foreach (MonoBehaviour m in scriptsToLock)
			m.enabled = false;
		NextInput ();
	}

	public void EndDialog(){
		dialogMode = false;
		cam.transform.rotation = initialLook;
		foreach (MonoBehaviour m in scriptsToLock)
			m.enabled = true;
		text.text = "";
	}

	void NextInput()
    {
        if (index == dialogs.Count)
        {
            EndDialog ();
			return;
		}
		bool fireNext = false;
		DialogElement d = dialogs [index];
		if (d.type == DialogElement.Type.Dialog) {
			text.text = d.string1;
			Vector2 v = text.rectTransform.sizeDelta;
			v.y = lineHeight * text.text.Split (newLine, System.StringSplitOptions.RemoveEmptyEntries).Length;
			text.rectTransform.sizeDelta = v;
		} else if (d.type == DialogElement.Type.Event) {
			Messenger.Broadcast (d.string1);
			fireNext = true;
		}else if (d.type == DialogElement.Type.LookAt) {
			lookStart = cam.transform.rotation;
			cam.transform.LookAt(d.transform1);
			lookTarget = cam.transform.rotation;
			cam.transform.rotation = lookStart;
			dialogMode = false;
			StartCoroutine( Utils.AnimationCoroutine (AnimationCurve.EaseInOut(0,0,1,1), d.float1, LookAt, FinishLookAt));
		}else if (d.type == DialogElement.Type.Sound) {
			d.audio.Play ();
			fireNext = true;
        }
        index++;
		if (fireNext)
			NextInput ();
    }

	void FinishLookAt()
    {
        dialogMode = true;
        NextInput ();
	}

	void LookAt(float f){
		cam.transform.rotation = Quaternion.Lerp (lookStart, lookTarget, f);
	}
}