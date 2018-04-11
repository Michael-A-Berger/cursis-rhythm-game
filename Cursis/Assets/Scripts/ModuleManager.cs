using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;

public class ModuleManager : MonoBehaviour
{
	//=============
    // MODULES API
    //=============

    //ModuleName -- Global string that lists the module name
    private const string api_ModuleName = "ModuleName";

    //ModuleCreator -- Global string that lists the module creator
    private const string api_ModuleCreator = "ModuleCreator";

    //ChartExtension -- Global string of the file extension used for chart files
    private const string api_ChartExtension = "ChartExtension";

	//ChartFolder -- Global string of the subfolder to search for chart files within "Charts"
	private const string api_ChartFolder = "ChartFolder";

    //ReadChartFile() -- Reads the chart into Lua memory, returns boolean
    private const string api_ReadChartFile = "ReadChartFile";

    //DumpChartFile() -- "Dumps" the chart that was read into Lua memory, returns boolean
    private const string api_DumpChartFile = "DumpChartFile";

    //GetMetaInfo() -- Returns Lua table for the meta info (artist, charter, etc.) of the selected chart file
    private const string api_GetMetaInfo = "GetMetaInfo";

	//GetAudioFile() -- Returns a string to the audio file of the loaded chart
	private const string api_GetAudioFile = "GetAudioFile";

	//GetBackground() -- Returns a string to the image/video file to put in the background
	private const string api_GetBackground = "GetBackground";

    //GetBPMs() -- Returns a table of the beats per minute (BPM) profiles of the selected chart file (defaults to loaded)
    private const string api_GetBPMs = "GetBPMs";

	//GetChartDifficulties() -- Returns Lua string table of the difficulties in the chart file
	private const string api_GetChartDifficulties = "GetChartDifficulties";

	//GetChartOffset() -- Returns Lua number for how many bars to skip before overlaying the note pattern
	private const string api_GetChartOffset = "GetChartOffset";

	//ReadChart() -- Reads a chart from the selected
	private const string api_ReadChart = "ReadChart";

    //NoteBeatTimes() -- Returns Lua number table of the timing of each note in beats
    private const string api_NoteBeatTimes = "GetNoteBeatTimes";

    //NoteTypes() -- Returns Lua table of the type of each note (denoted by number)
    private const string api_NoteTypes = "GetNoteTypes";

    //NoteLanes() -- Returns Lua table of the lane each note travels down (denoted by number)
    private const string api_NoteLanes = "GetNoteLanes";

    //============
	// ATTRIBUTES
	//============

    //MODULE DYNAMIC VALUES (ATTRIBUTES & FUNCTIONS)
    private DynValue mod_Name = null;
    private DynValue mod_Creator = null;
    private DynValue mod_ChartExtension = null;
	private DynValue mod_ChartFolder = null;
    private DynValue mod_ReadChartFile = null;
    private DynValue mod_DumpChartFile = null;
    private DynValue mod_GetMetaInfo = null;
	private DynValue mod_GetAudioFile = null;
	private DynValue mod_GetBackground = null;
    private DynValue mod_GetBPMs = null;
	private DynValue mod_GetChartDifficulties = null;
	private DynValue mod_GetChartOffset = null;
	private DynValue mod_ReadChart = null;
    private DynValue mod_NoteBeatTimes = null;
    private DynValue mod_NoteTypes = null;
    private DynValue mod_NoteLanes = null;

    //Other Attributes
	private const string readersLocation = "Readers\\";
	private const string chartsLocation = "Charts\\";
    public Text display;
    private Script module = null;
    private bool chartLoaded;

	//===========
    // ACCESSORS
	//===========
    public bool ModuleLoaded
    {
        get
        { return (module != null); }
    }
    public string ModuleName
    {
        get
        {
            AbortCheck(false);
            if (mod_Name.Type == DataType.String)
                return mod_Name.String;
            else
                return "[module name is not a string]";
        }
    }
    public string ModuleCreator
    {
        get
        {
            AbortCheck(false);
            if (mod_Creator.Type == DataType.String)
                return mod_Creator.String;
            else
                return "[creator name is not a string]";
        }
    }
    public string ModuleExtension
    {
        get
        {
            AbortCheck(false);
            if (mod_ChartExtension.Type == DataType.String)
                return mod_ChartExtension.String;
            else
                return "[chart extension is not a string]";
        }
    }
	public string ModuleFolder
	{
		get
		{
			AbortCheck(false);
			if (mod_ChartFolder.Type == DataType.String)
				return mod_ChartFolder.String;
			else
				return "[chart folder is not a string]";
		}
	}

	//=================
	// MANAGER METHODS
	//=================

    //Start()
	void Start ()
    {
		Script.DefaultOptions.DebugPrint = s => Debug.Log ("~LUA~\t" + s);
		if (Script.GlobalOptions.Platform.GetType() == typeof(LimitedPlatformAccessor))
		{
			Script.GlobalOptions.Platform = new StandardPlatformAccessor ();
			Debug.Log ("Changed from Limited to Standard Platform Accessor");
		}

        display.text = string.Empty;
	}

	//GetChartCount()
	public int GetChartCount()
	{
		AbortCheck (false);

		int result = 0;

		try
		{
			string[] chartFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Charts\\" + ModuleFolder + "\\", "*" + ModuleExtension, SearchOption.AllDirectories);
			result = chartFiles.Length;
		}
		catch (Exception e)
		{
			throw e;
		}

		return result;
	}

	//GetAllCharts9)
	public string[] GetAllCharts()
	{
		string[] chartFiles;

		try
		{
			chartFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Charts\\" + ModuleFolder + "\\", "*" + ModuleExtension, SearchOption.AllDirectories);
		}
		catch (Exception e)
		{
			throw e;
		}
		
		return chartFiles;
	}

	//EarlyAbort()
	private void AbortCheck(bool checkChartRead)
	{
		if (!ModuleLoaded)
			throw new Exception("MODULE NOT LOADED");

		if (checkChartRead && !chartLoaded)
			throw new Exception("CHART NOT LOADED");
	}

	//LoadModule()
	public bool LoadModule(string moduleFile)
	{
		bool result = false;

		//Checking if another module is loaded
		if (ModuleLoaded)
			UnloadModule();

		//Read Lua script from disk
		module = new Script(CoreModules.Preset_Default);
		FileStream reader = new FileStream(moduleFile, FileMode.Open);
		using (reader)
		{
			try
			{
				module.DoStream(reader);
				reader.Close();
				result = true;
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		//Setting the module associations
		mod_Name = module.Globals.Get (api_ModuleName);
		mod_Creator = module.Globals.Get (api_ModuleCreator);
		mod_ChartExtension = module.Globals.Get (api_ChartExtension);
		mod_ChartFolder = module.Globals.Get (api_ChartFolder);
		mod_ReadChartFile = module.Globals.Get (api_ReadChartFile);
		mod_DumpChartFile = module.Globals.Get (api_DumpChartFile);
		mod_GetMetaInfo = module.Globals.Get (api_GetMetaInfo);
		mod_GetAudioFile = module.Globals.Get (api_GetAudioFile);
		mod_GetBackground = module.Globals.Get (api_GetBackground);
		mod_GetBPMs = module.Globals.Get (api_GetBPMs);
		mod_GetChartDifficulties = module.Globals.Get (api_GetChartDifficulties);
		mod_GetChartOffset = module.Globals.Get (api_GetChartOffset);
		mod_ReadChart = module.Globals.Get (api_ReadChart);
		mod_NoteBeatTimes = module.Globals.Get (api_NoteBeatTimes);
		mod_NoteTypes = module.Globals.Get (api_NoteTypes);
		mod_NoteLanes = module.Globals.Get (api_NoteLanes);

		//Returning the result
		return result;
	}

	//UnloadModule()
	public bool UnloadModule()
	{
		bool result = true;

		if (ModuleLoaded)
		{
			result = module.Call(mod_DumpChartFile).Boolean;

			if (result)
			{
				module = null;
			}
			else
			{
				throw new Exception ("ERROR: LUA UNABLE TO DUMP CHART");
			}
		}

		return result;
	}

	public List<float>[] CalculateNotes()
	{
		AbortCheck (true);
		
		//Creating the array of float lists
		List<float>[] notes = new List<float>[8];
		for (int num = 0; num < 8; num++)
			notes[num] = new List<float>();

		//Getting the note data
		double[] note_BeatTimes = GetBeatTimes();
		double[] note_Lanes = GetNoteLanes ();
		double[] note_Types = GetNoteTypes ();

		//Getting the BPMs
		Vector2[] bpms = GetBPMs();			//X = Beat of BPM Change, Y = BPM value

		//Creating the current time, current beat, and BPM holding variables
		float passedTime = 0f;
		float passedBeats = 0f;
		float currentBPM = bpms [0].y;
		int bpmsCounter = 0;
		float crotchet = 60f / currentBPM;

		//Iterating through every note
		for (int num = 0; num < note_BeatTimes.Length; num++)
		{
			//Updating the BPMs counter + passed beats/time
			if (bpmsCounter < bpms.Length - 1 && note_BeatTimes[num] > bpms[bpmsCounter + 1].x)
			{
				//Calculating the passed beats
				float beatsToMultiplyBy = bpms [bpmsCounter + 1].x - bpms [bpmsCounter].x;

				//Incrementing the passed beats and time
				passedBeats += beatsToMultiplyBy;
				passedTime += crotchet * beatsToMultiplyBy;

				//Increamenting the beats counter
				bpmsCounter++;

				//Recalculating the crotchet
				currentBPM = bpms[bpmsCounter].y;
				crotchet = 60f / currentBPM;
			}

			//Adding the current note to the float list array
			if (note_Types [num] == 0)
			{
				float time = passedTime + crotchet * ((float)note_BeatTimes [num] - passedBeats);
				notes [(int)note_Lanes [num]].Add (time);
				//Debug.Log (crotchet + "\t" + time);
			}
		}

		return notes;
	}

	//================
	// MODULE METHODS
	//================

	//ReadChartFile()
	public bool ReadChartFile(string chartLocation)
	{
		AbortCheck(false);

		chartLoaded = module.Call(mod_ReadChartFile, DynValue.NewString(chartLocation)).Boolean;

		return chartLoaded;
	}

	public bool DumpChartFile()
	{
		AbortCheck (false);

		chartLoaded = !module.Call (mod_DumpChartFile).Boolean;

		return !chartLoaded;
	}

	//GetChartMetaInfo()
	public Dictionary<string, string> GetChartMetaInfo()
	{
		AbortCheck (true);
		
		Dictionary<string, string> metaInfo = new Dictionary<string, string>();

		Table metaTable = module.Call (mod_GetMetaInfo).Table;

		List<DynValue> metaKeys = new List<DynValue>(metaTable.Keys);
		List<DynValue> metaValues = new List<DynValue>(metaTable.Values);

		for (int num = 0; num < metaKeys.Count; num++)
			metaInfo.Add (metaKeys[num].String.ToLower(), metaValues[num].String);
		
		return metaInfo;
	}

	//GetAudioFile()
	public string GetAudioFile()
	{
		AbortCheck (true);

		string audioFile = module.Call (mod_GetAudioFile).String;
		
		return audioFile;
	}

	//GetBackground() -- Returns a string to the image/video file to put in the background
	public string GetBackground()
	{
		AbortCheck (true);

		string bg = module.Call(mod_GetBackground).String;

		return bg;
	}

	//GetBPMs() -- Returns a Vector2 array of the beats per minute (BPM) profiles of the selected chart file
	public Vector2[] GetBPMs()
	{
		AbortCheck (true);

		Vector2[] bpms;

		Table chartBPMs = module.Call (mod_GetBPMs).Table;

		List<DynValue> bpmTimes = new List<DynValue>(chartBPMs.Keys);
		List<DynValue> bpmValues = new List<DynValue>(chartBPMs.Values);

		bpms = new Vector2[bpmTimes.Count];

		for (int num = 0; num < bpmTimes.Count; num++)
		{
			bpms [num] = new Vector2 ((float) bpmTimes [num].Number, (float) bpmValues [num].Number);
		}

		return bpms;
	}

	//GetChartDifficulties
	public string[] GetChartDifficulties()
	{
		AbortCheck (false);

		List<DynValue> diffTable = new List<DynValue>(module.Call (mod_GetChartDifficulties).Table.Values);
		string[] chartDifficulties = new string[diffTable.Count];
		for (int num = 0; num < diffTable.Count; num++)
		{
			chartDifficulties [num] = diffTable [num].String;
		}
			
		return chartDifficulties;
	}

	public double GetChartOffset()
	{
		AbortCheck (true);
		
		double offset = module.Call(mod_GetChartOffset).Number;
		
		return offset;
	}

	//ReadChartData()
	public bool ReadChartData(string difficulty)
	{
		AbortCheck (false);

		bool result = false;

		if (module.Call (mod_ReadChart, DynValue.NewString (difficulty)).Boolean)
		{
			result = true;
		}

		return result;
	}

	public double[] GetBeatTimes()
	{
		AbortCheck (true);

		List<DynValue> dynBeatTimes = new List<DynValue>(module.Call(mod_NoteBeatTimes).Table.Values);
		double[] beatTimes = new double[dynBeatTimes.Count];
		for (int num = 0; num < dynBeatTimes.Count; num++)
			beatTimes [num] = dynBeatTimes [num].Number;
		
		return beatTimes;
	}

	public double[] GetNoteTypes()
	{
		AbortCheck (true);

		List<DynValue> dynNoteTypes = new List<DynValue>(module.Call(mod_NoteTypes).Table.Values);
		double[] noteTypes = new double[dynNoteTypes.Count];
		for (int num = 0; num < dynNoteTypes.Count; num++)
			noteTypes [num] = dynNoteTypes [num].Number;

		return noteTypes;
	}

	public double[] GetNoteLanes()
	{
		AbortCheck (true);

		List<DynValue> dynNoteLanes = new List<DynValue>(module.Call(mod_NoteLanes).Table.Values);
		double[] noteLanes = new double[dynNoteLanes.Count];
		for (int num = 0; num < dynNoteLanes.Count; num++)
			noteLanes [num] = dynNoteLanes [num].Number;

		return noteLanes;
	}
}