using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TouchManager_Gameplay : MonoBehaviour
{
	public GameObject hitOctagon;
    public Text inputText;
    public GameObject notePrefab;
	public GameObject modManager;
	private LaneScript[] laneScripts;
	private ModuleManager modScript;
    private AudioSource song;
    private float defaultNoteSpeed = 1.0f;
    private int songBPM = 80;
    private float crochet;
    private float beatsInAdvance = 2.0f;
    private float secondsInAdvance;
    private float songStartTime = 0.0f;
	private float measuredTime = 0.0f;
	private string testReader = "Readers\\real_reader.lua";
	private string testChart = string.Empty;
	private string chartsLocation = string.Empty;
	private UnityWebRequest webAudio;
	public bool debug;
	private bool canLoadSong = true;
	private float loadSongTime = float.MaxValue * 0.8f;

	void Start()
	{
		//Taking care of the framerate
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		//Managing the module + loading the chart
		modScript = modManager.GetComponent<ModuleManager> ();
		chartsLocation = Directory.GetCurrentDirectory () + "\\Charts\\";
		testChart = Directory.GetCurrentDirectory () + "\\Charts\\Stepmania Simfiles\\";
		//testChart += "DDR Supernova 2 (AC)\\Bloody Tears (IIDX EDITION)\\Bloody Tears (IIDX EDITION).sm";
		testChart += "O2Jam MIX\\Cross Time\\Cross Time.sm";

		//Taking care of the song properties & variables
        crochet = 60f / songBPM;
        secondsInAdvance = (beatsInAdvance * crochet) / defaultNoteSpeed;

        song = this.GetComponent<AudioSource>();
        song.pitch = 1f;
        laneScripts = hitOctagon.GetComponentsInChildren<LaneScript> ();
	}

    //FixedUpdate()
	void FixedUpdate()
	{
		//
		if (!modScript.ModuleLoaded)
		{
			modScript.LoadModule (testReader);
			chartsLocation += modScript.ModuleFolder + "\\";
		}

        //Starting the song
		if (canLoadSong && Input.GetKeyDown(KeyCode.P))
        {
			canLoadSong = false;
			if (modScript.ReadChartFile(testChart))
			{
				string[] difficulties = modScript.GetChartDifficulties ();
				if(modScript.ReadChartData(difficulties[1]))
				{
					//Getting the audio file
					string audioLoc = modScript.GetAudioFile();
					string toReplace = testChart.Substring (testChart.LastIndexOf("\\") + 1);
					audioLoc = testChart.Replace (toReplace, audioLoc);

					string audioExtension = audioLoc.Substring (audioLoc.LastIndexOf(".") + 1);
					AudioType ext;

					switch (audioExtension)
					{
					case "mp3":
						ext = AudioType.MPEG;
						break;
					case "mp2":
						ext = AudioType.MPEG;
						break;
					case "ogg":
						ext = AudioType.OGGVORBIS;
						break;
					case "wav":
						ext = AudioType.WAV;
						break;
					default:
						throw new Exception ("AUDIO TYPE NOT SUPPORTED");
					}

					try
					{
						webAudio = UnityWebRequestMultimedia.GetAudioClip("file:///" + audioLoc, ext);
						webAudio.SendWebRequest();
					}
					catch (Exception e)
					{
						throw e;
					}

					List<float>[] chartNotes = modScript.CalculateNotes ();
					for (int num = 0; num < chartNotes.Length; num++)
					{
						laneScripts [num].SetNotes (chartNotes [num]);
					}

					loadSongTime = Time.realtimeSinceStartup;
				}
			}
        }

		//
		if (Time.realtimeSinceStartup > (4f + loadSongTime) && songStartTime == 0.0f)
		{
			//Changing the song audio
			song.clip = DownloadHandlerAudioClip.GetContent(webAudio);

			//Playing the song
			song.Play();

			//
			songStartTime = (float) AudioSettings.dspTime;
		}

        //Updating the measured song time
        if (songStartTime > 0.0)
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
            Note_Move(lane, lane.NoteObjects[num]);
			float msDiff = lane.NoteObjects [num].songPosition - audioTime;

            if (lane.NoteObjects[num].lerpFactor >= 1.15f)
            {
                lane.ChangeHitText(false, 0f);
                destroyNote = true;
            }

			if (lane.holdCounter % 5 > 0   &&   Mathf.Abs(msDiff) < 5f * (1f/60f))
            {
				lane.ChangeHitText(true, msDiff * 1000f);
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
		for (int num = 0; num < 75; num++)
		{
			rightNotes.Add (crochet * (5f + num));
		}

        List<float> leftNotes = new List<float>();
		for (int num = 0; num < 75; num++)
		{
			leftNotes.Add (crochet * (5.5f + num));
		}

        laneScripts[0].SetNotes(rightNotes);
        laneScripts[4].SetNotes(leftNotes);
    }


}