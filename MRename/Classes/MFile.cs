using System;
using System.ComponentModel;
using System.IO;

namespace MRename
{
    public class MFile : INotifyPropertyChanged
    {
        private string _name;
        private string _newName;
        private string _folderPath;
        private string _status;
        private FileInfo _fileInfo;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public string NewName
        {
            get
            {
                return _newName;
            }
            set
            {
                _newName = value;
                NotifyPropertyChanged("NewName");
            }
        }

        public string FolderPath
        {
            get
            {
                return _folderPath;
            }
            set
            {
                _folderPath = value;
                NotifyPropertyChanged("FolderPath");
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
                NotifyPropertyChanged("CanRename");
            }
        }

        public string Extension
        {
            get
            {
                return Info.Extension;
            }
        }

        public string NameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(NewName);
            }
        }

        public bool CanRename
        {
            get
            {
                return (Status != "OK" || Status != "");
            }
        }

        public FileInfo Info
        {
            get
            {
                return _fileInfo;
            }
            set
            {
                _fileInfo = value;
                NotifyPropertyChanged("FileInfo");
                Name = Info.Name;
                NewName = Info.Name;
                FolderPath = Info.DirectoryName;
            }
        }

        public MFile()
        {
            Status = "";
        }

        public MFile(string filepath)
        {
            Info = new FileInfo(filepath);
            Status = "";
        }

        public void Rename(bool extension = false)
        {
            if (CanRename)
            {
                string oldPath = Path.Combine(FolderPath, Name);
                string newPath = Path.Combine(FolderPath, NewName);

                File.Move(oldPath, newPath);
                Info = new FileInfo(newPath);
                Status = "Renamed!";
            }
        }

        private void NotifyPropertyChanged(String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
