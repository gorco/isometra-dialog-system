﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using Isometra.Sequences;
using Isometra;

public class CustomDialogManager : DialogEventManager {

    public enum State
    {
        Opening, Showing, Closing, Idle, Options
    }

    // Public variables
    public float timePerCharacter;
    public float fadeTime;
    public Text nameText;
    public Text textHolder;
    public Text optionsMessage;
    public GameObject interactionBlocker;
    public CanvasGroup dialogGroup;
    public CanvasGroup optionsGroup;
    public LayoutGroup optionsHolder;
    public GameObject optionPrefab;
    public CharacterCuller faceCamera;
    public Image face;

    // Private variables
    private State state = State.Idle;
    private Fragment frg;
    private List<Option> opt;
    private List<GameObject> instancedOptions = new List<GameObject>();
    private int charactersShown;
    private float accumulated;
    private string msg = "";
    private CanvasGroup managingGroup;
    private Option optionSelected;

    void Start()
    {
        state = State.Idle;
    }

    // ------------------------------
    // Abstract class implementation
    // ------------------------------

    protected override void DoFragment(Fragment fragment)
    {
        frg = fragment;
        msg = frg.Msg;
        nameText.text = frg.Name;
        charactersShown = 0;
        state = State.Opening;
        managingGroup = dialogGroup;
        managingGroup.gameObject.SetActive(true);
        interactionBlocker.SetActive(true);

        if (frg.Character != "")
        {
            faceCamera.character = GameObject.Find(frg.Character);
        }

        face.rectTransform.offsetMax = new Vector2(faceCamera.character ? face.rectTransform.rect.height : 40, face.rectTransform.offsetMax.y);
    }


    protected override void DoOptions(string question, List<Option> options)
    {
        opt = options;
        msg = question;

        optionSelected = null;
        optionsMessage.text = msg;
        managingGroup = optionsGroup;
        managingGroup.gameObject.SetActive(true);
        interactionBlocker.SetActive(true);
        state = State.Opening;
        foreach (var o in opt)
        {
            // create the options
            var option = GameObject.Instantiate(optionPrefab);
            option.transform.SetParent(optionsHolder.transform);
			option.transform.localScale = Vector3.one;
            var text = option.transform.GetChild(0).GetComponent<Text>().text = o.Text;
            option.GetComponent<Button>().onClick.AddListener(() => {
                optionSelected = opt.Find(e => e.Text == text);
            });
            instancedOptions.Add(option);
        }
    }

    protected override bool IsFragmentFinised() { return frg == null; }
    protected override int IsOptionSelected() { return state == State.Idle ? opt.FindIndex(o => o == optionSelected) : -1;}

    // -----------------------------------
    // State management during update
    // -----------------------------------
	
	void Update () {
        switch (state)
        {
            case State.Opening:
                // No matter what, when opening, just fade in the group
                managingGroup.alpha = Mathf.Clamp01(managingGroup.alpha + Time.deltaTime / fadeTime);
                if (managingGroup.alpha == 1)
                    state = State.Showing;
                break;
            case State.Showing:
                // When showing we'll care about the shown group
                if (managingGroup == dialogGroup)
                {

                    // If showing a fragment, show more characters until all of them are displayed
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (charactersShown < msg.Length)
                        {
                            charactersShown = msg.Length;
                            UpdateText();
                        }
                        else state = State.Closing;
                    }

                    accumulated += Time.deltaTime;
                    while (accumulated > timePerCharacter)
                    {
                        charactersShown = Mathf.Clamp(charactersShown + 1, 0, msg.Length);
                        UpdateText();
                    }
                }
                else if (managingGroup == optionsGroup)
                {
                    if (optionSelected != null) state = State.Closing;
                }

                break;
            case State.Closing:
                // No matter what, when closing, just fade out the displayed group
                managingGroup.alpha = Mathf.Clamp01(managingGroup.alpha - Time.deltaTime / fadeTime);
                if (managingGroup.alpha == 0)
                {
                    faceCamera.character = null;
                    managingGroup.gameObject.SetActive(false);
                    interactionBlocker.SetActive(false);
                    state = State.Idle;
                    textHolder.text = "";
                    frg = null;
                    foreach (var io in instancedOptions)
                        GameObject.DestroyImmediate(io);
                }
                break;
            case State.Idle:
                break;
        }

        if (face.isActiveAndEnabled)
        {
            textHolder.rectTransform.offsetMin = new Vector2(face.rectTransform.offsetMax.x + 20, 20);
        }
    }

	void OnDestroy()
	{
		Game.main.enqueueEvent(new GameEvent("abort sequence"));
		//Debug.LogError("Dialog destroyed");
	}

    private void UpdateText()
    {
        accumulated -= timePerCharacter;
        textHolder.text = msg.Substring(0, charactersShown) + "<color=#00000000>" + msg.Substring(charactersShown) + "</color>";
    }
}
