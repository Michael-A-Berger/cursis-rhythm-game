using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchManager_Gameplay : MonoBehaviour
{
	public GameObject hitOctagon;
    public Text inputText;
    public GameObject notePrefab;
	private LaneScript[] laneScripts;
    private AudioSource song;
    private float defaultNoteSpeed = 1.0f;
    private int songBPM = 75;
    private float crochet;
    private float beatsInAdvance = 2.0f;
    private float secondsInAdvance;
    private float songStartTime = 0.0f;
	private float measuredTime = 0.0f;
	public bool debug;

	void Start()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

        crochet = 60f / songBPM;
        secondsInAdvance = (beatsInAdvance * crochet) / defaultNoteSpeed;

        song = this.GetComponent<AudioSource>();
        song.pitch = 1f;
        laneScripts = hitOctagon.GetComponentsInChildren<LaneScript> ();

        createSongChart();
	}

    //FixedUpdate()
	void FixedUpdate()
	{
        //Starting the song
        if (songStartTime == 0.0 && Time.realtimeSinceStartup > 3)
        {
            song.Play();
            songStartTime = (float) AudioSettings.dspTime;
        }

        //Updating the measured song time
        if (songStartTime != 0.0)
        {
            measuredTime = (float) (AudioSettings.dspTime - songStartTime) * song.pitch;
            //Debug.Log("measuredTime: " + measuredTime);
        }

        //Defining the input list
		List<bool> inputs = new List<bool>();
		foreach (LaneScript lane in laneScripts)
			inputs.Add(false);


        if (Input.GetKeyDown(KeyCode.S))
        {
            debug = !debug;

            if (debug)
                inputText.text = "Input: Keyboard\n('S' to change)";
            else
                inputText.text = "Input: Touch\n('S' to change)";
                
        }

		if (debug)
		{
            if (Input.GetKey(KeyCode.D))
				inputs[0] = true;
            if (Input.GetKey(KeyCode.E))
				inputs[1] = true;
            if (Input.GetKey(KeyCode.W))
				inputs[2] = true;
            if (Input.GetKey(KeyCode.Q))
				inputs[3] = true;
            if (Input.GetKey(KeyCode.A))
				inputs[4] = true;
            if (Input.GetKey(KeyCode.Z))
				inputs[5] = true;
            if (Input.GetKey(KeyCode.X))
				inputs[6] = true;
            if (Input.GetKey(KeyCode.C))
				inputs[7] = true;
		}
		else
		{
			Vector3 screenPos = new Vector3(0, 0, 0);
			Vector3 worldPos = new Vector3(0, 0, 0);
			float rads = 0.0f;
			foreach (Touch tch in Input.touches)
			{
				screenPos.x = tch.position.x;
				screenPos.y = tch.position.y;
				worldPos = Camera.main.ScreenToWorldPoint(screenPos);

                rads = Mathf.Atan2(worldPos.y, worldPos.x) + (Mathf.PI / 8);        //Add π/8 to offset the angle for flooring
				rads = (rads + 2 * Mathf.PI) % (2 * Mathf.PI);
                int sector = Mathf.FloorToInt(rads * (4 / Mathf.PI));
                //Debug.Log("sector: " + sector);
                inputs[sector] = true;
			}
		}

        UpdateInput(inputs.ToArray(), measuredTime);
	}

    private void UpdateInput(bool[] inputs, float audioTime)
	{
		for (int num = 0; num < laneScripts.Length; num++)
		{
			//laneScripts[num].UpdateInput(inputs[num], audioTime);
            Lane_UpdateInput(laneScripts[num], inputs[num], audioTime);
		}
	}

    private void Lane_UpdateInput(LaneScript lane, bool input, float audioTime)
    {
        //Handling the actual input
        if (input)
        {
            if (lane.holdCounter < 5)
            {
                lane.ChangeHoldColor(true);
                lane.IncrementHoldCounter();
            }
            else
            {
                lane.ChangeHoldColor(false);
            }
        }
        else
        {
            lane.ChangeHoldColor(false);
            lane.ResetHoldCounter();
        }

        //Spawning New Notes
        for (int num = 0; num < lane.noteCount; num++)
        {
            float notePos = lane.NoteTimes[num];

            if (audioTime + beatsInAdvance > notePos)
            {
                NoteScript newNote = Instantiate(notePrefab, lane.startPos, Quaternion.identity).GetComponent<NoteScript>();
                newNote.SetProperties(notePos);
                lane.NoteObjects.Add(newNote);
                lane.NoteTimes.Remove(notePos);
                num--;
            }
        }

        //Moving/Deleting the Note Objects
        for (int num = 0; num < lane.NoteObjects.Count; num++)
        {
            bool destroyNote = false;
            //lane.NoteObjects[num].Move(audioTime);
            Note_Move(lane, lane.NoteObjects[num]);

            if (lane.NoteObjects[num].lerpFactor >= 1.15f)
            {
                lane.ChangeHitText(false);
                destroyNote = true;
            }

            if (lane.holdCounter % 5 > 0   &&   Mathf.Abs(lane.NoteObjects[num].songPosition - audioTime) < 5f * (1f/60f))
            {
                lane.ChangeHitText(true);
                destroyNote = true;
            }

            if (destroyNote)
            {
                NoteScript copy = lane.NoteObjects[num];
                lane.NoteObjects.Remove(copy);
                Destroy(copy.gameObject);
                num--;
            }
        }
    }

    //Note_Move()
    private void Note_Move(LaneScript lane, NoteScript note)
    {
        Vector3 newPos;

        float road1Length = Mathf.Abs(Vector3.Magnitude(lane.startPos - lane.lanePos));
        float road2Length = Mathf.Abs(Vector3.Magnitude(lane.lanePos - lane.endPos));
        float road2Percentage = road2Length / (road1Length + road2Length);
        note.lerpFactor = (secondsInAdvance - (note.songPosition - measuredTime)) / secondsInAdvance;
        if (note.lerpFactor <= 1f)
            newPos = Vector3.Lerp(lane.startPos, lane.lanePos, note.lerpFactor);
        else
            newPos = Vector3.Lerp(lane.lanePos, lane.endPos, (note.lerpFactor - 1f) / road2Percentage);
        
        note.transform.position = newPos;
    }

    private void createSongChart()
    {
        List<float> rightNotes = new List<float>();
        rightNotes.Add(crochet * 5f);
        rightNotes.Add(crochet * 6f);
        rightNotes.Add(crochet * 7f);
        rightNotes.Add(crochet * 8f);
        rightNotes.Add(crochet * 9f);
        rightNotes.Add(crochet * 10f);
        rightNotes.Add(crochet * 11f);
        rightNotes.Add(crochet * 12f);
        rightNotes.Add(crochet * 13f);
        rightNotes.Add(crochet * 14f);
        rightNotes.Add(crochet * 15f);
        rightNotes.Add(crochet * 16f);
        rightNotes.Add(crochet * 17f);
        rightNotes.Add(crochet * 18f);
        rightNotes.Add(crochet * 19f);
        rightNotes.Add(crochet * 20f);
        rightNotes.Add(crochet * 21f);
        rightNotes.Add(crochet * 22f);
        rightNotes.Add(crochet * 23f);
        rightNotes.Add(crochet * 24f);
        rightNotes.Add(crochet * 25f);

        List<float> leftNotes = new List<float>();
        leftNotes.Add(crochet * 5.5f);
        leftNotes.Add(crochet * 6.5f);
        leftNotes.Add(crochet * 7.5f);
        leftNotes.Add(crochet * 8.5f);
        leftNotes.Add(crochet * 9.5f);
        leftNotes.Add(crochet * 10.5f);
        leftNotes.Add(crochet * 11.5f);
        leftNotes.Add(crochet * 12.5f);
        leftNotes.Add(crochet * 13.5f);
        leftNotes.Add(crochet * 14.5f);
        leftNotes.Add(crochet * 15.5f);
        leftNotes.Add(crochet * 16.5f);
        leftNotes.Add(crochet * 17.5f);
        leftNotes.Add(crochet * 18.5f);
        leftNotes.Add(crochet * 19.5f);
        leftNotes.Add(crochet * 20.5f);
        leftNotes.Add(crochet * 21.5f);
        leftNotes.Add(crochet * 22.5f);
        leftNotes.Add(crochet * 23.5f);
        leftNotes.Add(crochet * 24.5f);
        leftNotes.Add(crochet * 25.5f);

        laneScripts[0].SetNotes(rightNotes);
        laneScripts[4].SetNotes(leftNotes);
    }


}