using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaneScript : MonoBehaviour
{
	//Lane Properties
    public Vector3 startPos;
    public Vector3 lanePos;
    public Vector3 endPos;
    private List<float> noteTimes;
    public GameObject notePrefab;
    public Text display;
    public string displayPrefix;
    private List<NoteScript> noteObjs;
    private byte holdCount = 0;
	private LineRenderer hitLine;
	private Color hitColor = Color.red;
    private Color baseColor = Color.white;

    //Property Accessors
    public byte holdCounter
    {
        get
        { return holdCount; }
    }
    public int noteCount
    {
        get
        { return noteTimes.Count; }
    }
    public List<float> NoteTimes
    {
        get
        { return noteTimes; }
    }
    public List<NoteScript> NoteObjects
    {
        get
        { return noteObjs; }
    }

    //Start()
	void Start()
	{
        display.text = displayPrefix + "N/A";
		hitLine = this.GetComponent<LineRenderer> ();
        noteTimes = new List<float>();
        noteObjs = new List<NoteScript>();
	}

    //SetNotes()
    public void SetNotes(List<float> notes)
    {
        noteTimes = notes;
    }

    //IncrementHoldCounter()
    public void IncrementHoldCounter()
    {
        holdCount++;
    }

    //ResetHoldCounter()
    public void ResetHoldCounter()
    {
        holdCount = 0;
    }

    //ChangeHoldColor()
    public void ChangeHoldColor(bool isHit)
    {
        if (isHit)
            hitLine.material.color = hitColor;
        else
            hitLine.material.color = baseColor;
    }

    //ChangeHitText()
	public void ChangeHitText(bool noteHit, float msOffset)
    {
        if (noteHit)
        {
            display.color = Color.green;
			display.text = displayPrefix + "Hit! (" + msOffset + ")";
        }
        else
        {
            display.color = Color.red;
            display.text = displayPrefix + "Miss!";
        }
    }
}