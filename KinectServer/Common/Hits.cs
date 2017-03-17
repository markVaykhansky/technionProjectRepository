using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Common
{
    public class Hits
    {
        public List<Hit> Random { get; set; }
        public List<Hit> Moving { get; set; }
        public Hits()
        {
            Random = new List<Hit>();
            Moving = new List<Hit>();
        }
    }
}
