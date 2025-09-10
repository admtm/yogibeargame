using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using YogiBear.Model;

namespace YogiBear.Model
{
    public class BasicTimerAggregation : IBasicTimer, IDisposable
    {
        private readonly System.Timers.Timer timer;

        public bool Enabled
        {
            get => timer.Enabled;
            set
            {
                timer.Enabled = value;
            }
        }

        public double Interval
        {
            get => timer.Interval;
            set
            {
                timer.Interval = value;
            }
        }

        public event EventHandler? Elapsed;

        public BasicTimerAggregation()
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += (sender, e) =>
            {
                Elapsed?.Invoke(sender, e);
            };
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}