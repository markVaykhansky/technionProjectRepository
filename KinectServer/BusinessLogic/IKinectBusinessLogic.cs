using KinectServer.DTO;
using KinectServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KinectServer.BusinessLogic
{
    public interface IKinectBusinessLogic
    {
        event Action<bool> KinectAvailabletyChanged;
        //event Action<List<ScreenPoint>,List<KinectPoint>> NewJointsDataReady;
        event Action<DisplayData, List<Point3D>, Point3D> NewJointsDataReady;

        bool StartKinect();
        bool StopKinect();

        float ScaleFactorX { get; set; }
        float ScaleFactorY { get; set; }
        bool IsOpen { get; }
        bool IsNextPointMoving { get; set; }
        bool IsGetNextPoint { get; set; }
        bool IsGetSamePoint { get; set; }
        void SetEnemiesList(List<UserDefinedPoint> enemies);
        void SetInflationRatios(Dictionary<string, float> inner, Dictionary<string, float> outter);

        //Mark 
        void setNextCue(UserDefinedPoint nexeEnemy);

    }
}
