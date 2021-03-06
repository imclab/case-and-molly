﻿using UnityEngine;
using System.Collections;

public class InitGame : MonoBehaviour {	
	public Color backgroundColor;
	
	public int numCubes = 5;
	public int maxDistance = 20;
	public GameObject cube;
	
	GameObject[] cubes;
	
	int currentCube = -1;
	public GameObject ovrParent;
	
	float lerpStarted = 0.0f;
	public float lerpTime = 0.5f;
	float moveLength = 0.0f;
	Vector3 dest;
	Vector3 prevDest;
	
	float movePercent = 1.0f;
	
	public Camera cameraLeft;
	public Camera cameraRight;
	
	ArrayList userSort;
	public GameObject lineHead;
	
	OVRGUI ovrGui;
	public OVRCameraController cameraController;
	
	public float timePerLevel = 60.0f;
	string timeString;
	public Color timeColor;
	public Font timeFont;
	
	float winTime = 0.0f;
		
	void cleanupCameras(){
		
		cameraLeft.backgroundColor = backgroundColor;
		cameraRight.backgroundColor = backgroundColor;		
	}
	
	// Use this for initialization
	void Start () {
		cleanupCameras();
		
		ovrGui = new OVRGUI();
		ovrGui.SetCameraController(ref cameraController);
		ovrGui.SetFontReplace(timeFont);
		
		cubes = new GameObject[numCubes];
		for(int i = 0; i < numCubes; i++){
			Vector3 position = Random.onUnitSphere * Random.Range(maxDistance/2, maxDistance);
			GameObject o = GameObject.Instantiate(cube, position, Quaternion.identity) as GameObject;
			

			BlinkySorty bs = o.GetComponent<BlinkySorty>();
			
			bs.leftCamera = cameraLeft;
						
			bs.numBlinks = (int)Random.Range(1, 7);
			cubes[i] = o;			
		}	
		
		dest = ovrParent.transform.position;	
		
		userSort = new ArrayList();
	}
	
	void NextCube(){
			currentCube++;
			if(currentCube > numCubes-1){
				currentCube = 0;
			}
						
			prevDest = dest;
			dest = cubes[currentCube].gameObject.transform.position - (Vector3.back * 2);	
			moveLength = Vector3.Distance(ovrParent.transform.position, dest);
			
			lerpStarted = Time.time;
	}
	
	bool CheckVictory(){
		bool result = true;
		
		if(userSort.Count < numCubes){
			result = false;
		} else{
		
			for(int i = 1; i < userSort.Count; i++){
				GameObject curr = (GameObject)userSort[i];
				GameObject prev = (GameObject)userSort[i-1];
				// if any of the items are out of order
				if(prev.GetComponent<BlinkySorty>().numBlinks > curr.GetComponent<BlinkySorty>().numBlinks ){
					result = false;
				}			
			}
		}
		
		return result;
	}
	
	bool CheckFailure(){
		return (timePerLevel - Time.timeSinceLevelLoad <= 0.0f);
	}
	
	void OnGUI(){
		if(!CheckVictory()){
			timeString = (timePerLevel - Time.timeSinceLevelLoad).ToString("0.000");
		} else {
			string win = "WIN";
			ovrGui.StereoBox(200,500,200,60, ref win, Color.red);
			if(winTime == 0){
				winTime = Time.time;
			}
			
			if(winTime > 0 && (Time.time - winTime > 2.0f)){
				Application.LoadLevel("cameraIntegration");
			}
		}
		
		
		if(CheckFailure()){
			string fail = "FAIL";
			ovrGui.StereoBox(200,500,200,60, ref fail, Color.red);
		} else {
			if(winTime == 0){
				ovrGui.StereoBox (200,500, 200, 60, ref timeString, timeColor);
			}
		
			Event e = Event.current;
			if(e.isKey && e.keyCode == KeyCode.RightArrow && movePercent == 1){
				NextCube();
			}
			else if(e.isKey && Input.GetKeyDown(KeyCode.Space) && movePercent == 1){
			
				if(!userSort.Contains(cubes[currentCube])){
					userSort.Add(cubes[currentCube]);
				} else {
					userSort.Remove(cubes[currentCube]);
					userSort.Add(cubes[currentCube]);
				}
			
			
				int i = 0;
				foreach(GameObject item in userSort){
					item.GetComponent<BlinkySorty>().GoTo(lineHead.transform.position + (Vector3.left * 2 * i));
					i++;
				}
			
			//NextCube();
			}
		}

		
	}
	
	// Update is called once per frame
	void Update () {
		if(moveLength > 0){
			movePercent = (Time.time - lerpStarted)/lerpTime;
			movePercent = Mathf.Clamp(movePercent, 0, 1);
			ovrParent.transform.position = Vector3.Lerp(prevDest, dest, movePercent);
			ovrParent.transform.LookAt(cubes[currentCube].gameObject.transform.position);
		}
		
		print (CheckVictory());

		
	}
}
