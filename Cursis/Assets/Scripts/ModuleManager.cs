using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

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

    //ReadChart() -- Reads the chart into Lua memory, returns boolean
    private const string api_ReadChart = "ReadChart";

    //DumpChart() -- "Dumps" the chart that was read into Lua memory, returns boolean
    private const string api_DumpChart = "DumpChart";

    //GetMetaInfo() -- Returns Lua table for the meta info (artist, charter, etc.) of the selected chart file (defaults to loaded)
    private const string api_GetMetaInfo = "GetMetaInfo";

	//GetAudioFile() -- Returns a string to the audio file of the loaded chart
	private const string api_GetAudioFile = "GetAudioFile";

	//GetBackground() -- Returns a string to the image/video file to put in the background
	private const string api_GetBackground = "GetBackground";

    //GetBPM() -- Returns the beats per minute (BPM) of the selected chart file (defaults to loaded)
    private const string api_GetBPM = "GetBPM";

    //NoteBeatTimes() -- Returns Lua table of the timing of each note in beats
    private const string api_NoteBeatTimes = "NoteBeatTimes";

    //NoteTypes() -- Returns Lua table of the type of each note (denoted by number)
    private const string api_NoteTypes = "NoteTypes";

    //NoteLanes() -- Returns Lua table of the lane each note travels down (denoted by number)
    private const string api_NoteLanes = "NoteLanes";

    //============
	// ATTRIBUTES
	//============

    //MODULE DYNAMIC VALUES (ATTRIBUTES & FUNCTIONS)
    private DynValue mod_Name = null;
    private DynValue mod_Creator = null;
    private DynValue mod_ChartExtension = null;
	private DynValue mod_ChartFolder = null;
    private DynValue mod_ReadChart = null;
    private DynValue mod_DumpChart = null;
    private DynValue mod_GetMetaInfo = null;
	private DynValue mod_GetAudioFile = null;
	private DynValue mod_GetBackground = null;
    private DynValue mod_GetBPM = null;
    private DynValue mod_NoteBeatTimes = null;
    private DynValue mod_NoteTypes = null;
    private DynValue mod_NoteLanes = null;

    //Other Attributes
	private const string readersLocation = "Readers/";
	private const string chartsLocation = "Charts/";
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

	//=========
	// METHODS
	//=========

    //Start()
	void Start ()
    {
        display.text = string.Empty;

        //TEST TEST TEST
		if (!System.IO.Directory.Exists (readersLocation))
		{
			display.text = "[Readers folder doesn't exist!]";
		}
		else
		{
			string[] fileToLoad = System.IO.Directory.GetFiles (readersLocation, "*.lua");
			if (fileToLoad.Length > 0)
			{
				LoadModule (fileToLoad [0]);
				chartLoaded = ReadChart (string.Empty);
				display.text = ModuleName + "\n" + ModuleCreator + "\n" + ModuleExtension;
			}
			else
			{
				display.text = "[no reader file in Readers folder]";
			}
		}
        //TEST TEST TEST
	}

    //LoadModule()
    public bool LoadModule(string moduleFile)
    {
        bool result = false;

        //Checking if another module is loaded
        if (ModuleLoaded)
            UnloadModule();

        //Read Lua script from disk
        module = new Script();
        FileStream reader = new FileStream(moduleFile, FileMode.Open);
        using (reader)
        {
            try
            {
                module.DoStream(reader);
                //Debug.Log(module.GetSourceCode(0).Code);
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
        mod_ReadChart = module.Globals.Get (api_ReadChart);
        mod_DumpChart = module.Globals.Get (api_DumpChart);
        mod_GetMetaInfo = module.Globals.Get (api_GetMetaInfo);
		mod_GetAudioFile = module.Globals.Get (api_GetAudioFile);
		mod_GetBackground = module.Globals.Get (api_GetBackground);
        mod_GetBPM = module.Globals.Get (api_GetBPM);
        mod_NoteBeatTimes = module.Globals.Get (api_NoteBeatTimes);
        mod_NoteTypes = module.Globals.Get (api_NoteTypes);
        mod_NoteLanes = module.Globals.Get (api_NoteLanes);

        //Returning the result
        return result;
    }

    //UnloadModule()
    public bool UnloadModule()
    {
        AbortCheck(false);

        bool result = false;

        result = module.Call(mod_DumpChart).Boolean;

        if (result)
        {
            module = null;
        }

        return result;
    }

    //ReadChart()
    public bool ReadChart(string chartLocation)
    {
        AbortCheck(false);

        //Reading the chart
        if (module.Call(mod_DumpChart).Boolean)
        {
            chartLoaded = module.Call(mod_ReadChart, DynValue.NewString(chartLocation)).Boolean;
        }

        return chartLoaded;
    }

    //EarlyAbort()
    private void AbortCheck(bool checkChartRead)
    {
        if (!ModuleLoaded)
            throw new Exception("MODULE NOT LOADED");

        if (checkChartRead && !chartLoaded)
            throw new Exception("CHART NOT LOADED");
    }

	//=========================
	// EXAMPLE MOON-SHARP CODE
	//=========================

    //Mul()
    int Mul(int a, int b)
    {
        return a * b;
    }

    //MoonSharpFactorial()
    double MoonSharpFactorial(int number)
    {
        string scriptCode = @"
        -- defines a factorial function
        function fact(n)
            if (n == 0) then
                return 1
            else
                return Mul(n, fact(n - 1));
            end
        end";
        
        Script script = new Script();

        script.Globals["Mul"] = (Func<int, int, int>) Mul;

        script.DoString(scriptCode);

        DynValue luaFunction = script.Globals.Get("fact");

        DynValue res = script.Call(luaFunction, DynValue.NewNumber(4));

        return res.Number;
    }

    //GetListFromLuaTable()
    List<double> GetListFromLuaTable()
    {
        List<double> result = new List<double>();

        string scriptCode = @"
        -- returns a table from LUA
        function notes()
            tb = {};
            tb[0] = 5.5;
            tb[1] = 6.5;
            tb[2] = 7.5;
            tb[3] = 8.5;
            return tb;
        end";

        Script script = new Script();
        script.DoString(scriptCode);
        DynValue luaFunction = script.Globals.Get("notes");
        Table tb = script.Call(luaFunction).Table;

        foreach (DynValue val in tb.Values)
        {
            result.Add(val.Number);
        }

        return result;
    }
}