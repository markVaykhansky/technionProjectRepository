using KinectServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Common
{
    public class TrajectoriesData
    {
        public List<PointRealWorld> Moving { get; set; }
        public List<PointRealWorld> Random { get; set; }
        public TrajectoriesData()
        {
            Moving = new List<PointRealWorld>();
            Random = new List<PointRealWorld>();
        }
    }
}
