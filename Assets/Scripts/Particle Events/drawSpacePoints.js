//__________________________________________________________________________________________________
//DrawSpacePoints.js
//Script to place spacepoints based on user-defined maximum spacepoint value.
//
//  Most recent changes: 
//  (7/29/15) Created the script from AlistairDrawParticleSpacePoints.js. Removed old code,
//            changed some names, and added ability to draw only a fixed number, evenly distributed.
//
//__________________________________________________________________________________________________

#pragma strict
import SimpleJSON;
import System.IO;
import System.Linq;

var dot : GameObject;
var maxPoints : float;
private var m_InGameLog = "";
private var m_Position = Vector2.zero;
var spacePointAlgoName : String = "recob::SpacePoints_pandoraCosmicKHit_RecoStage2";
var fileName : String;
public var pointParent : GameObject;

function Awake()
{
	maxPoints = PlayerPrefs.GetInt("maxSpacePoints");
	spacePointAlgoName = PlayerPrefs.GetString("pointAlgorithm");

	if(PlayerPrefs.HasKey("File To Load"))
	{
		fileName = PlayerPrefs.GetString("File To Load");
	}
	else
	{
		fileName = "prodgenie_bnb_nu_cosmic.json";
		Debug.Log("<color=red>No Event Loaded</color>");
	}
}
function P( aText : String)
{
	m_InGameLog += aText + "\n";
}

function DrawSpacePoints(N : JSONNode)
{
	//spacePointsArray seems to be unused -Owen
    var spacePointsArray = new Array ();
    var totalPts = N["record"]["spacepoints"][spacePointAlgoName].Count;
    
    //Calculate the proper iterator so that certain points can be skipped (for performance reasons)
    var iter : float = totalPts / maxPoints;
    
    //If the event has fewer points than the maximum number allowed, draw all of them.
    if (iter < 1.0) {
        iter = 1.0;
    }
    
    for(var key : float = 0.0; key < totalPts; key += iter){
	    var clone : GameObject;
	    
	    //Round the loop variable here (to get an index) to minimize rounding error in the loop.
	    var roundKey : int = Mathf.Round(key);
	    
	 	clone = Instantiate(dot , transform.position, transform.rotation);
    	clone.transform.position = transform.position + Vector3(
    	    0.1*N["record"]["spacepoints"][spacePointAlgoName][roundKey]["xyz"][0].AsFloat,
    	    0.1*N["record"]["spacepoints"][spacePointAlgoName][roundKey]["xyz"][1].AsFloat,
           -0.1*N["record"]["spacepoints"][spacePointAlgoName][roundKey]["xyz"][2].AsFloat);
    	clone.transform.localScale = Vector3(0.005,0.005,0.005);
    	clone.gameObject.layer = 10;
    	clone.transform.SetParent(pointParent.transform);
	    spacePointsArray.Push(clone);	
    }
}

function Start() 
{

}

public function drawPoints(node : JSONNode)
{
	if(maxPoints != 0){
		DrawSpacePoints(node);
	}
}

public function togglePoints(){
	if(pointParent.activeInHierarchy){
		pointParent.SetActive(false);
	}
	else{
		pointParent.SetActive(true);
	}
}

function OnGUI()
{
	m_Position = GUILayout.BeginScrollView(m_Position);
	GUILayout.Label(m_InGameLog);
	GUILayout.EndScrollView();
}


function Update () 
{

}
