using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFinder
{
    class FileOrFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsFile { get; set; }
        public ObservableCollection<FileOrFolder> ChldFileOrFolders { get; set; } = new ObservableCollection<FileOrFolder>();

        public static ObservableCollection<FileOrFolder> FillResultCollection(IEnumerable<FileInfo> selectedFile)
        {
            List<FileOrFolder> selectedFileOrFolder = selectedFile.Select(c =>
            new FileOrFolder
            {
                Name = c.Name,
                IsFile = true,
                Path = c.FullName,
                ChldFileOrFolders = new ObservableCollection<FileOrFolder>()
            }).ToList();
            return new ObservableCollection<FileOrFolder>(selectedFileOrFolder);
        }
    }
}
