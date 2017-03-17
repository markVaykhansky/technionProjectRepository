using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.DTO
{
    public class HitData
    {
        public int jointIndex { get; set; }
        public int skeletonIndex { get; set; }
        public string stage { get; set; }

    }
}
