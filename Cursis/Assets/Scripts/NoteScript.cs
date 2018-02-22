using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour
{
    private float lerpFct = 0f;
    private float songPos;
    public float songPosition
    {
        private set
        { songPos = value; }
        get
        { return songPos; }
    }
    public float lerpFactor
    {
        set
        { lerpFct = value; }
        get
        { return lerpFct; }
    }

    //Start()
    void Start()
    {
        //
    }

    //SetProperties()
    public void SetProperties(float pos)
    {
        songPosition = pos;
    }

    /*
    //Move()
    public void Move(float audioTime)
    {
        Vector3 newPos;
        lerpFct = (secondsInAdvance - (songPosition - audioTime)) / secondsInAdvance;
        if (lerpFactor <= 1f)
            newPos = Vector3.Lerp(startPos, lanePos, lerpFct);
        else
            newPos = Vector3.Lerp(lanePos, endPos, (lerpFct - 1f) / 0.15f);
        this.transform.position = newPos;
    }
    */
}
