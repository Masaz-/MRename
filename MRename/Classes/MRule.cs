using System;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace MRename
{
    public class MRule : INotifyPropertyChanged
    {
        private bool _extension;
        private bool _insert;
        private bool _replace;
        private bool _remove;
        private bool _text;
        private bool _replaceIsRegex;
        private bool _removeStartIsNumber;
        private bool _removeEndIsNumber;
        private bool _cleanDoubleSpaces;
        private bool _trimWhitespace;
        private bool _randomNumbering;
        private bool _casingLowercase;
        private bool _casingUppercase;
        private bool _casingUppercaseWords;
        private bool _casingUppercaseFirstWord;
        private bool _randomizeFilenames;
        private string _insertText;
        private string _replaceText;
        private string _replaceTextWith;
        private string _removeStartText;
        private string _removeEndText;
        private string _linesText;
        private string _name;
        private string _description;
        private int _insertTextAt;
        private int _randomNumbersCount;
        private int _randomNumbersAt;

        public event PropertyChangedEventHandler PropertyChanged;

        public long Id { get; }

        public bool Extension
        {
            get
            {
                return _extension;
            }
            set
            {
                _extension = value;
                NotifyPropertyChanged("Extension");
            }
        }

        public bool Insert
        {
            get
            {
                return _insert;
            }
            set
            {
                _insert = value;
                NotifyPropertyChanged("Insert");
            }
        }

        public bool Replace
        {
            get
            {
                return _replace;
            }
            set
            {
                _replace = value;
                NotifyPropertyChanged("Replace");
            }
        }

        public bool Remove
        {
            get
            {
                return _remove;
            }
            set
            {
                _remove = value;
                NotifyPropertyChanged("Remove");
            }
        }

        public bool Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                NotifyPropertyChanged("Text");
            }
        }

        public bool ReplaceIsRegex
        {
            get
            {
                return _replaceIsRegex;
            }
            set
            {
                _replaceIsRegex = value;
                NotifyPropertyChanged("ReplaceIsRegex");
            }
        }

        public bool RemoveStartIsNumber
        {
            get
            {
                return _removeStartIsNumber;
            }
            set
            {
                _removeStartIsNumber = value;
                NotifyPropertyChanged("RemoveStartIsNumber");
            }
        }

        public bool RemoveEndIsNumber
        {
            get
            {
                return _removeEndIsNumber;
            }
            set
            {
                _removeEndIsNumber = value;
                NotifyPropertyChanged("RemoveEndIsNumber");
            }
        }

        public bool CleanDoubleSpaces
        {
            get
            {
                return _cleanDoubleSpaces;
            }
            set
            {
                _cleanDoubleSpaces = value;
                NotifyPropertyChanged("CleanDoubleSpaces");
            }
        }

        public bool TrimWhitespace
        {
            get
            {
                return _trimWhitespace;
            }
            set
            {
                _trimWhitespace = value;
                NotifyPropertyChanged("TrimWhitespace");
            }
        }

        public bool RandomNumbering
        {
            get
            {
                return _randomNumbering;
            }
            set
            {
                _randomNumbering = value;
                NotifyPropertyChanged("RandomNumbering");
            }
        }

        public bool CasingLowercase
        {
            get
            {
                return _casingLowercase;
            }
            set
            {
                _casingLowercase = value;
                NotifyPropertyChanged("CasingLowercase");
            }
        }

        public bool CasingUppercase
        {
            get
            {
                return _casingUppercase;
            }
            set
            {
                _casingUppercase = value;
                NotifyPropertyChanged("CasingUppercase");
            }
        }

        public bool CasingUppercaseWords
        {
            get
            {
                return _casingUppercaseWords;
            }
            set
            {
                _casingUppercaseWords = value;
                NotifyPropertyChanged("CasingUppercaseWords");
            }
        }

        public bool CasingUppercaseFirstWord
        {
            get
            {
                return _casingUppercaseFirstWord;
            }
            set
            {
                _casingUppercaseFirstWord = value;
                NotifyPropertyChanged("CasingUppercaseFirstWord");
            }
        }

        public bool RandomizeFilenames
        {
            get
            {
                return _randomizeFilenames;
            }
            set
            {
                _randomizeFilenames = value;
                NotifyPropertyChanged("RandomizeFilenames");
            }
        }

        public string InsertText
        {
            get
            {
                return _insertText;
            }
            set
            {
                _insertText = value;
                NotifyPropertyChanged("InsertText");
            }
        }

        public string ReplaceText
        {
            get
            {
                return _replaceText;
            }
            set
            {
                _replaceText = value;
                NotifyPropertyChanged("ReplaceText");
            }
        }

        public string ReplaceTextWith
        {
            get
            {
                return _replaceTextWith;
            }
            set
            {
                _replaceTextWith = value;
                NotifyPropertyChanged("ReplaceTextWith");
            }
        }

        public string RemoveStartText
        {
            get
            {
                return _removeStartText;
            }
            set
            {
                _removeStartText = value;
                NotifyPropertyChanged("RemoveStartText");
            }
        }

        public string RemoveEndText
        {
            get
            {
                return _removeEndText;
            }
            set
            {
                _removeEndText = value;
                NotifyPropertyChanged("RemoveEndText");
            }
        }

        public string LinesText
        {
            get
            {
                return _linesText;
            }
            set
            {
                _linesText = value;
                NotifyPropertyChanged("LinesText");
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public int InsertTextAt
        {
            get
            {
                return _insertTextAt;
            }
            set
            {
                _insertTextAt = value;
                NotifyPropertyChanged("InsertTextAt");
            }
        }

        public int RandomNumbersCount
        {
            get
            {
                return _randomNumbersCount;
            }
            set
            {
                _randomNumbersCount = value;
                NotifyPropertyChanged("RandomNumbersCount");
            }
        }

        public int RandomNumbersAt
        {
            get
            {
                return _randomNumbersAt;
            }
            set
            {
                _randomNumbersAt = value;
                NotifyPropertyChanged("RandomNumbersAt");
            }
        }

        public MRule()
        {
            Id = DateTime.Now.Ticks;

            Extension = false;
            Insert = false;
            Replace = false;
            Text = false;
            Name = "Empty rule";
            Description = "This rule is empty.";
        }

        private void NotifyPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
