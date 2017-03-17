using Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KinectServer.DTO
{
    public class DisplayData
    {
        public List<ScreenPoint> Joints { get; set; }
        //public SkeletonCoordinateSystem SCS { get; set; }
        public int SkeletonIndex { get; set; }
        public ScreenPoint NextEnemyPoint { get; set; }
        public ScreenPoint RadiusPoint { get; set; }

        public Dictionary<int, List<ScreenPoint>> Slices { get; set; }

        public Dictionary<int, List<ScreenPoint>> Rects { get; set; }
        
        
    }
}
