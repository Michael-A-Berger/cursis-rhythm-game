using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour
{
	//Attributes
    private float lerpFct = 0f;
    private float songPos;

	//Attribute Accessors
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
}
