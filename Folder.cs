using System;
namespace ProgettoPDS
{
    public class Folder
    {
        private DirectoryInfo _folder;
        private List _subFolders;
        private List _files;

        public Folder()
        {
            this.FullPath = @"C:\";

        }

        public string Name
        {
            get
            {
                return this._folder.Name;
            }
            set
            {
            }
        }

        public string FullPath
        {
            get
            {
                return this._folder.FullName;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    this._folder = new DirectoryInfo(value);
                }
                else
                {
                    throw new ArgumentException("Directory must exist", "full path");
                }
            }
        }

        public List Files
        {
            get
            {
                if (this._files == null)
                {
                    this._files = new List();
                    FileInfo[] fi = this._folder.GetFiles();
                    for (int i = 0; i < fi.Length; i++)
                    {
                        this._files.Add(fi[i]);
                    }
                }
                return this._files;
            }
        }

        public List SubFolders
        {
            get
            {
                if (this._subFolders == null)
                {
                    this._subFolders = new List();
                    DirectoryInfo[] di = this._folder.GetDirectories();
                    for (int i = 0; i < di.Length; i++)
                    {
                        Folder newFolder = new Folder();
                        newFolder.FullPath = di[i].FullName;
                        this._subFolders.Add(newFolder);

                    }
                }
                return this._subFolders;
            }
        }
    }
}