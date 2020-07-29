using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FileFinder
{
    class TimerForWPF
    {
        public delegate void EditRunningTimeString(string value);
        public event EditRunningTimeString EventEditRunningTimeString;
        private DispatcherTimer dT = new DispatcherTimer();
        private string runningTimeString = "00 : 00 : 00";
        public string RunningTimeString { 
            get 
            {
                return runningTimeString;                
            } set 
            {
                EventEditRunningTimeString?.Invoke(runningTimeString);
                runningTimeString = value;
            }
        }
        private int seconds = 0;
        public int Seconds
        {
            get { return seconds; }
            set
            {
                seconds = value;
                CheckTime(ref seconds, ref minutes);
                RunningTimeString = $"{Hours:D2} : {Minutes:D2} : {Seconds:D2}";
            }
        }
        private int minutes = 0;
        public int Minutes
        {
            get { return minutes; }
            set
            {
                minutes = value;
                CheckTime(ref minutes, ref hours);
                RunningTimeString = $"{Hours:D2} : {Minutes:D2} : {Seconds:D2}";
            }
        }
        private int hours;
        public int Hours { get; set; } = 0;
        public TimerForWPF()
        {
            dT.Interval = TimeSpan.FromMilliseconds(1000);
            dT.Tick += DTMilSec_Tick;
        }
        public void Start()
        {            
            dT.Start();
        }
        private void DTMilSec_Tick(object sender, EventArgs e)
        {
            Seconds++;
        }
        private void CheckTime(ref int time1, ref int time2)
        {
            if (time1 == 59)
            {
                time1 = 0;
                time2++;
            }
        }
        public void Stop()
        {
            Seconds = 0;
            Minutes = 0;
            Hours = 0;
            dT.Stop();            
        }
        public void Pause()
        {
            dT.Stop();
        }
        public void Resume()
        {
            dT.Start();
        }
    }
}
