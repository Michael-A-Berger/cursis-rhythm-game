using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchManager_Gameplay : MonoBehaviour
{
	public GameObject hitOctagon;
    public Text inputText;
	private LaneScript[] laneScripts;
    private AudioSource song;
    private float defaultNoteSpeed = 1.0f;
    private int songBPM = 75;
    private float crochet;
    private float beatsInAdvance = 2.0f;
    private float songStartTime = 0.0f;
	private float measuredTime = 0.0f;
	public bool debug;

	void Start()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

        crochet = 60f / songBPM;

        song = this.GetComponent<AudioSource>();
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
            Debug.Log("measuredTime: " + measuredTime);
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
			laneScripts[num].UpdateInput(inputs[num], audioTime);
		}
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