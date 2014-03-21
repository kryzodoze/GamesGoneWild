﻿using UnityEngine;
using System.Collections;

public class SaveTheFloorsContoller : MonoBehaviour {

	// Controller script in order to move from game to game and save any data necessary
	public GameObject globalController;

	// Creating our gameobject for controlling the scene
	public GameObject [] pukePrefabArray;

	// Identifier for our head gameobject
	public GameObject head;

	// Handle for the countdown text object
	public GameObject countdown;

	// Timer for when to start the game and destroy countdown
	float countdownTimer;

	// True if the countdown is still going
	bool countdownPhase;

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

	// Use this for initialization
	void Start () {

		// Finding our controller gameobject
		globalController = GameObject.Find("Global Controller");

		// Finding our head gameobject
		head = GameObject.Find ("Head");

		// Set our current index of gameobject array
		pukePrefabIndex = 0;

		// Set the number of pukes needed to win minigame
		if( globalController )
			pukesToWin = globalController.GetComponent<GlobalController>().pukeLevel;
		else  // For testing purposes, when ran not from splash screen
			pukesToWin = 5;

		// Initializing identifier so no gravity is given to puke gameobject
		StartPuking = false;

		// Set the countdown timer to 3.5 seconds
		countdownTimer = 3.5f;
		countdownPhase = true;

		// Set up all the variables for rotation
		rotateSpeed = 20.0f;
		rotateDirection = "Right";
		rotateTimer = 2.0f;
		rotating = false;
	}
	
	// Update is called once per frame
	void Update () {

		// Condition whether to send a puke gameobject or not
		if (StartPuking == true && pukePrefabIndex < 14) {

			// Debugging purposes
			//Debug.Log ("StartPuking is " + StartPuking + " before switch case");
			Debug.Log ("pukePrefabIndex is " + pukePrefabIndex);

			// 
			float pukeAngle;
			if( head.transform.rotation.eulerAngles.z > 180.0f )
				pukeAngle = -1.0f * (360.0f - head.transform.rotation.eulerAngles.z);
			else
				pukeAngle = head.transform.rotation.eulerAngles.z;

			// Giving the current element in pukePrefabArray gravity
			pukePrefabArray [pukePrefabIndex].GetComponent<Puke_Behavior> ().Shoot( pukeAngle );
			StartPuking = false;
		}

		// To Do: when pukePrefabIndex == 14, take minigame out of rotation

		// Detecting current location of the puke gameobject
		currentPos = pukePrefabArray [pukePrefabIndex].transform.position;

		// If puke gameobject falls below this range, player loses game and exits game
		if ( currentPos.y < -9)
		{
			if( globalController )
				globalController.GetComponent<GlobalController>().LostMinigame();
			else
				Debug.Log( "Lost game, but no global controller!" );
		}

		// If we're still counting down
		if( countdownPhase )
		{
			// If the timer is still going, decrement it
			if( countdownTimer > 0.0f )
			{
				countdownTimer -= Time.deltaTime;
			}
			else
			{
				// Start the pukes coming and nullify the countdown stuff
				StartPuking = true;
				countdownPhase = false;
				Destroy( countdown );
			}
		}
		else
		{
			rotating = true;
		}

		if( rotating )
		{
			rotateTimer -= Time.deltaTime;

			if( rotateTimer <= 0.0f )
			{
				Debug.Log( "Angle: " + head.transform.rotation.eulerAngles.z );
				if( head.transform.rotation.eulerAngles.z > 180.0f )
				{
					rotateDirection = "Right";
				}
				else
				{
					rotateDirection = "Left";
				}

				rotateTimer = Random.value * 4.0f;
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
	}

	// Destroy this instance puke gameobject.
	public void DestroyPuke(){

		// Debugging purposes
		Debug.Log ("In DestroyPuke() from controller");

		// Destroy puke gameobject at curreent element in list
		DestroyObject (pukePrefabArray[pukePrefabIndex]);

		// Go the next element of pukePrefabArray
		pukePrefabIndex++;

		// Give the next pukePrefab element some gravity
		StartPuking = true;

		//
		if( pukePrefabIndex == pukesToWin )
		{
			if( globalController )
			{
				globalController.GetComponent<GlobalController>().pukeLevel++;
				globalController.GetComponent<GlobalController>().BeatMinigame();
			}
			else
			{
				Debug.Log( "Won game, but no global controller!" );
			}
		}
	}
}