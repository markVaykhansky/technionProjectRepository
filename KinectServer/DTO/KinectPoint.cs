using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.DTO
{
    public class PointRealWorld
    {
        public PointRealWorld(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public PointRealWorld()
        {

        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float XDelta { get; set; }
        public float YDelta { get; set; }
    }
}
