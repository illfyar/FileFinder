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
    enum SearchStatus {NotRun,Run,Pause}
    class Model
    {
        private FileOrFolder rootFileOrFolder;
        public static ManualResetEvent PauseEvent = new ManualResetEvent(true);
        public FileOrFolder RootFileOrFolder {
            get 
            {
                return rootFileOrFolder;
            } set 
            {
                rootFileOrFolder = value;
            } }
        public FilterForSearch FilterForSearch { get; set; }
        private string fileNameInProcess;
        public string FileNameInProcess { get { return fileNameInProcess; } set 
            {
                fileNameInProcess = value;
                EventEditFileNameInProcess(fileNameInProcess);
            } }
        public delegate void EditFileNameInProcess(string value);
        public event EditFileNameInProcess EventEditFileNameInProcess;
        Mutex Mutex { get; set; } = new Mutex();
        CancellationTokenSource CancellationTokenSource { get; set; }
        public SearchStatus SearchStatus { get; set; } = SearchStatus.NotRun;
        public Model()
        {
            
        }
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
        public async Task<bool> SearchFileOrFolder(FileOrFolder fileOrFolder)
        {
            CancellationToken cancellationToken = CancellationTokenSource.Token;
            fileOrFolder = await Search(fileOrFolder, cancellationToken);
            return true;
        }
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
         ObservableCollection<FileOrFolder> FillChildren(FileOrFolder fileOrFolder, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} отменён");
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
        void SelectFiles(FileInfo[] fileInfos, out ObservableCollection<FileOrFolder> fileOrFolders)
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
        bool FindText(string str, string fitr)
        {
            string regstr = fitr.Replace("*", "(\\w*)");
            Regex regex = new Regex(regstr);
            MatchCollection matchCollection = regex.Matches(str);
            return matchCollection.Count > 0;
        }
        public void Pause()
        {
            SearchStatus = SearchStatus.Pause;
            PauseEvent.Reset();
        }
        public void Resume()
        {
            SearchStatus = SearchStatus.Run;
            PauseEvent.Set();
        }
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
