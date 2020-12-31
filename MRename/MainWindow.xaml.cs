using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace MRename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<MFile> Files { get; set; }
        public ObservableCollection<MRule> Rules { get; set; }

        Random rnd = new Random();
        RuleWindow ruleWindow = null;

        public MainWindow()
        {
            InitializeComponent();

            Files = new ObservableCollection<MFile>();
            Rules = new ObservableCollection<MRule>();

            Rules.CollectionChanged += Rules_CollectionChanged;

            TbVersion.Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

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

        private void OpenFilePicker()
        {
            List<MFile> tmpFiles = new List<MFile>();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            var result = dialog.ShowDialog();

            if (result.HasValue)
            {
                var filenames = dialog.FileNames;

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

                        if (Rules[r].Remove && Rules[r].RemoveStartText != "" && Rules[r].RemoveEndText != "")
                        {
                            int start = 0;
                            int end = fullname.Length - 1;

                            if (Rules[r].RemoveStartIsNumber)
                            {
                                int.TryParse(Rules[r].RemoveStartText, out start);

                                if (start == -1)
                                {
                                    start = fullname.Length - 1;
                                }
                            }
                            else
                            {
                                start = fullname.IndexOf(Rules[r].RemoveStartText);
                            }

                            if (start > fullname.Length - 1)
                            {
                                start = fullname.Length - 1;
                            }

                            if (Rules[r].RemoveEndIsNumber)
                            {
                                int.TryParse(Rules[r].RemoveEndText, out end);

                                if (end == -1)
                                {
                                    end = fullname.Length - 1;
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
                                    end = fullname.Length - 1;
                                }
                            }

                            if (end > fullname.Length - 1)
                            {
                                end = fullname.Length - 1;
                            }

                            if (start == end)
                            {
                                fullname = fullname.Substring(start);
                            }
                            else
                            {
                                if (start == -1)
                                {
                                    start = 0;
                                }

                                if (end == -1)
                                {
                                    end = fullname.Length - 1;
                                }

                                fullname = fullname.Substring(start, end);
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

                        NewName = fullname;

                        // Add extension back
                        if (!Rules[r].Extension)
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
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (DgRules.SelectedIndex != -1 && DgRules.SelectedIndex > 0)
            {
                Rules.Move(DgRules.SelectedIndex, DgRules.SelectedIndex - 1);
            }
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (DgRules.SelectedIndex != -1 && DgRules.SelectedIndex < Rules.Count)
            {
                Rules.Move(DgRules.SelectedIndex, DgRules.SelectedIndex + 1);
            }
        }

        private void BtnAddRule_Click(object sender, RoutedEventArgs e)
        {
            OpenRuleWindow();
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
            OpenFilePicker();
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
            OpenFilePicker();
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
    }
}
