/***********************************************************************************************************************
 * Unity-SCORM Integration Kit
 * 
 * Scorm Javascript layer
 * (No extra functionality from the original, simply tidied up a little)
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


///Rob Chadwick - 6/1/12 - default to false
var debug = false;  // set this to false to turn debugging off.

var output = window.console; // output can be set to any object that has a log(string) function
                             // such as: var output = { log: function(str){alert(str);} };

// Define exception/error codes
var _NoError = {"code":"0","string":"No Error","diagnostic":"No Error"};;
var _GeneralException = {"code":"101","string":"General Exception","diagnostic":"General Exception"};
var _AlreadyInitialized = {"code":"103","string":"Already Initialized","diagnostic":"Already Initialized"};

// Initialized state of the content; default false
var initialized = false;

// Handle to the SCORM  API instance
var apiHandle = null;

// Global for SCORM version; Default 2004 and set during initialize
var versionIsSCORM2004 = true;

/*******************************************************************************
**
** Function: doInitialize()
** Inputs:  None
** Return:  true if the initialization was successful, or
**          false if the initialization failed.
**
** Description:
** Initialize communication with LMS by calling the Initialize
** function which will be implemented by the LMS.
**
*******************************************************************************/
function doInitialize()
{
   if (initialized) return "true";
   
   var api = getAPIHandle();
   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nInitialize was not successful.");
      return "false";
   }

   // switch Initialize method based on SCORM version
   if (versionIsSCORM2004 == true)
   {
      var result = api.Initialize("");
   }
   else
   {
      var result = api.LMSInitialize("");
   }
   
   if (result.toString() != "true")
   {
      var err = ErrorHandler();
      message("Initialize failed with error code: " + err.code);
   }
   else
   {
      initialized = true;
   }

   return result.toString();
}

/*******************************************************************************
**
** Function doTerminate()
** Inputs:  None
** Return:  true if successful
**          false if failed.
**
** Description:
** Close communication with LMS by calling the Terminate
** function which will be implemented by the LMS
**
*******************************************************************************/
function doTerminate()
{  
   if (! initialized) return "true";
   
   var api = getAPIHandle();
   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nTerminate was not successful.");
      return "false";
   }
   else
   {
      // switch Terminate/LMSFinish method based on SCORM version
     if (versionIsSCORM2004 == true)
     {
        var result = api.Terminate("");
     }
     else
     {
        var result = api.LMSFinish("");
     }     
     
      if (result.toString() != "true")
      {
         var err = ErrorHandler();
         message("Terminate failed with error code: " + err.code);
      }
   }
   
   initialized = false;

   return result.toString();
}

/*******************************************************************************
**
** Function doGetValue()
** Inputs:  name - string representing the cmi data model defined category or
**             element (e.g. cmi.learner_id)
** Return:  The value presently assigned by the LMS to the cmi data model
**       element defined by the element or category identified by the name
**       input value.
**
** Description:
** Wraps the call to the GetValue method
**
*******************************************************************************/
function doGetValue(identifier,objectname,callbackname,randomnumber)
{  
   // JP TODO - temp hack to get rid of the value for strings and associated language data model elements
   var dotBindingName = identifier.replace(".Value", "");
   var api = getAPIHandle();
   var result = "";

   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nGetValue was not successful.");
   }
   else if (!initialized && ! doInitialize())
   {
      var err = ErrorHandler();
      message("GetValue failed - Could not initialize communication with the LMS - error code: " + err.code);
   }
   else
   {
   
      // switch Get method based on SCORM version
     if (versionIsSCORM2004 == true)
     {
        result = api.GetValue(dotBindingName);
     }
     else
     {
        result = api.LMSGetValue(dotBindingName);
     }
      
      var error = ErrorHandler();
     
     //DebugPrint(dotBindingName + ":" + result);
     
      if (error.code != _NoError.code)
      {
         // an error was encountered so display the error description
         message("GetValue("+dotBindingName +") failed. \n"+ error.code + ": " + error.string);
         result = "";
       ///Rob Chadwick - 6/1/12 - fixed bug where result.tostring failed where the LMS returns null
       ///This should never happed, but it does in some broken LMS's
       GetUnity().SendMessage(objectname, callbackname, result+"" + "|" + error.code + "|" + error.string + "|" + randomnumber);
      }else
     {
      GetUnity().SendMessage(objectname, callbackname, result+"" + "|" + "|" + "|" + randomnumber);
     }
   }
  }

/*******************************************************************************
**
** Function doSetValue()
** Inputs:  name -string representing the data model defined category or element
**          value -the value that the named element or category will be assigned
** Return:  true if successful
**          false if failed.
**
** Description:
** Wraps the call to the SetValue function
**
*******************************************************************************/
function doSetValue(identifier,value,objectname,callbackname,randomnumber)
{
   // JP TODO - temp hack to get rid of the value for strings and associated language data model elements
   var dotBindingName = identifier.replace(".Value", "");
   var api = getAPIHandle();
   var result = "false";

   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nSetValue was not successful.");
   }
   else if (!initialized && !doInitialize())
   {
      var error = ErrorHandler();
      message("SetValue failed - Could not initialize communication with the LMS - error code: " + error.code);
   }
   else
   {
   
      // switch set method based on SCORM version
     if (versionIsSCORM2004)
     {
        result = api.SetValue(dotBindingName, value);
     }
     else
     {
        result = api.LMSSetValue(dotBindingName, value);
     }
     
     //DebugPrint(dotBindingName + ":" + value + ":" + result);
     
      if (result.toString() != "true")
      {
         var err = ErrorHandler();
         message("SetValue("+dotBindingName+", "+value+") failed. \n"+ err.code + ": " + err.string);
       GetUnity().SendMessage(objectname, callbackname, result.toString() + "|" + err.code + "|" + err.string + "|" + randomnumber);
      }else
     {
      GetUnity().SendMessage(objectname, callbackname, result.toString() + "|"  + "|"  + "|" + randomnumber);
     }
   }
}   

/*******************************************************************************
**
** Function doIsScorm2004()
** Return:  true if the api is a 2004 api
**          false if the api is a 1.2 api.
**
** Description:
** Sends a message to unity declaring the API version
**
*******************************************************************************/
function doIsScorm2004(objectname,callbackname,randomnumber)
{
   
   var api = getAPIHandle();
   var result = "false";
   if (api == null)
   {
      //DebugPrint("Unable to locate the LMS's API Implementation.\nSetValue was not successful.");
   }
   else if (!initialized && !doInitialize())
   {
      var error = ErrorHandler();
      //DebugPrint("doIsScorm2004 failed - Could not initialize communication with the LMS - error code: " + error.code);
   }
  
   GetUnity().SendMessage(objectname, callbackname, versionIsSCORM2004 + "|"+"|"+"|" + randomnumber);
}

/*******************************************************************************
**
** Function doCommit()
** Inputs:  None
** Return:  true if successful
**          false if failed
**
** Description:
** Commits the data to the LMS. 
**
*******************************************************************************/
function doCommit()
{
   var api = getAPIHandle();
   var result = "false";
   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nCommit was not successful.");
   }
   else if (!initialized && ! doInitialize())
   {
      var error = ErrorHandler();
      message("Commit failed - Could not initialize communication with the LMS - error code: " + error.code);
   }
   else
   {
      // switch method based on SCORM Version
     if (versionIsSCORM2004)
     {
        result = api.Commit("");
     }
     else
     {
        result = api.LMSCommit("");
     }
     
      if (result != "true")
      {
         var err = ErrorHandler();
         message("Commit failed - error code: " + err.code);
      }
   }

   return result.toString();
}

/*******************************************************************************
**
** Function doGetLastError()
** Inputs:  None
** Return:  The error code that was set by the last LMS function call
**
** Description:
** Call the GetLastError function 
**
*******************************************************************************/
function doGetLastError()
{
   var api = getAPIHandle();
   var result = "";
   
   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nGetLastError was not successful.");
      //since we can't get the error code from the LMS, return a general error
      return _GeneralException.code;
   }

   if (versionIsSCORM2004 == true)
   {
      result = api.GetLastError().toString();
   }
   else
   {
      result = api.LMSGetLastError().toString();
   }
    
   return result;
}

/*******************************************************************************
**
** Function doGetErrorString(errorCode)
** Inputs:  errorCode - Error Code
** Return:  The textual description that corresponds to the input error code
**
** Description:
** Call the GetErrorString function 
**
********************************************************************************/
function doGetErrorString(errorCode)
{
   var api = getAPIHandle();
   var result = "";
   
   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nGetErrorString was not successful.");
      return _GeneralException.string;
   }

   // switch method based on SCORM version
   if (versionIsSCORM2004)
   {
      result = api.GetErrorString(errorCode).toString();
   }
   else
   {
      result = api.LMSGetErrorString(errorCode).toString();
   }
   
   return result;
}

/*******************************************************************************
**
** Function doGetDiagnostic(errorCode)
** Inputs:  errorCode - Error Code(integer format), or null
** Return:  The vendor specific textual description that corresponds to the 
**          input error code
**
** Description:
** Call the GetDiagnostic function
**
*******************************************************************************/
function doGetDiagnostic(errorCode)
{
   var api = getAPIHandle();
   var result = "";
   
   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nGetDiagnostic was not successful.");
      return "Unable to locate the LMS's API Implementation. GetDiagnostic was not successful.";
   }

   // switch method based on SCORM version
   if (versionIsSCORM2004)
   {
      result = api.GetDiagnostic(errorCode).toString();
   }
   else
   {
      result = api.LMSGetDiagnostic(errorCode).toString();
   }
   
   return result;
}

/*******************************************************************************
**
** Function ErrorHandler()
** Inputs:  None
** Return:  The current error
**
** Description:
** Determines if an error was encountered by the previous API call
** and if so, returns the error.
**
** Usage:
** var last_error = ErrorHandler();
** if (last_error.code != _NoError.code)
** {
**    message("Encountered an error. Code: " + last_error.code + 
**                                "\nMessage: " + last_error.string +
**                                "\nDiagnostics: " + last_error.diagnostic);
** }
*******************************************************************************/
function ErrorHandler()
{
   var error = {"code":_NoError.code, "string":_NoError.string, "diagnostic":_NoError.diagnostic};
   var api = getAPIHandle();
   if (api == null)
   {
      message("Unable to locate the LMS's API Implementation.\nCannot determine LMS error code.");
      error.code = _GeneralException.code;
      error.string = _GeneralException.string;
      error.diagnostic = "Unable to locate the LMS's API Implementation. Cannot determine LMS error code.";
      return error;
   }

   // check for errors caused by or from the LMS; switch based on SCORM version
   if (versionIsSCORM2004 == true)
   {
      error.code = api.GetLastError().toString();
   }
   else
   {
      error.code = api.LMSGetLastError().toString();
   }
   
   if (error.code != _NoError.code)
   {
      // an error was encountered so display the error description
     if (versionIsSCORM2004 == true)
     {
         error.string = api.GetErrorString(error.code);
         error.diagnostic = api.GetDiagnostic(null);
      }
     else
     {
        error.string = api.LMSGetErrorString(error.code);
         error.diagnostic = api.LMSGetDiagnostic(null);
     }
   }

   return error;
}

/******************************************************************************
**
** Function getAPIHandle()
** Inputs:  None
** Return:  value contained by APIHandle
**
** Description:
** Returns the handle to API object if it was previously set,
** otherwise it returns null
**
*******************************************************************************/
function getAPIHandle()
{
   if (apiHandle == null)
   {
      apiHandle = getAPI();
   }

   return apiHandle;
}

/*******************************************************************************
**
** Function findAPI(win)
** Inputs:  win - a Window Object
** Return:  If an API object is found, it's returned, otherwise null is returned
**
** Description:
** This function looks for an object named API_1484_11 in parent and opener
** windows
**
*******************************************************************************/
function findAPI(win)
{
   var findAPITries = 0;
   while ((win.API_1484_11 == null) && (win.parent != null) && (win.parent != win) && (win.API == null))
   {
      findAPITries++;
      
      if (findAPITries > 500) 
      {
         message("Error finding API -- too deeply nested.");
         return null;
      }
      
      win = win.parent;

   }
   
   // return SCORM 2004 or SCORM Version 1.2 API - whichever is found or null
   if (win.API != null)
   {
      versionIsSCORM2004 = false;
     return win.API;
   }
   else
   {
      versionIsSCORM2004 = true;
     return win.API_1484_11;
   }
   
}

/*******************************************************************************
**
** Function getAPI()
** Inputs:  none
** Return:  If an API object is found, it's returned, otherwise null is returned
**
** Description:
** This function looks for an object named API_1484_11, first in the current window's 
** frame hierarchy and then, if necessary, in the current window's opener window
** hierarchy (if there is an opener window).
**
*******************************************************************************/
function getAPI()
{
   var theAPI = findAPI(window);
   if ((theAPI == null) && (window.opener != null) && (typeof(window.opener) != "undefined"))
   {
      theAPI = findAPI(window.opener);
   }
   if (theAPI == null)
   {
      message("Unable to find an API adapter");
     
   }
  
   return theAPI
}


/*******************************************************************************
**
** Function message(str)
** Inputs:  String - message you want to send to the designated output
** Return:  none
** Depends on: boolean debug to indicate if output is wanted
**             object output to handle the messages. must implement a function 
**             log(string)
**
** Description:
** This function outputs messages to a specified output. You can define your own 
** output object. It will just need to implement a log(string) function. This 
** interface was used so that the output could be assigned the window.console object.
*******************************************************************************/
function message(str)
{
   if(debug)
   {
     ///Rob Chadwick - 6/1/12 - Fixed bug where console.log is sometimes not available
     ///happens in IE 8 and 9
     if(output && output.log) 
      output.log(str);
   }
}


//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////

var scorm = (function(){
   return {
      foo: function(str){
         alert(str);
         GetUnity().SendMessage("GameObject", "fromweb", "message from web");
      }
   };
   
})();



function DebugPrint(str)
{
   document.getElementById('console').innerHTML = document.getElementById('console').innerHTML + str +"<br />";
}

function pausecomp(millis)
 {
   var date = new Date();
   var curDate = null;
   do { curDate = new Date(); }
   while(curDate-date < millis);
}