using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour, IPointerDownHandler
{
	private List<GameObject> touchPoints = new List<GameObject>();
	float measuredTime = 0.0f;
	private bool lifeSucks = false;

	void Start()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}


	public void OnPointerDown(PointerEventData eventData)
	{
		/*
		float now = Time.realtimeSinceStartup;
		float difference = (now - measuredTime) * 1000.0f;
		print("Time (RT, measured, difference): (" + now + ", " + measuredTime + ", " + difference + ")");
		*/

	}

	void FixedUpdate()
	{
		measuredTime = Time.realtimeSinceStartup;

		/*
		if (!lifeSucks && Input.touches.Length > 0)
		{
			print ("life sucks (fixed)");
			lifeSucks = true;
		}
		*/

		foreach(GameObject point in touchPoints)
		{
			Destroy (point);
		}

		touchPoints.Clear();

		foreach (Touch tch in Input.touches)
		{
			Vector3 screenPos = new Vector3(tch.position.x, tch.position.y, 0.0f);
			Vector3 worldPos = Camera.main.ScreenToWorldPoint (screenPos);

			GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			//print ("Touch Pos: (" + worldPos.x + ", " + worldPos.y + ")");
			sphere.transform.position = new Vector3(worldPos.x, worldPos.y, -2.0f);

			touchPoints.Add (sphere);

		}
	}

}
