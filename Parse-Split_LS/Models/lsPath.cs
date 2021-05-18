using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Parse_Split_LS.Models
{
    public class lsPath
    {
        string OWNER, COMMENT, CREATE, MODIFIED, FILE_NAME, PROTECT, DEFAULT_GROUP, CONTROL_CODE;
        int PROG_SIZE, VERSION, LINE_COUNT, MEMORY_SIZE;
        lsTcd TCD;
        //int cUFRAME_NUM, cUTOOL_NUM;
        Dictionary<int, LsPathStep> dctSteps = new Dictionary<int, LsPathStep>();
        Dictionary<int, lsPathPoint> dctPoints = new Dictionary<int, lsPathPoint>();
        string fullPointToParse;
        int currentReadingPoint;
        bool inReadingPoint = false; //During parsing several lines for points, true, when we have encountered the point start

        MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();



        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 if ok, otherwise -1</returns>
        public int ParseFile(string fullPathName)
        {
            int cLine = 0;
            lsSection lsPathSection = lsSection.HeaderInitial;
            int anyInt;
            // 0: Header
            // 1: Header TCD
            // 2: Header after TCD
            // 3

            //string fullPathName = @"F:\NikosF\SW\repos\Parse-Split_LS/rCAM_Bumper_.ls";

            try
            {

                using (TextReader rdr = new StreamReader(fullPathName))
                {
                    string line;
                    string patternInitialHeader, patternCheckToFallInTCD, patternInTCD, patternCheckEndTCD, patternInHeaderAfterTCD;

                    while ((line = rdr.ReadLine()) != null)
                    {
                        cLine++;
                        if (line[0] == '/')
                        {
                            // We move to Steps after we encounter CONTROL_CODE in Header after TCD
                            //if ((lsPathSection == lsSection.HeaderAfterTCD) && (line == "//MN"))
                            //    lsPathSection = lsSection.Steps;
                            // we have a comment. Drop it
                            continue;
                        }

                        switch (lsPathSection)
                        {
                            case lsSection.HeaderInitial: // In Header
                                                          //we first search for state change
                                patternCheckToFallInTCD = @"TCD:(.*)";
                                Match resultCheckToFallInTCD = Regex.Match(line, patternCheckToFallInTCD);
                                if (resultCheckToFallInTCD.Success)
                                {
                                    line = resultCheckToFallInTCD.Groups[1].Value;
                                    lsPathSection = lsSection.HeaderTCD;
                                    goto case lsSection.HeaderTCD;
                                }

                                patternInitialHeader = @"([_a-zA-Z]+)\s*=\s*(.+)[ \t]*;";
                                Match resultInitialHeader = Regex.Match(line, patternInitialHeader);

                                bool isInt = int.TryParse(resultInitialHeader.Groups[2].Value, out anyInt);

                                if (resultInitialHeader.Success)
                                {
                                    switch (resultInitialHeader.Groups[1].Value)
                                    {
                                        case "OWNER":
                                            OWNER = resultInitialHeader.Groups[2].Value;
                                            break;
                                        case "COMMENT":
                                            COMMENT = resultInitialHeader.Groups[2].Value;
                                            break;
                                        case "PROG_SIZE":
                                            if (isInt)
                                                PROG_SIZE = anyInt;
                                            else
                                                throw new InvalidOperationException("There is no integer in Group[2]");
                                            break;
                                        case "CREATE":
                                            CREATE = resultInitialHeader.Groups[2].Value;
                                            break;
                                        case "MODIFIED":
                                            MODIFIED = resultInitialHeader.Groups[2].Value;
                                            break;
                                        case "FILE_NAME":
                                            FILE_NAME = resultInitialHeader.Groups[2].Value;
                                            break;
                                        case "VERSION":
                                            if (isInt)
                                                VERSION = anyInt;
                                            else
                                                throw new InvalidOperationException("There is no integer in Group[2]");
                                            break;
                                        case "LINE_COUNT":
                                            if (isInt)
                                                LINE_COUNT = anyInt;
                                            else
                                                throw new InvalidOperationException("There is no integer in Group[2]");
                                            break;
                                        case "MEMORY_SIZE":
                                            if (isInt)
                                                MEMORY_SIZE = anyInt;
                                            else
                                                throw new InvalidOperationException("There is no integer in Group[2]");
                                            break;
                                        case "PROTECT":
                                            PROTECT = resultInitialHeader.Groups[2].Value;
                                            break;
                                        default:
                                            throw new InvalidOperationException("Not recognized token in Header Initial");
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException("Not recognized line in Header Initial");
                                }
                                break;

                            case lsSection.HeaderTCD:
                                patternCheckEndTCD = @"([_a-zA-Z]+)\s*=\s*(.+)[ \t]*;";
                                Match resultInTCD = Regex.Match(line, patternCheckEndTCD);
                                if (resultInTCD.Success)
                                    lsPathSection = lsSection.HeaderAfterTCD;
                                else
                                {
                                    patternInTCD = @"([_a-zA-Z]+)\s*=\s*(.+)[ \t]*,";
                                    resultInTCD = Regex.Match(line, patternInTCD);
                                }

                                if (resultInTCD.Success)
                                {
                                    if (!int.TryParse(resultInTCD.Groups[2].Value, out anyInt))
                                        throw new InvalidOperationException("There is no integer in Group[2]");
                                    switch (resultInTCD.Groups[1].Value)
                                    {
                                        case "STACK_SIZE":
                                            TCD.STACK_SIZE = anyInt;
                                            break;
                                        case "TASK_PRIORITY":
                                            TCD.TASK_PRIORITY = anyInt;
                                            break;
                                        case "TIME_SLICE":
                                            TCD.TIME_SLICE = anyInt;
                                            break;
                                        case "BUSY_LAMP_OFF":
                                            TCD.BUSY_LAMP_OFF = anyInt;
                                            break;
                                        case "ABORT_REQUEST":
                                            TCD.ABORT_REQUEST = anyInt;
                                            break;
                                        case "PAUSE_REQUEST":
                                            TCD.PAUSE_REQUEST = anyInt;
                                            break;
                                    }
                                }
                                break;
                            case lsSection.HeaderAfterTCD:
                                patternInHeaderAfterTCD = @"([_a-zA-Z]+)\s*=\s*(.+)[ \t]*;";
                                Match resultInHeaderAfterTCD = Regex.Match(line, patternInHeaderAfterTCD);

                                if (resultInHeaderAfterTCD.Success)
                                {
                                    if (resultInHeaderAfterTCD.Groups[1].Value == "CONTROL_CODE")
                                    {
                                        lsPathSection = lsSection.Steps;
                                    }

                                    switch (resultInHeaderAfterTCD.Groups[1].Value)
                                    {
                                        case "DEFAULT_GROUP":
                                            DEFAULT_GROUP = resultInHeaderAfterTCD.Groups[2].Value;
                                            break;
                                        case "CONTROL_CODE":
                                            CONTROL_CODE = resultInHeaderAfterTCD.Groups[2].Value;
                                            break;
                                    }
                                }
                                break;


                            case lsSection.Steps:
                                string patMainInSteps;
                                string stepBody;
                                char stepType;
                                Match resMainInSteps;
                                int stepNr;
                                int pointNr;

                                patMainInSteps = @"(\d+):(.) (.*)";
                                resMainInSteps = Regex.Match(line, patMainInSteps);
                                if (!resMainInSteps.Success)
                                {
                                    lsPathSection = lsSection.Points;
                                    goto case lsSection.Points;
                                }

                                if (!int.TryParse(resMainInSteps.Groups[1].Value, out stepNr))
                                    throw new InvalidOperationException("Not valid Step Number: " + resMainInSteps.Groups[1].Value);
                                stepType = resMainInSteps.Groups[2].Value[0];
                                stepBody = resMainInSteps.Groups[3].Value;

                                switch (stepType)
                                {
                                    case '!':
                                        // We have a comment
                                        dctSteps.Add(stepNr, new LsPathStep(stepType, stepBody, 0));
                                        break;
                                    case ' ':
                                        // We have a command
                                        dctSteps.Add(stepNr, new LsPathStep(stepType, stepBody, 0));
                                        break;
                                    case 'J':
                                    case 'L':
                                        stepBody = stepBody.Replace(@"\", @"\\");
                                        // I keep the ; into the end of the body and I do not add it during file creation
                                        Match resPathStep = Regex.Match(stepBody, @"P[[](\d+)[]]\s*(.*)$");
                                        if (!resPathStep.Success)
                                            throw new InvalidOperationException("Cannot Parse Step Path Body: " + stepBody);
                                        if (!int.TryParse(resPathStep.Groups[1].Value, out pointNr))
                                            throw new InvalidOperationException("Not valid Point Number: " + resPathStep.Groups[1].Value);

                                        // We have a Path Step
                                        dctSteps.Add(stepNr, new LsPathStep(stepType, resPathStep.Groups[2].Value, pointNr));
                                        break;
                                    default:
                                        throw new InvalidOperationException("Unidentified step type ");
                                }

                                break;

                            case lsSection.Points:
                                if (!inReadingPoint)
                                {
                                    Match resStartPoint = Regex.Match(line, @"P[[](\d+)[]]\s*{$");
                                    if (resStartPoint.Success)
                                    {
                                        if (!int.TryParse(resStartPoint.Groups[1].Value, out currentReadingPoint))
                                            throw new InvalidOperationException("Not valid Point Number: " + resStartPoint.Groups[1].Value);
                                        fullPointToParse = "";
                                        inReadingPoint = true;
                                    }
                                }
                                else
                                {
                                    Match resEndPoint = Regex.Match(line, @"};$");
                                    if (resEndPoint.Success)
                                    {

                                        Match result, result2;
                                        // parse fullPointToParse
                                        lsPathPoint cPathPoint = new lsPathPoint();
                                        fullPointToParse = fullPointToParse.Replace("\t", "");
                                        // UF, UT
                                        result = Regex.Match(fullPointToParse, @"UF\s*:\s*(\d+),\s*UT\s*:\s*(\d+)");
                                        if (!((result.Success) &&
                                            (int.TryParse(result.Groups[1].Value, out cPathPoint.UF)) &&
                                            (int.TryParse(result.Groups[2].Value, out cPathPoint.UT))
                                            ))
                                            throw new InvalidOperationException("Cant find UF, or UT " + fullPointToParse);

                                        // J1, J2, J3, J4, J5, J6, X, Y, Z, W, P, R
                                        result = Regex.Match(fullPointToParse, @"J1\s*=\s*(-?\d+\.\d+)([^J]*)J2\s*=\s*(-?\d+\.\d+)([^J]*)J3\s*=\s*(-?\d+\.\d+)([^J]*)J4\s*=\s*(-?\d+\.\d+)([^J]*)J5\s*=\s*(-?\d+\.\d+)([^J]*)J6\s*=\s*(-?\d+\.\d+)");
                                        result2 = Regex.Match(fullPointToParse, @"CONFIG\s*:\s*'(.+)'([^X]*) X\s*=\s*(-?\d+\.\d+)([^Y]*)Y\s*=\s*(-?\d+\.\d+)([^Z]*)Z\s*=\s*(-?\d+\.\d+)([^W]*)W\s*=\s*(-?\d+\.\d+)([^P]*)P\s*=\s*(-?\d+\.\d+)([^R]*)R\s*=\s*(-?\d+\.\d+)");
                                        if (result.Success)
                                        {

                                            if (!(
                                            (double.TryParse(result.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.J1)) &&
                                            (double.TryParse(result.Groups[3].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.J2)) &&
                                            (double.TryParse(result.Groups[5].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.J3)) &&
                                            (double.TryParse(result.Groups[7].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.J4)) &&
                                            (double.TryParse(result.Groups[9].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.J5)) &&
                                            (double.TryParse(result.Groups[11].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.J6))
                                            ))
                                                throw new InvalidOperationException("Cant parse J Values");

                                        }
                                        else if (result2.Success)
                                        {
                                            cPathPoint.CONFIG = result2.Groups[1].Value;
                                            if (!(
                                                (double.TryParse(result2.Groups[3].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.X)) &&
                                                (double.TryParse(result2.Groups[5].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.Y)) &&
                                                (double.TryParse(result2.Groups[7].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.Z)) &&
                                                (double.TryParse(result2.Groups[9].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.W)) &&
                                                (double.TryParse(result2.Groups[11].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.P)) &&
                                                (double.TryParse(result2.Groups[13].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out cPathPoint.R))
                                               ))
                                                throw new InvalidOperationException("Cant parse Cartesian Values");
                                        }
                                        else throw new InvalidOperationException("Ppoint without J or Cartesian");


                                        dctPoints.Add(currentReadingPoint, cPathPoint);
                                        inReadingPoint = false;
                                    }
                                    else
                                        fullPointToParse += line;
                                }



                                break;
                        }

                        // use line here
                    }
                }
            }
            catch (Exception ex)
            {
                //txtEditor.Text += "\n" + "Creating Split File at: " + txbOutputFolder.Text + @"\" + txbSubFolderName.Text;

                return -1;
            }
            return 0;
        }

        //public const int NumberOfLinesPerTP = 7000;
        //public const double ClearZLevel = 10.000;
        string OuputPath;
        string OutputBaseFilaname;
        DirectoryInfo di;
        string FileName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int CreateSplitFiles(string outputPath, string outputBaseFilaname, int maxNumberOfLinesInSplitFile, double clearZLevel)
        {
            int FirstLineIntoCurrentTP;
            int LastLineIntoCurentTP = 0;
            int NumberOfPartialFile = 0;
            string maketpBatchFile, karelAutoloaderFile;

            // Set the file structure
            OuputPath = outputPath;
            OutputBaseFilaname = outputBaseFilaname;
            if (Directory.Exists(OuputPath))
            {
                DirectoryInfo di1 = new DirectoryInfo(OuputPath);
                try
                    {
                    di1.Delete(true);
                }
                catch(Exception ex)
                {
                    mainWindow.writeToTextWindow("ERROR deleting output directory: " + ex.Message);
                    return -1;
                }
            }
            while (Directory.Exists(OuputPath)) { };
            di = Directory.CreateDirectory(OuputPath);
            while (!Directory.Exists(OuputPath)) { };
            Directory.SetCurrentDirectory(OuputPath);

            try
            {
                maketpBatchFile = OuputPath + "\\MakeTp_" + outputBaseFilaname + ".bat";
                karelAutoloaderFile = OuputPath + "\\RunSplitFiles.kl";

                StreamWriter MakeBatchwriter = File.CreateText(maketpBatchFile);
                StreamWriter KarelAutoloaderwriter = File.CreateText(karelAutoloaderFile);
                KarelAutoloaderwriter.Write(Properties.Resources.KL_Template_kl);
                KarelAutoloaderwriter.Flush();
                //if(File.Exists(karelAutoloaderFile))
                //   File.Delete(karelAutoloaderFile);
                //while (File.Exists(karelAutoloaderFile)) { };

                //File.Copy(Properties.Resources.KL_Template_kl, karelAutoloaderFile);
                //FileStream KarelAutoloaderwriter = File.OpenWrite(karelAutoloaderFile);

                // Main Loop
                do
                {
                    FirstLineIntoCurrentTP = LastLineIntoCurentTP + 1;
                    LastLineIntoCurentTP = FirstLineIntoCurrentTP + maxNumberOfLinesInSplitFile;
                    if (LastLineIntoCurentTP >= dctSteps.Count)
                    {
                        LastLineIntoCurentTP = dctSteps.Count;

                    }
                    else
                    {
                        // Reverse Search for a MOVE to CLEAR PLANE
                        int tmpLine = LastLineIntoCurentTP;
                        while (!ZinClearLevel(tmpLine, clearZLevel))
                        {
                            tmpLine--;
                            if (tmpLine < ((FirstLineIntoCurrentTP+LastLineIntoCurentTP)/2))
                            {
                                // I will load next program while robot is in cutting position. I will not throw exception
                                //throw new InvalidOperationException("Error in reverse search. No Clear Z Level found.");
                                tmpLine = LastLineIntoCurentTP;
                                break;
                            }
                        }
                        if (tmpLine > FirstLineIntoCurrentTP)
                            LastLineIntoCurentTP = tmpLine;
                    }

                    // Write 
                    MakeBatchwriter.WriteLine("\"" + AppConfig.FANUCFolder + "\\maketp\" " + OutputBaseFilaname + "_" + NumberOfPartialFile.ToString("0000") + " /config \"" + AppConfig.FANUCFolder + "\\robot.ini\" ");
                    MakeBatchwriter.Flush();

                    KarelAutoloaderwriter.WriteLine("\tstatus = runTP('" + AppConfig.FanucStorage + ":\\" + AppConfig.SplitFilesOutputFolder + "\\','" + OutputBaseFilaname + "_" + NumberOfPartialFile.ToString("0000") + "')");
                    KarelAutoloaderwriter.Flush();

                    WritePartialLS(FirstLineIntoCurrentTP, LastLineIntoCurentTP, NumberOfPartialFile++);
                    //NumberOfLinesPerTP += 500;
                } while (LastLineIntoCurentTP < dctSteps.Count);

                MakeBatchwriter.WriteLine("\"" + AppConfig.FANUCFolder + "\\ktrans\" RunSplitFiles " + " /config \"" + AppConfig.FANUCFolder + "\\robot.ini\" ");
                MakeBatchwriter.Flush();
                MakeBatchwriter.Close();

                KarelAutoloaderwriter.WriteLine();
                KarelAutoloaderwriter.WriteLine("END RunSplitFiles");
                KarelAutoloaderwriter.Flush();
                KarelAutoloaderwriter.Close();
            }
            catch (Exception ex)
            {
                mainWindow.writeToTextWindow("ERROR in splitting files: " + ex.Message);
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// I will build a new step and points dictionaries and write the file
        /// </summary>
        /// <param name="firstLine"></param>
        /// <param name="LastLine"></param>
        /// <param name="FileNumber"></param>
        /// <returns></returns>
        public int WritePartialLS(int firstLine, int lastLine, int fileNumber)
        {
            Dictionary<int, LsPathStep> tdctSteps = new Dictionary<int, LsPathStep>();
            Dictionary<int, lsPathPoint> tdctPoints = new Dictionary<int, lsPathPoint>();
            LsPathStep tmpStep, newStep;
            lsPathPoint tmpPoint, newPoint;
            int stepNr = 1;
            int pointNr = 1;



            for (int i = firstLine; i <= lastLine; i++)
            {
                if (!dctSteps.TryGetValue(i, out tmpStep))
                    throw new InvalidDataException("Dictionaery Step: Cannnot get Step Nr " + i.ToString(CultureInfo.InvariantCulture));
                newStep = new LsPathStep(tmpStep._StepType, tmpStep._Body, 0);
                tdctSteps.Add(stepNr++, newStep);
                if (IsMotionStep(tmpStep))
                {
                    //We need a point...
                    if (!dctPoints.TryGetValue(tmpStep._PointNr, out tmpPoint))
                        throw new InvalidDataException("Dictionaery Point: Cannnot get Point Nr " + tmpStep._PointNr.ToString(CultureInfo.InvariantCulture));
                    newPoint = new lsPathPoint(tmpPoint);
                    tdctPoints.Add(pointNr, newPoint);
                    newStep._PointNr = pointNr++;
                }
            }
            pointNr--;
            stepNr--;

            //
            FileName = OutputBaseFilaname + "_" + fileNumber.ToString("0000", CultureInfo.InvariantCulture);
            mainWindow.writeToTextWindow("Writing split file: " + FileName);

            using (StreamWriter writer = File.CreateText(FileName + ".ls"))
            {
                WriteHeader(writer, stepNr, FileName);

                string step;
                string pl1, pl2 = "  GP1 :", pl3, pl4, pl5, pl6 = "};";

                //Write Steps
                writer.WriteLine(@"/MN");
                for (int i = 1; i <= stepNr; i++)
                {
                    if (!tdctSteps.TryGetValue(i, out tmpStep))
                        throw new InvalidDataException("Dictionary Step: Cannnot get Step Nr " + i.ToString(CultureInfo.InvariantCulture));

                    step = " " + i.ToString(CultureInfo.InvariantCulture).PadLeft(6) + ":" + tmpStep._StepType + " ";
                    if (IsMotionStep(tmpStep))
                        step += "P[" + tmpStep._PointNr.ToString(CultureInfo.InvariantCulture) + "] ";
                    step += tmpStep._Body;// + ";";
                    writer.WriteLine(step);
                }

                //Write Points
                writer.WriteLine(@"/POS");
                for (int i = 1; i <= pointNr; i++)
                {
                    if (!tdctPoints.TryGetValue(i, out tmpPoint))
                        throw new InvalidDataException("Dictionary Points: Cannnot get Point Nr " + i.ToString(CultureInfo.InvariantCulture));
                    pl1 = "P[" + i.ToString(CultureInfo.InvariantCulture) + "] {";
                    if (!tmpPoint.CONFIG.Any())
                    {
                        // JOINT MOTION
                        pl3 = "    UF : " + tmpPoint.UF.ToString(CultureInfo.InvariantCulture) + ", UT : " + tmpPoint.UT.ToString(CultureInfo.InvariantCulture) + ",";
                        pl4 = String.Format(CultureInfo.InvariantCulture, "    J1 = {0:0.000} deg, J2 = {1:0.000} deg, J3 = {2:0.000} deg,", tmpPoint.J1, tmpPoint.J2, tmpPoint.J3);
                        pl5 = String.Format(CultureInfo.InvariantCulture, "    J4 = {0:0.000} deg, J5 = {1:0.000} deg, J6 = {2:0.000} deg", tmpPoint.J4, tmpPoint.J5, tmpPoint.J6);
                    }
                    else
                    {
                        // LINEAR MOTION
                        pl3 = "    UF : " + tmpPoint.UF.ToString(CultureInfo.InvariantCulture) + ", UT : " + tmpPoint.UT.ToString(CultureInfo.InvariantCulture) + ", CONFIG : '" + tmpPoint.CONFIG + "',";
                        pl4 = String.Format(CultureInfo.InvariantCulture, "    X = {0:0.000} mm, Y = {1:0.000} mm, Z = {2:0.000} mm,", tmpPoint.X, tmpPoint.Y, tmpPoint.Z);
                        pl5 = String.Format(CultureInfo.InvariantCulture, "    W = {0:0.000} deg,  P = {1:0.000} deg,  R = {2:0.000} deg", tmpPoint.W, tmpPoint.P, tmpPoint.R);
                    }

                    writer.WriteLine(pl1);
                    writer.WriteLine(pl2);
                    writer.WriteLine(pl3);
                    writer.WriteLine(pl4);
                    writer.WriteLine(pl5);
                    writer.WriteLine(pl6);
                }
                writer.WriteLine(@"/END");

            }
            return 0;
        }

        bool ZinClearLevel(int Line, double _clearZLevel)
        {
            int j;
            LsPathStep resultStep;
            lsPathPoint resultPoint;
            if (!dctSteps.TryGetValue(Line, out resultStep))
                throw new InvalidDataException("Dictionaery Step: Cannnot get Step Nr " + Line.ToString(CultureInfo.InvariantCulture));

            if(resultStep._StepType != 'L')
                return false;

            if (!dctPoints.TryGetValue(resultStep._PointNr, out resultPoint))
                throw new InvalidDataException("Dictionary Point: Cannnot get Point Nr " + resultStep._PointNr.ToString(CultureInfo.InvariantCulture));
            //Debug.WriteLine("Line: {0}, Z:{1}",Line,resultPoint.Z);
            if (resultPoint.Z > _clearZLevel) 
                return true;
            else
                return false;
        }

        static bool IsMotionStep(LsPathStep ps)
        {
            if (ps._StepType == 'L' | ps._StepType == 'J')
                return true;
            else
                return false;
        }

        int WriteHeader(StreamWriter w, int lineCount, string fileName)
        {
            w.WriteLine(@"/PROG " + fileName);
            w.WriteLine(@"/ATTR");
            w.WriteLine("OWNER".PadRight(9) + "\t= " + OWNER + ";");
            w.WriteLine("COMMENT".PadRight(9) + "\t= " + COMMENT + ";");
            w.WriteLine("PROG_SIZE".PadRight(9) + "\t= " + PROG_SIZE.ToString(CultureInfo.InvariantCulture) + ";");
            w.WriteLine("CREATE".PadRight(9) + "\t= " + CREATE + ";");
            w.WriteLine("MODIFIED".PadRight(9) + "\t= " + MODIFIED + ";");
            w.WriteLine("FILE_NAME".PadRight(9) + "\t= " + FileName + ";");
            w.WriteLine("VERSION".PadRight(9) + "\t= " + VERSION.ToString(CultureInfo.InvariantCulture) + ";");
            w.WriteLine("LINE_COUNT".PadRight(9) + "\t= " + lineCount.ToString(CultureInfo.InvariantCulture) + ";");
            w.WriteLine("MEMORY_SIZE".PadRight(9) + "\t= 0;");
            w.WriteLine("PROTECT\t= READ_WRITE;");
            w.WriteLine("TCD:\t" + "STACK_SIZE".PadRight(12) + "\t= " + TCD.STACK_SIZE.ToString(CultureInfo.InvariantCulture) + ",");
            w.WriteLine("\tTASK_PRIORITY\t= " + TCD.TASK_PRIORITY.ToString(CultureInfo.InvariantCulture) + ",");
            w.WriteLine("\tTIME_SLICE\t= " + TCD.TIME_SLICE.ToString(CultureInfo.InvariantCulture) + ",");
            w.WriteLine("\tBUSY_LAMP_OFF\t= " + TCD.BUSY_LAMP_OFF.ToString(CultureInfo.InvariantCulture) + ",");
            w.WriteLine("\tABORT_REQUEST\t= " + TCD.ABORT_REQUEST.ToString(CultureInfo.InvariantCulture) + ",");
            w.WriteLine("\tPAUSE_REQUEST\t= " + TCD.PAUSE_REQUEST.ToString(CultureInfo.InvariantCulture) + ";");
            w.WriteLine("DEFAULT_GROUP\t= " + DEFAULT_GROUP + ";");
            w.WriteLine("CONTROL_CODE\t= " + CONTROL_CODE + ";");
            return 0;
        }
    }
}
