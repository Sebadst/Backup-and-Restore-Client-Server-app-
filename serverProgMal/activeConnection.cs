using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serverPDS
{
    class activeConnection
    {
        //ConcurrentQueue<string> connessioni;
        List<string> connessioni;
        public activeConnection()
        {
            //connessioni = new ConcurrentQueue<string>();
            connessioni = new List<string>();
        }

        public List<string> Connessioni
        {
            get;
            set;
        }

    }
}
