using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    public class IllegalMoveException(string message = "Illegal Move") : Exception(message)
    {
    }
    public class  NewWorkException(string message = "New Work Has Been Given") : Exception(message)
    {
    }
}
