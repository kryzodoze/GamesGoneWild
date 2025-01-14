﻿using UnityEngine;
using System.Collections;

public class SelectionController : MonoBehaviour 
{
	public GameObject globalController;
	
	public RaycastHit hit;
	public Ray ray;

	// How far the drag was
	Vector3 startDrag;
	float dragDistance;

	// For arrow transitions
	public GameObject rightArrow;
	public GameObject leftArrow;
	bool transitioning;
	string direction;
	Vector2 transitionVector;
	int currentGroup;
	public GameObject[] groups;

	public GameObject ClickSound;

	// Use this for initialization
	void Start () 
	{
		globalController = GameObject.Find("Global Controller");
		
		hit = new RaycastHit();

		transitioning = false;
		direction = "";
		transitionVector = new Vector2( .4f, 0.0f );
		currentGroup = 0;
		
		leftArrow.renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( Input.GetMouseButtonDown( 0 ) )
		{
			startDrag = Input.mousePosition;
		}
		if( Input.GetMouseButtonUp( 0 ) )
		{
			dragDistance = startDrag.x - Input.mousePosition.x;
			if( Mathf.Abs( dragDistance ) < 15.0f ) 
			{
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if( Physics.Raycast(ray,out hit) && !transitioning)
				{
					switch( hit.collider.name )
					{
					case "LeftArrow":
						GoLeft();
						ClickSound.GetComponent<AudioSource>().Play();
						break;
					case "RightArrow":
						GoRight();
						ClickSound.GetComponent<AudioSource>().Play();
						break;
					case "BackText":
						ClickSound.GetComponent<AudioSource>().Play();
						Application.LoadLevel( "MenuScene" );
						break;
					case "BPText":
					case "BPPic":
						ClickSound.GetComponent<AudioSource>().Play();
						globalController.GetComponent<GlobalController>().StartMode( "Selection", "BeerPong" );
						break;
					case "FlippyText":
					case "FlippyPic":
						ClickSound.GetComponent<AudioSource>().Play();
						globalController.GetComponent<GlobalController>().StartMode( "Selection", "FlippyCup" );
						break;
					case "DartsText":
					case "DartsPic":
						ClickSound.GetComponent<AudioSource>().Play();
						globalController.GetComponent<GlobalController>().StartMode( "Selection", "Darts" );
						break;
					case "ArmWrestlingText":
					case "ArmWrestlingPic":
						ClickSound.GetComponent<AudioSource>().Play();
						globalController.GetComponent<GlobalController>().StartMode( "Selection", "ArmWrestle" );
						break;
					case "ThrowUpText":
					case "ThrowUpPic":
						ClickSound.GetComponent<AudioSource>().Play();
						globalController.GetComponent<GlobalController>().StartMode( "Selection", "Save_The_Floor" );
						break;
					case "TiltText":
					case "TiltPic":
						ClickSound.GetComponent<AudioSource>().Play();
						globalController.GetComponent<GlobalController>().StartMode( "Selection", "fall" );
						break;
					}
				}
			}
			else
			{
				// If a drag to the left
				if( dragDistance <= 0.0f )
				{
					GoLeft();
					ClickSound.GetComponent<AudioSource>().Play();
				}
				else
				{
					GoRight();
					ClickSound.GetComponent<AudioSource>().Play();
				}
			}
		}

		if( transitioning )
		{
			if( direction == "Left" )
			{
				for( int i=0; i<groups.Length; i++ )
				{
					groups[i].transform.Translate( transitionVector * 60.0f * Time.deltaTime );
				}
				
				if( groups[currentGroup-1].transform.position.x >= -3.0f )
				{
					transitioning = false;
					currentGroup--;
					
					rightArrow.renderer.enabled = true;
					rightArrow.layer = 0;
					if( 0 == currentGroup )
					{
						leftArrow.renderer.enabled = false;
						leftArrow.layer = 2;
					}
					else
					{
						leftArrow.renderer.enabled = true;
						leftArrow.layer = 0;
					}
				}
			}
			else // direction == "Right" 
			{
				for( int i=0; i<groups.Length; i++ )
				{
					groups[i].transform.Translate( -transitionVector * 60.0f * Time.deltaTime );
				}

				if( groups[currentGroup+1].transform.position.x <= -3.0f )
				{
					transitioning = false;
					currentGroup++;
					
					leftArrow.renderer.enabled = true;
					leftArrow.layer = 0;
					if( groups.Length == (currentGroup+1) )
					{
						rightArrow.renderer.enabled = false;
						rightArrow.layer = 2;
					}
					else
					{
						rightArrow.renderer.enabled = true;
						rightArrow.layer = 0;
					}
				}
			}
		}
	}

	void GoLeft()
	{
		if( leftArrow.renderer.enabled )
		{
			direction = "Left";
			transitioning = true;
			rightArrow.renderer.enabled = false;
			leftArrow.renderer.enabled = false;
		}
	}
	
	void GoRight()
	{
		if( rightArrow.renderer.enabled )
		{
			direction = "Right";
			transitioning = true;
			rightArrow.renderer.enabled = false;
			leftArrow.renderer.enabled = false;
		}
	}
}
