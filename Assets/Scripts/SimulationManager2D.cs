using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager2D : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		Debug.Log("Setting up simulation...");
		frametime_ = 1f / (float)maxFps;
		scuffed_spinlocky_thing_ = 0;
		sim_ = new Simulation2D(defaultState);
		texture_ = sim_.Render(resolution.x, resolution.y);
		gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", texture_);
		Debug.Log("Setup complete.");
	}

	// Update is called once per frame
	void Update()
	{
		// Using FixedUpdate cripples framerates and doesn't render most simulation updates
		if (scuffed_spinlocky_thing_ + Time.deltaTime < frametime_)
		{
			scuffed_spinlocky_thing_ += Time.deltaTime;
		}
		else
		{
			scuffed_spinlocky_thing_ = 0;
			sim_.NextFrame(frametime_);
			texture_ = sim_.Render(resolution.x, resolution.y);
			gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", texture_);
		}
	}

	public void LoadState(string state)
	{
		sim_.Import(state);
	}

	Simulation2D sim_;
	Texture2D texture_;
	float frametime_;
	float scuffed_spinlocky_thing_;
	public Vector2Int resolution;
	public string defaultState;
	public int maxFps;
}
