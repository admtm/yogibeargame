using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBear.Persistence
{
    public class YogiBoardDataException : Exception
    {
        public YogiBoardDataException(string message, Exception innerException) : base(message, innerException) { }
        public YogiBoardDataException() { }
    }
}
