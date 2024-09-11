using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using MRename.Classes;

namespace MRename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string githubLink = "https://github.com/Masaz-/MRename";

        public ObservableCollection<MFile> Files { get; set; }
        public ObservableCollection<MRule> Rules { get; set; }

        readonly Random rnd = new Random();
        RuleWindow ruleWindow = null;

        public MainWindow()
        {
            InitializeComponent();

            Files = new ObservableCollection<MFile>();
            Rules = new ObservableCollection<MRule>();

            Rules.CollectionChanged += Rules_CollectionChanged;

            LinkGithub.Inlines.Add("Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString());

            DataContext = this;
        }

        long LongRandom(long min, long max, Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

        private void OpenRuleWindow(MRule rule = null)
        {
            ruleWindow = new RuleWindow(rule);
            ruleWindow.Closed += RuleWindow_Closed;
            ruleWindow.ShowDialog();
        }

        private string[] OpenFiles(string path, string filter, out string pickedPath)
        {
            string[] files;
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = filter,
                InitialDirectory = path
            };
            var result = dialog.ShowDialog();

            if (result.HasValue && dialog.FileNames.Length > 0)
            {
                pickedPath = Path.GetDirectoryName(dialog.FileNames[0]);
                files = dialog.FileNames;
            }
            else
            {
                pickedPath = "";
                files = new string[0];
            }

            return files;
        }

        private void RenameFiles()
        {
            for (int i = 0; i < Files.Count; i++)
            {
                try
                {
                    Files[i].Rename();
                }
                catch (Exception ex)
                {
                    Files[i].Status = "Could not rename the file: " + ex.Message;
                }
            }
        }

        private void PickFiles(string path, out string pickedPath)
        {
            List<MFile> tmpFiles = new List<MFile>();
            string[] filenames = OpenFiles(path, "All Files|*.*", out pickedPath);

            foreach (string filename in filenames)
            {
                tmpFiles.Add(new MFile(filename));
            }

            tmpFiles = new List<MFile>(tmpFiles.OrderByDescending(f => f.Name));

            foreach (MFile file in tmpFiles)
            {
                Files.Add(file);
            }

            ApplyRules();
        }

        private void SaveRules()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "MRule files (.xml)|*.xml"
            };

            var result = dlg.ShowDialog();

            if (result == true)
            {
                List<MRule> listOfRules = new List<MRule>();

                foreach (MRule rule in Rules)
                {
                    listOfRules.Add(rule);
                }

                MSerializer.WriteToXmlFile(dlg.FileName, listOfRules);
            }
        }

        private void ApplyRules()
        {
            try
            {
                TextInfo ti = new CultureInfo("fi-Fi", false).TextInfo;
                string[] lines = new string[0];

                for (int r = 0; r < Rules.Count; r++)
                {
                    if (Rules[r].Text)
                    {
                        lines = Rules[r].LinesText.Split(
                            new[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None
                        );
                    }
                }

                for (int f = 0; f < Files.Count; f++)
                {
                    string NewName = Files[f].Name;
                    string ext = Path.GetExtension(NewName);

                    for (int r = 0; r < Rules.Count; r++)
                    {
                        string extLessName = Path.GetFileNameWithoutExtension(NewName);
                        string fullname = Rules[r].Extension ? extLessName + ext : extLessName;

                        // Apply rules

                        if (Rules[r].Insert) // Insert text
                        {
                            if (Rules[r].InsertTextAt == -1 || Rules[r].InsertTextAt > fullname.Length)
                            {
                                fullname += Rules[r].InsertText;
                            }
                            else
                            {
                                fullname = fullname.Insert(Rules[r].InsertTextAt, Rules[r].InsertText);
                            }
                        }

                        if (Rules[r].Replace && Rules[r].ReplaceText != "") // Replace text
                        {
                            if (Rules[r].ReplaceTextWith == null)
                            {
                                Rules[r].ReplaceTextWith = "";
                            }

                            if (Rules[r].ReplaceIsRegex)
                            {
                                Regex regex = new Regex(Rules[r].ReplaceText);

                                fullname = regex.Replace(fullname, Rules[r].ReplaceTextWith);
                            }
                            else
                            {
                                fullname = fullname.Replace(Rules[r].ReplaceText, Rules[r].ReplaceTextWith);
                            }
                        }

                        if (Rules[r].Remove && Rules[r].RemoveStartText != "" && Rules[r].RemoveEndText != "") // Remove text
                        {
                            int start = 0;
                            int end = fullname.Length;

                            if (Rules[r].RemoveStartIsNumber)
                            {
                                int.TryParse(Rules[r].RemoveStartText, out start);

                                if (start == -1)
                                {
                                    start = fullname.Length;
                                }
                            }
                            else
                            {
                                start = fullname.IndexOf(Rules[r].RemoveStartText);
                            }

                            if (start > fullname.Length)
                            {
                                start = fullname.Length;
                            }

                            if (Rules[r].RemoveEndIsNumber)
                            {
                                int.TryParse(Rules[r].RemoveEndText, out end);

                                if (end == -1)
                                {
                                    end = fullname.Length;
                                }
                            }
                            else
                            {
                                if (Rules[r].RemoveEndText == "")
                                {
                                    end = fullname.IndexOf(Rules[r].RemoveEndText);
                                }
                                else
                                {
                                    end = fullname.Length;
                                }
                            }

                            if (end > fullname.Length)
                            {
                                end = fullname.Length;
                            }

                            if (start == end)
                            {
                                fullname = fullname.Remove(start);
                            }
                            else
                            {
                                if (start == -1)
                                {
                                    start = 0;
                                }

                                if (end == -1)
                                {
                                    end = fullname.Length;
                                }

                                fullname = fullname.Remove(start, end - start);
                            }
                        }

                        if (Rules[r].Text && lines.Length > 0)
                        {
                            if (f < lines.Length)
                            {
                                fullname = lines[f];
                            }
                        }

                        if (Rules[r].CasingLowercase) // Convert casing to lowercase
                        {
                            fullname = fullname.ToLower();
                        }

                        if (Rules[r].CasingUppercase) // Convert casing to uppercase
                        {
                            fullname = fullname.ToUpper();
                        }

                        if (Rules[r].CasingUppercaseWords) // Uppercase first letter of each word
                        {
                            fullname = ti.ToTitleCase(fullname);
                        }

                        if (Rules[r].CasingUppercaseFirstWord) // Uppercase first letter of text
                        {
                            if (fullname.Length > 0)
                            {
                                fullname = char.ToUpper(fullname[0]) + fullname.Substring(1);
                            }
                        }

                        if (Rules[r].TrimWhitespace) // Trim whitespace
                        {
                            fullname = fullname.Trim();
                        }

                        if (Rules[r].CleanDoubleSpaces) // Clean double space
                        {
                            fullname = fullname.Replace("   ", " ").Replace("  ", " ");
                        }

                        if (Rules[r].RandomNumbering) // Add random numbering
                        {
                            long rndMin = (long)Math.Pow(10, Rules[r].RandomNumbersCount);
                            long rndMax = rndMin * 10;

                            if (Rules[r].RandomNumbersAt == -1 || Rules[r].RandomNumbersAt > fullname.Length)
                            {
                                fullname += LongRandom(rndMin, rndMax, rnd);
                            }
                            else
                            {
                                fullname = fullname.Insert(Rules[r].RandomNumbersAt, LongRandom(rndMin, rndMax, rnd).ToString());
                            }
                        }

                        if (Rules[r].RandomizeFilenames) // Randomize filenames
                        {
                            long rndMin = 100000000;
                            long rndMax = rndMin * 10;

                            fullname = LongRandom(rndMin, rndMax, rnd).ToString();
                        }

                        NewName = fullname;

                        if (!Rules[r].Extension) // Add extension back
                        {
                            NewName += ext;
                        }
                    }

                    Files[f].NewName = NewName;
                }
            }
            catch (Exception ex)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

                MessageBox.Show(ex.Message + ex.StackTrace, "Applying Rules Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Rules

        private void Rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (var i = 0; i < Rules.Count; i++)
            {
                Rules[i].Name = "Rule " + (i+1);
            }

            ApplyRules();
        }

        private void BtnRemoveRule_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DgRules.SelectedIndex;

            if (selectedIndex != -1)
            {
                Rules.RemoveAt(selectedIndex);

                if (Rules.Count > 0)
                {
                    if (Rules.Count - 1 < selectedIndex)
                    {
                        selectedIndex--;
                    }

                    DgRules.SelectedIndex = selectedIndex;
                }
            }

            ApplyRules();
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (DgRules.SelectedIndex != -1 && DgRules.SelectedIndex > 0)
            {
                Rules.Move(DgRules.SelectedIndex, DgRules.SelectedIndex - 1);
            }

            ApplyRules();
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (DgRules.SelectedIndex != -1 && DgRules.SelectedIndex < Rules.Count)
            {
                Rules.Move(DgRules.SelectedIndex, DgRules.SelectedIndex + 1);
            }

            ApplyRules();
        }

        private void BtnAddRule_Click(object sender, RoutedEventArgs e)
        {
            OpenRuleWindow();
        }

        private void BtnAddRuleFromFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] filepaths = OpenFiles(Properties.Settings.Default.LastLocation, "MRule Files (.xml)|*.xml", out string pickedPath);

                Properties.Settings.Default.LastLocation = pickedPath;
                Properties.Settings.Default.Save();

                foreach (string filepath in filepaths)
                {
                    List<MRule> r = MSerializer.ReadFromXmlFile<List<MRule>>(filepath);

                    foreach (MRule rule in r)
                    {
                        Rules.Add(rule);
                    }
                }
            }
            catch (Exception ex)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

                MessageBox.Show("Failed to read from file: " + ex.Message + ex.StackTrace, "Reading Rules from a File Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveRuleToFile_Click(object sender, RoutedEventArgs e)
        {
            if (Rules.Count > 0)
            {
                SaveRules();
            }
        }

        private void DgRules_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenRuleWindow((MRule)DgRules.SelectedItem);
        }

        private void DgRules_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    ((DataGrid)sender).UnselectAll();
                }
                else if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
                {
                    OpenRuleWindow();
                }
            }
        }

        // Files

        private void DgFiles_Drop(object sender, DragEventArgs e)
        {
            try
            {
                List<MFile> tmpFiles = new List<MFile>();

                foreach (string filename in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    tmpFiles.Add(new MFile(filename));
                }

                tmpFiles = new List<MFile>(tmpFiles.OrderBy(f => f.Name));

                foreach (MFile file in tmpFiles)
                {
                    Files.Add(file);
                }

                ApplyRules();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "Dropping Files Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgFiles_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PickFiles("", out string pickedPath);
        }

        private void BtnSortUp_Click(object sender, RoutedEventArgs e)
        {
            Files = new ObservableCollection<MFile>(Files.OrderBy(f => f.Name));

            DgFiles.ItemsSource = Files;

            ApplyRules();
        }

        private void BtnSortDown_Click(object sender, RoutedEventArgs e)
        {
            Files = new ObservableCollection<MFile>(Files.OrderByDescending(f => f.Name));

            DgFiles.ItemsSource = Files;

            ApplyRules();
        }

        private void BtnRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DgFiles.SelectedIndex;

            if (selectedIndex != -1)
            {
                Files.RemoveAt(selectedIndex);

                if (Files.Count > 0)
                {
                    if (Files.Count - 1 < selectedIndex)
                    {
                        selectedIndex--;
                    }

                    DgFiles.SelectedIndex = selectedIndex;
                }
            }
        }

        private void BtnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            PickFiles("", out string pickedPath);
        }

        // Bottom

        private void BtnClearRules_Click(object sender, RoutedEventArgs e)
        {
            Rules.Clear();
        }

        private void BtnClearFiles_Click(object sender, RoutedEventArgs e)
        {
            Files.Clear();
        }

        private void BtnRename_Click(object sender, RoutedEventArgs e)
        {
            RenameFiles();
        }

        private void RuleWindow_Closed(object sender, EventArgs e)
        {
            RuleWindow rw = (RuleWindow)sender;

            if (rw.Save)
            {
                if (rw.Editing)
                {
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        if (Rules[i].Id == rw.Rule.Id)
                        {
                            Rules[i] = rw.Rule;
                            i = Rules.Count;
                        }
                    }
                }
                else
                {
                    Rules.Add(rw.Rule);
                }
            }
        }

        private void LinkGithub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(githubLink));
        }
    }
}
