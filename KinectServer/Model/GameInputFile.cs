using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Model
{
    public class GameInputFile
    {
        public Dictionary<string, float> InnerBoundaryInflRatio { get; set; }
        public Dictionary<string, float> OuterBoundaryInflRatio { get; set; }
        public Dictionary<int,UserDefinedPoint> Actions { get; set; }
        public Dictionary<string, Dictionary<string, List<int>>> Players { get; set; }
    }
}
