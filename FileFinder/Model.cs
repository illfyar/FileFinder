using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;

namespace FileFinder
{
    //Статус выполнения поиска
    enum SearchStatus {NotRun,Run,Pause}
    class Model
    {
        /// <summary>
        /// Событие для паузы во время поиска
        /// </summary>
        public static ManualResetEvent PauseEvent = new ManualResetEvent(true);
        /// <summary>
        /// Корневой каталог
        /// </summary>
        private FileOrFolder rootFileOrFolder;        
        public FileOrFolder RootFileOrFolder {
            get 
            {
                return rootFileOrFolder;
            } set 
            {
                rootFileOrFolder = value;
            } }
        /// <summary>
        /// фильтр описка
        /// </summary>
        public FilterForSearch FilterForSearch { get; set; }
        /// <summary>
        /// Содержит имя файла
        /// который в данный момент обрабатывается
        /// </summary>
        private string fileNameInProcess;
        public string FileNameInProcess { get { return fileNameInProcess; } set 
            {
                fileNameInProcess = value;
                EventEditFileNameInProcess(fileNameInProcess);
            } }
        /// <summary>
        /// Событие при изменении FileNameInProcess
        /// </summary>
        /// <param name="value"></param>
        public delegate void EditFileNameInProcess(string value);
        public event EditFileNameInProcess EventEditFileNameInProcess;
        Mutex Mutex { get; set; } = new Mutex();
        CancellationTokenSource CancellationTokenSource { get; set; }
        public SearchStatus SearchStatus { get; set; } = SearchStatus.NotRun;
        public Model()
        {
            
        }
        /// <summary>
        /// Стартовый метод поиска для внешнего вызова
        /// </summary>
        /// <returns>bool - если false то выполнение было прервано</returns>
        public async Task<bool> SearchRun()
        {
            try
            {
                CancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = CancellationTokenSource.Token;
                SearchStatus = SearchStatus.Run;                
                RootFileOrFolder = await Search(RootFileOrFolder, cancellationToken);
                if (RootFileOrFolder == null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("выполнение поиска прервано");
                return false;
            }            
            return true;
        }
        /// <summary>
        /// стартовый метод поиска для внутреннего вызова,
        ///  поиск в поиске
        /// </summary>
        /// <param name="fileOrFolder"></param>
        /// <returns>bool</returns>
        private async Task<bool> SearchFileOrFolder(FileOrFolder fileOrFolder)
        {
            CancellationToken cancellationToken = CancellationTokenSource.Token;
            fileOrFolder = await Search(fileOrFolder, cancellationToken);
            return true;
        }
        /// <summary>
        /// Стартовый метод поиска
        /// </summary>
        /// <param name="fileOrFolder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task<FileOrFolder>, если null то выполнение было прервано</returns>
        public async Task<FileOrFolder> Search(FileOrFolder fileOrFolder,CancellationToken cancellationToken)
        {
            try
            {
                fileOrFolder.ChldFileOrFolders = await Task.Run(() => FillChildren(fileOrFolder, cancellationToken));
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} отменён");
                    return null;
                }
                return fileOrFolder;
            }
            catch (Exception)
            {
                Console.WriteLine("прервано");
                return null;
            }
        }
        /// <summary>
        /// Заполнение коллекции FileOrFolder дочерних файлов и папок
        /// </summary>
        /// <param name="fileOrFolder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>ObservableCollection<FileOrFolder>, если null то выполнение быол прервано</returns>
        ObservableCollection<FileOrFolder> FillChildren(FileOrFolder fileOrFolder, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            PauseEvent.WaitOne();
            ObservableCollection<FileOrFolder> childFileOrFolders = new ObservableCollection<FileOrFolder>();
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo(fileOrFolder.Path);
            SelectFiles(currentDirectoryInfo.GetFiles(FilterForSearch.Name), out childFileOrFolders);
            List<FileOrFolder> listDirectoryFileOrFolder = currentDirectoryInfo.GetDirectories().Select(c =>            
                new FileOrFolder
                {
                    Name = c.Name,
                    IsFile = false,
                    Path = c.FullName,
                    ChldFileOrFolders = new ObservableCollection<FileOrFolder>()
                }
            ).ToList();
            List<Task> tasks = new List<Task>();
            listDirectoryFileOrFolder.ForEach(DirectoryFileOrFolder =>
            {
            childFileOrFolders.Add(DirectoryFileOrFolder);
            tasks.Add(SearchFileOrFolder(DirectoryFileOrFolder));
            });
            Task.WaitAll(tasks.ToArray());
            return childFileOrFolders;
        }
        /// <summary>
        /// Заполнение коллекции FileOrFolder
        /// </summary>
        /// <param name="fileInfos"></param>
        /// <param name="fileOrFolders"></param>
        private void SelectFiles(FileInfo[] fileInfos, out ObservableCollection<FileOrFolder> fileOrFolders)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            
            if (String.IsNullOrEmpty(FilterForSearch.Content))
            {
                fileOrFolders = FileOrFolder.FillResultCollection(fileInfos);
                return;
            }
            var selectedFileInfo = fileInfos.Where(c =>
           {
               Thread.Sleep(new Random().Next(2000,5000));//TODO для эмуляции загруженности
               Mutex.WaitOne();
               FileNameInProcess = c.Name;
               Mutex.ReleaseMutex();
               Console.WriteLine(FileNameInProcess);
               using (StreamReader strmRead = c.OpenText())
               {
                   bool thisFileComtent = false;
                   try
                   {
                       string textFile = strmRead.ReadToEnd();
                       thisFileComtent = FindText(textFile, FilterForSearch.Content);
                   }
                   catch (Exception)
                   { }
                   return thisFileComtent;
               }
           }).ToList();
            fileOrFolders = FileOrFolder.FillResultCollection(selectedFileInfo);
        }
        /// <summary>
        /// Поиск внутри файла указанного на форме содержимого
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fitr"></param>
        /// <returns>bool, если true содержимое обнаруженно</returns>
        bool FindText(string str, string fitr)
        {
            string regstr = fitr.Replace("*", "(\\w*)");
            Regex regex = new Regex(regstr);
            MatchCollection matchCollection = regex.Matches(str);
            return matchCollection.Count > 0;
        }
        /// <summary>
        /// Пауза поиска
        /// </summary>
        public void Pause()
        {
            SearchStatus = SearchStatus.Pause;
            PauseEvent.Reset();
        }
        /// <summary>
        /// Продолжить поиск
        /// </summary>
        public void Resume()
        {
            SearchStatus = SearchStatus.Run;
            PauseEvent.Set();
        }
        /// <summary>
        /// Остановить поиск
        /// </summary>
        public void Stop()
        {
            if (SearchStatus == SearchStatus.Pause)
            {
                Resume();
            }
            SearchStatus = SearchStatus.NotRun;
            CancellationTokenSource.Cancel();
        }
    }
}
