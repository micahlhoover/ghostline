using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostCode
{
    public class CycleException : Exception
    {
        public CycleException() : base()
        {
        }

        public CycleException(string message)
            : base(message)
        {
        }

        public CycleException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
    }
}
