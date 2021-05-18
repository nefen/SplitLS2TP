using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Parse_Split_LS.Models
{
    public static class AppConfig
    {
        const string FANUCFolderNodeName = "FANUC Folder";
        const string SplitFileNameRadicalNodeName = "Split File Name Radical";
        const string RobotIniFolderNodeName = "RobotIni Folder";
        const string OutputFolderNodeName = "Output Folder";
        const string MaxTpSizeNodeName = "Max TP Size";
        const string LsSplitFileLinesNumberNodeName = "LS Split File Lines Number";
        const string SplitFilesOutputFolderNodeName = "Split Files Output Folder";
        const string CLearZLevelNodeName = "Clear Z Level";
        const string FanucStorageNodeName = "UD1, UD2 or UT1";

        public static string FANUCFolder;
        public static string SplitFileNameRadical;
        public static string RobotIniFolder;
        public static string OutputFolder;
        public static int MaxTpSize = 40000;
        public static int LsSplitFileLinesNumber = 7300;
        public static string SplitFilesOutputFolder;
        public static double ClearZLevel;
        public static string FanucStorage;

        static string appDataMainfolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string appDataFolder = appDataMainfolder + @"\Split_LS";
        static string appDataInifile = appDataFolder + @"\Split_LS.xml";

        public static int ReadConfigFile()
        {

            DirectoryInfo di;

            //   OutputBaseFilaname = outputBaseFilaname;
            if (Directory.Exists(appDataFolder))
            {
                // Read config File
                if (File.Exists(appDataInifile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(appDataInifile);

                    // Make TP Folder
                    XmlNodeList xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + FANUCFolderNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        FANUCFolder = xnList[0].InnerText;

                    // ktrans folder
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + SplitFileNameRadicalNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        SplitFileNameRadical = xnList[0].InnerText;

                    // robot.ini folder
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + RobotIniFolderNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        RobotIniFolder = xnList[0].InnerText;

                    // LS file folder
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + OutputFolderNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        OutputFolder = xnList[0].InnerText;

                    // Max TP Size
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + MaxTpSizeNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        if (!int.TryParse(xnList[0].InnerText, out MaxTpSize)) return -1;

                    // LS Split File Lines
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + LsSplitFileLinesNumberNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        if (!int.TryParse(xnList[0].InnerText, out LsSplitFileLinesNumber)) return -1;


                    // Output LS files folder
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + SplitFilesOutputFolderNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        SplitFilesOutputFolder = xnList[0].InnerText;

                    // Clear Z Level
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + CLearZLevelNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        if (!double.TryParse(xnList[0].InnerText, out ClearZLevel)) return -1;

                    // FANUC Device
                    xnList = doc.SelectNodes(@"/Settings/Setting[@Name='" + FanucStorageNodeName + "']");
                    if (xnList.Count != 1)
                        return -1;
                    else
                        FanucStorage = xnList[0].InnerText;

                }
            }

            return 0;
        }
        public static int WriteConfigFile()
        {
            //Write a config file
            if (!Directory.Exists(appDataFolder))
            {
                DirectoryInfo di = Directory.CreateDirectory(appDataFolder);
            }

            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode settingsNode = doc.CreateElement("Settings");
            doc.AppendChild(settingsNode);


            AppendNode(doc, settingsNode, FANUCFolderNodeName, FANUCFolder);
            AppendNode(doc, settingsNode, SplitFileNameRadicalNodeName, SplitFileNameRadical);
            AppendNode(doc, settingsNode, RobotIniFolderNodeName, RobotIniFolder);
            AppendNode(doc, settingsNode, OutputFolderNodeName, OutputFolder);
            AppendNode(doc, settingsNode, MaxTpSizeNodeName, MaxTpSize.ToString());
            AppendNode(doc, settingsNode, LsSplitFileLinesNumberNodeName, LsSplitFileLinesNumber.ToString());
            AppendNode(doc, settingsNode, SplitFilesOutputFolderNodeName, SplitFilesOutputFolder);
            AppendNode(doc, settingsNode, CLearZLevelNodeName, ClearZLevel.ToString());
            AppendNode(doc, settingsNode, FanucStorageNodeName, FanucStorage);


            try
            {
                doc.Save(appDataInifile);
            }
            catch (Exception ex)
            {
                return -1;
            }

            return 0;
        }

        static void AppendNode(XmlDocument doc, XmlNode node, string name, string value)
        {
            XmlNode tmpNode = doc.CreateElement("Setting");
            XmlAttribute tmpAttribute = doc.CreateAttribute("Name");
            tmpAttribute.Value = name;
            tmpNode.InnerText = value;
            tmpNode.Attributes.Append(tmpAttribute);
            node.AppendChild(tmpNode);
        }

    }
}
