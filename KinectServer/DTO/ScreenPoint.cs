using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.DTO
{
    public class ScreenPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public ScreenPoint()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        
    }
}
