/***********************************************************************************************************************
 * Unity-SCORM Integration Kit
 * 
 * Unity3D Editor Plugin for in Unity3D functions (creation, and export)
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

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Web;
using Ionic.Zip;
using System.IO;
using System;
 
/// <summary>
/// This class handles the editor window for the Unity-SCORM Integration Kit
/// </summary>
public class ScormExport : EditorWindow {

	public GUISkin skin;
    
	static bool foldout1,foldout2;
	static ScormExport window; 
	static Vector2 scrollview;

    /// <summary>
	/// The menu item to allow the creation of the export SCORM package
    /// </summary>
    [MenuItem("SCORM/Export SCORM Package",false,0)]
    static void ShowWindow() {
		EditorUtility.DisplayDialog("Export this scene as a WebPlayer first","Because this software is developed for Unity Basic, we cannot automatically build the web player. Please export your simulation to the web player first. Remember to select the SCORM Integration web player template.","OK");
        window = (ScormExport)EditorWindow.GetWindow (typeof (ScormExport));
		window.Show ();
		foldout1=foldout2 = true;
    }
	
	/// <summary>
	/// Get existing open window or if none, make a new one.
	/// </summary>
	static void Init () {
		window = (ScormExport)EditorWindow.GetWindow (typeof (ScormExport));
		window.ShowAuxWindow();		
	}

	/// <summary>
	/// The menu item to allow the initialisation of a scene to allow integration with SCORM
	/// </summary>
	[MenuItem("SCORM/Create SCORM Manager",false,0)]
    static void CreateManager() {
		GameObject manager = GameObject.Find("ScormManager");
		if(manager == null) {
        	manager = (GameObject)UnityEditor.SceneView.Instantiate(Resources.Load("ScormManager"));
			manager.name = "ScormManager";
			EditorUtility.DisplayDialog("The SCORM Manager has been added to the scene","Remember to place objects that need messages from the ScormManager under it in the scene heirarchy. It will send the message 'Scorm_Initialize_Complete' when it finishes communicating with the LMS.","OK");
		} else {
			EditorUtility.DisplayDialog("SCORM Manager is already present","You only need one SCORM Manager game object in your simulation. Remember to place objects that need messages from the ScormManager under it in the scene heirarchy.","OK");
		}
		
    }
	
	/// <summary>
	/// The menu item to display a short about message
	/// </summary>
    [MenuItem("SCORM/About SCORM Integration",false,0)]
    static void About() {
		EditorUtility.DisplayDialog("Unity-SCORM Integration Kit","This software enables the integration between web deployed Unity3D applications and a Learning Managment System (LMS) using the Sharable Content Object Reference Model (SCORM) developed at the US Department of Defence Advance Distributed Learning (ADL) Inititive. This software is provided 'as-is' and is available free of charge at http://www.adlnet.gov. This software may be used under the provisions of the Apache 2.0 license. This project is derived from the Unity-SCORM Integration Toolkit Version 1.0 Beta project from the ADL (Advance Distributed Learning) [http://www.adlnet.gov]. Source code is available from unity3d.stals.com.au/scorm-integration-kit  ","OK");
    }

	/// <summary>
	/// The menu item to open a browser window to the support page for this package
	/// </summary>
    [MenuItem("SCORM/Help",false,0)]
    static void Help() {
		Application.OpenURL("http://unity3d.stals.com.au/scorm-integration-kit");
    }


	/// <summary>
	/// Copies the files recursively.
	/// </summary>
	/// <param name="source">Source.</param>
	/// <param name="target">Target.</param>
	public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
	    foreach (DirectoryInfo dir in source.GetDirectories())
	        CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
	    foreach (FileInfo file in source.GetFiles()) {
			// Skip hidden files and meta files
			if (file.Name.Substring(0,1) != "." & file.Name.Substring(file.Name.Length - 4) != "meta") {
				file.CopyTo (Path.Combine (target.FullName, file.Name));
			}
		}
	}

	/// <summary>
	/// Convert Seconds to the SCORM timeInterval.
	/// </summary>
	/// <returns>timeInterval string.</returns>
	/// <param name="seconds">Seconds.</param>
	private static string SecondsToTimeInterval(float seconds) {
		TimeSpan t = TimeSpan.FromSeconds( seconds );		
		return string.Format("P{0:D}DT{1:D}H{2:D}M{3:F}S",t.Days, t.Hours, t.Minutes, t.Seconds); //This is good enough to feed into SCORM, no need to include Years and Months
	}

	/// <summary>
	/// Parses the float.
	/// </summary>
	/// <remarks>float.Parse fails on an empty string, so we use this to return a 0 if an empty string is encountered.</remarks>
	/// <returns>The float.</returns>
	/// <param name="str">String.</param>
	private static float ParseFloat(string str) {
		float result = 0f;
		float.TryParse(str,out result);
		return result;
	}

	/// <summary>
	/// Publish this SCORM package to a conformant zip file.
	/// </summary>
    void Publish() {

		string timeLimitAction = "";
		switch (PlayerPrefs.GetInt ("Time_Limit_Action")) {
		case 0:
			timeLimitAction = "";
			break;
		case 1:
			timeLimitAction = "exit,message";
			break;
		case 2:
			timeLimitAction = "exit,no message";
			break;
		case 3:
			timeLimitAction = "continue,message";
			break;
		case 4:
			timeLimitAction = "continue,no message";
			break;
		}

		string timeLimit = SecondsToTimeInterval (ParseFloat(PlayerPrefs.GetString ("Time_Limit_Secs")));

		string webplayer = PlayerPrefs.GetString("Course_Export");
		string tempdir = System.IO.Path.GetTempPath() + System.IO.Path.GetRandomFileName();
		System.IO.Directory.CreateDirectory(tempdir);
		CopyFilesRecursively(new System.IO.DirectoryInfo(webplayer),new System.IO.DirectoryInfo(tempdir));
		string zipfile = EditorUtility.SaveFilePanel("Choose Output File",webplayer,PlayerPrefs.GetString("Course_Title"),"zip");

		if(zipfile!= "") {
			if(File.Exists(zipfile))
				File.Delete(zipfile);
			   
			Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipfile);
			zip.AddDirectory(tempdir); 
			
			zip.AddItem(Application.dataPath + "/SCORM/Plugins/2004");
				
			string manifest = 	"<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
								"<manifest identifier=\""+PlayerPrefs.GetString("Manifest_Identifier")+"\" version=\"1\"\n" +
                  				"\t\txmlns = \"http://www.imsglobal.org/xsd/imscp_v1p1\"\n" +
                 				"\t\txmlns:adlcp = \"http://www.adlnet.org/xsd/adlcp_v1p3\"\n" +
								"\t\txmlns:adlseq = \"http://www.adlnet.org/xsd/adlseq_v1p3\"\n" +
								"\t\txmlns:adlnav = \"http://www.adlnet.org/xsd/adlnav_v1p3\"\n" +
								"\t\txmlns:imsss = \"http://www.imsglobal.org/xsd/imsss\"\n" +
								"\t\txmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\"\n" +
								"\t\txmlns:lom=\"http://ltsc.ieee.org/xsd/LOM\"\n" +
								"\t\txsi:schemaLocation = \"http://www.imsglobal.org/xsd/imscp_v1p1 imscp_v1p1.xsd\n" +
								"\t\t\thttp://www.adlnet.org/xsd/adlcp_v1p3 adlcp_v1p3.xsd\n" +
								"\t\t\thttp://www.adlnet.org/xsd/adlseq_v1p3 adlseq_v1p3.xsd\n" +
								"\t\t\thttp://www.adlnet.org/xsd/adlnav_v1p3 adlnav_v1p3.xsd\n" +
								"\t\t\thttp://www.imsglobal.org/xsd/imsss imsss_v1p0.xsd\n" +
								"\t\t\thttp://ltsc.ieee.org/xsd/LOM lom.xsd\" >\n" +
  								"<metadata>\n" +
								"\t<schema>ADL SCORM</schema>\n" +
    							"\t<schemaversion>2004 4th Edition</schemaversion>\n" +
								"<lom:lom>\n" +
      							"\t<lom:general>\n" +
								"\t\t<lom:description>\n" +
								"\t\t\t<lom:string language=\"en-US\">"+PlayerPrefs.GetString("Course_Description")+"</lom:string>\n" +
								"\t\t</lom:description>\n" +
      							"\t</lom:general>\n" +
								"\t</lom:lom>\n" +
  								"</metadata>\n" +
								"<organizations default=\"B0\">\n" +
								"\t<organization identifier=\"B0\" adlseq:objectivesGlobalToSystem=\"false\">\n" +
								"\t\t<title>"+PlayerPrefs.GetString("Course_Title")+"</title>\n" +
      							"\t\t<item identifier=\"i1\" identifierref=\"r1\" isvisible=\"true\">\n" +
								"\t\t\t<title>"+PlayerPrefs.GetString("SCO_Title")+"</title>\n" +
								"\t\t\t<adlcp:timeLimitAction>"+timeLimitAction+"</adlcp:timeLimitAction>\n" +
								"\t\t\t<adlcp:dataFromLMS>"+PlayerPrefs.GetString("Data_From_Lms")+"</adlcp:dataFromLMS> \n" +
								"\t\t\t<adlcp:completionThreshold completedByMeasure = \""+ (System.Convert.ToBoolean(PlayerPrefs.GetInt("completedByMeasure"))).ToString().ToLower() +"\" minProgressMeasure= \""+PlayerPrefs.GetFloat("minProgressMeasure") +"\" />\n" +
								"\t\t\t<imsss:sequencing>\n" +
								"\t\t\t<imsss:limitConditions attemptAbsoluteDurationLimit=\""+timeLimit+"\"/>\n" +
								"\t\t\t</imsss:sequencing>\n" +
								"\t\t</item>\n" +
								"\t</organization>\n" +
								"</organizations>\n" +
								"<resources>\n" +
								"\t<resource identifier=\"r1\" type=\"webcontent\" adlcp:scormType=\"sco\" href=\""+PlayerPrefs.GetString("Course_Export_Name")+".html\">\n" +
								"\t\t<file href=\""+PlayerPrefs.GetString("Course_Export_Name")+".html\" />\n" +
								"\t\t<file href=\"scripts/scorm.js\" />\n" +
								"\t\t<file href=\"scripts/ScormSimulator.js\" />\n" +
								"\t\t<file href=\""+PlayerPrefs.GetString("Course_Export_Name")+".unity3d\" />\n" +
								"\t</resource>\n" +
  								"</resources>\n" +
								"</manifest>";
				
				zip.AddEntry("imsmanifest.xml",".",System.Text.ASCIIEncoding.ASCII.GetBytes(manifest));
			
			zip.Save();
			EditorUtility.DisplayDialog("SCORM Package Published","The SCORM Package has been published to "+zipfile,"OK");

		}
	}

	/// <summary>
	/// Display the Export SCORM dialog
	/// </summary>
    void OnGUI() {
		EditorStyles.miniLabel.wordWrap = true;
		EditorStyles.foldout.fontStyle = FontStyle.Bold;

		// Foldout 1 - the Prevuously exported web player location and details.
		GUILayout.BeginHorizontal();
		foldout1 = EditorGUILayout.Foldout(foldout1,"Exported Player Location", EditorStyles.foldout);

		bool help1 = GUILayout.Button(new GUIContent ("Help", "Help for the Exported Player Location section"),EditorStyles.miniBoldLabel);
		if(help1)
			EditorUtility.DisplayDialog("Help","You must export this simulation as a webplayer, then tell this packaging tool the location of that exported webplayer folder. Be sure to select the SCORM webplayer template, or the necessary JavaScript components will not be included, and the system will fail to connect to the LMS.","OK");

		GUILayout.EndHorizontal();


		if(foldout1) {
			GUILayout.BeginVertical("TextArea");
			GUILayout.Label("Choose the location of the folder where the Webplayer was exported.", EditorStyles.miniLabel);
			PlayerPrefs.SetString("Course_Export", EditorGUILayout.TextField("Folder Location", PlayerPrefs.GetString("Course_Export")));
			PlayerPrefs.SetString("Course_Export_Name", EditorGUILayout.TextField("Application Name", PlayerPrefs.GetString("Course_Export_Name")));

			GUI.skin.button.fontSize = 8;
			GUILayout.BeginHorizontal();
			GUILayout.Space(window.position.width - 85);	
			bool ChooseDir = GUILayout.Button(new GUIContent("Choose Folder","Select the folder containing the webplayer"),GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			if(ChooseDir)
			{
				string export_dir = EditorUtility.OpenFolderPanel("Choose WebPlayer",PlayerPrefs.GetString("Course_Export"),"WebPlayer");
				if(export_dir != "")
				{
					PlayerPrefs.SetString("Course_Export",export_dir);
					PlayerPrefs.SetString("Course_Export_Name",export_dir.Substring(export_dir.LastIndexOf('/')+1,(export_dir.Length-(export_dir.LastIndexOf('/')+1))));
				}
			}

	        GUILayout.EndVertical();
		}


		// Foldout 2 - Set the SCORM properties (manhy of the options available in the imsmanifest.xml file)
		GUILayout.BeginHorizontal();                            
		foldout2 = EditorGUILayout.Foldout(foldout2,"SCORM Properties", EditorStyles.foldout);

		bool help2 = GUILayout.Button(new GUIContent ("Help", "Help for the SCORM Properties section"),EditorStyles.miniBoldLabel);
		if(help2)
			EditorUtility.DisplayDialog("Help","The properties will control how the LMS controls and displays your SCORM content. These values will be written into the imsmanifest.xml file within the exported zip package. There are many other settings that can be specified in the manifest - for more information read the Content Aggregation Model documents at http://www.adlnet.gov/capabilities/scorm","OK");

		GUILayout.EndHorizontal();
		
		if(foldout2) {
			GUILayout.BeginVertical("TextArea");
			GUILayout.Label("Information about your SCORM package including the title and various configuration values.", EditorStyles.miniLabel);
			PlayerPrefs.SetString("Manifest_Identifier", EditorGUILayout.TextField(new GUIContent("Identifier:","The unique IMS Manifest Identifier (e.g. au.com.stals.myapp)"), PlayerPrefs.GetString("Manifest_Identifier")));
			PlayerPrefs.SetString("Course_Title", EditorGUILayout.TextField(new GUIContent("Title:","The title of the SCORM content, as you want it to be displayed in the learning management system (LMS)"), PlayerPrefs.GetString("Course_Title")));
			PlayerPrefs.SetString("Course_Description", EditorGUILayout.TextField(new GUIContent("Description:","Description of the SCORM content."), PlayerPrefs.GetString("Course_Description")));
			PlayerPrefs.SetString("SCO_Title", EditorGUILayout.TextField(new GUIContent("Module Title:","The title of the Unity content.  Note, this title may show as the first item in an LMS-provided table of contents."), PlayerPrefs.GetString("SCO_Title")));
			PlayerPrefs.SetString("Data_From_Lms", EditorGUILayout.TextField(new GUIContent("Launch Data:","User-defined string value that can be used as initial learning experience state data."), PlayerPrefs.GetString("Data_From_Lms")));

			bool progress = GUILayout.Toggle(System.Convert.ToBoolean(PlayerPrefs.GetInt("completedByMeasure")),new GUIContent("Completed By Measure","If true, then this activity's completion status will be determined by the progress measure's relation to the minimum progress measure. This derived completion status will override what it explicitly set."));
			PlayerPrefs.SetInt("completedByMeasure",System.Convert.ToInt16(progress));
			if(progress)
			{
				GUILayout.Label(new GUIContent("Minimum Progress Measure: " + PlayerPrefs.GetFloat("minProgressMeasure").ToString(),"Defines a minimum completion percentage for this activity for use in conjunction with completed by measure.") , EditorStyles.miniLabel);
				PlayerPrefs.SetFloat("minProgressMeasure",(float)System.Math.Round(GUILayout.HorizontalSlider(PlayerPrefs.GetFloat("minProgressMeasure"),0.0f,1.0f)*100.0f)/100.0f);
			}
			GUILayout.Label("If set, this indicates that this activity’s completion status will be determined soley by the relation of the progress measure to Minimum Progress Measure.", EditorStyles.miniLabel);



			GUILayout.Label("Select the Time Limit Action to be passed to the SCO", EditorStyles.largeLabel);
			PlayerPrefs.SetInt("Time_Limit_Action",EditorGUILayout.Popup(PlayerPrefs.GetInt("Time_Limit_Action"),new string[]{"Not Set","exit,message","exit,no message","continue,message","continue,no message"},GUILayout.ExpandWidth(false)));
			PlayerPrefs.SetString("Time_Limit_Secs", EditorGUILayout.TextField(new GUIContent("Time Limit (secs):","The time limit for this SCO in seconds."), PlayerPrefs.GetString("Time_Limit_Secs")));

			GUILayout.EndVertical();
		}

		// Publish Button
		GUIStyle s =  new GUIStyle();
		GUI.skin.button.fontSize = 12;
		bool publish = GUILayout.Button(new GUIContent ("Publish", "Export this course to a SCORM package."));
		if(publish)
			Publish();
    }
}