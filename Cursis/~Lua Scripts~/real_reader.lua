-- Module Constants
ModuleName = "SM Reader v0.5";
ModuleCreator = "Michael Berger";
ChartExtension = ".sm";
ChartFolder = "Stepmania Simfiles";

-- Chart Variables
chartFile = "";					-- String
chart_Meta = {};				-- Table (of strings)
chart_AudioFile = "";			-- String (to resource)
chart_BG = "";					-- String (to resource)
chart_BPMs = {};				-- Table (of numbers)
chart_Difficulties = {};		-- Table (of strings)
chart_Offset = 0;				-- Number
chart_BeatTimes = {};			-- Table (of numbers)
chart_NoteTypes = {};			-- Table (of numbers)
chart_NoteLanes = {};			-- Table (of numbers)

-- ==========================
-- ===== MODULE METHODS =====
-- ==========================

-- ReadChartFile()
function ReadChartFile(fileLocation)
	result = readFile(fileLocation);
	return result;
end

-- DumpChartFile()
function DumpChartFile()
	result = true;

	-- Clearing the properties
	chartFile = "";
	chart_AudioFile = "";
	chart_BG = "";
	chart_Offset = 0;

	-- Clearing the tables
	chart_Meta = {};
	chart_BPMs = {};
	chart_Difficulties = {};
	chart_BeatTimes = {};
	chart_NoteTypes = {};
	chart_NoteLanes = {};

	return result;
end

-- GetMetaInfo()
function GetMetaInfo()
	return chart_Meta;
end

-- GetAudioFile()
function GetAudioFile()
	return chart_AudioFile;
end

-- GetBackground()
function GetBackground()
	return chart_BG;
end

-- GetBPMs()
function GetBPMs()
	local headerSection = chartFile:sub(1, chartFile:find("#NOTES") - 1);
	local bpmString = "";
	local bpmLocation = headerSection:find("#BPMS:");
	local bpmMeasure = 0;
	local bpmValue = 0;
	local equalsLocation = 0;

	bpmString = headerSection:sub(bpmLocation + 6):gsub("\n", "");
	bpmString = bpmString:sub(1, bpmString:find(";") - 1):gsub(",", " "):gmatch("%S+");

	for bpm in bpmString do
		equalsLocation = bpm:find("=");

		if (equalsLocation ~= nil) then
			bpmMeasure = tonumber(bpm:sub(1, equalsLocation - 1));
			bpmValue = tonumber(bpm:sub(equalsLocation + 1));
			chart_BPMs[bpmMeasure] = bpmValue;
		end
	end

	return chart_BPMs;
end

-- GetChartDifficulties()
function GetChartDifficulties()
	local currentChart = string.find(chartFile, "#NOTES:");
	local myFile = string.sub(chartFile, 1);
	local nextLinePos = 0;
	local currentLine = "";
	local currentDifficulty = "";
	local difficultyIterator = 1;
	local doNotAdd = false;

	while (currentChart ~= nil) do
		myFile = string.sub(myFile, currentChart + 8);

		nextLinePos = string.find(myFile, "\n,\n");
		currentLine = string.sub(myFile, 1, nextLinePos);

		if (currentLine:find("dance%-single") ~= nil) then
			currentDifficulty = "Single ";
		elseif (currentLine:find("dance%-double") ~= nil) then
			currentDifficulty = "Double ";
		else
			doNotAdd = true;
		end

		for num=1,3,1 do
			nextLinePos = string.find(currentLine, ":\n");
			currentLine = string.sub(currentLine, nextLinePos + 1);
		end

		nextLinePos = string.find(currentLine, ":\n");
		currentLine = string.sub(currentLine, 1, nextLinePos - 1);
		currentLine = string.gsub(currentLine, "%s", "");

		currentDifficulty = currentDifficulty .. currentLine;

		if (not doNotAdd) then
			table.insert(chart_Difficulties, difficultyIterator, currentDifficulty);
			difficultyIterator = difficultyIterator + 1;
		end

		--print(currentDifficulty);

		currentChart = string.find(myFile, "#NOTES:");
	end


	return chart_Difficulties;
end

-- GetChartOffset()
function GetChartOffset()
	return chart_Offset;
end

-- ReadChart()
function ReadChart(chartToRead)
	result = readChartFromDifficulty(chartToRead);
	return result;
end

-- GetNoteBeatTimes()
function GetNoteBeatTimes()
	return chart_BeatTimes;
end

-- GetNoteTypes()
function GetNoteTypes()
	return chart_NoteTypes;
end

-- GetNoteLanes()
function GetNoteLanes()
	return chart_NoteLanes;
end

-- ==========================
-- ===== CUSTOM METHODS =====
-- ==========================

-- readFile()
function readFile(fileToRead)
	result = true;

	local file,err = io.open(fileToRead, "r");
	if file==nil then
		result = false;
		print(err);
	end

	if result then
		io.input(file);
		chartFile = io.read("*all");
		chartFile = chartFile:gsub("\r\n?", "\n");
		readHeaderSection();

		--print(chartFile);
	end

	file:close();

	return result;
end

-- readHeaderSection()
function readHeaderSection()
	-- Local variables
	local headerSection = chartFile:sub(1, chartFile:find("#NOTES") - 1):gmatch("(.-)\n");
	local matchFound = 0;

	for line in headerSection do

		matchFound = line:find("#TITLE:");
		if (matchFound ~= nil) then
			chart_Meta["Title"] = line:sub(matchFound + 7, line:find(";") - 1);
		end

		matchFound = line:find("#ARTIST:");
		if (matchFound ~= nil) then
			chart_Meta["Artist"] = line:sub(matchFound + 8, line:find(";") - 1);
		end

		matchFound = line:find("#CREDIT:");
		if (matchFound ~= nil) then
			chart_Meta["Creator"] = line:sub(matchFound + 8, line:find(";") - 1);
		end

		matchFound = line:find("#DISPLAYBPM:");
		if (matchFound ~= nil) then
			chart_Meta["Display BPM"] = line:sub(matchFound + 12, line:find(";") - 1);
		end

		matchFound = line:find("#BACKGROUND:");
		if (matchFound ~= nil) then
			chart_BG = line:sub(matchFound + 12, line:find(";") - 1);
		end

		matchFound = line:find("#MUSIC:");
		if (matchFound ~= nil) then
			chart_AudioFile = line:sub(matchFound + 7, line:find(";") - 1);
		end

		matchFound = line:find("#OFFSET:");
		if (matchFound ~= nil) then
			chart_Offset = tonumber(line:sub(matchFound + 8, line:find(";") - 1));
		end

	end

end

-- readChartFromDifficulty()
function readChartFromDifficulty(difficulty)
	local result = true;
	local currentChart = nil;
	local diffNumber = difficulty:sub(difficulty:find(" ") + 1);
	local chartType = "";
	local chartLoc = 0;

	if (difficulty:find("Single") ~= nil) then
		chartType = "dance%-single";
	elseif (difficulty:find("Double") ~= nil) then
		chartType = "dance%-double";
	end

	-- Getting the chart difficluties
	local allChartDifficulties = chartFile:gmatch("#NOTES:.-\n,\n");

	-- Getting the appropriate chart
	for diff in allChartDifficulties do
		currentChart = diff:match(chartType .. ":.*:.*:.*" .. diffNumber .. ":.*:");

		if (currentChart ~= nil) then
			break;
		end
	end

	chartLoc = chartFile:find(currentChart, 1, true);
	currentChart = chartFile:sub(chartLoc);
	local chartFluff = currentChart:sub(currentChart:find(".*:.*:.*:.*:.*:"), currentChart:find("%d*\n%d*\n") - 1);
	currentChart = currentChart:sub(chartFluff:len() + 2, currentChart:find(";"));

	-- Splicing the chart along the commas
	local chartMeasures = currentChart:gmatch("(.-)\n[,;]");

	-- Processing each measure
	local beatTime = 0;
	for measure in chartMeasures do
		local beatLineFunc = measure:gsub("\n", " "):gmatch("%S+");
		local beatStrings = {};
		local beatStringsNumber = 0;
		for ln in beatLineFunc do
			beatStringsNumber = beatStringsNumber + 1;
			table.insert(beatStrings, ln);
		end

		local incrementPerLine = 1 / (beatStringsNumber / 4);

		for _,ln in ipairs(beatStrings) do
			if (chartType == "dance%-single") then
				processNoteString_Single(ln, beatTime);
			elseif  (chartType == "dance%-double") then
				processNoteString_Double(ln, beatTime);
			end
			beatTime = beatTime + incrementPerLine;
		end
	end

	return result;
end

-- processNoteString_Single()
function processNoteString_Single(str, beat)
	if (str:match("1111") ~= nil) then				--X Pattern
		for i=1,4 do
			table.insert(chart_BeatTimes, beat);
			table.insert(chart_NoteTypes, 0);
		end
		table.insert(chart_NoteLanes, 1);
		table.insert(chart_NoteLanes, 3);
		table.insert(chart_NoteLanes, 5);
		table.insert(chart_NoteLanes, 7);
	elseif (str:match("111.") ~= nil) then		--Y (From Left)
		for i=1,3 do
			table.insert(chart_BeatTimes, beat);
			table.insert(chart_NoteTypes, 0);
		end
		table.insert(chart_NoteLanes, 0);
		table.insert(chart_NoteLanes, 3);
		table.insert(chart_NoteLanes, 5);
	elseif (str:match("1.11") ~= nil) then		--Y (From Below)
		for i=1,3 do
			table.insert(chart_BeatTimes, beat);
			table.insert(chart_NoteTypes, 0);
		end
		table.insert(chart_NoteLanes, 2);
		table.insert(chart_NoteLanes, 5);
		table.insert(chart_NoteLanes, 7);
	elseif (str:match(".111") ~= nil) then		--Y (From Right)
		for i=1,3 do
			table.insert(chart_BeatTimes, beat);
			table.insert(chart_NoteTypes, 0);
		end
		table.insert(chart_NoteLanes, 1);
		table.insert(chart_NoteLanes, 4);
		table.insert(chart_NoteLanes, 7);
	elseif (str:match("11..") ~= nil) then		--Lower-Left
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 5);
	elseif (str:match("1.1.") ~= nil) then		--Upper-Left
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 3);
	elseif (str:match("1..1") ~= nil) then		--Left & Right
		for i=1,2 do
			table.insert(chart_BeatTimes, beat);
			table.insert(chart_NoteTypes, 0);
		end
		table.insert(chart_NoteLanes, 0);
		table.insert(chart_NoteLanes, 4);
	elseif (str:match(".11.") ~= nil) then		--Up & Down
		for i=1,2 do
			table.insert(chart_BeatTimes, beat);
			table.insert(chart_NoteTypes, 0);
		end
		table.insert(chart_NoteLanes, 2);
		table.insert(chart_NoteLanes, 6);
	elseif (str:match(".1.1") ~= nil) then		--Lower-Right
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 7);
	elseif (str:match("..11") ~= nil) then		--Upper-Right
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 1);
	elseif (str:match("1...") ~= nil) then		--Left
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 4);
	elseif (str:match(".1..") ~= nil) then		--Down
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 6);
	elseif (str:match("..1.") ~= nil) then		--Up
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 2);
	elseif (str:match("...1") ~= nil) then		--Right
		table.insert(chart_BeatTimes, beat);
		table.insert(chart_NoteTypes, 0);
		table.insert(chart_NoteLanes, 0);
	end
end

-- processNoteString_Double()
function processNoteString_Double(str, beat)

end

-- printChartInfo()
function printChartInfo()
	for i=1,table.getn(chart_BeatTimes) do
		print("#" .. i .. ":", chart_NoteTypes[i], chart_NoteLanes[i], chart_BeatTimes[i]);
	end
end

--[===[
-- TEST TEST TEST TEST TEST
chartToRead = "../Charts/" .. ChartFolder .. "/DDR Supernova 2 (AC)/Bloody Tears (IIDX EDITION)/Bloody Tears (IIDX EDITION).sm";
--chartToRead = "../Charts/" .. ChartFolder .. "/O2Jam MIX/Cross Time/Cross Time.sm";
chartDiff = "Single 10";
print(chartToRead .. "\n" .. chartDiff);
ReadChartFile(chartToRead);

readHeaderSection();

--readChartFromDifficulty(chartDiff);

--printChartInfo();

--print(GetBackground());
--print(GetAudioFile());
--print(GetChartOffset());

testDifficulties = GetChartDifficulties();
for key,value in pairs(testDifficulties) do
	print(key .. ":\t" .. value);
end

--testMeta = GetMetaInfo();
--for key,value in pairs(testMeta) do
	--print(key .. ":   " .. value);
--end

--testBPMs = GetBPMs();
--for key,value in pairs(testBPMs) do
	--print(key .. ":   " .. value);
--end


-- TEST TEST TEST TEST TEST
--[===[
--]===]














