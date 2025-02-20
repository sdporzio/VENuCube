﻿// gameMenuApp.js
//
// created by Marco Del Tutto, marco.deltutto@physics.ox.ac.uk


#pragma strict

@Tooltip("How many seconds to read instructions.")
public var waitSec : int;

//public var tutorialExplanationPanels : GameObject[];
var tutorialExplanationPanels =  Array ();




function Start () {

  PlayerPrefs.SetInt ("PlayWithCardboard", 0); // FIXME (would like to remember preference troughout the game)

  var scene = SceneManager.GetActiveScene();
  if (scene.name == "GamePlayLevel1App"          || 
      scene.name == "GamePlayLevel2App"          ||
      scene.name == "GamePlayLevel1CardboardApp" ||
      scene.name == "GamePlayLevel2CardboardApp" ||
      scene.name == "GamePlayTutorialCardboardApp" ||
      scene.name == "GameTutorialApp") 
      Screen.orientation = ScreenOrientation.LandscapeLeft;
  else Screen.orientation = ScreenOrientation.Portrait;
  Debug.Log("The active scene name is " + scene.name);

  // Get the tutorial panels and put them in GameObjects, we will activate these panels later
  var canvas = GameObject.Find("MainCanvas");
  if (canvas != null) {
    for (var child : Transform in canvas.transform)
    {
      if(child.gameObject.name == "TutorialPanel_NuBeam"){
        tutorialExplanationPanels.Push(child.gameObject);
      }
      if(child.gameObject.name == "TutorialPanel_NuCrossSection"){
        tutorialExplanationPanels.Push(child.gameObject);
      }
      if(child.gameObject.name == "TutorialPanel_NuInteraction"){
        tutorialExplanationPanels.Push(child.gameObject);
      }
      if(child.gameObject.name == "TutorialPanel_NuEventShape"){
        tutorialExplanationPanels.Push(child.gameObject);
      }
      if(child.gameObject.name == "TutorialPanel_Cosmics"){
        tutorialExplanationPanels.Push(child.gameObject);
      }
    }
  }

  if (scene.name == "GameMenuApp"){
   Debug.Log("tutorialExplanationPanels[0]" + tutorialExplanationPanels[0]);
   Debug.Log("tutorialExplanationPanels[1]" + tutorialExplanationPanels[1]);
   Debug.Log("tutorialExplanationPanels[2]" + tutorialExplanationPanels[2]);
   Debug.Log("tutorialExplanationPanels[3]" + tutorialExplanationPanels[3]);
   Debug.Log("tutorialExplanationPanels[4]" + tutorialExplanationPanels[4]);
  }

   // *******************
   // We need to check if the user coming back to tutorial because he previously started it and then decide to learn something
   // *******************
   if (PlayerPrefs.HasKey("LearnFromTutorial")) {
    if(PlayerPrefs.GetInt("LearnFromTutorial") == 1) {
      // The user is learning from the tutorial, understand what subject and load the relative panel immediately.
      // Reset the preference
      PlayerPrefs.SetInt("LearnFromTutorial", 0); // 0=false;  1=true
      if (PlayerPrefs.HasKey("LearnFromTutorialSubject")) {
      if (PlayerPrefs.GetString("LearnFromTutorialSubject") == "NuBeam")         goToTutorialExplanation(0);
      if (PlayerPrefs.GetString("LearnFromTutorialSubject") == "NuCrossSection") goToTutorialExplanation(1);
      if (PlayerPrefs.GetString("LearnFromTutorialSubject") == "NuInteraction")  goToTutorialExplanation(2);
      if (PlayerPrefs.GetString("LearnFromTutorialSubject") == "Cosmics")        goToTutorialExplanation(4);

      }
    }
  }

  if (PlayerPrefs.HasKey("CongratsAndEndGameMain")) {
    if(PlayerPrefs.GetInt("CongratsAndEndGameMain") == 1) {

      PlayerPrefs.SetInt("CongratsAndEndGameMain", 0);
      var canvasm = GameObject.Find("MainCanvas");
      for (var child : Transform in canvasm.transform){
        if(child.gameObject.name == "CongratsAndEndGame"){
          Screen.orientation = ScreenOrientation.LandscapeLeft;
          child.gameObject.SetActive(true);

        }
      }
    }
  }



  // If the user is playing the game the =n we don't want him able to go
  // to the next or previous event!
  if (scene.name == "GamePlayLevel1App"          || 
      scene.name == "GamePlayLevel2App"          ||
      scene.name == "GameTutorialApp") {

    var menuc = GameObject.Find("MenuCanvas");
    for (var child : Transform in menuc.transform){
      if(child.gameObject.name == "InGameMenu"){
        for (var child2 : Transform in child.transform){
          if(child2.gameObject.name == "Panel"){
            for (var child3 : Transform in child2.transform){
              if(child3.gameObject.name == "ButtonsPanel"){
                for (var child4 : Transform in child3.transform){
                  if(child4.gameObject.name == "MenuButtons"){
                    for (var child5 : Transform in child4.transform){
                      if(child5.gameObject.name == "NextEventButton"){
                        child5.gameObject.GetComponent(Selectable).interactable = false;
                      }
                      if(child5.gameObject.name == "PrevEventButton"){
                        child5.gameObject.GetComponent(Selectable).interactable = false;
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }

}


function Update () {
 
}


function WaitAndStop(panel : GameObject) {
        panel.SetActive (true);
		yield WaitForSeconds(waitSec);
		panel.SetActive (false);

	}

function goToTutorialMain () {

  Screen.orientation = ScreenOrientation.LandscapeLeft;

  var canvas = GameObject.Find("MainCanvas");
  var cardInit : GameObject;
  for (var child : Transform in canvas.transform)
  {
    if(child.gameObject.name == "StartTutorialPanel"){
      child.gameObject.SetActive(true);
    }
    if(child.gameObject.name == "LoadCardboardPanel"){
      cardInit = child.gameObject;
    }
  }
  yield WaitForSeconds (waitSec);

  // Check if the user wants to play with cardbord or not, then start the appropriate scene

  // If the user didn't decide, then start normal game
  if (!PlayerPrefs.HasKey ("PlayWithCardboard"))
    PlayerPrefs.SetInt ("PlayWithCardboard", 0);

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 0)
    Application.LoadLevel("GameTutorialApp");

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 1){
    cardInit.SetActive(true);
    yield WaitForSeconds (waitSec);
    //cardInit.SetActive(false);
    Screen.orientation = ScreenOrientation.LandscapeLeft;
    Application.LoadLevel("GameTutorialCardboardApp");
  }

}

function goToTutorial() {

    StartCoroutine(goToTutorialMain()); //need to start a coroutine to use WaitForSeconds.

}

function prepareTutorial() {

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 0) {

    goToTutorial();

  }

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 1) {

    var canvas = GameObject.Find("MainCanvas");
      for (var child : Transform in canvas.transform) {
        if(child.gameObject.name == "CardboardExplanationPanel"){
          child.gameObject.SetActive(true);
        }
      }
   }
}

function deactivateCardbordHelp() {

var canvas = GameObject.Find("MainCanvas");
  for (var child : Transform in canvas.transform) {
    if(child.gameObject.name == "CardboardExplanationPanel"){
      child.gameObject.SetActive(false);
    }
  }

}

function goToTutorialDirectly() {

  // If the user didn't decide, then start normal game
  if (!PlayerPrefs.HasKey ("PlayWithCardboard"))
    PlayerPrefs.SetInt ("PlayWithCardboard", 0);

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 0)
    Application.LoadLevel("GameTutorialApp");

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 1)
    Application.LoadLevel("GameTutorialCardboardApp");
}

function goToTutorialExplanation(panel : int){

  Screen.orientation = ScreenOrientation.LandscapeLeft;

  var canvas = GameObject.Find("MainCanvas");
  for (var child : Transform in canvas.transform)
  {
    if(child.gameObject.name == "MainPanel"){
      child.gameObject.SetActive(false);
    }
  }

  for (var i : int = 0; i < 5; i++){
    var temp : GameObject = tutorialExplanationPanels[i];
    if (i != panel) temp.SetActive(false);
    if (i == panel) temp.SetActive(true);

  }
}

function abortTutorial() {

  for (var i : int = 0; i < 5; i++){
    var temp : GameObject = tutorialExplanationPanels[i];
    temp.SetActive(false);
  }

  var canvas = GameObject.Find("MainCanvas");
  for (var child : Transform in canvas.transform)
  {
    if(child.gameObject.name == "MainPanel"){
      child.gameObject.SetActive(true);
    }
  }
  Screen.orientation = ScreenOrientation.Portrait;

}


function goToRealGameMain(level: int) {

  Screen.orientation = ScreenOrientation.LandscapeLeft;

  var canvas = GameObject.Find("MainCanvas");
  var cardInit : GameObject;
  for (var child : Transform in canvas.transform)
  {
    if(child.gameObject.name == "StartGameLevel1Panel" && level == 1 ){
      child.gameObject.SetActive(true);
    }
    if(child.gameObject.name == "StartGameLevel2Panel" && level == 2 ){
      child.gameObject.SetActive(true);
    }
    if(child.gameObject.name == "LoadCardboardPanel"){
      cardInit = child.gameObject;
    }
  }
  yield WaitForSeconds (waitSec);

  // Check if the user wants to play with cardbord or not, then start the appropriate scene

  // If the user didn't decide, then start normal game
  if (!PlayerPrefs.HasKey ("PlayWithCardboard"))
    PlayerPrefs.SetInt ("PlayWithCardboard", 0);

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 0) {
    Screen.orientation = ScreenOrientation.LandscapeLeft;
    if (level == 1) Application.LoadLevel("GamePlayLevel1App");
    if (level == 2) Application.LoadLevel("GamePlayLevel2App");
  }

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 1){
    cardInit.SetActive(true);
    yield WaitForSeconds (waitSec);
    //cardInit.SetActive(false);
    Screen.orientation = ScreenOrientation.LandscapeLeft;
    if (level == 1) Application.LoadLevel("GamePlayLevel1CardboardApp");
    if (level == 2) Application.LoadLevel("GamePlayLevel2CardboardApp");
  }
   
}


function goToRealGame(level: int) {

  var canvas = GameObject.Find("MainCanvas");
  for (var child : Transform in canvas.transform)
  {
    if(child.gameObject.name == "MainPanel"){
      child.gameObject.SetActive(false);
    }
  }

  StartCoroutine(goToRealGameMain(level)); //need to start a coroutine to use WaitForSeconds.
}

function goToRealGameDirectly() {

  // If the user didn't decide, then start normal game
  if (!PlayerPrefs.HasKey ("PlayWithCardboard"))
    PlayerPrefs.SetInt ("PlayWithCardboard", 0);

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 0)
    Application.LoadLevel("GamePlayApp");

  if (PlayerPrefs.GetInt ("PlayWithCardboard") == 1)
    Application.LoadLevel("GamePlayCardboardApp");
}



function removePanel(){

  var canvas = GameObject.Find("GameCanvas");
  for (var child : Transform in canvas.transform)
  {
    if(child.gameObject.name == "PanelNext"){
      child.gameObject.SetActive(false);
    }
    if(child.gameObject.name == "PanelNextWithCosmics"){
      child.gameObject.SetActive(false);
    }
    if(child.gameObject.name == "PanelContinueSearch"){
      child.gameObject.SetActive(true);
    }
  }

}


function goToLearnNuBeam() {

  PlayerPrefs.SetInt("LearnFromTutorial", 1); // 0=false;  1=true
  PlayerPrefs.SetString("LearnFromTutorialSubject", "NuBeam");
  Application.LoadLevel("LearnApp");

}

function goToLearnNuCrossSection() {

  PlayerPrefs.SetInt("LearnFromTutorial", 1); // 0=false;  1=true
  PlayerPrefs.SetString("LearnFromTutorialSubject", "NuCrossSection");
  Application.LoadLevel("LearnApp");

}

function goToLearnNuInteraction() {

  PlayerPrefs.SetInt("LearnFromTutorial", 1); // 0=false;  1=true
  PlayerPrefs.SetString("LearnFromTutorialSubject", "NuInteraction");
  Application.LoadLevel("LearnApp");

}

function goToLearnCosmics() {

  PlayerPrefs.SetInt("LearnFromTutorial", 1); // 0=false;  1=true
  PlayerPrefs.SetString("LearnFromTutorialSubject", "Cosmics");
  Application.LoadLevel("LearnApp");

}

function setGameCardboardOnOff() {

  // If this is the first time the user is clicking, then play w/ cardboard
  if (!PlayerPrefs.HasKey ("PlayWithCardboard")){
    PlayerPrefs.SetInt ("PlayWithCardboard", 1);
  }

  // Otherwise check what is the current key and change it
  else if (PlayerPrefs.GetInt ("PlayWithCardboard") == 1){
    PlayerPrefs.SetInt ("PlayWithCardboard", 0);
  }

  else if (PlayerPrefs.GetInt ("PlayWithCardboard") == 0){
    PlayerPrefs.SetInt ("PlayWithCardboard", 1);
  }

}

function deactivateCongratsAndEndGamePanel () {

  var canvasm = GameObject.Find("MainCanvas");
  for (var child : Transform in canvasm.transform)
  {
    if(child.gameObject.name == "CongratsAndEndGame"){
      child.gameObject.SetActive(false);
      Screen.orientation = ScreenOrientation.Portrait;
    }
  }

}



