using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tup.Utilities.Logging;

namespace Tup.Utilities.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.LogFactory = new Logging.Log4Net.Log4NetLoggerFactory();
        }
    }
}
