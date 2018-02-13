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
    private List<Note> noteObjs;
    private float beatsInAdvance = 2.0f;
    private float noteSpeed = 1.0f;
    private byte holdCounter = 0;
	private LineRenderer hitLine;
	private Color hitColor = Color.red;
    private Color baseColor = Color.white;

	void Start()
	{
        display.text = displayPrefix + "N/A";
		hitLine = this.GetComponent<LineRenderer> ();
        noteTimes = new List<float>();
        noteObjs = new List<Note>();
	}

    //SetNotes()
    public void SetNotes(List<float> notes)
    {
        noteTimes = notes;
    }

	//UpdateInput()
	public void UpdateInput(bool input, float audioTime)
	{
        if (input)
        {
            if (holdCounter < 5)
            {
                hitLine.material.color = hitColor;
                holdCounter++;
            }
            else
            {
                hitLine.material.color = baseColor;
            }
        }
        else
        {
            hitLine.material.color = baseColor;
            holdCounter = 0;
        }

        //Spawning New Notes
        for (int num = 0; num < noteTimes.Count; num++)
        {
            float notePos = noteTimes[num];
            
            if (audioTime + beatsInAdvance > notePos)
            {
                Note newNote = Instantiate(notePrefab, startPos, Quaternion.identity).GetComponent<Note>();
                newNote.SetProperties(notePos, noteSpeed, beatsInAdvance, startPos, lanePos, endPos);
                noteObjs.Add(newNote);
                noteTimes.Remove(notePos);
                num--;
            }
        }

        //Moving/Deleting the Note Objects
        for (int num = 0; num < noteObjs.Count; num++)
        {
            bool destroyNote = false;
            noteObjs[num].Move(audioTime);

            if (noteObjs[num].lerpFactor >= 1.15f)
            {
                display.color = Color.red;
                display.text = displayPrefix + "Miss!";
                destroyNote = true;
            }

            if (holdCounter % 5 > 0   &&   Mathf.Abs(noteObjs[num].songPosition - audioTime) < 5f * (1f/60f))
            {
                display.color = Color.green;
                display.text = displayPrefix + "Hit!";
                destroyNote = true;
            }

            if (destroyNote)
            {
                Note copy = noteObjs[num];
                noteObjs.Remove(copy);
                Destroy(copy);
                num--;
            }
        }
	}


}