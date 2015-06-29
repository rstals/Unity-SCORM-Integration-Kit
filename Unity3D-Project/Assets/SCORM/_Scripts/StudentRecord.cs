/***********************************************************************************************************************
 * Unity-SCORM Integration Kit
 *
 * Class to store the Student Record returned from the LMS via SCORM
 * 
 * Copyright (C) 2015, Richard Stals (http://stals.com.au)
 * ==========================================
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
/// Stores the SCORM data returned from the LMS.
/// </summary>
/// <remarks>
/// Only used to define the properties to reflect the SCORM data model.  No methods are defined.
/// </remarks>
public class StudentRecord {
	
	/// <summary>cmi.completion_status datatype</summary>
	public enum CompletionStatusType {completed, incomplete, not_attempted, unknown, not_set};

	/// <summary>cmi.credit datatype</summary>
	public enum CreditType {credit, no_credit, not_set};

	/// <summary>cmi.entry datatype</summary>
	public enum EntryType {start, resume, not_set};

	/// <summary>cmi.exit datatype</summary>
	public enum ExitType {timeout, suspend, normal};

	/// <summary>cmi.interactions.n.type datatype</summary>
	public enum InteractionType {true_false, choice, fill_in, long_fill_in, likert, matching, performance, sequencing, numeric, other, not_set};

	/// <summary>cmi.mode datatype</summary>
	public enum ModeType {browse, normal, review, not_set};

	/// <summary>cmi.interactions.n.result datatype</summary>
	/// <remarks>The 'estimate' value is set when the result is a real number that reflects a judgment on the correctness of the result.
	/// If a value of 'estimate' is set, then the 'estimate' float value stored in the LearnerInteractionRecord is sent to the LMS as the result</remarks>
	/// <see cref="LearnerInteractionRecord"/>
	public enum ResultType {correct, incorrect, unanticipated, neutral, estimate, not_set};

	/// <summary>cmi.success_status and cmi.objectives.n.success_status  datatype</summary>
	public enum SuccessStatusType {passed, failed, unknown, not_set};

	/// <summary>cmi.time_limit_action datatype</summary>
	public enum TimeLimitActionType {exit_message, continue_message, exit_no_message, continue_no_message, not_set};


	/// <summary>cmi.comments_from_learner.X datatype</summary>
	public class CommentsFromLearner {
		/// <summary>cmi.comments_from_learner.n.comment</summary>
		/// <remarks>Textual input</remarks>
		public string comment;	

		/// <summary>cmi.comments_from_learner.n.location</summary>
		/// <remarks>Point in the SCO to which the comment applies</remarks>
		public string location;

		/// <summary>cmi.comments_from_learner.n.timestamp</summary>
		/// <remarks>Point in time at which the comment was created or most recently changed</remarks>
		public DateTime timeStamp;
	}

	/// <summary>cmi.comments_from_lms.X datatype</summary>
	public class CommentsFromLMS {
		/// <summary>cmi.comments_from_lms.n.comment</summary>
		/// <remarks>Comments or annotations associated with a SCO</remarks>
		public string comment;

		/// <summary>cmi.comments_from_lms.n.location</summary>
		/// <remarks>Point in the SCO to which the comment applies</remarks>
		public string location;

		/// <summary>cmi.comments_from_lms.n.timestamp</summary>
		/// <remarks>Point in time at which the comment was created or most recently changed</remarks>
		public DateTime timeStamp;
	}

	/// <summary>cmi.learner_preference.X datatype</summary>
	public class LearnerPreference {
		/// <summary>cmi.learner_preference.audio_level</summary>
		public float audioLevel;

		/// <summary>cmi.learner_preference.language</summary>
		public string langauge;

		/// <summary>cmi.learner_preference.delivery_speed</summary>
		public float deliverySpeed;

		/// <summary>cmi.learner_preference.audio_captioning</summary>
		public int audioCaptioning;
	}

	/// <summary>cmi.score.X datatype</summary>
	/// <remarks>Also used by cmi.objectives.n.score.X</remarks>
	public class LearnerScore {
		/// <summary>cmi.score.scaled</summary>
		/// <remarks>Also cmi.objectives.n.score.scaled.  Number that reflects the performance of the learner</remarks>
		public float scaled;

		/// <summary>cmi.score.raw</summary>
		/// <remarks>Also cmi.objectives.n.score.raw  Number that reflects the performance of the learner relative to the range bounded by the values of min and max</remarks>
		public float raw;

		/// <summary>cmi.score.max</summary>
		/// <remarks>Also cmi.objectives.n.score.max Maximum value in the range for the raw score</remarks>
		public float max;

		/// <summary>cmi.score.min</summary>
		/// <remarks>Also cmi.objectives.n.score.min Minimum value in the range for the raw score</remarks>
		public float min;
	}

	/// <summary>cmi.interactions.n.X datatype</summary>
	public class LearnerInteractionRecord {
		/// <summary>cmi.interactions.n.id</summary>
		/// <remarks>Label for objectives associated with the interaction. You do not need to set this when creating a new LearnerInteractionRecord.  The ScormManager class automatically creates a unique identifier for you.</remarks>
		public string id;

		/// <summary>cmi.interactions.n.type</summary>
		/// <remarks>Which type of interaction is recorded.</remarks>
		public InteractionType type;

		/// <summary>cmi.interactions.n.timestamp</summary>
		/// <remarks>Point in time at which the interaction was first made available to the learner for learner interaction and response.  It is converted to the ISO601 Times format used in SCORM (see https://en.wikipedia.org/?title=ISO_8601#Times) before being set to the LMS</remarks>
		public DateTime timeStamp;

		/// <summary>cmi.interactions.n.weighting</summary>
		/// <remarks>Weight given to the interaction relative to other interactions</remarks>
		public float weighting;

		/// <summary>cmi.interactions.n.learner_response</summary>
		/// <remarks>Data generated when a learner responds to an interaction.</remarks>
		public string response;

		/// <summary>cmi.interactions.n.latency</summary>
		/// <remarks>
		/// Time elapsed between the time the interaction was made available to the learner for response and the time of the first response.  Stored in seconds.
		/// It is converted to the ISO601 Duration format used in SCORM (see https://en.wikipedia.org/?title=ISO_8601#Durations) before being set to the LMS
		/// </remarks>
		public float latency;

		/// <summary>cmi.interactions.n.description</summary>
		/// <remarks>Brief informative description of the interaction.  Commonly the question text.</remarks>
		public string description;

		/// <summary>cmi.interactions.n.result</summary>
		/// <remarks>Judgment of the correctness of the learner response.</remarks>
		public ResultType result;

		/// <summary>cmi.interactions.n.estimate</summary>
		/// <remarks>if the result is intended to be a real number, set the result type to ResultType.estimate, and then set the float value here.  When a SetValue is called to the LMS, the float value is stored.</remarks>
		public float estimate;

		/// <summary>cmi.interactions.n.objectives.n.X</summary>
		/// <see cref="LearnerInteractionObjective"/>
		public List<LearnerInteractionObjective> objectives;

		/// <summary>cmi.interactions.n.correct_responses.n.X</summary>
		/// <see cref="LearnerInteractionCorrectResponse"/>
		public List<LearnerInteractionCorrectResponse> correctResponses;
	}

	/// <summary>cmi.interactions.n.objectives.X datatype</summary>
	/// <remarks>Basically stores a reference (via the objective id) to the Objective that relates to this interaction.</remarks>
	public class LearnerInteractionObjective {
		/// <summary>cmi.interactions.n.objectives.n.id</summary>
		/// <remarks> Label for objectives associated with the interaction.</remarks>
		public string id;
	}

	/// <summary>cmi.interactions.n.correct_responses.n.X datatype</summary>
	/// <remarks>Basically stores a set of patterns for this interaction (optional)</remarks>
	public class LearnerInteractionCorrectResponse {
		/// <summary>cmi.interactions.n.correct_responses.n.pattern</summary>
		public string pattern;
	}

	/// <summary>cmi.objectives.n.X datatype</summary>
	public class Objectives {
		/// <summary>cmi.objectives.n.id</summary>
		/// <remarks>Unique label for the objective</remarks>
		public string id;

		/// <summary>cmi.objectives.n.score.X</summary>
		/// <remarks>Numbers that reflects the performance of the learner for the objective</remarks>
		public LearnerScore score;

		/// <summary>cmi.objectives.n.success_status</summary>
		/// <remarks></remarks>
		public SuccessStatusType successStatus;

		/// /// <summary>cmi.objectives.n.completion_status</summary>
		/// <remarks>Indicates whether the learner has completed the associated objective</remarks>
		public CompletionStatusType completionStatus;

		/// <summary>cmi.objectives.n.progress_measure</summary>
		/// <remarks>Measure of the progress the learner has made toward completing the objective</remarks>
		public float progressMeasure;

		/// <summary>cmi.objectives.n.description</summary>
		/// <remarks>Provides a brief informative description of the objective</remarks>
		public string description;
	}

	/// <summary>cmi._version</summary>
	/// <remarks>Represents the version of the data model</remarks>
	public string version { get; set; }

	/// <summary>cmi.comments_from_learner.X</summary>
	/// <remarks>The collection of comments made by the learner.</remarks>
	public List<CommentsFromLearner> commentsFromLearner { get; set; }

	/// <summary>cmi.comments_from_lms.X</summary>
	/// <remarks>The collection of comments from the LMS for this SCO.</remarks>
	public List<CommentsFromLMS> commentsFromLMS { get; set; }

	/// <summary>cmi.completion_status</summary>
	/// <remarks>Indicates whether the learner has completed the SCO</remarks>
	public CompletionStatusType completionStatus { get; set;}

	/// <summary>cmi.completion_threshold</summary>
	/// <remarks>Used to determine whether the SCO should be considered complete</remarks>
	public float completionThreshold { get; set; }

	/// <summary>cmi.credit</summary>
	/// <remarks>Indicates whether the learner will be credited for performance in the SCO</remarks>
	public CreditType credit { get; set; }

	/// <summary>cmi.entry</summary>
	/// <remarks>Asserts whether the learner has previously accessed the SCO</remarks>
	public EntryType entry { get; set; }

	/// <summary>cmi.cmi.interactions.n.X</summary>
	/// <remarks>The collection of interactions for this student</remarks>
	public List<LearnerInteractionRecord> interactions { get; set; }

	/// <summary>cmi.launch_data</summary>
	/// <remarks>Data provided to a SCO after launch, initialized from the dataFromLMS manifest element</remarks>
	public string launchData { get; set; }

	/// <summary>cmi.learner_id</summary>
	/// <remarks>Identifies the learner on behalf of whom the SCO was launched</remarks>
	public string learnerID { get; set; }

	/// <summary>cmi.learner_name</summary>
	/// <remarks>Name provided for the learner by the LMS</remarks>
	public string learnerName { get; set; }

	/// <summary>cmi.learner_preference.X</summary>
	/// <remarks>Aggregate object to store the learner preference items</remarks>
	public LearnerPreference learnerPreference { get; set; }

	/// <summary>cmi.score.</summary>
	/// <remarks>Aggregate object to store the learner's score for this SCO</remarks>
	public LearnerScore learnerScore { get; set; }

	/// <summary>cmi.location</summary>
	/// <remarks>The learner’s current location in the SCO.  Acts as a bookmark for when a student re-visits an incomplete SCO.  Set the location, and then set the exit type to 'suspend'.</remarks>
	public string location { get; set; }

	/// <summary>cmi.objectives.n.X</summary>
	/// <remarks>The collection of objectives for this SCO</remarks>
	public List<Objectives> objectives { get; set; }

	/// <summary>cmi.max_time_allowed</summary>
	/// <remarks>Amount of accumulated time the learner is allowed to use a SCO</remarks>
	public float maxTimeAllowed { get; set; }

	/// <summary>cmi.mode</summary>
	/// <remarks>Identifies one of three possible modes in which the SCO may be presented to the learner</remarks>
	public ModeType mode { get; set; }

	/// <summary>cmi.progress_measure</summary>
	/// <remarks>Measure of the progress the learner has made toward completing the SCO</remarks>
	public float progressMeasure { get; set; }

	/// <summary>cmi.scaled_passing_score</summary>
	/// <remarks>Scaled passing score required to master the SCO</remarks>
	public float scaledPassingScore { get; set; }

	/// <summary>cmi.success_status</summary>
	/// <remarks> Indicates whether the learner has mastered the SCO</remarks>
	public SuccessStatusType successStatus { get; set; }

	/// <summary>cmi.suspend_data</summary>
	/// <remarks>Provides space to store and retrieve data between learner sessions/remarks>
	public string suspendData  { get; set; }

	/// <summary>cmi.time_limit_action</summary>
	/// <remarks>Indicates what the SCO should do when cmi.max_time_allowed is exceeded</remarks>
	public TimeLimitActionType timeLimitAction { get; set; }

	/// <summary>cmi.total_time</summary>
	/// <remarks>Sum of all of the learner’s session times accumulated in the current learner attempt</remarks>
	public float totalTime { get; set; }

}