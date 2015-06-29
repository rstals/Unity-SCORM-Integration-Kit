/***********************************************************************************************************************
 * Unity-SCORM Integration Kit
 * 
 * Example Use of the Unity-SCORM Integration Kit via calls to the ScormManager (Manages the Objectives)
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
/// Allows the display of Objectives in the GUI.
/// </summary>
public class AnObjective : MonoBehaviour {

	// These hold the references to the GUI elements that match the various Objective data items.  These need to be set in the editor.
	// The example scene uses the AnObjective prefab in which these are all pre-set for you.

	public GameObject objectiveIndexGO;
	public GameObject objectiveIDGO;
	public GameObject objectiveDescriptionGO;
	public GameObject objectiveScoreMinGO;
	public GameObject objectiveScoreMaxGO;
	public GameObject objectiveScoreRawGO;
	public GameObject objectiveProgressMeasureGO;
	public GameObject objectiveProgressMeasureLabelGO;
	public GameObject objectiveSuccessPassedGO;
	public GameObject objectiveSuccessFailedGO;
	public GameObject objectiveSuccessUnknownGO;
	public GameObject objectiveCompletedGO;
	public GameObject objectiveIncompleteGO;
	public GameObject objectiveNotAttemptedGO;
	public GameObject objectiveUnknownGO;

	/// <summary>The index of this Objective that matches the position in the StudentRecord.objectives.</summary>
	private int index;

	/// <summary>The data of this objective.</summary>
	private StudentRecord.Objectives data;

	/// <summary>Is this object initialising?</summary>
	/// <remarks>Used to prevent any updates while the panel is loading the objective data.</remarks>
	private bool isInitialising = false;

	/// <summary>
	/// Initialise the specified objective.
	/// </summary>
	/// <param name="newIndex">New index.</param>
	/// <param name="newData">New data of the objective.</param>
	public void Initialise(int newIndex, StudentRecord.Objectives newData) {
		isInitialising = true;																	// Set the isInitialising flag to true (stops the updating of the Objective while the data is loading

		index = newIndex;
		data = newData;

		objectiveIndexGO.GetComponent<Text> ().text = index.ToString ();						// Set the text of the Objective Index GUI Element
		objectiveIDGO.GetComponent<Text> ().text = newData.id;									// Set the text of the Objective ID GUI Element
		objectiveDescriptionGO.GetComponent<Text> ().text = newData.description;				// Set the text of the Objective Description GUI Element

		objectiveScoreMinGO.GetComponent<InputField> ().text = newData.score.min.ToString();	// Set the text of the Min Score GUI Element
		objectiveScoreMaxGO.GetComponent<InputField> ().text = newData.score.max.ToString();	// Set the text of the Max Score GUI Element
		objectiveScoreRawGO.GetComponent<InputField> ().text = newData.score.raw.ToString();	// Set the text of the Raw Score GUI Element

		objectiveProgressMeasureGO.GetComponent<Slider> ().value = newData.progressMeasure;		// Set the text of the Progress Measure GUI Element

		switch(newData.successStatus) {															// Set the correct Success Status toggle
		case StudentRecord.SuccessStatusType.passed:
			objectiveSuccessPassedGO.GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.SuccessStatusType.failed:
			objectiveSuccessFailedGO.GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.SuccessStatusType.unknown:
			objectiveSuccessUnknownGO.GetComponent<Toggle>().isOn = true;
			break;
		}
		
		switch(newData.completionStatus) {														// Set the correct Completion Status toggle
		case StudentRecord.CompletionStatusType.completed:
			objectiveCompletedGO.GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.CompletionStatusType.incomplete:
			objectiveIncompleteGO.GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.CompletionStatusType.not_attempted:
			objectiveNotAttemptedGO.GetComponent<Toggle>().isOn = true;
			break;
		case StudentRecord.CompletionStatusType.unknown:
			objectiveUnknownGO.GetComponent<Toggle>().isOn = true;
			break;
		}

		ResizeParent ();																		// I could not get the Content Size Fitter working correctly on the PanelObjectivesList GUI element, calculated manually
		isInitialising = false;																	// Initialise complete, update reactivated.
	}

	/// <summary>
	/// Resizes the parent container element every time a new item is added.
	/// </summary>
	private void ResizeParent() {
		Transform parent = this.transform.parent;												// The PanelObjectivesList GUI element

		int count = 0;																			// Count the number of Objective panels
		foreach (Transform child in parent) {
			count++;
		}

		float height = count * 320f;															// Calculate and set the height of the PanelObjectivesList GUI element
		parent.GetComponent<RectTransform> ().sizeDelta = new Vector2 (492f, height);
	}

	/// <summary>
	/// Fires when the InputFieldMinObjective InputField has finished editing
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Score Min Input Field.</param>
	public void OnMinEndEdit (string value) {
		if (!isInitialising) {
			data.score.min = float.Parse (value);
			ScormManager.UpdateObjective (index, data);
		}
	}

	/// <summary>
	/// Fires when the InputFieldMaxObjective InputField has finished editing
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Score Max Input Field.</param>
	public void OnMaxEndEdit (string value) {
		if (!isInitialising) {
			data.score.max = float.Parse (value);
			ScormManager.UpdateObjective (index, data);
		}
	}

	/// <summary>
	/// Fires when the InputFieldRawObjective InputField has finished editing
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Score Raw Input Field.</param>
	public void OnRawEndEdit (string value) {
		if (!isInitialising) {
			data.score.raw = float.Parse (value);
			data.score.scaled = float.Parse (value) / data.score.max;
			ScormManager.UpdateObjective (index, data);
		}
	}

	/// <summary>
	/// Fires when the SliderProgressMeasureObjective Slider value changes
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager.  Also updates the label to display the Slider value.</remarks>
	/// <param name="value">Value of the Progress Measure Slider.</param>
	public void OnProgressMeasureChange(float value) {
		objectiveProgressMeasureLabelGO.GetComponent<Text> ().text = value.ToString ();
		if (!isInitialising) {
			data.progressMeasure = value;
			ScormManager.UpdateObjective (index, data);
		}
	}

	/// <summary>
	/// Fires when the ToggleGroupSuccessStatus Passed toggle value changes.
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Toggle.</param>
	public void OnSuccessPassedChanged (bool value) {
		if (!isInitialising) {
			if (value) {
				data.successStatus = StudentRecord.SuccessStatusType.passed;
				ScormManager.UpdateObjective (index, data);
			}
		}
	}

	/// <summary>
	/// Fires when the ToggleGroupSuccessStatus Failed toggle value changes.
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Toggle.</param>
	public void OnSuccessFailedChanged (bool value) {
		if (!isInitialising) {
			if (value) {
				data.successStatus = StudentRecord.SuccessStatusType.failed;
				ScormManager.UpdateObjective (index, data);
			}
		}
	}

	/// <summary>
	/// Fires when the ToggleGroupSuccessStatus Unknown toggle value changes.
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Toggle.</param>
	public void OnSuccessUnknownChanged (bool value) {
		if (!isInitialising) {
			if (value) {
				data.successStatus = StudentRecord.SuccessStatusType.unknown;
				ScormManager.UpdateObjective (index, data);
			}
		}
	}

	/// <summary>
	/// Fires when the ToggleGroupCompletionStatus Completed toggle value changes.
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Toggle.</param>
	public void OnCompletionCompletedChanged (bool value) {
		if (!isInitialising) {
			if (value) {
				data.completionStatus = StudentRecord.CompletionStatusType.completed;
				ScormManager.UpdateObjective (index, data);
			}
		}
	}

	/// <summary>
	/// Fires when the ToggleGroupCompletionStatus Incomplete toggle value changes.
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Toggle.</param>
	public void OnCompletionIncompleteChanged (bool value) {
		if (!isInitialising) {
			if (value) {
				data.completionStatus = StudentRecord.CompletionStatusType.incomplete;
				ScormManager.UpdateObjective (index, data);
			}
		}
	}

	/// <summary>
	/// Fires when the ToggleGroupCompletionStatus NotAttempted toggle value changes.
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Toggle.</param>
	public void OnCompletionNotAttemptedChanged (bool value) {
		if (!isInitialising) {
			if (value) {
				data.completionStatus = StudentRecord.CompletionStatusType.not_attempted;
				ScormManager.UpdateObjective (index, data);
			}
		}
	}

	/// <summary>
	/// Fires when the ToggleGroupCompletionStatus Unknown toggle value changes.
	/// </summary>
	/// <remarks>Update the data and update the objective via the ScormManager</remarks>
	/// <param name="value">Value of the Toggle.</param>
	public void OnCompletionUnknownChanged (bool value) {
		if (!isInitialising) {
			if (value) {
				data.completionStatus = StudentRecord.CompletionStatusType.unknown;
				ScormManager.UpdateObjective (index, data);
			}
		}
	}

}
