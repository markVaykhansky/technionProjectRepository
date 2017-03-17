using KinectServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Common
{
    public class Hit
    {
        public int HitJointIndex { get; set; }
        public int T1 { get; set; }
        public int T2 { get; set; }
        //public TrajectoriesData Trajectory { get; set; }
        //public Hit()
        //{
        //    Trajectory = null;
        //}
    }
}
