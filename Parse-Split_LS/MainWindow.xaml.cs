using Parse_Split_LS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Parse_Split_LS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AppConfig.ReadConfigFile();
            txbMaketpPath.Text = AppConfig.FANUCFolder;
            txbOutputFolder.Text = AppConfig.OutputFolder;
            txbSubFolderName.Text = AppConfig.SplitFilesOutputFolder;
            txtLinesPerSplitFile.Text = AppConfig.LsSplitFileLinesNumber.ToString(CultureInfo.InvariantCulture);
            txtSplitFileName.Text = AppConfig.SplitFileNameRadical;
            txtClearZLevel.Text = AppConfig.ClearZLevel.ToString(CultureInfo.InvariantCulture);
            int cmbIndex = 0;
            switch ( AppConfig.FanucStorage)
            {
                case "UD1": cmbIndex = 0;
                    break;
                case "UD2":
                    cmbIndex = 1;
                    break;
                case "UT1":
                    cmbIndex = 2;
                    break;
                default: cmbIndex = 0;break;
            }

            cmbFanucDevice.SelectedIndex = cmbIndex;

        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog openFolderDialog = new FolderBrowserDialog())
            {
                openFolderDialog.Description = "FANUC OLPC Folder (ktrans.exe, maketp.exe, robot.ini).";
                openFolderDialog.ShowNewFolderButton = false;
                openFolderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                openFolderDialog.SelectedPath = txbMaketpPath.Text;
                if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    txbMaketpPath.Text = openFolderDialog.SelectedPath;
            }
        }

        private void btnSelectLS_Click(object sender, RoutedEventArgs e)
        {
            string filePath;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "LS files (*.LS)|*.LS";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    txbLsPath.Text = filePath;

                }
            }
        }

        private void btnOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog openFolderDialog = new FolderBrowserDialog())
            {
                openFolderDialog.Description = "FANUC OLPC Folder (ktrans.exe, maketp.exe, robot.ini).";
                openFolderDialog.ShowNewFolderButton = false;
                openFolderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                openFolderDialog.SelectedPath = txbOutputFolder.Text;
                if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    txbOutputFolder.Text = openFolderDialog.SelectedPath;
            }
        }

        private void btnSplitFile_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.FANUCFolder = txbMaketpPath.Text;
            AppConfig.OutputFolder= txbOutputFolder.Text  ;
            AppConfig.SplitFilesOutputFolder=txbSubFolderName.Text ;
            int.TryParse(txtLinesPerSplitFile.Text, out AppConfig.LsSplitFileLinesNumber);
            AppConfig.SplitFileNameRadical=txtSplitFileName.Text ;
            double.TryParse(txtClearZLevel.Text, out AppConfig.ClearZLevel);
            AppConfig.FanucStorage = cmbFanucDevice.Text;

            int maxLinesInSplit;
            double clearZLevel;
            if (File.Exists(txbLsPath.Text) & int.TryParse(txtLinesPerSplitFile.Text, out maxLinesInSplit) & double.TryParse(txtClearZLevel.Text, out clearZLevel))
            {
                writeToTextWindow( "Writing Config File");
                AppConfig.WriteConfigFile();
                lsPath path = new lsPath();
                writeToTextWindow("Parsing Ls File: " +txbLsPath.Text);
                path.ParseFile(txbLsPath.Text);
                writeToTextWindow("Creating Split File at: " + txbOutputFolder.Text + @"\" + txbSubFolderName.Text);
                if (path.CreateSplitFiles(txbOutputFolder.Text + @"\" + txbSubFolderName.Text, txtSplitFileName.Text, maxLinesInSplit, clearZLevel) == -1)
                    writeToTextWindow("Error Creating Files ");
                else
                {
                    writeToTextWindow("Split Files have been created ");
                    if (cbRunBat.IsChecked == true)
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = txbOutputFolder.Text + @"\" + txbSubFolderName.Text + "\\MakeTp_" + txtSplitFileName.Text + ".bat";
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        process.StartInfo.UseShellExecute = true;
                        //process.StartInfo.RedirectStandardOutput = false;  
                        process.Start();
                        //string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        process.Dispose();
                        foreach (string f in Directory.EnumerateFiles(txbOutputFolder.Text + @"\" + txbSubFolderName.Text, "*.ls"))
                        {
                            File.Delete(f);
                        }
                        File.Delete(txbOutputFolder.Text + @"\" + txbSubFolderName.Text + "\\MakeTp_" + txtSplitFileName.Text + ".bat");
                        File.Delete(txbOutputFolder.Text + @"\" + txbSubFolderName.Text + "\\RunSplitFiles.kl");
                    }
                }
            }
            else
                txtEditor.Text += "\n" + "LS file does not exist, \n or Max Lines Number has not been parsed, \n or Clear Z Level has not been parsed";

        }

        public void writeToTextWindow(string msg)
        {
            txtEditor.Text += "\n" + msg;
            System.Windows.Forms.Application.DoEvents();
        }
    }
}
