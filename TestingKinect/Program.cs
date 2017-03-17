using Microsoft.Kinect;
using Newtonsoft.Json;
using Quobject.SocketIoClientDotNet.Client;
using SocketIOClient;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TestingKinect
{

    class Program
    {
        private static KinectSensor _sensor;
        private static Body[] _bodies;
        private static BodyFrameReader _bodyFrameReader;
        private static CoordinateMapper _mapper;
        private static double _scaleFactorX;
        private static double _scaleFactorY;
        private static byte _currentInstruction;
        private static Dictionary<byte, Instruction> _instructionsSet = new Dictionary<byte, Instruction>();
        static void Main(string[] args)
        {
            //_instructionsSet.Add(1, new Instruction() { Text = "Move your arms and legs in order to touch the astroid\n", State = "random" });
            _instructionsSet.Add(1, new Instruction() { Text = "Move your arms and legs in order to touch the astroid\n", State = "random" });
            _instructionsSet.Add(2, new Instruction() { Text = "Try to touch the moving objects on screen", State = "moving" });
            _instructionsSet.Add(3, new Instruction() { Text = "Thank you for participating!", State = "win" });
            _currentInstruction = 1;
            var server = new WebSocketServer();
            server.Setup(2012);
            server.NewSessionConnected += server_NewSessionConnected;
            server.NewMessageReceived += server_NewMessageReceived;
            server.SessionClosed += server_SessionClosed;
            SetKinect();
            server.Start();
            Console.ReadKey();
            server.Stop();

        }

        private static void SetKinect()
        {
            _scaleFactorX = 1600.0 / 512.0;
            _scaleFactorY = 1000.0 / 424.0;
            _sensor = KinectSensor.GetDefault();
            _mapper = _sensor.CoordinateMapper;

            _sensor.IsAvailableChanged += Sensor_IsAvailableChanged;
            _bodies = null;
            _bodyFrameReader = _sensor.BodyFrameSource.OpenReader();

            _bodyFrameReader.FrameArrived += _bodyFrameReader_FrameArrived;

        }

        private static WebSocketSession _session = null;
        static void server_NewSessionConnected(WebSocketSession session)
        {
            _session = session;
            _currentInstruction = 1;
        }

        static void server_NewMessageReceived(WebSocketSession session, string value)
        {
            var message = JsonConvert.DeserializeObject<ClientMessageNew>(value);
            switch (message.Type)
            {
                case Protocol.KINECT_START:
                    if (!_sensor.IsOpen)
                    {
                        Console.WriteLine("Starting kinect");
                        ServerMessage<string> response = new ServerMessage<string>();
                        response.Type = Protocol.KINECT_START;
                        response.Data = "Starting Kinect";
                        _session.Send(JsonConvert.SerializeObject(response));
                        _sensor.Open();
                    }
                    break;
                case Protocol.KINECT_STOP:
                    if (_sensor.IsOpen)
                    {
                        Console.WriteLine("Stopping kinect");
                        ServerMessage<string> response = new ServerMessage<string>();
                        response.Type = Protocol.KINECT_STOP;
                        response.Data = "Kinect Stopped";
                        _session.Send(JsonConvert.SerializeObject(response));
                        _sensor.Close();
                    }
                    break;
                case Protocol.GET_NEXT_INSTRUCTIONS:
                    Console.WriteLine("Sending next instruction: "+_currentInstruction);
                    ServerMessage<Instruction> nextIncRes = new ServerMessage<Instruction>();
                    nextIncRes.Type = Protocol.GET_NEXT_INSTRUCTIONS;
                    nextIncRes.Data = _instructionsSet[_currentInstruction++];
                    _session.Send(JsonConvert.SerializeObject(nextIncRes));
                    break;
            }

        }



        static void server_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }

        }

        static void _bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
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
                Console.WriteLine("=========Frame===========");
                foreach (Body body in _bodies)
                {
                    if (body.IsTracked)
                    {
                        Console.WriteLine("=========TrackedBody===========");
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                        foreach (JointType jointType in joints.Keys)
                        {

                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f;
                            }
                            DepthSpacePoint depthPoint = _mapper.MapCameraPointToDepthSpace(position);

                            Point p = new Point()
                            {
                                X = (int)(depthPoint.X * _scaleFactorX),
                                Y = (int)(depthPoint.Y * _scaleFactorY),
                            };

                            jointPoints[jointType] = p;
                            Console.WriteLine("Point:" + p.X + " " + p.Y);

                        }
                        ServerMessage<List<Point>> message = new ServerMessage<List<Point>>();
                        message.Type = Protocol.SKELETON_DATA;
                        message.Data = jointPoints.Values.ToList<Point>();
                        _session.Send(JsonConvert.SerializeObject(message));

                    }
                }
            }
        }


        private static void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            Console.WriteLine(_sensor.IsAvailable ? "Running" : "NotAvailable");
            if (_session != null)
            {
                ServerMessage<bool> message = new ServerMessage<bool>();
                message.Type = Protocol.KINECT_CHANGED_AVAILABILITY;
                message.Data = _sensor.IsAvailable;
                _session.Send(JsonConvert.SerializeObject(message));
            }
        }
    }
}
