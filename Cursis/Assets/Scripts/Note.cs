using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 lanePos;
    private Vector3 endPos;
    private float beatsInAdvance;
    private float lerpFct = 0f;
    private float songPos;
    private float speed;
    public float songPosition
    {
        private set
        { songPos = value; }
        get
        { return songPos; }
    }
    public float noteSpeed
    {
        private set
        { speed = value; }
        get
        { return speed; }
    }
    public float lerpFactor
    {
        private set
        { lerpFct = value; }
        get
        { return lerpFct; }
    }

    //Start()
    void Start()
    {
        startPos = this.transform.position;
    }

    //SetProperties()
    public void SetProperties(float pos, float spd, float beatsAdv, Vector3 start, Vector3 lane, Vector3 end)
    {
        songPosition = pos;
        noteSpeed = spd;
        beatsInAdvance = beatsAdv;
        startPos = start;
        lanePos = lane;
        endPos = end;
    }

    public extern void Move(ref float lerpFactor, float audioTime);

    //Move()
    public void Move(float audioTime)
    {
        Vector3 newPos;
        lerpFct = (beatsInAdvance - (songPosition - audioTime)) / beatsInAdvance;
        if (lerpFactor <= 1f)
            newPos = Vector3.Lerp(startPos, lanePos, lerpFct);
        else
            newPos = Vector3.Lerp(lanePos, endPos, (lerpFct - 1f) / 0.15f);
        this.transform.position = newPos;
    }

    //OnDestroy()
    void OnDestroy()
    {
        Destroy(this.gameObject);
    }
}
