using KinectServer.DTO;
using KinectServer.Model;
using Microsoft.Kinect;
using Physics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using KinectServer.Common;

namespace KinectServer.BusinessLogic
{
    public class KinectBL : IKinectBusinessLogic
    {
        private KinectSensor _sensor;
        private Body[] _bodies;
        private BodyFrameReader _bodyFrameReader;
        private CoordinateMapper _mapper;
        private int _skeletonIndex;

        private List<UserDefinedPoint> _predefinedEnemiesList;
        private float _innerRectInflationRatioX = 1.0f;
        private float _innerRectInflationRatioY = 1.0f;
        private float _outterRectInflationRatioX = 1.0f;
        private float _outterRectInflationRatioY = 1.0f;
        private int _enemyCounter;
        private float _g=10.0f, _v=10.0f;
        private Moveable _currentEnemyObject;

        //Mark
        private UserDefinedPoint _currentEnemyDefenition;
        private UserDefinedPoint _nextEnemeyDefenition;
        private bool _nextEnemyReady;

        public KinectBL()
        {
            SetKinect();
            _skeletonIndex = 1;
            IsGetNextPoint = false;
            IsGetSamePoint = false;
            _predefinedEnemiesList = null;
            _enemyCounter = 0;
            _currentEnemyObject = null;
            // Mark
            _currentEnemyDefenition = null;
            _nextEnemeyDefenition = null;
            _nextEnemyReady = false;
        }

        private void SetKinect()
        {
            ScaleFactorX = (float)(1600.0 / 1919.0);
            ScaleFactorY = (float)(1000.0 / 1079.0);
            _sensor = KinectSensor.GetDefault();
            _mapper = _sensor.CoordinateMapper;

            _sensor.IsAvailableChanged += Sensor_IsAvailableChanged;
            _bodies = null;
            _bodyFrameReader = _sensor.BodyFrameSource.OpenReader();
            _bodyFrameReader.FrameArrived += _bodyFrameReader_FrameArrived;

        }

        private ColorSpacePoint ConverEnemyLocationToDepthSpace(Point3D enemyLocation)
        {
            CameraSpacePoint enemy = new CameraSpacePoint();
            enemy.X = (float)enemyLocation.X;
            enemy.Y = (float)enemyLocation.Y;
            if (enemyLocation.Z < 0)
            {
                enemy.Z = 0.1f;
            }
            else
            {
                enemy.Z = (float)enemyLocation.Z;
            }
            return _mapper.MapCameraPointToColorSpace(enemy);
            //return _mapper.MapCameraPointToDepthSpace(enemy);

        }

        private void _bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataRecived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (_bodies == null)
                    {
                        _bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(_bodies);
                    dataRecived = true;
                }
            }
            if (dataRecived)
            {
                foreach (Body body in _bodies)
                {
                    if (body.IsTracked)
                    {
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                        var screenPoints = new List<ScreenPoint>();
                        var kinectPoints = new List<Point3D>();
                        float zPlane = 0;
                        //Save points from current body joints
                        foreach (JointType jointType in joints.Keys)
                        {

                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f;
                            }
                            kinectPoints.Add(new Point3D(position.X, position.Y, position.Z));

                            var screenSpacePoint = _mapper.MapCameraPointToColorSpace(position);
                            ScreenPoint p = new ScreenPoint()
                            {
                                X = (int)(screenSpacePoint.X * ScaleFactorX),
                                Y = (int)(screenSpacePoint.Y * ScaleFactorY)
                            };
                            //DepthSpacePoint depthPoint = _mapper.MapCameraPointToDepthSpace(position);
                            //ScreenPoint p = new ScreenPoint()
                            //{
                            //    X = (int)(depthPoint.X * ScaleFactorX),
                            //    Y = (int)(depthPoint.Y * ScaleFactorY),
                            //};

                            screenPoints.Add(p);
                        }
                        Slice currentSlice = null;
                        if (NewJointsDataReady != null)
                        {
                            var displayData = new DisplayData();
                            if (IsGetNextPoint) // Mark: get the next enemy (?)
                            {
                                /*
                                  *Get bounding rectangle coordinates in camera space 
                                  *               TopY=Head
                                  *                 _____
                                  *                 | 0  |
                                  *                 |/|\ |
                                  *   MinX=Left hand| |  | MaxX=Right Hand
                                  *                 |/ \ |
                                  *                 |___\|
                                  * BottomY= Min(Left Foot, Right Foot)
                                  * 
                                 */

                                var leftSide = body.Joints[JointType.ShoulderLeft].Position;
                                var rightSide = body.Joints[JointType.ShoulderRight].Position;
                                
                                var head = body.Joints[JointType.Head].Position;
                                var leftFoot = body.Joints[JointType.FootLeft].Position;
                                var rightFoot = body.Joints[JointType.FootRight].Position;
                                zPlane = body.Joints[JointType.SpineBase].Position.Z;
                                if (zPlane < 0)
                                {
                                    zPlane = 0.1f;
                                }

                                var topY = head.Y;
                                var bottomY = Math.Min(leftFoot.Y, rightFoot.Y);
                                var minX = Math.Min(leftSide.X, rightSide.X);
                                var maxX = Math.Max(leftSide.X, rightSide.X);

                                float shoulderWidth = 0.4f; //average shoulder width is 30cm
                                var totalWidth = maxX - minX; //difference between both hands
                                if (totalWidth < shoulderWidth) //can happen if person stand on side
                                {
                                    var halfDiff = (shoulderWidth - totalWidth)/2;
                                    maxX += halfDiff;
                                    minX -= halfDiff;
                                }

                                BoundingRect bodyRect = new BoundingRect(minX, topY, zPlane, maxX - minX, topY - bottomY, 1.0f);
                                var innerRect = bodyRect.Inflate(_innerRectInflationRatioX, _innerRectInflationRatioY, 1.0f);
                                var outterRect = bodyRect.Inflate(_outterRectInflationRatioX, _outterRectInflationRatioY, 1.0f);
                                var boundariesRect = outterRect.Inflate(_outterRectInflationRatioX, _outterRectInflationRatioY,1.0f);

                                displayData.Rects = new Dictionary<int, List<ScreenPoint>>();
                                displayData.Rects.Add(0, new List<ScreenPoint>());
                                var r = displayData.Rects[0];
                                foreach (var p in bodyRect.GetCorners())
                                {
                                    var csp = ConverEnemyLocationToDepthSpace(p);
                                    var sp = new ScreenPoint();

                                    sp.X = (int)(csp.X * ScaleFactorX);
                                    sp.Y = (int)(csp.Y * ScaleFactorY);

                                    r.Add(sp);
                                }

                                displayData.Rects.Add(1, new List<ScreenPoint>());
                                var r2 = displayData.Rects[1];
                                foreach (var p in innerRect.GetCorners())
                                {
                                    var csp = ConverEnemyLocationToDepthSpace(p);
                                    var sp = new ScreenPoint();

                                    sp.X = (int)(csp.X * ScaleFactorX);
                                    sp.Y = (int)(csp.Y * ScaleFactorY);

                                    r2.Add(sp);
                                }

                                displayData.Rects.Add(2, new List<ScreenPoint>());
                                var r3 = displayData.Rects[2];
                                foreach (var p in outterRect.GetCorners())
                                {
                                    var csp = ConverEnemyLocationToDepthSpace(p);
                                    var sp = new ScreenPoint();

                                    sp.X = (int)(csp.X * ScaleFactorX);
                                    sp.Y = (int)(csp.Y * ScaleFactorY);

                                    r3.Add(sp);
                                }
                                
                                
                                var slices = Slicer.SliceRect(innerRect, outterRect, 4); //3

                                displayData.Slices = new Dictionary<int, List<ScreenPoint>>();
                                var SlicesCorners = new Dictionary<int, List<Point3D>>();
                                SlicesCorners.Add(0, slices[0].GetCorners());
                                SlicesCorners.Add(1, slices[1].GetCorners());
                                SlicesCorners.Add(2, slices[2].GetCorners());
                                SlicesCorners.Add(3, slices[3].GetCorners());
                                for (int i = 0; i < 4; i++)
                                {
                                    displayData.Slices.Add(i, new List<ScreenPoint>());
                                    foreach (var p in SlicesCorners[i])
                                    {
                                        var csp = ConverEnemyLocationToDepthSpace(p);
                                        var sp = new ScreenPoint();

                                        sp.X = (int)(csp.X * ScaleFactorX);
                                        sp.Y = (int)(csp.Y * ScaleFactorY);
                                        
                                        displayData.Slices[i].Add(sp);
                                    }
                                }

                                // Mark: this is the where we should decide upon the new enemy using our algorithm (according to probabilities (?))
                                UserDefinedPoint nextDefinedPoint;
                                if (IsGetSamePoint == true)
                                {
                                    nextDefinedPoint = _currentEnemyDefenition;
                                    IsGetSamePoint = false;
                                }
                                else
                                {
                                    while (_nextEnemyReady == false)
                                    {
                                            // busy wait for the next enemy to arrive 
                                    }
                                    nextDefinedPoint = _nextEnemeyDefenition;
                                    _currentEnemyDefenition = _nextEnemeyDefenition;
                                    _nextEnemyReady = false; // we've used the latest enemy
                                }
                                
                                  
                              /*   
                                 //* This is the previouse cue selection
                                if (IsGetSamePoint == true && _enemyCounter>0)
                                {
                                    _enemyCounter--; //So we repeat the previous point
                                    IsGetSamePoint = false;
                                }
                                var nextDefinedPoint = _predefinedEnemiesList[_enemyCounter];
                               */ 

                                currentSlice = slices[nextDefinedPoint.SliceId];
                                var slicePoint = currentSlice.ConvertPoint(nextDefinedPoint.X, nextDefinedPoint.Y, nextDefinedPoint.Z);
                                _currentEnemyObject = new Moveable(
                                    (float)slicePoint.X, (float)slicePoint.Y, (float)slicePoint.Z,
                                    nextDefinedPoint.V0X*_v, nextDefinedPoint.V0Y*_v, nextDefinedPoint.V0Z*_v,
                                    nextDefinedPoint.AX*_g, nextDefinedPoint.AY*_g, nextDefinedPoint.AZ*_g);
                                //_currentEnemyObject.SetBoundaries(-1.5f, -1.5f, -5.0f, 1.5f, 1.5f, 5.0f);
                                //var boundaries = boundariesRect.GetCorners();
                                //_currentEnemyObject.SetBoundaries((float)boundaries[0].X, (float)boundaries[0].Y, -5.0f, (float)boundaries[3].X, (float)boundaries[3].Y, 5.0f);
                                _currentEnemyObject.Init(); //Start time of object for trajectory calculations, t0
                                IsGetNextPoint = false;
                                _enemyCounter++;
                            }

                            if (_currentEnemyObject != null) // Mark: if we have an enemy (whether new or previous star) 
                            {                                // previous star: we update it's loaction with GetNextPosition.
                                var nextEnemyPosition = _currentEnemyObject.GetNextPosition();
                                var mappedEnemy = ConverEnemyLocationToDepthSpace(nextEnemyPosition);
                                displayData.NextEnemyPoint = new ScreenPoint();
                                displayData.NextEnemyPoint.X = (int)(mappedEnemy.X * ScaleFactorX);
                                displayData.NextEnemyPoint.Y = (int)(mappedEnemy.Y * ScaleFactorY);
                                displayData.NextEnemyPoint.Z = (int)zPlane;

                                //Calculate radius point
                                nextEnemyPosition.Y += 0.2f;
                                var mappedEnemyRadius = ConverEnemyLocationToDepthSpace(nextEnemyPosition);
                                displayData.RadiusPoint = new ScreenPoint();
                                displayData.RadiusPoint.X = (int)(mappedEnemyRadius.X * ScaleFactorX);
                                displayData.RadiusPoint.Y = (int)(mappedEnemyRadius.Y * ScaleFactorY);
                                displayData.RadiusPoint.Z = (int)zPlane;
                            }
                            displayData.Joints = screenPoints;
                            displayData.SkeletonIndex = _skeletonIndex++;

                            NewJointsDataReady(displayData, kinectPoints, _currentEnemyObject!=null?_currentEnemyObject.GetLastPosition():new Point3D(0,0,0));
                        }

                    }
                }
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (KinectAvailabletyChanged != null)
            {
                KinectAvailabletyChanged(_sensor.IsAvailable);
            }
        }
        
        public float ScaleFactorX { get; set; }
        public float ScaleFactorY { get; set; }
        public bool IsOpen { get { return _sensor.IsOpen; } }
        public bool IsNextPointMoving { get; set; }
        public bool IsGetNextPoint { get; set; }
        public bool IsGetSamePoint { get; set; }

        public event Action<bool> KinectAvailabletyChanged;
        /// <summary>
        /// DisplayData - Screen points of body in pixels, next enemy location in pixels, skeleton index
        /// List<PointRealWorld>Body skeleton coordinates in camera space</PointRealWorld>
        /// PointRealWorld  - Next enemy point in camera space
        /// </summary>
        public event Action<DisplayData, List<Point3D>, Point3D> NewJointsDataReady;

        public bool StartKinect()
        {
            if (!_sensor.IsOpen)
            {
                _sensor.Open();
                _skeletonIndex = 1;
                _predefinedEnemiesList = null;
                _enemyCounter = 0;
                _g = float.Parse(ConfigurationManager.AppSettings["g"]);
                _v = float.Parse(ConfigurationManager.AppSettings["v"]);
                return true;
            }
            return false;
        }

        public bool StopKinect()
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
                return true;
            }
            return false;
        }

        public void SetEnemiesList(List<UserDefinedPoint> enemies)
        {
            _predefinedEnemiesList = enemies;
        }

        public void setNextCue(UserDefinedPoint nexeEnemy)
        {
            _nextEnemeyDefenition = nexeEnemy;
            _nextEnemyReady = true;
        }

        public void SetInflationRatios(Dictionary<string, float> inner, Dictionary<string, float> outter)
        {
            _innerRectInflationRatioX = inner["RatioX"];
            _innerRectInflationRatioY = inner["RatioY"];
            _outterRectInflationRatioX = outter["RatioX"];
            _outterRectInflationRatioY = outter["RatioY"];
        }

    }
}
