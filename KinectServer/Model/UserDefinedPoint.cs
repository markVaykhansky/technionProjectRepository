using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Model
{
    public class UserDefinedPoint
    {
        public int SliceId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float V0X { get; set; }
        public float V0Y { get; set; }
        public float V0Z { get; set; }
        public float AX { get; set; }
        public float AY { get; set; }
        public float AZ { get; set; }
    }
}
