using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager_Gameplay : MonoBehaviour
{
	public GameObject hitOctagon;
	private LineRenderer[] hitLines;
	private List<Touch> touchPoints = new List<Touch>();
	private float measuredTime = 0.0f;
	private Color hitColor = Color.red;

	void Start()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		hitLines = hitOctagon.GetComponentsInChildren<LineRenderer> ();
	}

	void FixedUpdate()
	{
		measuredTime = Time.realtimeSinceStartup;
		
		foreach (LineRenderer line in hitLines)
		{
			line.material.color = Color.white;
		}

		Vector3 screenPos = new Vector3 (0, 0, 0);
		Vector3 worldPos = new Vector3 (0, 0, 0);
		float rads = 0.0f;
		float degrees = 0.0f;
		foreach (Touch tch in Input.touches)
		{
			screenPos.x = tch.position.x;
			screenPos.y = tch.position.y;
			worldPos = Camera.main.ScreenToWorldPoint (screenPos);

			rads = Mathf.Atan2 (worldPos.y, worldPos.x);
			int sector = Mathf.RoundToInt(rads * (4 / Mathf.PI));
			Debug.Log ("Sector: " + sector);

			switch (sector)
			{
				case 0:
				hitLines [2].material.color = hitColor;
					break;
				case 1:
				hitLines [1].material.color = hitColor;
					break;
				case 2:
				hitLines [0].material.color = hitColor;
					break;
				case 3:
				hitLines [7].material.color = hitColor;
					break;
				case 4:
				case -4:
					hitLines [6].material.color = hitColor;
					break;
				case -3:
					hitLines [5].material.color = hitColor;
					break;
				case -2:
					hitLines [4].material.color = hitColor;
					break;
				case -1:
					hitLines [3].material.color = hitColor;
					break;
				default:
					break;
			}

			/*
			degrees = rads * (180 / Mathf.PI);
			Debug.Log ("Degrees: " + Mathf.Round(degrees));
			*/
		}
	}

}
