using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileFinder
{
    class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// модель выполгнения поиска
        /// </summary>
        Model Model { get; set; }
        /// <summary>
        /// коллекция для дерева значений на форме
        /// </summary>
        public ObservableCollection<FileOrFolder> FileOrFolder { get; set; } = new ObservableCollection<FileOrFolder>();
        /// <summary>
        /// Шаблон имени файла для описка
        /// </summary>
        private string templateNameFile;
        public string TemplateNameFile { get
            {
                return templateNameFile;
            } set
            {
                templateNameFile = value;
                OnPropertyChanged("templateNameFile");
            } }
        /// <summary>
        /// Содержимое файла, которе надо найти
        /// </summary>
        private string contentFile;
        public string ContentFile
        {
            get
            {
                return contentFile;
            }
            set
            {
                contentFile = value;
                OnPropertyChanged("contentFile");
            }
        }
        /// <summary>
        /// Таймер отсчета времени поиска
        /// </summary>
        public TimerForWPF TimerForWPF { get; set; } = new TimerForWPF();
        /// <summary>
        /// Свойство для отображения времени поиска
        /// </summary>
        private string runningTimeString = "00 : 00 : 00";
        public string RunningTimeString { get { return runningTimeString; }
            set 
            {
                runningTimeString = value;
                OnPropertyChanged("runningTimeString");
            }
        }
        /// <summary>
        /// для отображения текущего обрабатываемого файла
        /// </summary>
        private string fileNameInProcess;
        public string FileNameInProcess { get
            {
                return fileNameInProcess;
            }set 
            {
                fileNameInProcess = value;
                OnPropertyChanged("fileNameInProcess");
            } }
        /// <summary>
        /// для отображения корневой папки
        /// </summary>
        private string rootFolder;
        public string RootFolder
        {
            get
            {
                return rootFolder;
            }
            set
            {
                rootFolder = value;
                OnPropertyChanged("rootFolder");
            }
        }
        public ViewModel()
        {
            Model = new Model();
            TimerForWPF.EventEditRunningTimeString += EditRunningTimeString;
            Model.EventEditFileNameInProcess += EditFileNameInProcess;
        }
        #region Commands
        #region Find
        private MyCommands find;
        public MyCommands Find
        {
            get
            {
                return find ?? (find = new MyCommands(FindRun));
            }
        }
        private async void FindRun(Object obj)
        {
            TimerForWPF.Stop();
            TimerForWPF.Start();
            Model.RootFileOrFolder = new FileOrFolder
            {
                Name = RootFolder,
                IsFile = false,
                Path = RootFolder,
                ChldFileOrFolders = new System.Collections.ObjectModel.ObservableCollection<FileOrFolder>()
            };
            Model.FilterForSearch = new FilterForSearch { Name = TemplateNameFile??"*.*",Content = ContentFile??"" };
            FileOrFolder.Clear();
            if (await Model.SearchRun())
            {
                TimerForWPF.Pause();
                if (Model.RootFileOrFolder != null)
                {
                    FileOrFolder.Add(Model.RootFileOrFolder);
                }
                OnPropertyChanged("FileOrFolder");
            }
        }
        #endregion
        #region Pause
        private MyCommands pause;
        public MyCommands Pause
        {
            get
            {
                return pause ?? (pause = new MyCommands(PauseRun));
            }
        }
        private void PauseRun(Object obj)
        {
            if (Model.SearchStatus == SearchStatus.NotRun)
            {
                return;
            }
            if (Model.SearchStatus == SearchStatus.Pause)
            {
                if (obj is Button button1)
                {
                    button1.Background = new SolidColorBrush(Color.FromArgb(221, 221, 221, 221));
                }
                Model.Resume();
                TimerForWPF.Resume();
                return;
            }
            if (obj is Button button)
            {
                button.Background = Brushes.Aqua;
            }
            Model.Pause();
            TimerForWPF.Pause();
        }
        #endregion
        #region Stop
        private MyCommands stop;
        public MyCommands Stop
        {
            get
            {
                return stop ?? (stop = new MyCommands(StopRun));
            }
        }
        private void StopRun(Object obj)
        {
            if (Model.SearchStatus == SearchStatus.NotRun)
            {
                return;
            }
            Model.Stop();
            TimerForWPF.Stop();
        }
        #endregion
        #region OpenFolder
        private MyCommands openFolder;
        public MyCommands OpenFolder
        {
            get
            {
                return openFolder ?? (openFolder = new MyCommands(OpenFolderMethod));
            }
        }

        public object DialogResult { get; private set; }

        private void OpenFolderMethod(Object obj)
        {
            using(CommonOpenFileDialog cofd = new CommonOpenFileDialog())
            {
                cofd.IsFolderPicker = true;
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    RootFolder = cofd.FileName;
                }
            }            
        }
        private IDisposable OpenFileDialog()
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion
        void EditRunningTimeString(string value)
        {
            RunningTimeString = value;
        }
        void EditFileNameInProcess(string value)
        {
            FileNameInProcess = value;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
