/***********************************************************************************************************************
 * Unity-SCORM Integration Kit
 * 
 * Example Use of the Unity-SCORM Integration Kit via calls to the ScormManager
 * 
 * Copyright (C) 2015, Richard Stals (http://stals.com.au)
 * ==========================================
 * 
 *
 ***********************************************************************************************************************
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 *
 **********************************************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

/// <summary>
/// The example scene's main controller
/// </summary>
/// <remarks>
/// This gives you a fairly complete example of how to use the various functions of the ScormManager.\n
/// To use this correctly in your own Unity3D project, the scene must be set up as follows:\n
/// ScormManager (an empty GameObject that has the ScormManager script attached.\n
///   |-> Controller (an empty GameObject that has your Main Controller script [e.g. this one] attached.\n
/// </remarks>
public class ControllerMain : MonoBehaviour {

	/// <summary>Hold the Completion Status of this  attempt.</summary>
	private StudentRecord.CompletionStatusType completionStatus = StudentRecord.CompletionStatusType.not_set;

	/// <summary>The time that the SCO begins.</summary>
	private float startTime;

	/// <summary>The time since the SCO began.</summary>
	private float currentTime;


	// GUI Elements set in teh editor.

	public GameObject PanelLearnerData;
	public GameObject PanelSCORMData;
	public GameObject PanelScore;
	public GameObject PanelObjectives;
	public GameObject PanelInteractions;
	public GameObject PanelExit;
	public GameObject LoadingPanel;
	public Transform AnObjective;
	public GameObject LogText;
	public GameObject TextTimer;
	

	/// <summary>Fires on Object's Unity3D Awake event, set the start time.</summary>
	void Awake () {
		startTime = Time.time;
	}



	/// <summary>Fires on Object's Unity3D Update event, calculate and display the current time in the timer GUI element.</summary>
	void Update () {
		currentTime = Time.time - startTime;
		TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
		TextTimer.GetComponent<Text>().text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	}

	/// <summary>
	/// Responds to the Scorm_Initialize_Complete message from the ScormManager
	/// </summary>
	/// <description>
	/// This is the point where you should 'kick off' the activity of your SCORM object.
	/// Make sure the SCORM object does not begin or pauses until you receive this message.
	/// </description>
	public void Scorm_Initialize_Complete() {

		StartCoroutine (Startup());
	}

	/// <summary>Allows a delay after the ScormManager finishes loading the StudentRecord from the LMS.</summary>
	/// <remarks>I hit a weird issue where certain GUI elements would not load correctly without a small delay.</remarks>
	public IEnumerator Startup() {
		yield return new WaitForSeconds(2);
		InitializeData ();
		SetStatusPanelLearnerData (true);
		SetStatusPanelSCORMData (false);
		SetStatusPanelScore (false);
		SetStatusPanelObjectves (false);
		SetStatusPanelInteractions (false);
		SetStatusPanelExit (false);
		LoadingPanel.SetActive (false);
	}

	/// <summary>
	/// Responds to the Scorm_Commit_Complete message from the ScormManager
	/// </summary>
	/// <description>
	/// This is the point where you should exit the Scorm object.
	/// Make sure the SCORM object does not exit until you receive this message.
	/// </description>
	public void Scorm_Commit_Complete() {
		ScormManager.Terminate ();
	}

	/// <summary>
	/// Log the specified data.
	/// </summary>
	/// <param name="data">Data.</param>
	public void Log(string data) {
		//UnityEngine.Application.ExternalCall("DebugPrint",data);						//Log into HTML Wrapper (don't use in production as it is messy, useful for testing in development)
		GameObject.Find ("LogText").GetComponent<Text> ().text += data + "\n";			//Log into Log Text in Unity3D App
	}

	/// <summary>
	/// Initializes the data into the GUI.
	/// </summary>
	private void InitializeData() {

		//Learner Data
		GameObject.Find ("TextID").GetComponent<Text> ().text = ScormManager.GetLearnerId ();
		GameObject.Find ("TextName").GetComponent<Text> ().text = ScormManager.GetLearnerName ();

		StudentRecord.LearnerPreference learnerPreference = ScormManager.GetLearnerPreference ();
		GameObject.Find ("InputFieldAudioCaptioning").GetComponent<InputField> ().text = learnerPreference.audioCaptioning.ToString ();
		GameObject.Find ("InputFieldAudioLevel").GetComponent<InputField> ().text = learnerPreference.audioLevel.ToString ();
		GameObject.Find ("InputFieldDeliverySpeed").GetComponent<InputField> ().text = learnerPreference.deliverySpeed.ToString ();
		GameObject.Find ("InputFieldLanguage").GetComponent<InputField> ().text = learnerPreference.langauge;

		LoadCommentFromLearnerList ();

		//SCORM Data
		GameObject.Find ("TextSCORMAPIVersion").GetComponent<Text> ().text = ScormManager.GetVersion ();

		GameObject.Find ("TextCredit").GetComponent<Text> ().text = ScormManager.GetCredit ().ToString();
		GameObject.Find ("TextEntry").GetComponent<Text> ().text = ScormManager.GetEntry ().ToString();
		GameObject.Find ("TextMode").GetComponent<Text> ().text = ScormManager.GetMode ().ToString();
		GameObject.Find ("TextLocation").GetComponent<Text> ().text = ScormManager.GetLocation ();
		GameObject.Find ("TextMaxTimeAllowed").GetComponent<Text> ().text = ScormManager.GetMaxTimeAllowed ().ToString();
		GameObject.Find ("TextTotalTime").GetComponent<Text> ().text = ScormManager.GetTotalTime ().ToString();
		GameObject.Find ("TextTimeLimitAction").GetComponent<Text> ().text = ScormManager.GetTimeLimitAction ().ToString();
		GameObject.Find ("TextLaunchData").GetComponent<Text> ().text = ScormManager.GetLaunchData ();

		LoadCommentFromLMSList ();

		//Score
		GameObject.Find ("TextScaledPassingScore").GetComponent<Text> ().text = ScormManager.GetScaledPassingScore ().ToString();

		StudentRecord.LearnerScore score = ScormManager.GetScore();
		GameObject.Find ("InputFieldMin").GetComponent<InputField> ().text = score.min.ToString();
		GameObject.Find ("InputFieldMax").GetComponent<InputField> ().text = score.max.ToString();
		GameObject.Find ("InputFieldScore").GetComponent<InputField> ().text = score.raw.ToString();

		GameObject.Find ("SliderScoreProgressMeasure").GetComponent<Slider> ().value = ScormManager.GetProgressMeasure();
		GameObject.Find ("LabelScoreProgressMeasureAmount").GetComponent<Text> ().text = ScormManager.GetProgressMeasure().ToString ();
		//Objectives
		LoadObjectives ();

		//Interactions
		LoadLearnerInteractions ();

		//Exit
		GameObject.Find ("TextCompletionThreshold").GetComponent<Text> ().text = ScormManager.GetCompletionThreshold ().ToString();
		switch(ScormManager.GetSuccessStatus()) {
		case StudentRecord.SuccessStatusType.passed:
			GameObject.Find("TogglePassedFinal").GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.SuccessStatusType.failed:
			GameObject.Find("ToggleFailedFinal").GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.SuccessStatusType.unknown:
			GameObject.Find("ToggleUnknownFinal").GetComponent<Toggle>().isOn = true;
			break;
		}
		
		switch(ScormManager.GetCompletionStatus()) {
		case StudentRecord.CompletionStatusType.completed:
			GameObject.Find("ToggleCompletedFinal").GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.CompletionStatusType.incomplete:
			GameObject.Find("ToggleIncompleteFinal").GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.CompletionStatusType.not_attempted:
			GameObject.Find("ToggleNotAttemptedFinal").GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.CompletionStatusType.unknown:
			GameObject.Find	("ToggleUnknownCompletedFinal").GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.CompletionStatusType.not_set:
			ScormManager.SetCompletionStatus (StudentRecord.CompletionStatusType.incomplete);  			// Set this first for every SCORM object, otherwise the LMS tends to treat it as a completed SCO by default
			GameObject.Find("ToggleIncompleteFinal").GetComponent<Toggle>().isOn = true;
			break;
		}
	}

	/// <summary>
	/// Adds the comment from learner to the list.
	/// </summary>
	/// <remarks>When a new comment is added via the GUI, we need to add it to the list in the GUI and a string</remarks>
	/// <param name="comment">Comment.</param>
	public void AddCommentFromLearnerToList(StudentRecord.CommentsFromLearner comment) {
		string stringCommentsFromLearnerList = "";
		string timeStamp = comment.timeStamp.ToString();
		string location = comment.location;
		string text = comment.comment;
		stringCommentsFromLearnerList += timeStamp + " :: " + location + "\n" + text + "\n\n";
		GameObject.Find ("LearnerCommentList").GetComponent<Text> ().text += stringCommentsFromLearnerList;
	}

	/// <summary>
	/// Loads the comment from learner into the list.
	/// </summary>
	/// <remarks>Get the List of Learner comments from SCORM and display it in the GUI as a string</remarks>
	public void LoadCommentFromLearnerList() {
		List<StudentRecord.CommentsFromLearner> commentsFromLearnerList = ScormManager.GetCommentsFromLearner ();
		string stringCommentsFromLearnerList = "";
		foreach (StudentRecord.CommentsFromLearner comment in commentsFromLearnerList) {
			string timeStamp = comment.timeStamp.ToString();
			string location = comment.location;
			string text = comment.comment;
			stringCommentsFromLearnerList += timeStamp + " :: " + location + "\n" + text + "\n\n";
		}
		GameObject.Find ("LearnerCommentList").GetComponent<Text> ().text = stringCommentsFromLearnerList;
	}

	/// <summary>
	/// Loads the comment from LMS to the list.
	/// </summary>
	/// <remarks>Get the List of LMS comments from SCORM and display it in the GUI as a string</remarks>
	public void LoadCommentFromLMSList() {
		List<StudentRecord.CommentsFromLMS> commentsFromLMSList = ScormManager.GetCommentsFromLMS ();
		string stringCommentsFromLMSList = "";
		foreach (StudentRecord.CommentsFromLMS comment in commentsFromLMSList) {
			string timeStamp = comment.timeStamp.ToString();
			string location = comment.location;
			string text = comment.comment;
			stringCommentsFromLMSList += timeStamp + " :: " + location + "\n" + text + "\n\n";
		}
		GameObject.Find ("TextCommentsFromLMS").GetComponent<Text> ().text = stringCommentsFromLMSList;
	}

	/// <summary>
	/// Adds the learner interaction to the list.
	/// </summary>
	/// /// <remarks>When a new interaction is added via the GUI, we need to add it to the list in the GUI</remarks>
	/// <param name="record">Record.</param>
	public void AddLearnerInteractionToList (StudentRecord.LearnerInteractionRecord record) {
		string stringInteractionsList = "";

		string timeStamp = record.timeStamp.ToString();													// Get the Interaction data as strings
		String id = record.id;
		string type = record.type.ToString();
		string weighting = record.weighting.ToString();
		string latency = record.latency.ToString();
		string description = record.description;
		string response = record.response;

		string result = record.result.ToString();														// If the Interaction result is set as the 'estimate', the result needs to be set to the numeric estimate value
		if(record.result == StudentRecord.ResultType.estimate) {
			result = record.estimate.ToString();
		}
		
		string stringInteractionObjectivesList = "";													// Load the list of objectives and format it into a string
		if(record.objectives != null) {
			List<StudentRecord.LearnerInteractionObjective> interactionObjectivesList = record.objectives;
			if(interactionObjectivesList.Count > 0) {
				string stringObjectiveIds = "";
				foreach(StudentRecord.LearnerInteractionObjective singleObjective in interactionObjectivesList) {
					stringObjectiveIds += singleObjective.id + " ";
				}
				stringInteractionObjectivesList = "Objectives ("+ stringObjectiveIds+")\n";
			}
		}
		
		string stringInteractionCorrectResponseList = "";												// Load the list of correct response patterns and format it into a string
		if(record.correctResponses != null) {
			List<StudentRecord.LearnerInteractionCorrectResponse> interactionCorrectResponseList = record.correctResponses;
			if(interactionCorrectResponseList.Count > 0) {
				string stringPatterns = "";
				foreach(StudentRecord.LearnerInteractionCorrectResponse singleCorrectResponse in interactionCorrectResponseList) {
					stringPatterns += singleCorrectResponse.pattern + " ";
				}
				stringInteractionCorrectResponseList = "Correct Response Patterns ("+ stringPatterns+")\n";
			}
		}

		stringInteractionsList += timeStamp + " :: " + id + "\n" + description + "\n(type: "+type+" weighting: "+weighting+" latency: "+latency+"secs)\nResponse: "+response+" Result: "+result+"\n"+stringInteractionCorrectResponseList+stringInteractionObjectivesList+"\n";		// Format the entire Interaction data set as a string and add to the GUI element
		GameObject.Find ("InteractionList").GetComponent<Text> ().text += stringInteractionsList;
	}

	/// <summary>
	/// Loads the learner interactions.
	/// </summary>
	/// <remarks>Load the learner interactions from SCORM and present as a formatted string to the GUI.</remarks>
	public void LoadLearnerInteractions() {
		List<StudentRecord.LearnerInteractionRecord> interactionsList = ScormManager.GetInteractions ();
		string stringInteractionsList = "";

		foreach (StudentRecord.LearnerInteractionRecord record in interactionsList) {
			string timeStamp = record.timeStamp.ToString();												// Get the Interaction data as strings
			String id = record.id;
			string type = record.type.ToString();
			string weighting = record.weighting.ToString();
			string latency = record.latency.ToString();
			string description = record.description;
			string response = record.response;
			
			string result = record.result.ToString();													// If the Interaction result is set as the 'estimate', the result needs to be set to the numeric estimate value
			if(record.result == StudentRecord.ResultType.estimate) {
				result = record.estimate.ToString();
			}
			
			string stringInteractionObjectivesList = "";												// Load the list of objectives and format it into a string
			if(record.objectives != null) {
				List<StudentRecord.LearnerInteractionObjective> interactionObjectivesList = record.objectives;
				if(interactionObjectivesList.Count > 0) {
					string stringObjectiveIds = "";
					foreach(StudentRecord.LearnerInteractionObjective singleObjective in interactionObjectivesList) {
						stringObjectiveIds += singleObjective.id + " ";
					}
					stringInteractionObjectivesList = "Objectives ("+ stringObjectiveIds+")\n";
				}
			}
			
			string stringInteractionCorrectResponseList = "";											// Load the list of correct response patterns and format it into a string
			if(record.correctResponses != null) {
				List<StudentRecord.LearnerInteractionCorrectResponse> interactionCorrectResponseList = record.correctResponses;
				if(interactionCorrectResponseList.Count > 0) {
					string stringPatterns = "";
					foreach(StudentRecord.LearnerInteractionCorrectResponse singleCorrectResponse in interactionCorrectResponseList) {
						stringPatterns += singleCorrectResponse.pattern + " ";
					}
					stringInteractionCorrectResponseList = "Correct Response Patterns ("+ stringPatterns+")\n";
				}
			}

			stringInteractionsList += timeStamp + " :: " + id + "\n" + description + "\n(type: "+type+" weighting: "+weighting+" latency: "+latency+"secs)\nResponse: "+response+" Result: "+result+"\n"+stringInteractionCorrectResponseList+stringInteractionObjectivesList+"\n";		// Format the entire Interaction data set as a string and add to the GUI element
		}
		GameObject.Find ("InteractionList").GetComponent<Text> ().text = stringInteractionsList;
	}

	/// <summary>
	/// Loads the objectives from SCORM.
	/// </summary>
	/// <remarks>Load the objectives from SCORM and add a new 'AnObjective' prefab for each objective</remarks>
	private void LoadObjectives() {
		Transform panelObjectivesList = GameObject.Find ("PanelObjectivesList").transform;
		
		int count = 0;
		List<StudentRecord.Objectives> objectivesList = ScormManager.GetObjectives ();
		foreach (StudentRecord.Objectives objective in objectivesList) {
			Transform NewObjectve = Instantiate(AnObjective);
			NewObjectve.SetParent (panelObjectivesList);
			NewObjectve.GetComponent<AnObjective>().Initialise(count,objective);
			count++;
		}
	}

	/// <summary>
	/// Adds the objective to list.
	/// </summary>
	/// <remarks>When a new objective is added via the GUI, we need to add a new 'AnObjective' prefab.</remarks>
	/// <param name="index">Index.</param>
	/// <param name="newObjective">New objective.</param>
	private void AddObjectiveToList (int index, StudentRecord.Objectives newObjective) {
		Transform panelObjectivesList = GameObject.Find ("PanelObjectivesList").transform;
		Transform transformNewObjectve = Instantiate(AnObjective);
		transformNewObjectve.SetParent (panelObjectivesList);
		transformNewObjectve.GetComponent<AnObjective>().Initialise(index,newObjective);
	}

	/// <summary>
	/// The exit SCORM button is pressed.
	/// </summary>
	public void ButtonExitSCORMPressed() {
		ScormManager.SetSessionTime (currentTime);								// Set this sessions time from the timer
		ScormManager.SetExit (StudentRecord.ExitType.normal);					// In Blackboard, this will complete the SCO attenmpt, even if the Completion Status is not set to completed.
		ScormManager.Commit ();
	}

	/// <summary>
	/// The progress measure change event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnProgressMeasureChange(float value) {
		GameObject.Find ("LabelScoreProgressMeasureAmount").GetComponent<Text> ().text = value.ToString ();
		ScormManager.SetProgressMeasure (value);
	}

	/// <summary>
	/// The learner score minimum edit end event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnLearnerScoreMinEditEnd(string value) {
		ScormManager.SetScoreMin(float.Parse (value));
	}

	/// <summary>
	/// The learner score max edit end event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnLearnerScoreMaxEditEnd(string value) {
		ScormManager.SetScoreMax(float.Parse (value));
		float scoreScaled = float.Parse (GameObject.Find ("InputFieldScore").GetComponent<InputField> ().text) / float.Parse (value);
		ScormManager.SetScoreScaled (scoreScaled);
	}

	/// <summary>
	/// The learner score raw edit end event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnLearnerScoreRawEditEnd(string value) {
		ScormManager.SetScoreRaw(float.Parse (value));
		float scoreScaled = float.Parse (value) / float.Parse (GameObject.Find ("InputFieldMax").GetComponent<InputField> ().text);
		ScormManager.SetScoreScaled (scoreScaled);
	}

	/// <summary>
	/// The learner preference audio captioning end edit event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnLearnerPreferenceAudioCaptioningEndEdit(string value) {
		ScormManager.SetLearnerPreferenceAudioCaptioning (int.Parse (value));
	}

	/// <summary>
	/// The learner preference audio level end edit event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnLearnerPreferenceAudioLevelEndEdit(string value) {
		ScormManager.SetLearnerPreferenceAudioLevel (float.Parse (value));
	}

	/// <summary>
	/// The learner preference delivery speed end edit event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnLearnerPreferenceDeliverySpeedEndEdit(string value) {
		ScormManager.SetLearnerPreferenceDeliverySpeed (float.Parse (value));
	}

	/// <summary>
	/// The learner preference language end edit event.
	/// </summary>
	/// <param name="value">Value.</param>
	public void OnLearnerPreferenceLanguageEndEdit(string value) {
		ScormManager.SetLearnerPreferenceLanguage (value);
	}

	/// <summary>
	/// The add comment button pressed.
	/// </summary>
	/// <remarks>Validates the input GUI elements are not empty, then adds a new comment to SCORM and the GUI</remarks>
	public void ButtonAddCommentPressed() {

		GameObject inputFieldComment = GameObject.Find ("InputFieldComment");
		GameObject inputFieldCommentLocation = GameObject.Find ("InputFieldCommentLocation");

		string commentText = inputFieldComment.GetComponent<InputField> ().text;
		string commentLocation = inputFieldCommentLocation.GetComponent<InputField> ().text;

		if (commentText == "" | commentLocation == "") {												// Validate the Input Elements
			if (commentText == "") {
				inputFieldComment.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter a comment!";
			}
			if (commentLocation == "") {
				inputFieldCommentLocation.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter a location!";
			}
		} else {																						// Add the Comment
			StudentRecord.CommentsFromLearner comment = new StudentRecord.CommentsFromLearner();
			comment.comment = commentText;
			comment.location = commentLocation;
		
			ScormManager.AddCommentFromLearner(comment);
			comment.timeStamp = DateTime.Now;

			AddCommentFromLearnerToList(comment);

			//Reset Fields
			inputFieldComment.GetComponent<InputField> ().text = "";
			inputFieldCommentLocation.GetComponent<InputField> ().text = "";
			inputFieldComment.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "New Learner Comment...";
			inputFieldCommentLocation.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "Comment Location...";
		}
	}

	/// <summary>
	/// The add objective button pressed.
	/// </summary>
	/// <remarks>Validates the input GUI elements are not empty, then adds a new objective to SCORM and the GUI</remarks>
	public void ButtonAddObjectivePressed() {
		GameObject inputFieldObjectiveId = GameObject.Find ("InputFieldObjectiveId");
		GameObject inputFieldObjectiveDescription = GameObject.Find ("InputFieldObjectiveDescription");
		
		string objectiveId = inputFieldObjectiveId.GetComponent<InputField> ().text;
		string objectiveDescription = inputFieldObjectiveDescription.GetComponent<InputField> ().text;
		
		if (objectiveId == "" | objectiveDescription == "") {												// Validate the Input Elements
			if (objectiveId == "") {
				inputFieldObjectiveId.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter an ID!";
			}
			if (objectiveDescription == "") {
				inputFieldObjectiveDescription.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter a description!";
			}
		} else {																							// Add the Objective
			StudentRecord.Objectives newObjective = new StudentRecord.Objectives();
			newObjective.id = objectiveId;
			newObjective.description = objectiveDescription;

			StudentRecord.LearnerScore score = new StudentRecord.LearnerScore();
			score.min = 0f;
			score.max = 100f;
			score.raw = 0f;
			score.scaled = 0f;

			newObjective.score = score;
			newObjective.successStatus = StudentRecord.SuccessStatusType.unknown;
			newObjective.completionStatus = StudentRecord.CompletionStatusType.not_attempted;
			newObjective.progressMeasure = 0f;

			ScormManager.AddObjective(newObjective);
			int index = ScormManager.GetObjectives().Count;
			AddObjectiveToList(index, newObjective);
			
			//Reset Fields
			inputFieldObjectiveId.GetComponent<InputField> ().text = "";
			inputFieldObjectiveDescription.GetComponent<InputField> ().text = "";
			inputFieldObjectiveId.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "Objective ID...";
			inputFieldObjectiveDescription.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "Objective Description...";
		}
	}

	/// <summary>
	/// The add interaction button pressed.
	/// </summary>
	/// <remarks>Validates the input GUI elements are not empty, then adds a new interaction to SCORM and the GUI</remarks>
	public void ButtonAddInteractionPressed() {
		GameObject inputFieldDescription = GameObject.Find("InputFieldDescription");
		GameObject inputFieldWeighting = GameObject.Find("InputFieldWeighting");
		GameObject inputFieldResponse = GameObject.Find("InputFieldResponse");
		GameObject toggleCorrect = GameObject.Find("ToggleCorrect");

		string description = inputFieldDescription.GetComponent<InputField>().text;
		string weighting = inputFieldWeighting.GetComponent<InputField>().text;
		string studentResponse = inputFieldResponse.GetComponent<InputField>().text;
		bool correct = toggleCorrect.GetComponent<Toggle>().isOn;

		if (description == "" | weighting == "" | studentResponse == "") {										// Validate the Input Elements
			if (description == "") {
				inputFieldDescription.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter a description!";
			}
			if (weighting == "") {
				inputFieldWeighting.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter a weighting!";
			}
			if (studentResponse == "") {
				inputFieldResponse.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter a student response!";
			}
		} else {																								// Add the Interaction
			StudentRecord.LearnerInteractionRecord newInteraction = new StudentRecord.LearnerInteractionRecord ();
			newInteraction.id = ScormManager.GetNextInteractionId();
			newInteraction.timeStamp = DateTime.Now;
			newInteraction.type = StudentRecord.InteractionType.other;
			newInteraction.weighting = float.Parse(weighting);
			newInteraction.response = studentResponse;
			newInteraction.latency = 18.4f;
			newInteraction.description = description;
			StudentRecord.ResultType result = StudentRecord.ResultType.incorrect;
			if (correct) {
				result = StudentRecord.ResultType.correct;
			}
			newInteraction.result = result;

			ScormManager.AddInteraction (newInteraction);
			AddLearnerInteractionToList (newInteraction);

			//Reset
			inputFieldDescription.GetComponent<InputField>().text = "";
			inputFieldWeighting.GetComponent<InputField>().text = "";
			inputFieldResponse.GetComponent<InputField>().text = "";
			toggleCorrect.GetComponent<Toggle>().isOn = true;
			inputFieldDescription.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "Description...";
			inputFieldWeighting.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "Weighting...";
			inputFieldResponse.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "Student response...";
		}

	}

	/// <summary>
	/// The set location button pressed.
	/// </summary>
	/// <remarks>Set the Location (Bookmark) via SCORM and exit with a status of 'suspend'.\n
	/// This allows the LMS to launch the SCO without starting a new attempt.</remarks>
	public void ButtonSetLocationPressed() {
		GameObject inputFieldLocation = GameObject.Find("InputFieldLocation");
		string location = inputFieldLocation.GetComponent<InputField>().text;

		if (location == "") {
			inputFieldLocation.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "You must enter a location!";
		} else {
			//Reset
			inputFieldLocation.GetComponent<InputField>().text = "";
			inputFieldLocation.transform.FindChild ("Placeholder").gameObject.GetComponent<Text> ().text = "Location string...";

			ScormManager.SetSessionTime (currentTime);
			ScormManager.SetLocation (location);
			ScormManager.SetExit (StudentRecord.ExitType.suspend);
			ScormManager.Commit ();
		}
	}

	/// <summary>
	/// The toggle completed final change event.
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void OnToggleCompletedFinalChange(bool value) {
		if (value) {
			ScormManager.SetCompletionStatus (StudentRecord.CompletionStatusType.completed);
		}
	}

	/// <summary>
	/// The toggle incomplete final change event.
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void OnToggleIncompleteFinalChange(bool value) {
		if (value) {
			ScormManager.SetCompletionStatus (StudentRecord.CompletionStatusType.incomplete);
		}
	}

	/// <summary>
	/// The toggle not attempted final change event.
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void OnToggleNotAttemptedFinalChange(bool value) {
		if (value) {
			ScormManager.SetCompletionStatus (StudentRecord.CompletionStatusType.not_attempted);
		}
	}

	/// <summary>
	/// The toggle unknown completed final change event.
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void OnToggleUnknownCompletedFinalChange(bool value) {
		if (value) {
			ScormManager.SetCompletionStatus (StudentRecord.CompletionStatusType.unknown);
		}
	}

	/// <summary>
	/// The toggle passed final change event.
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void OnTogglePassedFinalChange(bool value) {
		if (value) {
			ScormManager.SetSuccessStatus (StudentRecord.SuccessStatusType.passed);
		}
	}

	/// <summary>
	/// The toggle failed final change event.
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void OnToggleFailedFinalChange(bool value) {
		if (value) {
			ScormManager.SetSuccessStatus (StudentRecord.SuccessStatusType.failed);
		}	
	}

	/// <summary>
	/// The toggle unknown final change event.
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void OnToggleUnknownFinalChange(bool value) {
		if (value) {
			ScormManager.SetSuccessStatus (StudentRecord.SuccessStatusType.unknown);
		}	
	}

	/// <summary>
	/// The learner data button pressed.
	/// </summary>
	/// <remarks>Enables the appropriate panel in the GUI</remarks>
	public void ButtonLearnerDataPressed() {
		SetStatusPanelLearnerData (true);
		SetStatusPanelSCORMData (false);
		SetStatusPanelScore (false);
		SetStatusPanelObjectves (false);
		SetStatusPanelInteractions (false);
		SetStatusPanelExit (false);
	}

	/// <summary>
	/// The Scorm data button pressed.
	/// </summary>
	/// <remarks>Enables the appropriate panel in the GUI</remarks>
	public void ButtonScormDataPressed() {
		SetStatusPanelLearnerData (false);
		SetStatusPanelSCORMData (true);
		SetStatusPanelScore (false);
		SetStatusPanelObjectves (false);
		SetStatusPanelInteractions (false);
		SetStatusPanelExit (false);
	}

	/// <summary>
	/// The Score button pressed.
	/// </summary>
	/// <remarks>Enables the appropriate panel in the GUI</remarks>
	public void ButtonScorePressed() {
		SetStatusPanelLearnerData (false);
		SetStatusPanelSCORMData (false);
		SetStatusPanelScore (true);
		SetStatusPanelObjectves (false);
		SetStatusPanelInteractions (false);
		SetStatusPanelExit (false);
	}

	/// <summary>
	/// The Objectives button pressed.
	/// </summary>
	/// <remarks>Enables the appropriate panel in the GUI</remarks>
	public void ButtonObjectivesPressed() {
		SetStatusPanelLearnerData (false);
		SetStatusPanelSCORMData (false);
		SetStatusPanelScore (false);
		SetStatusPanelObjectves (true);
		SetStatusPanelInteractions (false);
		SetStatusPanelExit (false);
	}

	/// <summary>
	/// The Interactions button pressed.
	/// </summary>
	/// <remarks>Enables the appropriate panel in the GUI</remarks>
	public void ButtonInteractionsPressed() {
		SetStatusPanelLearnerData (false);
		SetStatusPanelSCORMData (false);
		SetStatusPanelScore (false);
		SetStatusPanelObjectves (false);
		SetStatusPanelInteractions (true);
		SetStatusPanelExit (false);
	}

	/// <summary>
	/// The Exit button pressed.
	/// </summary>
	/// <remarks>Enables the appropriate panel in the GUI</remarks>
	public void ButtonExitPressed() {
		SetStatusPanelLearnerData (false);
		SetStatusPanelSCORMData (false);
		SetStatusPanelScore (false);
		SetStatusPanelObjectves (false);
		SetStatusPanelInteractions (false);
		SetStatusPanelExit (true);
	}

	/// <summary>
	/// Sets the active status of the learner data panel.
	/// </summary>
	/// <param name="status">status (bool)</param>
	void SetStatusPanelLearnerData(bool status) {
		PanelLearnerData.SetActive (status);
	}

	/// <summary>
	/// Sets the active status of the Scorm data panel.
	/// </summary>
	/// <param name="status">status (bool)</param>
	void SetStatusPanelSCORMData(bool status) {
		PanelSCORMData.SetActive (status);
	}

	/// <summary>
	/// Sets the active status of the score panel.
	/// </summary>
	/// <param name="status">status (bool)</param>
	void SetStatusPanelScore(bool status) {
		PanelScore.SetActive (status);
	}

	/// <summary>
	/// Sets the active status of the Objectives panel.
	/// </summary>
	/// <param name="status">status (bool)</param>
	void SetStatusPanelObjectves(bool status) {
		PanelObjectives.SetActive (status);
	}

	/// <summary>
	/// Sets the active status of the Interactions panel.
	/// </summary>
	/// <param name="status">status (bool)</param>
	void SetStatusPanelInteractions(bool status) {
		PanelInteractions.SetActive (status);
	}

	/// <summary>
	/// Sets the active status of the Exit panel.
	/// </summary>
	/// <param name="status">status (bool)</param>
	void SetStatusPanelExit(bool status) {
		PanelExit.SetActive (status);
	}
}
