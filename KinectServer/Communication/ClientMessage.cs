using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Communication
{
    //maybe should be an interface
    public class ClientMessage
    {
        public Protocol Type { get; set; }
        public string Data { get; set; }


    }
}
