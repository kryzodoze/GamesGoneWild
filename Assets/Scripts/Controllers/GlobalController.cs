﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalController : MonoBehaviour 
{
	string gameMode;
	List<string> allMinigames;
	List<string> currentMinigames;
	public string previousMode;

	// Facebook User Information -- name, picture, etc.
	public GameObject menuController;
	public string FBUsername;
	public Texture profilePic;

	// All of the music to play
	public GameObject menuMusic;
	public GameObject playGameMusic;

	// Variables kept for overall progress between mini-games
	public int oldPartyPoints;
	public int partyPoints;
	public int beersDrank;
	public int beerLives;
	public int turnUpLevel;
	public int musicSpeed;
	bool musicPlaying;

	// Variables kept for progress in mini-games
	public bool[] CupsPlaced;	// For beer pong.
	public int beerPongLevel;
	public int beerPongStreak;
	public int armEnemyLevel;	// For arm wrestle.
	public int dartLevel;
	public int pukeLevel;
	public int flippyCupLevel;

	// If in selection mode, this is filled with the current game being played
	public string currentSelectionLevel;
	// Which minigame are we playing? In numerics
	public int currentLevel;

	// Variables to handle pause menu
	public GameObject pauseButton;
	public GameObject pauseMenu;
	public GameObject resumeButton;
	public GameObject soundToggleButton;
	public GameObject quitButton;
	public bool isPaused;
	public RaycastHit hit;	// To track press of the pause button
	public Ray ray;

	// Game timer stuff
	public GameObject timerFront;
	public GameObject timerBack;
	Vector3 defaultTimerFrontPos;
	Vector3 defaultTimerBackPos;

	// Use this for initialization
	void Start () 
	{
		AdColony.Configure(
			"1.0",               // Arbitrary app version
			"app96b45d51f0844b008a",  // ADC App ID from adcolony.com
			"vzf5c3966785eb4f32a2",    // A zone ID from adcolony.com
			"vz7a22d0afa02045aa80"     // Any number of additional Zone IDS
			);  

		allMinigames = new List<string>();
		allMinigames.Add("BeerPong");
		allMinigames.Add("FlippyCup");
		allMinigames.Add("Darts");
		allMinigames.Add("Save_The_Floor");
		allMinigames.Add("fall");
		allMinigames.Add("ArmWrestle");

		// Essential for making this "global" and persistent.
		Object.DontDestroyOnLoad( this );

		// No mode to start
		pauseMenu.renderer.enabled = false;
		pauseMenu.collider.enabled = false;
		resumeButton.collider.enabled = false;
		soundToggleButton.collider.enabled = false;
		quitButton.collider.enabled = false;

		musicPlaying = true;

		previousMode = "";

		isPaused = false;
		hit = new RaycastHit();

		defaultTimerFrontPos = new Vector3( -2.0f, 
		                                      timerFront.transform.position.y,
		                                   timerFront.transform.position.z );
		defaultTimerBackPos = new Vector3( .4f, 
		                                   timerBack.transform.position.y,
		                                  timerBack.transform.position.z );
		
		// Set all the children of the global controller to invisible for now.
		Component[] children = GetComponentsInChildren(typeof(Renderer));
		foreach( Component c in children )
		{
			Renderer r = (Renderer)c;
			r.enabled = false;
		}

		// Technically setting for the first time, but hey, modularization..
		ResetVariables();

		StartMenuMusic();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( Input.GetMouseButtonDown( 0 ) )
		{
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if( !isPaused )	// If game is not paused
			{
				// Check if player click is on the pause button
				if( Physics.Raycast(ray,out hit) && hit.collider.name == "PauseButton" )
				{
					Pause();
				}
			}
			else
			{
				if( Physics.Raycast(ray,out hit) )
				{
					switch( hit.collider.name )
					{
					case "ResumeButton":
						UnPause();
						break;
					case "QuitButton":
						UnPause();
						ResetVariables();
						if( gameMode == "Selection" )
							Application.LoadLevel( "SelectionScene" );
						else
							Application.LoadLevel( "MenuScene" );
						break;
					case "SoundToggleButton":
						MusicToggle();
						break;
					}
				}
			}
		}
	}

	public void StartMode( string mode, string game )
	{
		gameMode = mode;
		currentSelectionLevel = game;

		// If playing in practice mode, and not playing flippy cup, then don't set a
		// previous mode. This is mostly used for displaying score right now.
		if( gameMode == "Selection" )
		{
			if( game == "FlippyCup" )
				previousMode = game;
			else
				previousMode = "";
		}
		
		NextMinigame();
	}

	public void NextMinigame()
	{
		timerFront.renderer.enabled = false;
		timerBack.renderer.enabled = false;
		timerFront.transform.position = defaultTimerFrontPos;
		timerBack.transform.position = defaultTimerBackPos;

		if( beersDrank < beerLives )	// If we haven't lost yet
		{
			if( gameMode == "Normal Mode" )
			{
				if( previousMode=="BeerPong" && beerPongStreak>=3 )
				{
					Application.LoadLevel( previousMode );
				}
				else if( currentMinigames.Count > 0 )
				{
					int random = Random.Range( 0, currentMinigames.Count);
					previousMode = currentMinigames[random];
					currentMinigames.Remove( previousMode );
					currentLevel = random;

					if( previousMode == "FlippyCup" )
					{
						Vector3 tempVect = new Vector3(4.3f, 5.3f, -2.0f);
						pauseButton.transform.position = tempVect;
						tempVect = pauseButton.transform.localScale;
						tempVect *= .9f;
						pauseButton.transform.localScale = tempVect;
					}

					Application.LoadLevel( previousMode );
				}
				else // it's time to turn up
				{
					turnUpLevel++;

					for( int i=0; i<(2+turnUpLevel); i++ )
					{
						currentMinigames.Add( allMinigames[i] );
					}

					Application.LoadLevel( "TurnUpScene" );
				}
			}
			else
			{
				Application.LoadLevel( currentSelectionLevel );
			}
		}
		else 	// If we've lost..
		{
			if( gameMode == "Normal Mode" )
			{
				Application.LoadLevel( "HighScore" );
			}
			else
			{
				LostGame();
			}
		}
	}

	public void RenderPauseButton( )
	{
		pauseButton.renderer.enabled = true;
		pauseButton.collider.enabled = true;
	}
	
	// Call this if the player won a minigame and make sure to increment
	// any global variables located in this associated with that minigame.
	public void BeatMinigame( int score )	
	{
		oldPartyPoints = partyPoints;
		partyPoints += score;
		pauseButton.renderer.enabled = false;
		pauseButton.collider.enabled = false;

		// Change location of pause screen stuff for flippy cup
		Vector3 tempPos = new Vector3(7.7f, 3.9f, -5.0f);
		pauseButton.transform.position = tempPos;

		Application.LoadLevelAdditive( "MinigameWin");
	}

	// Call this if the player lost the minigame
	public void LostMinigame()
	{
		beersDrank++;
		
		pauseButton.renderer.enabled = false;
		pauseButton.collider.enabled = false;
		timerFront.renderer.enabled = false;
		timerBack.renderer.enabled = false;
		
		// Change location of pause screen stuff for flippy cup
		Vector3 tempPos = new Vector3(7.7f, 3.9f, -5.0f);
		pauseButton.transform.position = tempPos;

		Application.LoadLevel( "MinigameFail");
	}

	// When you drink all of your beers
	public void LostGame()
	{
		// Reset all variables.
		ResetVariables();
		Application.LoadLevel( "MenuScene" );
	}

	// This is called when the game is started
	public void StartModeMusic()
	{
		if( menuMusic.GetComponent<AudioSource>().isPlaying )
		{
			menuMusic.GetComponent<AudioSource>().Stop();
		}
		if( playGameMusic.GetComponent<AudioSource>().isPlaying )
		{
			playGameMusic.GetComponent<AudioSource>().Stop();
		}
		// Disabled till we get real music.
		if( musicPlaying )
			playGameMusic.GetComponent<AudioSource>().Play();
	}

	// This is called when the menu is started
	void StartMenuMusic()
	{
		if( menuMusic.GetComponent<AudioSource>().isPlaying )
		{
			menuMusic.GetComponent<AudioSource>().Stop();
		}
		if( playGameMusic.GetComponent<AudioSource>().isPlaying )
		{
			playGameMusic.GetComponent<AudioSource>().Stop();
		}
		// Disabled till we get real music.
		if( musicPlaying )
			menuMusic.GetComponent<AudioSource>().Play();
	}

	void MusicToggle()
	{
		if( musicPlaying )
		{
			playGameMusic.audio.Pause();
			musicPlaying = false;
		}
		else
		{
			playGameMusic.audio.Play();
			musicPlaying = true;
		}
	}

	public void SpeedUpMusic()
	{
		if( musicSpeed < 5 )
		{
			musicSpeed++;
			playGameMusic.GetComponent<AudioSource>().pitch += .05f;
		}
	}

	public void SetUserName(string name)
	{
		FBUsername = name;
	}
	public void SetProfilePic(Texture picture)
	{
		profilePic = picture;
	}
	
	// Adds new score and returns the list.
	// HACK - Only shows four high scores right now, will need to change in the future.
	public List<int> SaveHighScore(int score)
	{
		List<int> tempList = GetHighScores();

		if( score > tempList[0] )
		{
			if( tempList.Count >= 5 )
			{
				tempList[0] = score;
			}
			else
			{
				tempList.Add( score );
			}
			tempList.Sort();

			// Save the high scores to the preferences
			for( int i=0; i < tempList.Count; i++ )
			{
				PlayerPrefs.SetInt("HighScore" + i.ToString(), tempList[i]);
				
				if( i >= 5 )
				{
					break;
				}
			}
			PlayerPrefs.Save();
		}

		return tempList;
	}

	// Returns list of highscores in ascending order.
	public List<int> GetHighScores()
	{
		int tempScore; 
		List<int> newHighscores = new List<int>();
		// Get the high scores from the preferences
		for( int i=0; i < 5; i++ )
		{
			tempScore = PlayerPrefs.GetInt("HighScore" + i.ToString());
			if( tempScore != 0 )
			{
				newHighscores.Add( tempScore );
			}
			else
			{
				break;
			}
		}

		return newHighscores;
	}

	public void Pause()
	{
		if( previousMode == "FlippyCup" )
		{
			pauseMenu.transform.Translate( new Vector3( 0.0f, 6.0f, 0.0f ) );
			resumeButton.transform.Translate( new Vector3( 0.0f, 6.0f ) );
			soundToggleButton.transform.Translate( new Vector3( 0.0f, 6.0f ) );
			quitButton.transform.Translate( new Vector3( 0.0f, 7.5f, -.68f ) );

			pauseMenu.transform.localScale = new Vector3( 1.2f, 1.2f );
			pauseMenu.transform.Rotate( new Vector3( 40.0f, 0.0f ) );
			resumeButton.transform.localScale = new Vector3( .6f, .6f );
			resumeButton.transform.Rotate( new Vector3( 40.0f, 0.0f ) );
			soundToggleButton.transform.localScale = new Vector3( .6f, .6f );
			soundToggleButton.transform.Rotate( new Vector3( 40.0f, 0.0f ) );
			quitButton.transform.localScale = new Vector3( .6f, .6f );
			quitButton.transform.Rotate( new Vector3( 40.0f, 0.0f ) );
		}
		if( !isPaused )
		{
			isPaused = true;
			pauseMenu.renderer.enabled = true;
			pauseMenu.collider.enabled = true;
			resumeButton.collider.enabled = true;
			soundToggleButton.collider.enabled = true;
			quitButton.collider.enabled = true;

			Time.timeScale = 0;
		}
	}

	public void UnPause()
	{
		if( previousMode == "FlippyCup" )
		{
			pauseMenu.transform.localScale = new Vector3( 1.84f, 1.84f );
			pauseMenu.transform.Rotate( new Vector3( -40.0f, 0.0f ) );
			resumeButton.transform.localScale = new Vector3( 1.0f, 1.0f );
			resumeButton.transform.Rotate( new Vector3( -40.0f, 0.0f ) );
			soundToggleButton.transform.localScale = new Vector3( 1.0f, 1.0f );
			soundToggleButton.transform.Rotate( new Vector3( -40.0f, 0.0f ) );
			quitButton.transform.localScale = new Vector3( 1.0f, 1.0f );
			quitButton.transform.Rotate( new Vector3( -40.0f, 0.0f ) );
			
			pauseMenu.transform.Translate( new Vector3( 0.0f, -6.0f, 0.0f ) );
			resumeButton.transform.Translate( new Vector3( 0.0f, -6.0f ) );
			soundToggleButton.transform.Translate( new Vector3( 0.0f, -6.0f ) );
			quitButton.transform.Translate( new Vector3( 0.0f, -7.5f, .68f ) );
		}
		if( isPaused )
		{
			isPaused = false;
			pauseMenu.renderer.enabled = false;
			pauseMenu.collider.enabled = false;
			resumeButton.collider.enabled = false;
			soundToggleButton.collider.enabled = false;
			quitButton.collider.enabled = false;
			
			Time.timeScale = 1;
		}
	}
	
	void ResetVariables()
	{
		currentMinigames = new List<string>();
		currentMinigames.Add("BeerPong");
		currentMinigames.Add("FlippyCup");
		currentMinigames.Add("Darts");

		oldPartyPoints = 0;
		partyPoints = 0;	
		beersDrank = 0;	// Lives lost
		beerLives = 4;	// Total lives
		turnUpLevel = 1;
		musicSpeed = 1;
		
		// Beer Pong
		CupsPlaced = new bool[10];
		for(int i=0; i<10; i++)
		{
			CupsPlaced[i] = true;
		}

		Vector3 tempPos = new Vector3(7.7f, 3.9f, -5.0f);
		pauseButton.transform.position = tempPos;
		
		// Mini-game "levels"
		beerPongLevel = 1;
		beerPongStreak = 0;
		dartLevel = 1;
		pukeLevel = 4;
		armEnemyLevel = 1;
		flippyCupLevel = 1;

		pauseButton.renderer.enabled = false;
		pauseButton.collider.enabled = false;

		timerFront.renderer.enabled = false;
		timerBack.renderer.enabled = false;
		timerFront.transform.position = defaultTimerFrontPos;
		timerBack.transform.position = defaultTimerBackPos;
	}

}
