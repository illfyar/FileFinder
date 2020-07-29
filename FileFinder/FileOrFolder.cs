using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFinder
{
    /// <summary>
    /// Содержит данные и методы для работы с папками и файлами
    /// </summary>
    class FileOrFolder
    {
        /// <summary>
        /// Имя файла или папки
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Путь к файлу или папке
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Признак что объект сожержит данные файла
        /// </summary>
        public bool IsFile { get; set; }
        /// <summary>
        /// Коллекция дочерних элементов
        /// </summary>
        public ObservableCollection<FileOrFolder> ChldFileOrFolders { get; set; } = new ObservableCollection<FileOrFolder>();
        /// <summary>
        /// Заполнение дочерних элементов текущего объекта
        /// </summary>
        /// <param name="selectedFile"></param>
        /// <returns></returns>
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
