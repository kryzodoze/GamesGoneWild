﻿using UnityEngine;
using System.Collections;

public class SaveTheFloorsContoller : MonoBehaviour {

	// Controller script in order to move from game to game and save any data necessary
	public GameObject globalController;
	bool gameOver;
	// Creating our gameobject for controlling the scene
	public GameObject [] pukePrefabArray;
	// Identifier for our head gameobject
	public GameObject body;
	public GameObject head;
	// Animation for head
	public Sprite bodyTwo;
	public Sprite bodyThree;
	public Sprite pukeHead;
	// Handle for the countdown text object
	public GameObject countdown;
	// True if the countdown is still going
	bool countdownPhase;
	// Timer for when to start the game and destroy countdown
	float countdownTimer;
	// Index for array of pukePrefab gameobject
	int pukePrefabIndex;
	// How many pukes you need to win
	int pukesToWin;
	// This will initiate gravity when needed
	bool StartPuking;
	// Storing y - location of current puke gameobject
	Vector3 currentPos;
	// Rotation speed each second
	float rotateSpeed;
	// Direction to rotate in
	string rotateDirection;
	// Timer for when to rotate
	float rotateTimer;
	// Whether we're actually rotating right now
	bool rotating;
	// Timer for how long to keep throw up face on person
	float throwupTimer;
	bool throwupAnimationOn;
	// Variables for fading out the instructions
	public GameObject instructionText;
	float fadeTimer;
	// Longer intervals for higher levels with puke
	float pukeIntervalTimer;
	bool pukeInterval = false;
	Color colorStart;
	Color colorEnd;
	float fadeValue;

	// Puke SFX
	public GameObject PukeSound;
	public GameObject BucketSound;
	
	public GameObject scoreText;
	public GameObject scoreTextFront;
	public GameObject scoreTextBack;

	// Use this for initialization
	void Start () 
	{
		// Finding our controller gameobject
		globalController = GameObject.Find("Global Controller");
		gameOver = false;

		// Finding our head gameobject
		head = GameObject.Find ("Head");
		head.renderer.enabled = false;

		// Set our current index of gameobject array
		pukePrefabIndex = 0;

		// Set the number of pukes needed to win minigame
		if( globalController )
		{
			globalController.GetComponent<GlobalController>().RenderPauseButton();
			pukesToWin = globalController.GetComponent<GlobalController>().pukeLevel;	// Starts at 4
		}
		else  // For testing purposes, when ran not from splash screen
			pukesToWin = 4;

		// Initializing identifier so no gravity is given to puke gameobject
		StartPuking = false;

		// Set the countdown timer to 2.7 seconds
		countdown.GetComponent<Animator>().speed = 1.4f;
		countdownTimer = 2.7f;
		countdownPhase = true;

		// Set up all the variables for rotation
		rotateSpeed = 24.0f;
		if( Random.value > .5 )
			rotateDirection = "Left";
		else
			rotateDirection = "Right";
		rotateTimer = 1.0f;
		rotating = false;

		// Whether the face is throwing up at the moment
		throwupAnimationOn = false;

		// Make all of the pukes invisible for now
		foreach( GameObject p in pukePrefabArray )
		{
			p.renderer.enabled = false;
		}
		
		// Fading instructions variables
		fadeTimer = 3.0f; // set duration time in seconds in the Inspector
		colorStart = instructionText.renderer.material.color;
		colorEnd = new Color( colorStart.r, colorStart.g, colorStart.b, 0.0f );
		fadeValue = 0.0f;
		
		scoreText.GetComponent<Animator>().enabled = false;
		scoreTextFront.renderer.enabled = false;
		scoreTextBack.renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( !gameOver )
		{
			// Condition whether to send a puke gameobject or not
			if (StartPuking && pukePrefabIndex < pukePrefabArray.Length) 
			{
				float pukeAngle;
				if( head.transform.rotation.eulerAngles.z > 180.0f )
					pukeAngle = -1.0f * (360.0f - head.transform.rotation.eulerAngles.z);
				else
					pukeAngle = head.transform.rotation.eulerAngles.z;
				
				// Giving the current element in pukePrefabArray gravity
				pukePrefabArray [pukePrefabIndex].renderer.enabled = true;
				pukePrefabArray [pukePrefabIndex].GetComponent<Puke_Behavior> ().Shoot( pukeAngle, pukesToWin);
				StartPuking = false;
				
				throwupAnimationOn = true;
				throwupTimer = 0.3f;
				head.renderer.enabled = true;
				PukeSound.GetComponent<AudioSource>().Play();
			}
			// Detecting current location of the puke gameobject
			currentPos = pukePrefabArray [pukePrefabIndex].transform.position;
			
			// If puke gameobject falls below this range, player loses game and exits game
			if ( currentPos.y < -9)
			{
				if( globalController )
					globalController.GetComponent<GlobalController>().LostMinigame();
			}
			
			// If we're still counting down
			if( countdownPhase )
			{
				if( countdownTimer <= 1.8f )	// A bit hacky, should have a boolean to control which state.
				{
					body.GetComponent<SpriteRenderer>().sprite = bodyTwo;
				}
				
				// If the timer is still going, decrement it
				if( countdownTimer > 0.0f )
				{
					countdownTimer -= Time.deltaTime;
				}
				else
				{
					// Start the pukes coming and nullify the countdown stuff
					body.GetComponent<SpriteRenderer>().sprite = bodyThree;
					StartPuking = true;
					countdownPhase = false;
					Destroy( countdown );
				}
			}
			else
			{
				rotating = true;
				
				if( fadeValue < 1.0f )
				{
					fadeTimer -= Time.deltaTime;
					fadeValue += Time.deltaTime;
					instructionText.renderer.material.color = Color.Lerp( colorStart, colorEnd, fadeValue/1.0f );
					
					if( fadeValue >= 1.0f )
					{
						Destroy( instructionText );
					}
				}
			}
			
			if( rotating )
			{
				rotateTimer -= Time.deltaTime;
				
				if( rotateTimer <= 0.0f )
				{
					if( head.transform.rotation.eulerAngles.z > 180.0f )
					{
						rotateDirection = "Right";
					}
					else
					{
						rotateDirection = "Left";
					}
					
					rotateTimer = (Random.value * .8f) + .8f;
				}
				
				if( rotateDirection == "Left" )
				{
					head.transform.Rotate ( 0, 0, -rotateSpeed * Time.deltaTime, Space.World);
				}
				else // if "Right"
				{
					head.transform.Rotate ( 0, 0, rotateSpeed * Time.deltaTime, Space.World);
				}
			}
			
			if( throwupAnimationOn )
			{
				throwupTimer -= Time.deltaTime;
				
				if( throwupTimer <= 0.0f )
				{
					throwupAnimationOn = false;
					body.GetComponent<SpriteRenderer>().sprite = bodyThree;
				}
			}
			// Create longer intervals when in higher levels
			if( pukeInterval )
			{
				pukeIntervalTimer -= Time.deltaTime;
				
				if( pukeIntervalTimer <= 0.0f )
				{
					// Give the next pukePrefab element some gravity
					StartPuking = true;

					pukeInterval = false;
				}
			}
		}	
	}

	// Destroy this instance puke gameobject.
	public void DestroyPuke()
	{

		// Destroy puke gameobject at curreent element in list
		BucketSound.GetComponent<AudioSource>().Play();
		DestroyObject (pukePrefabArray[pukePrefabIndex]);

		// Go the next element of pukePrefabArray
		pukePrefabIndex++;

		pukeInterval = true;
		if( globalController )
			//pukeIntervalTimer = .25f + (globalController.GetComponent<GlobalController>().pukeLevel * .1f);
			pukeIntervalTimer = .005f + (globalController.GetComponent<GlobalController>().pukeLevel * .1f);
		else
			pukeIntervalTimer = .26f;

		if( pukePrefabIndex == pukesToWin )
		{
			if( globalController )
			{
				gameOver = true;
				if( globalController.GetComponent<GlobalController>().pukeLevel <= 7 )
				{
					globalController.GetComponent<GlobalController>().pukeLevel++;
				}

				scoreTextFront.GetComponent<TextMesh>().text = "+ 100";
				scoreTextBack.GetComponent<TextMesh>().text = "+ 100";
				scoreTextFront.renderer.enabled = true;
				scoreTextBack.renderer.enabled = true;
				scoreText.GetComponent<Animator>().enabled = true;

				globalController.GetComponent<GlobalController>().BeatMinigame( 100 );
			}
		}
	}
}
