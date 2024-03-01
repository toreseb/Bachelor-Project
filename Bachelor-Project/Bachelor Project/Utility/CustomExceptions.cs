using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    public class IllegalMoveException : Exception
    {
        public IllegalMoveException(string message = "Illegal Move") : base(message) { }
    }
}
