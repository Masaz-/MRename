using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MRename
{
    /// <summary>
    /// Interaction logic for RuleWindow.xaml
    /// </summary>
    public partial class RuleWindow : Window
    {
        public MRule Rule { get; set; }
        public bool Save = false;
        public bool Editing = false;

        public RuleWindow(MRule rule = null)
        {
            InitializeComponent();
            Rule = new MRule();

            if (rule != null)
            {
                Rule = rule;
                Editing = true;
            }

            DataContext = this;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Save = true;

            Rule.Name = "Rule Name Placeholder";
            Rule.Description = "";

            if (Rule.Insert)
            {
                Rule.Description += String.Format("Insert text \"{0}\" at {1}. ", Rule.InsertText, Rule.InsertTextAt > -1 ? Rule.InsertTextAt.ToString() : "the end");
            }

            if (Rule.Replace)
            {
                Rule.Description += String.Format("Replace text \"{0}\" with \"{1}\"{2}", Rule.ReplaceText, Rule.ReplaceTextWith, Rule.ReplaceIsRegex ? " using Regular Expression. " : ". ");
            }

            if (Rule.Remove)
            {
                Rule.Description += String.Format("Remove from {0} until {1}. ",
                    Rule.RemoveStartIsNumber ? Rule.RemoveStartText : "\"" + Rule.RemoveStartText + "\"",
                    Rule.RemoveEndIsNumber ? (Rule.RemoveEndText == "-1" ? "the end" : Rule.RemoveEndText) : "\"" + Rule.RemoveEndText + "\""
                );
            }

            if (Rule.Text)
            {
                Rule.Description += String.Format("Replace filenames with rows from the given text. ");
            }

            if (Rule.CasingLowercase)
            {
                Rule.Description += String.Format("Convert casing to lowercase. ");
            }

            if (Rule.CasingUppercase)
            {
                Rule.Description += String.Format("Convert casing to uppercase. ");
            }

            if (Rule.CasingUppercaseWords)
            {
                Rule.Description += String.Format("Uppercase the first letter of words. ");
            }

            if (Rule.CasingUppercaseFirstWord)
            {
                Rule.Description += String.Format("Uppercase the first letter of the first word. ");
            }

            if (Rule.TrimWhitespace)
            {
                Rule.Description += String.Format("Trim whitespaces. ");
            }

            if (Rule.CleanDoubleSpaces)
            {
                Rule.Description += String.Format("Clean double spaces. ");
            }

            if (Rule.RandomNumbering)
            {
                Rule.Description += String.Format("Insert {0} random {1} at {2}. ", Rule.RandomNumbersCount, Rule.RandomNumbersCount > 1 ? "numbers" : "number", Rule.RandomNumbersAt > -1 ? Rule.RandomNumbersAt.ToString() : "the end");
            }

            if (Rule.RandomizeFilenames)
            {
                Rule.Description += "Randomize filenames. ";
            }

            if (Rule.Description == "")
            {
                Rule.Description += "Do nothing.";
            }
            else
            {
                if (Rule.Extension)
                {
                    Rule.Description += "Include extension.";
                }
                else
                {
                    Rule.Description += "Skip extension.";
                }
            }

            Close();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            UIElementCollection children;

            if (tb.Parent is Grid grid)
            {
                children = grid.Children;
            }
            else if (tb.Parent is StackPanel stackpanel)
            {
                children = stackpanel.Children;
            }
            else
            {
                return;
            }

            string cbTag = tb.Tag.ToString();
            string inp = tb.Text.Trim();

            foreach (var child in children)
            {
                if (child is CheckBox cb)
                {
                    if (cb.Tag == null)
                    {
                        continue;
                    }

                    string tag = cb.Tag.ToString();

                    if (tag == cbTag)
                    {
                        cb.IsChecked = (inp != "");
                    }
                }
            }
        }
    }
}
