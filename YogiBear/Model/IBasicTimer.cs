using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBear.Model
{
    public interface IBasicTimer
    {
        bool Enabled { get; set; }
        double Interval { get; set; }
        event EventHandler? Elapsed;
        void Start();
        void Stop();
    }
}
