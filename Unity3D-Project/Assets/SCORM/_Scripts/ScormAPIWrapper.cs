/***********************************************************************************************************************
 * Unity-SCORM Integration Kit
 * 
 * Unity-SCORM Integration Wrapper (Bridge)
 * 
 * Copyright (C) 2015, Richard Stals (http://stals.com.au)
 * ==========================================
 * 
 * 
 * Derived from:
 * Unity-SCORM Integration Toolkit Version 1.0 Beta
 * ==========================================
 *
 * Copyright (C) 2011, by ADL (Advance Distributed Learning). (http://www.adlnet.gov)
 * http://www.adlnet.gov/UnityScormIntegration/
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


using System;
using System.Collections.Generic;

/// <summary>
/// Scorm API wrapper.  Forms the 'Bridge' between the Unity3D code and the scorm.js code that communicates with teh LMS.
/// </summary>
/// <remarks>
/// In order for Unity3D to communicate with the LMS, it needs to do so via the scorm.js code located in the HTML that 'wraps' the Unity3D webplayer.
/// Unity allows us to run extrenal calls to the enclosing HTML wrapper via UnityEngine.Application.ExternalCall().  In turn, the HTML wrapper can call Unity3D function via GetUnity().SendMessage().
/// However, this kind of communication is asynchronous, which makes it difficult to get return values from a call to the HTML wrapper back into the calling Unity3D function.
/// (E.g. if you call a GetValue() to the LMS via scorm.js, how do you get the result?
/// 
/// This ScormAPI wrapper class allows calls to scorm.js appear to be synchronous by doing the following:
/// 
/// 1. When sending a call to scorm.js in the HTML wrapper, you give it the following information:
/// 	a. The CallbackObjectName - the name of the Unity3D object that the 'reply' to the call should be sent.  This is set in the ScormManager class as the ScormManager object.
/// 	b. The CallbackFunctionName - the name of the function in the ScormManager (CallbackObjectName) that responds to the the 'reply' from teh HTML wrapper.
/// 	c. The key - a unique key that identifies the function that sent the call.
/// 
/// 2. The HTML wrapper (scorm.js) processes the call, and then sends the result to Unity3D (the callback function in ScormManager as defined by the CallbackObjectName and CallbackFunctionName).
/// It also send the key back, which allows the ScormManager Callback Function to assign the return result back to the correct calling function.
/// 
/// </remarks>
public class ScormAPIWrapper {


	/// <summary>This holds the values of the result of the SCORM API Callback from scorm.js</summary>
	public class APICallResult {
		/// <summary>The string result of the call to the SCORM API</summary>
		public string Result;
		/// <summary>The random key assigned to the call to uniquely identify it for the callback</summary>
		public string Key;
		/// <summary>Possible error code</summary>
		public string ErrorCode;
		/// <summary>Possible error description</summary>
		public string ErrorDescription;
	}

	/// <summary>Timout for waiting for a reply to the SCORM API</summary>
	int TimeToWaitForReply = 15000;

	/// <summary>How long to wait between each poll to check for a reply from the SCORM API</summary>
	int TimePerPoll = 10;

	/// <summary>The name of the Unity3D object (Should be assigned as ScormManager)</summary>
	string CallbackObjectName;

	/// <summary>The function name in the CallbackObjectName object that scorm.js calls with the API call result (Should be assign as ScormValueCallback in ScormManager</summary>
	string CallbackFunctionName;

	/// <summary>Holds the callback values</summary>
	Dictionary<int, APICallResult> CallbackValues;

	/// <summary>Random number generator to create random keys for CallbackValues</summary>
	Random random;

	/// <summary>Holds the queue of responses from API call results</summary>
	Queue<string> ResponseQueue;

	/// <summary>Allows interoperability bewteen SCORM 2004 and 1.2 (Not yet implemented)</summary>
	public bool IsScorm2004;


	/// <summary>
	/// Initializes a new instance of the <see cref="ScormAPIWrapper"/> class.
	/// </summary>
	/// <param name="obj">Callback object name.</param>
	/// <param name="callback">Callback function name.</param>
	public ScormAPIWrapper(string obj, string callback) {
		CallbackObjectName = obj;
		CallbackFunctionName = callback;
		CallbackValues = new Dictionary<int, APICallResult>();
		random = new Random();
		ResponseQueue = new Queue<string>();		
	}

	/// <summary>
	/// Start up the SCORM API Wrapper.
	/// </summary>
	/// <remarks>
	/// This will check with the Javascript layer to see if it's executing in a 1.2 or 2004 LMS
	/// </remarks>
	public void Initialize() 	{
		IsScorm2004 = true;
		
		int key = SetupCallback();
		UnityEngine.Application.ExternalCall("doIsScorm2004",new object[] {CallbackObjectName, CallbackFunctionName, key });
		
		string result = WaitForReturn(key).Result;
		IsScorm2004 = System.Convert.ToBoolean(result);
		if(IsScorm2004)
			Log("ScormVersion is 2004");
		else
			Log("ScormVersion is 1.2");
	}

	/// <summary>
	/// Get a random key value not currently used by the list of waiting commands.
	/// </summary>
	/// <remarks>
	/// the key is used to associate a javascript command with the message that responds to it.
	/// </remarks>
	/// <returns>
	/// A key value that is not currently being used in the map.
	/// </returns>
	int GetRandomKey() {		
		int key = random.Next(65536);
		while (CallbackValues.ContainsKey(key))
			key = random.Next(65536);
		return key;		
	}

	/// <summary>
	/// Create a callback with a unique key and add it to the map.
	/// </summary>
	/// <remarks>
	/// the key is used to associate a javascript command with the message that responds to it.
	/// </remarks>
	/// <returns>
	/// A key value that is not used in the map previously.
	/// </returns>
	int SetupCallback() 	{
		lock (CallbackValues) {
			int key = GetRandomKey();
			CallbackValues[key] = null;
			return key;
		}
	}

	/// <summary>
	/// Wait until the ScormManager calls the object and inserts the results of a javascript message
	/// </summary>
	/// <remarks>
	/// the key is used to associate a javascript command with the message that responds to it. This key is
	/// sent into the javascript layer, and sent back to the ScormManager as part of the response. The 
	/// ScormManager inserts the message value into the queue. After this, the thread sleeps a bit (the TimePerPoll value), it checks
	/// the queue to see if it got an answer to this request. If so, you get back the value of that message.
	/// This all happens to allow the ScormManager to pretend that the set functions it calls on the Scorm API Wrapper
	/// are synchronous.
	/// </remarks>
	/// <returns>
	/// The return of the javascript commands associated with this key.
	/// </returns>
	APICallResult WaitForReturn(int key) {
		int timeout = 0;
		bool wait = true;

		lock (CallbackValues) {
			wait = CallbackValues[key] == null && timeout < TimeToWaitForReply;		//Wait = true if the CallbackValues for this call is empty (not yet processed or timeout is reached
		}

		while (wait) {
			processQueue();															//Look for any queued results that match this call's key
			lock (CallbackValues) {
				wait = CallbackValues[key] == null && timeout < TimeToWaitForReply;
			}                
			if(wait)
				System.Threading.Thread.Sleep(TimePerPoll);							//Sleep for the poll period (so we don't check obsesively)
			timeout += TimePerPoll;
		}

		if (timeout >= TimeToWaitForReply)											//On timeout send log call too
			Log("timeout");

		lock (CallbackValues) {
			APICallResult ret = CallbackValues[key];								//Fetch the callback values for this call (key)
			CallbackValues.Remove(key);												//Remove this callback from the list (don't need to process it as we now have the value)
			return ret;																//Return the APICallResult for this call
		}
	}


	/// <summary>
	/// Check the queue of incomming message.
	/// </summary>
	/// <remarks>
	/// When an incomming message is in the queue, add it to the response map using the key
	/// that is part of the message string.
	/// </remarks>
	void processQueue() {
		try {
			lock (ResponseQueue) {
				while (ResponseQueue.Count > 0) {											//Only process if we have something in the queue
			
					string input;
					input = ResponseQueue.Dequeue();								//Get the next queued response from scorm.js

					string[] tokens = input.Split('|');									//Split the result into the components (see the scorm.js file for details
					int key = System.Convert.ToInt32(tokens[tokens.Length-1]);			//Fetch the key (as an int32)
				
					lock (CallbackValues) {
						CallbackValues[key] = new ScormAPIWrapper.APICallResult();		//Create a new APICallResult object for this call (key)
						CallbackValues[key].Result = tokens[0];							//Fetch the result string
						CallbackValues[key].ErrorCode = tokens[1];						//Fetch the error code
						CallbackValues[key].ErrorDescription = tokens[2];				//Fetch the error description
						CallbackValues[key].Key = tokens[tokens.Length-1];				//Fetch the key (as a string)
					}
				}
			}
		} catch (Exception e) {
				UnityEngine.Application.ExternalCall("DebugPrint", "***processQueue***" + e.Message +"<br/>" + e.StackTrace + "<br/>" + e.Source);	//Send debug error message to the html wrapper
		}

	}

	void processQueueOrig() {
		while (ResponseQueue.Count > 0) {											//Only process if we have something in the queue
			try {
				string input;
				lock (ResponseQueue) {
					
					input = ResponseQueue.Dequeue();								//Get the next queued response from scorm.js
					
				}
				
				string[] tokens = input.Split('|');									//Split the result into the components (see the scorm.js file for details
				int key = System.Convert.ToInt32(tokens[tokens.Length-1]);			//Fetch the key (as an int32)
				
				lock (CallbackValues) {
					CallbackValues[key] = new ScormAPIWrapper.APICallResult();		//Create a new APICallResult object for this call (key)
					CallbackValues[key].Result = tokens[0];							//Fetch the result string
					CallbackValues[key].ErrorCode = tokens[1];						//Fetch the error code
					CallbackValues[key].ErrorDescription = tokens[2];				//Fetch the error description
					CallbackValues[key].Key = tokens[tokens.Length-1];				//Fetch the key (as a string)
				}
			}
			catch (Exception e) {
				UnityEngine.Application.ExternalCall("DebugPrint", "***processQueue***" + e.Message +"<br/>" + e.StackTrace + "<br/>" + e.Source);	//Send debug error message to the html wrapper
			}
		}
	}

	/// <summary>
	/// Add a response to a javascript call to the queue.
	/// </summary>
	/// <remarks>
	/// The ScormManager will add messages from the javascript layer to the queue, which will be
	/// processed by the other thread.
	/// </remarks>
	public void SetCallbackValue(string input) {
		lock (ResponseQueue) {
			ResponseQueue.Enqueue(input);											//Add the response to the Response Queue for processing
		}
	}

	/// <summary>
	/// Get a value from the javascript API
	/// </summary>
	/// <remarks>
	/// Implements the interface to the SCORM API, and looks syncronous to the caller.
	/// TODO: (This is where you would implement the 2004/1.2 conversion)
	/// </remarks>
	/// <returns>
	/// A string of the return value of the get command
	/// </returns>
	/// <param name="identifier">
	/// The dot notation identifier of the data model element to get
	/// </param>
	public string GetValue(string identifier) {
		string result = "";

		int key = SetupCallback();													//Set the key for this call (identifies the call when processing the returned results in the queue
		Log("Get " + identifier);

		UnityEngine.Application.ExternalCall("doGetValue", new object[] { identifier , CallbackObjectName, CallbackFunctionName, key });  //Call to scorm.js
		
		APICallResult returnval = WaitForReturn(key);

		result = returnval.Result;
		
		if(returnval.ErrorCode == "")
			Log("Got  " + result);													//No error, log result
		else
			Log("Error:" + returnval.ErrorCode.ToString() + " " + returnval.ErrorDescription + " Result: "+returnval.Result);		//Error, log error code and description
		
		return result;
	}



	/// <summary>
	/// Set a value on the javascript API
	/// </summary>
	/// <remarks> 
	/// Implements the interface to the SCORM API, and looks syncronous to the caller.
	/// </remarks>
	/// <returns>
	/// Bool return value of the set command
	/// </returns>
	/// <param name="identifier">The dot notation identifier of the data model element to set.</param>
	/// <param name="value">Value to set</param>
	public bool SetValue(string identifier, string value) 	{
		bool result = false;

		int key = SetupCallback();													//Set the key for this call (identifies the call when processing the returned results in the queue
		Log( "Set  " + identifier + " to " + value);

		UnityEngine.Application.ExternalCall("doSetValue", new object[] { identifier, value, CallbackObjectName, CallbackFunctionName, key });	//Call to scorm.js

		APICallResult returnval = WaitForReturn(key);
		if(returnval.ErrorCode == "") {
			Log("Result " + returnval.Result);
			result = true;
		} else {
			Log("Error:" + returnval.ErrorCode.ToString() + " " + returnval.ErrorDescription);
			result = false;
		}	
		return result;		
	}

	/// <summary>
	/// Call the commit function in the javascript layer
	/// </summary>
	public void Commit() {
		UnityEngine.Application.ExternalCall("doCommit");	
	}

	/// <summary>
	/// Call the terminate function in the javascript layer
	/// </summary>
	public void Terminate() {
		UnityEngine.Application.ExternalCall("doTerminate");	
	}


	/// <summary>
	/// Send a log command up to the parent GameObject.  The actual implementation of the Log function is up to your own code.
	/// </summary>
	/// <param name="text">
	/// the text to log
	/// </param>
	public void Log(string text) {
		UnityEngine.GameObject.Find(CallbackObjectName).SendMessage("LogMessage",text,UnityEngine.SendMessageOptions.DontRequireReceiver);
	}

}