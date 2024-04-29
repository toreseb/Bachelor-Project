using Bachelor_Project.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_ProjectTests
{
    [TestClass]
    public class Setup
    {
        [AssemblyInitialize]
        public static void SetupInit(TestContext context)
        {
            Settings.Outputting = false;
        }
    }
}
