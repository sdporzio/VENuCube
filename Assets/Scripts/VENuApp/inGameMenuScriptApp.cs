﻿// inGameMenuScriptApp.cs
//
// created by Marco Del Tutto, marco.deltutto@physics.ox.ac.uk


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class inGameMenuScriptApp: MonoBehaviour {
	
	public string EventMenuScene;
	public string GameMenuScene;
	
	//for animation
	private enum menuState {isIn, slidingOut, isOut, slidingIn};
	private menuState state;
	private float inPos;
	private float outPos;
	private float startTime;
	private bool showData, showSimulation, isGame;
	private RectTransform me;
	const float slideSpeed = 3;
	public RectTransform buttonsGroup;
	public GameObject slideButton;
	private GameObject nuBeam;

	int nPrefabs = 0;
	int currentPrefab = 0;
	GameObject[] prefabsToLoad = new GameObject[100];
	string[] namePrefabsToLoad = new string[100];
	string[] runInfoPrefabsToLoad = new string[100];
	GameObject evtContainer;


	void Awake() {

		Debug.Log ("Calling inGameMenuScriptApp.");
		print(gameObject.name);

		Screen.orientation = ScreenOrientation.LandscapeLeft;

		showSimulation = showData = isGame = false;

		// Understand if we need to show Simulation or Data events
		if (PlayerPrefs.HasKey ("ShowSimulationOrData")) {
			if (PlayerPrefs.GetInt ("ShowSimulationOrData") == 0) {  // 0: simulation, 1: data
				showSimulation = true;
			} else
				showData = true;
		} else
			Debug.Log ("Can't find key ShowSimulationOrData in inGameMenuScriptApp.cs.");


		if (SceneManager.GetActiveScene ().name == "GameTutorialApp"   || 
			SceneManager.GetActiveScene ().name == "GamePlayLevel1App" ||
			SceneManager.GetActiveScene ().name == "GamePlayLevel2App"   ) {
			isGame = true;
			showSimulation = showData = false;
		}


		if (isGame) {
			
			evtContainer = GameObject.Find ("EventsPrefab_simulation");

			Debug.Log ("This should be EventsPrefab_...: " + evtContainer.name);
			foreach (Transform child in evtContainer.transform) {
				Debug.Log ("The name of the child is " + child.name);
				prefabsToLoad [nPrefabs] = child.gameObject;
				nPrefabs++;
			}
			Debug.Log ("Event prefabs found: " + nPrefabs + 1);

		}

		if (showSimulation) {
			namePrefabsToLoad [0] = "Tracks/prodgenie_bnb_nu_cosmic_uboone_5.json"; nPrefabs++;
			namePrefabsToLoad [1] = "Tracks/prodgenie_bnb_nu_cosmic_uboone_16.json"; nPrefabs++;
			namePrefabsToLoad [2] = "Tracks/prodgenie_bnb_nu_cosmic_uboone_13.json"; nPrefabs++;
			namePrefabsToLoad [3] = "Tracks/prodgenie_bnb_nu_cosmic_uboone_12.json"; nPrefabs++; 
			namePrefabsToLoad [4] = "Tracks/prodgenie_bnb_nu_cosmic_uboone_10.json"; nPrefabs++;
		}

		if (showData) {
			namePrefabsToLoad [0] = "SpacePoints/data_ccpi0_r5975e4262.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [1] = "SpacePoints/data_ccnumu_r5153e2919.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [2] = "SpacePoints/data_ccnumu_r5153e2929.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [3] = "SpacePoints/data_ccnumu_r5155e6623.json.spacepoints_3cm.json"; nPrefabs++; 
			namePrefabsToLoad [4] = "SpacePoints/data_ccnumu_r5189e665.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [5] = "SpacePoints/data_ccnumu_r5192e1218.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [6] = "SpacePoints/data_ccnumu_r5208_e5108.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [7] = "SpacePoints/data_ccnumu_r5607_e2873.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [8] = "SpacePoints/data_ccnumu_r5820_e585.json.spacepoints_3cm.json"; nPrefabs++;
			namePrefabsToLoad [9] = "SpacePoints/data_ccnumu_r5823_e6135.json.spacepoints_3cm.json"; nPrefabs++;

			runInfoPrefabsToLoad [0] = "MicroBooNE Run 5975, Event 4262";
			runInfoPrefabsToLoad [1] = "MicroBooNE Run 5153, Event 2919";
			runInfoPrefabsToLoad [2] = "MicroBooNE Run 5153, Event 2929";
			runInfoPrefabsToLoad [3] = "MicroBooNE Run 5155, Event 6623";
			runInfoPrefabsToLoad [4] = "MicroBooNE Run 5189, Event 665";
			runInfoPrefabsToLoad [5] = "MicroBooNE Run 5192, Event 1218";
			runInfoPrefabsToLoad [6] = "MicroBooNE Run 5208, Event 5108";
			runInfoPrefabsToLoad [7] = "MicroBooNE Run 5607, Event 2873";
			runInfoPrefabsToLoad [8] = "MicroBooNE Run 5820, Event 585";
			runInfoPrefabsToLoad [9] = "MicroBooNE Run 5823, Event 6135";
		}


		// Start the scene loading the first event prefab. 
		// Then go on with the other prefabs as soon as the user clicks on next or previous event.
		if (!isGame) prefabsToLoad [currentPrefab] = (GameObject)Instantiate (Resources.Load (namePrefabsToLoad [currentPrefab]));
		prefabsToLoad[currentPrefab].SetActive(true);

	}
		
	void Start () {

		Screen.orientation = ScreenOrientation.LandscapeLeft;
		
		me = GetComponent<RectTransform>();
		
		#if MOBILE_INPUT
		me.sizeDelta = new Vector2(360, me.sizeDelta.y);
		foreach(LayoutElement child in buttonsGroup.GetComponentsInChildren<LayoutElement>()){
			child.minHeight = 60;
		}
		#else
		me.sizeDelta = new Vector2(320, me.sizeDelta.y);
		foreach(LayoutElement child in transform.GetComponentsInChildren<LayoutElement>()){
			child.minHeight = 70;
		}
		#endif
		
		state = menuState.isIn;
		inPos = -(me.rect.width / 2);
		outPos = (me.rect.width / 2);
		me.anchoredPosition = new Vector2(inPos, 0);

		// Get the nu beam for later
		nuBeam = GameObject.Find ("NuBeamParticleSystem");
		
	}
	
	void Update () {
		
		if(state == menuState.slidingOut){
			me.anchoredPosition = Vector2.Lerp (new Vector2(inPos, 0), new Vector2(outPos, 0), (Time.time - startTime) * slideSpeed);
			//slideButton.GetComponent<RectTransform>().eulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 180, 0), (Time.time - startTime) * slideSpeed);
			if (me.anchoredPosition == new Vector2(outPos, 0))
				state = menuState.isOut;
		}
		else if(state == menuState.slidingIn){
			me.anchoredPosition = Vector2.Lerp (new Vector2(outPos, 0), new Vector2(inPos, 0), (Time.time - startTime) * slideSpeed);
			//slideButton.GetComponent<RectTransform>().eulerAngles = Vector3.Lerp(new Vector3(0, 180, 0), new Vector3(0, 0, 0), (Time.time - startTime) * slideSpeed);
			if (me.anchoredPosition == new Vector2(inPos, 0))
				state = menuState.isIn;
		}
		
	}
	
	public void SlideMenu() {
		if(state == menuState.isIn){
			state = menuState.slidingOut;
			slideButton.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0); //AMCLEAN changed (0,0,180) to (0,0,0)
			startTime = Time.time;
		}	
		else if(state == menuState.isOut){
			state = menuState.slidingIn;
			slideButton.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
			startTime = Time.time;
		}
	}


	public void ToEventMenu() {
		SceneManager.LoadScene(EventMenuScene);
	}

	public void ToGameMenu() {
		SceneManager.LoadScene(GameMenuScene);
	}


	public void LoadNext(){

		// Remove current event
		prefabsToLoad[currentPrefab].SetActive(false);
		if (!isGame) Object.Destroy (prefabsToLoad[currentPrefab]);


		// Verify this is not the last event available, in that case, re-start from beginning
		if (currentPrefab == nPrefabs-1)
			currentPrefab = 0;
		else
			currentPrefab++;

		Debug.Log ("currentPrefab is " + currentPrefab);
		Debug.Log ("nPrefabs is " + nPrefabs);

		// Load the event
		//if (showSimulation) evtContainer.SetActive(true);
		if (!isGame) prefabsToLoad[currentPrefab] = (GameObject)Instantiate (Resources.Load (namePrefabsToLoad [currentPrefab]));
		prefabsToLoad[currentPrefab].SetActive(true);
		//Resources.UnloadUnusedAssets ();

		// If we are loading real data, then also display the run, event infos
		if (showData) {
			displayRunInfo (currentPrefab);
		}

	}

	public void LoadPrevious(){

		// Remove current event
		prefabsToLoad[currentPrefab].SetActive(false);
		if (!isGame) Object.Destroy (prefabsToLoad[currentPrefab]);

		// Verify this is not the first event available, in that case, go to the last one
		if (currentPrefab == 0)
			currentPrefab = nPrefabs-1;
		else
			currentPrefab--;

		// Load the event
		if (!isGame) prefabsToLoad[currentPrefab] = (GameObject)Instantiate (Resources.Load (namePrefabsToLoad [currentPrefab]));
		prefabsToLoad[currentPrefab].SetActive(true);
		//Resources.UnloadUnusedAssets ();

		// If we are loading real data, then also display the run, event infos
		if (showData) {
			displayRunInfo (currentPrefab);
		}

	}

	public void displayRunInfo(int currentPrefab) {

		//yield return new WaitForSeconds(1.0F);

		GameObject thisTextObj = GameObject.Find ("RunInfoCanvas"); // this assignment is just temporary

		GameObject runInfoCanvas = GameObject.Find ("RunInfoCanvas");
		foreach (Transform child in runInfoCanvas.transform) {
			if (child.name == "Panel") {
				child.gameObject.SetActive (true);
				foreach (Transform child2 in child.transform) {
					if (child2.name == "Text") {
						thisTextObj = child2.gameObject;
					}
				}
			}
		}

		thisTextObj.GetComponentInChildren<Text>().text = runInfoPrefabsToLoad[currentPrefab];


	}


	public void ToggleTracks() {

		Debug.Log ("I'm here.  " + prefabsToLoad [currentPrefab].activeInHierarchy + "   " + namePrefabsToLoad [currentPrefab]);

		if(prefabsToLoad [currentPrefab].activeInHierarchy) prefabsToLoad [currentPrefab].SetActive (false);
		else if(!prefabsToLoad [currentPrefab].activeInHierarchy) prefabsToLoad [currentPrefab].SetActive (true);

	}

	public void ToggleBeam() {

		if (nuBeam.activeInHierarchy) nuBeam.SetActive (false);
		else nuBeam.SetActive (true);

	}

	public void deactivateHelpPanel() {

		GameObject helpCanvas = GameObject.Find ("HelpCanvas");

		foreach (Transform child in helpCanvas.transform) {
			if (child.name == "HelpPanel") {
				child.gameObject.SetActive (false);
			}
		}

		// Also display run info for the first event
		if (showData) displayRunInfo(0);
	}


	/*



	
	public void ToEventMenu() {
		SceneManager.LoadScene(EventMenuScene);
	}
	
	public void LoadNext(){
		string currentEvent = PlayerPrefs.GetString("File To Load");
		DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
		FileInfo[] filesInfo = dir.GetFiles("*.json");
		int currentIndex = -1;
		
		for(int i = 0; i < filesInfo.Length; i++)
			if(filesInfo[i].Name == currentEvent)
				currentIndex = i;
		
		//if(currentIndex == filesInfo.Length - 1){
		//	//no more files!
		//	Debug.Log("No more files!");
		//}
		if(currentIndex == -1){
			//don't know where we are. did File To Load not get set?
			Debug.Log("file not found!");
			
		}
		else{
			PlayerPrefs.SetString("File To Load", filesInfo[(currentIndex + 1) % filesInfo.Length].Name);
			Debug.Log("loading file " + PlayerPrefs.GetString("File To Load"));
			SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
			//the file is loaded elsewhere. All that script needs is the name of the new file.
		}
	}
	
	public void LoadPrevious(){
		string currentEvent = PlayerPrefs.GetString("File To Load");
		DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
		FileInfo[] filesInfo = dir.GetFiles("*.json");
		int currentIndex = -1;
		
		for(int i = 0; i < filesInfo.Length; i++)
			if(filesInfo[i].Name == currentEvent)
				currentIndex = i;
		
		//if(currentIndex == filesInfo.Length - 1){
		//	//no more files!
		//	Debug.Log("No more files!");
		//}
		if(currentIndex == -1){
			//don't know where we are. did File To Load not get set?
			Debug.Log("file not found!");
			
		}
		else{
			PlayerPrefs.SetString("File To Load", filesInfo[(currentIndex -1) % filesInfo.Length].Name);
			Debug.Log("loading file " + PlayerPrefs.GetString("File To Load"));
			//SceneManager.LoadScene(Application.loadedLevel);
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			//the file is loaded elsewhere. All that script needs is the name of the new file.
		}
	}
	*/
}
