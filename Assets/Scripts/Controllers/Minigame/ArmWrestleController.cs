﻿using UnityEngine;
using System.Collections;

public class ArmWrestleController : MonoBehaviour 
{
	public GameObject globalController;

	bool gameOver;
	bool startedGame;
	float startTimer;	

	public GameObject arms;
	float initialArmX;
	public float loseX;
	public float winX;
	float winGuage;

	public GameObject countdownTimer;
	Vector3 tempPos;

	float playerStrength;
	float enemyStrength;
	int enemyMultiplier;
	float strengthRotation;
	
	// Variables for fading out the instructions
	public GameObject instructionText;
	float fadeTimer;
	Color colorStart;
	Color colorEnd;
	float fadeValue;
	
	public GameObject scoreText;
	public GameObject scoreTextFront;
	public GameObject scoreTextBack;
	
	public GameObject timerFront;
	public GameObject timerBack;
	bool timerStarted;
	Vector3 timerSpeed;

	// Use this for initialization
	void Start () 
	{
		globalController = GameObject.Find( "Global Controller" );

		gameOver = false;
		startedGame = false;
		countdownTimer.GetComponent<Animator>().speed = 1.4f;
		startTimer = 2.7f;

		initialArmX = arms.transform.position.x;
		winGuage = initialArmX;

		/*foreach (AnimationState state in countdownTimer.GetComponent<Animator>().animation) 
		{
			state.speed = 0.2F;
		}*/
		//countdownTimer.animation.Stop();
		
		playerStrength = .6f;
		enemyStrength = .015f;
		if( globalController )
		{
			globalController.GetComponent<GlobalController>().RenderPauseButton();
			enemyMultiplier = globalController.GetComponent<GlobalController>().armEnemyLevel;
		}
		else
		{
			enemyMultiplier = 1;
		}

		// Fading instructions variables
		fadeTimer = 3.0f; // set duration time in seconds in the Inspector
		colorStart = instructionText.renderer.material.color;
		colorEnd = new Color( colorStart.r, colorStart.g, colorStart.b, 0.0f );
		fadeValue = 0.0f;
		
		scoreText.GetComponent<Animator>().enabled = false;
		scoreTextFront.renderer.enabled = false;
		scoreTextBack.renderer.enabled = false;
		
		if( globalController )
		{
			timerFront = globalController.GetComponent<GlobalController>().timerFront;
			timerBack = globalController.GetComponent<GlobalController>().timerBack;
			
			timerFront.renderer.enabled = true;
			timerBack.renderer.enabled = true;
		}
		timerStarted = false;
		timerSpeed = new Vector3( -.05f, 0.0f, 0.0f );
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( !gameOver )
		{
			if( startedGame )
			{
				if( Input.GetMouseButtonDown( 0 ) )
				{
					winGuage -= playerStrength;
					strengthRotation = 15.0f * 60.0f * Time.deltaTime;
				}
				else
				{
					strengthRotation = (-.6f + (-.15f * enemyMultiplier)) * 60.0f * Time.deltaTime;
				}
				
				winGuage += (enemyStrength * enemyMultiplier);
				
				// Not needed at the moment.
				//tempPos = arms.transform.position;
				//tempPos.x = winGuage;
				//arms.transform.position = tempPos;
				
				arms.transform.Rotate( new Vector3( 0.0f, strengthRotation, 0.0f ) );
				
				
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
				
				if( timerStarted )
				{
					timerFront.transform.Translate( timerSpeed * Time.deltaTime * 60.0f);
					
					if( timerFront.transform.position.x < -20.0f )
					{
						globalController.GetComponent<GlobalController>().LostMinigame();
					}
				}
			}
			else
			{
				startTimer -= Time.deltaTime;
				
				if( startTimer <= 0.0f )
				{
					startedGame = true;
					timerStarted = true;
					Destroy( countdownTimer );
				}
			}
			
			// Check if win or lose.
			if( arms.transform.rotation.eulerAngles.y > 80.0f && arms.transform.rotation.eulerAngles.y < 180.0f )
			{
				gameOver = true;
				globalController.GetComponent<GlobalController>().armEnemyLevel++;

				scoreTextFront.GetComponent<TextMesh>().text = "+ 100";
				scoreTextBack.GetComponent<TextMesh>().text = "+ 100";
				scoreTextFront.renderer.enabled = true;
				scoreTextBack.renderer.enabled = true;
				scoreText.GetComponent<Animator>().enabled = true;

				globalController.GetComponent<GlobalController>().BeatMinigame( 100 );
			}
			if( arms.transform.rotation.eulerAngles.y < 280.0f && arms.transform.rotation.eulerAngles.y > 180.0f )
			{
				globalController.GetComponent<GlobalController>().LostMinigame();
			}
		}
	}
}
