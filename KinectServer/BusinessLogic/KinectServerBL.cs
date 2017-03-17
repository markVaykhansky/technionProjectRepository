using KinectServer.Common;
using KinectServer.Communication;
using KinectServer.DTO;
using KinectServer.Enum;
using KinectServer.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using KinectServer.Spline;
using KinectServer.Model;
using System.Configuration;

namespace KinectServer.BusinessLogic
{

    public class KinectServerBL
    {
        #region Constants

        private const int _averageResponeSize = 40;
        private const int _sampleDim = 57; // 19 joints (25 total - 21:25 and minus the first one which is the center) * 3 dimensions for each joint = 57
        private const int maxIterations = 10;
        private const double minEnthropy = double.MinValue;

        #endregion

        #region Private Fields

        private IKinectBusinessLogic _kinect;
        private GameLogic _game;
        private WebsocketBL _server;
        private SamplesDB _recordedSamples;
        private int _numOfUsers;
        
        private Dictionary<string, List<List<Point3D>>> _kinectSkeletonCoordinatePerStage;
        private Dictionary<string, List<List<ScreenPoint>>> _2dCoordinatePerStage;
        private Dictionary<string, List<ScreenPoint>> _trajectoriesDataIn2D;
        private Dictionary<string, List<Point3D>> _trajectoriesData;
        private Dictionary<string, List<Hit>> _recordedHitsData;
        private Dictionary<string, List<int>> _statesCues;
        
        private bool _waitingForPoint;
        private CustomPointsListParser _pointsParser;
        private string _currentPlayer;
        private string _instruction;
        
        private List<int> _currentCues;
        private int _t;
        private List<double[]> _PtU;
        private double _H;
        private int staticCue;
        private Random randomGenerator;
        private bool usePrecalculated;
        private string selectionMethod;
        private string dataFolderPath;
        private double minimumProbability;

        // ============= DEBUG ===============
        double[] matAfterDiv, matAfterLog, matAfterMul, matPxC, matCumSum;
        List<double[]> moves = new List<double[]>();
        // ===================================

        #endregion 

        #region Public Methods

        public KinectServerBL(IKinectBusinessLogic kbl, int port)
        {
            _waitingForPoint = false;
            Initialize(kbl, port);
        }
        
        public void Initialize(IKinectBusinessLogic kbl, int port)
        {
            if (_server != null)
            {
                _server.NewSessionStarted -= _server_NewSessionStarted;
                _server.NewMessageRecived -= _server_NewMessageRecived;
                _server.SessionClosed -= _server_SessionClosed;
                _server.StopServer();
            }

            _server = new WebsocketBL(port);
            _server.NewSessionStarted += _server_NewSessionStarted;
            _server.NewMessageRecived += _server_NewMessageRecived;
            _server.SessionClosed += _server_SessionClosed;

            if (_kinect != null)
            {
                _kinect.KinectAvailabletyChanged -= _kinect_KinectAvailabletyChanged;
                _kinect.NewJointsDataReady -= _kinect_NewJointsDataReady;
            }

            _kinect = kbl;
            _kinect.KinectAvailabletyChanged += _kinect_KinectAvailabletyChanged;
            _kinect.NewJointsDataReady += _kinect_NewJointsDataReady;

            _kinectSkeletonCoordinatePerStage = new Dictionary<string, List<List<Point3D>>>();
            _2dCoordinatePerStage = new Dictionary<string, List<List<ScreenPoint>>>();
            _recordedHitsData = new Dictionary<string, List<Hit>>();
            _trajectoriesData = new Dictionary<string, List<Point3D>>();
            _trajectoriesDataIn2D = new Dictionary<string, List<ScreenPoint>>();
            _pointsParser = new CustomPointsListParser();
            _statesCues = new Dictionary<string, List<int>>();
            _server.StartServer();

            //NewTestSystem();
            //TestSelectCue();

            var appSettings = ConfigurationManager.AppSettings;
            this.dataFolderPath = appSettings["dataFolder"];
            this.usePrecalculated = Convert.ToBoolean(appSettings["usePrecalculated"]);
            this.minimumProbability = Convert.ToDouble(appSettings["minimumProbabilityToDisplay"]);
            this.selectionMethod = appSettings["SelectionMethod"];
            _recordedSamples = new SamplesDB(this.dataFolderPath, this.usePrecalculated);
            _numOfUsers = _recordedSamples.NumOfUsers;
            
        }

        private int maxInd(double[] vec)
        {
            int res = 0;
            for (int i = 0; i < vec.Length; i++) {
                if (vec[i] > vec[res])
                {
                    res = i;
                }
            }
            return res;
        }

        /*
        private void readAfter(string path,ref double[] container)
        {
            char[] delimiter = { ',' };
            string[] lines = System.IO.File.ReadAllLines(path);
            container = new double[lines.Length];

            int counter = 0;
            foreach (string line in lines)
            {
                container[counter] = double.Parse(line);
                counter++;
            }
        }
         */
        /*
        private double dist(double[] v1, double[] v2)
        {
            double result = 0;
            double diff;
            double maxDiff = double.MinValue;
            double minDiff = double.MaxValue;

            for (int i = 0; i < v1.Length; i++)
            {
                diff = (v1[i] - v2[i]);
                if (Math.Abs(diff) > maxDiff)
                {
                    maxDiff = Math.Abs(diff);
                }
                if (Math.Abs(diff) < minDiff)
                {
                    minDiff = Math.Abs(diff);
                }
                result += Math.Pow(diff, (double)2);
            }

            result = Math.Sqrt(result);
            return result;
        }
         */

        private void readCues(string path, List<int> cues)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                cues.Add(Convert.ToInt32(line));
            }
        }
        
        private void readZandPtU(string path, List<double[]> container)
        {
            char[] delimiter = { ',' };
            string[] lines = System.IO.File.ReadAllLines(path);
            string[] vals;
            double[] row;
            int counter = 0;

            foreach (string line in lines)
            {
                counter = 0;
                vals = line.Split(delimiter);
                row = new double[vals.Length];
                foreach (string val in vals)
                {
                    row[counter] = double.Parse(val);
                    counter++;
                }
                container.Add(row);
            }
        }

        public void SetPlayerName(string player){
            _currentPlayer = player;
        }

        #endregion

        #region Events

        public event Action<List<Point3D>> NewKinectDataReady;
        public event Action<KinectState> KinectStateChanged;
        public event Action<string> GameStateChanged;
        public event Action SessionStarted;
        public event Action SessionClosed;
        
        #endregion

        #region Private Methods

        private void _server_SessionClosed()
        {
            if (_kinect != null)
            {
                _kinect.StopKinect();
                if (KinectStateChanged != null)
                {
                    KinectStateChanged(KinectState.STOPPED);
                }

                //GenerateMatlabFiles();
                _recordedHitsData.Clear();
                _trajectoriesData.Clear();
                _trajectoriesDataIn2D.Clear();
                _kinectSkeletonCoordinatePerStage.Clear();
                _2dCoordinatePerStage.Clear();
                _statesCues.Clear();
            }
            if (SessionClosed != null)
            {
                SessionClosed();
            }
        }

        private void _server_NewMessageRecived(Communication.ClientMessage message)
        {
            switch (message.Type)
            {
                case Protocol.PLAYER_NAME:
                    _currentPlayer = message.Data;
                    _pointsParser.Init();
                    break;

                case Protocol.KINECT_START:
                    if (!_kinect.IsOpen)
                    {
                        _server.PostMessage<string>("Starting Kinect", Protocol.KINECT_START);
                        _kinect.IsGetNextPoint = false;
                        _kinect.IsNextPointMoving = false;
                        _kinect.StartKinect();
                    }
                    break;

                case Protocol.KINECT_STOP:
                    if (_kinect.IsOpen)
                    {
                        _server.PostMessage<string>("Stopping Kinect", Protocol.KINECT_STOP);
                        _kinect.StopKinect();
                    }
                    break;

                case Protocol.GET_NEXT_INSTRUCTIONS:
                    var instruction = _game.NextInstruction;
                    if (instruction.State != "win")
                    {
                        _trajectoriesData.Add(instruction.State, new List<Point3D>());
                        _trajectoriesDataIn2D.Add(instruction.State, new List<ScreenPoint>());
                        _recordedHitsData.Add(instruction.State, new List<Hit>());

                        _currentCues = new List<int>();
                        _PtU = new List<double[]>();
                        double[] P0U = Enumerable.Repeat<double>(((double)1 / _numOfUsers), _numOfUsers).ToArray();
                        _PtU.Add(P0U);
                        _t = 1;
                        this.randomGenerator = new Random();
                        
                        _instruction = instruction.State;
                        int firstCueID = selectCue(_PtU.LastOrDefault(),this.selectionMethod);
                        _statesCues.Add(_instruction, new List<int>() { firstCueID }); 
                        _currentCues.Add(firstCueID);
                        _kinect.setNextCue(_pointsParser.GetCue(firstCueID)); 
                    }
                    _server.PostMessage<Instruction>(instruction, Protocol.GET_NEXT_INSTRUCTIONS);
                    if (GameStateChanged != null)
                    {
                        GameStateChanged(instruction.State);
                    }
                    break;

                case Protocol.GET_NEXT_STAR_POSITION:
                    _waitingForPoint = true;
                    _kinect.IsGetNextPoint = true;
                    break;

                case Protocol.GET_SAME_STAR_POSITION:
                    _waitingForPoint = true;
                    _kinect.IsGetNextPoint = true;
                    _kinect.IsGetSamePoint = true;
                    break;

                case Protocol.HITS_DATA:
                    var hitData = JsonConvert.DeserializeObject<HitData>(message.Data);
                    if (hitData == null) break;

                    var hit = _recordedHitsData[_game.CurrentState].LastOrDefault();
                    if (hit == null) throw new FieldAccessException("Missing hit");
                        
                    hit.T2 = hitData.skeletonIndex;
                    hit.HitJointIndex = hitData.jointIndex;
                    if (hit.T2 == -1 || ((hit.T2 - hit.T1) <= 0)) break; 
                        
                    int responseSize = hit.T2 - hit.T1;
                    List<List<Point3D>> responseFrames = _kinectSkeletonCoordinatePerStage[_game.CurrentState].GetRange(hit.T1 - 1, responseSize);

                    double[][] normalizedWRTCenterJoin = new double[_sampleDim][];
                    for (int i = 0; i < _sampleDim; i++)
                    {
                        normalizedWRTCenterJoin[i] = new double[responseFrames.Count];
                    }

                    double[][] normalizedPostSpline = new double[_sampleDim][];
                    for (int i = 0; i < _sampleDim; i++)
                    {
                        normalizedPostSpline[i] = new double[_averageResponeSize];
                    }

                    // normalized with respect to center heap joint and then temporal normalization 
                    normalizeWRTSpineCenter(responseFrames, normalizedWRTCenterJoin);
                    for (int i = 0; i < _sampleDim; i++)
                    {
                        calculateSpline(responseFrames.Count, _averageResponeSize, normalizedWRTCenterJoin[i], ref normalizedPostSpline[i]);
                    }

                    int motionPatternVectorSize = _sampleDim * _averageResponeSize;
                    double[] normalized = new double[motionPatternVectorSize]; // 40 * 57 = 2280
                    int curIndex = 0;
                    for (int i = 0; i < _averageResponeSize/*responseFrames.Count*/; i++)
                    {
                        for (int j = 0; j < _sampleDim; j++)
                        {
                            normalized[curIndex] = normalizedPostSpline[j][i];
                            curIndex++;
                        }
                    }

                    moves.Add(normalized);
                    updateProbabilities(normalized, _currentCues.LastOrDefault());
                               
                    //_H = computeEnthropy(_PtU.LastOrDefault());
                    _t++;

                    /*
                    if (_H < minEnthropy || _t > maxIterations)
                    {
                        // ????
                        Console.WriteLine("Enthropy: "+_H+"Number of iteretions: "+_t+"\nGoodbye!");
                    }
                    */ 

                    int nextCueID = selectCue(_PtU.LastOrDefault(), this.selectionMethod);
                    _currentCues.Add(nextCueID);
                    _statesCues[_instruction].Add(nextCueID);
                            
                    UserDefinedPoint nextCue = _pointsParser.GetCue(nextCueID);
                    _kinect.setNextCue(nextCue);


                    printProbabilities();
                    int maxID = maxInd(_PtU.LastOrDefault());
                    Console.WriteLine("maxID: " + maxID + " Player: " + _recordedSamples.getName(maxID));
                    double maxProb = (this._PtU.LastOrDefault() as IEnumerable<double>).Max();

                    if (maxProb > this.minimumProbability)
                    {
                        string probMessage = _recordedSamples.getName(maxID) + ':' + maxProb.ToString();
                        _server.PostMessage<string>(probMessage, Communication.Protocol.DISP_PROB);
                    }

                    break;

                case Protocol.GAME_DONE:
                    _kinect.StopKinect();
                    GenerateMatlabFiles();
                    break;
            }
        }

        private void saveVector(double[] z,string fileName)
        {
            using (System.IO.StreamWriter file =
            System.IO.File.AppendText(string.Format(@"C:\Users\Mark\Desktop\{0}.txt",fileName)))
            {
                foreach (double cord in z)
                {
                    file.Write(cord + " ");
                }

                file.WriteLine("");
            }
        }

        private void printProbabilities()
        {
            int curID = 0;
            string curName;

            Console.WriteLine("======================");
            Console.WriteLine("Probability Vector:");
            foreach (double p in _PtU.LastOrDefault())
            {
                curName = _recordedSamples.getName(curID);
                Console.WriteLine(curName + ": " + p);
                curID++;
            }
        }

        private void NewTestSystem()
        {
            string homeTrain = @"C:\Users\Mark\Desktop\TestTraining\Train\";
            string homeTest = @"C:\Users\Mark\Desktop\TestTraining\Test\";
            _recordedSamples = new SamplesDB(homeTrain, false);
            _numOfUsers = _recordedSamples.NumOfUsers;
            SamplesDB test = new SamplesDB(homeTest, false);
            int numOfRounds = 10;
            Random rnd = new Random();

            foreach (int user in test._unique_users)
            {
                double[] P0U = Enumerable.Repeat<double>(((double)1 / _numOfUsers), _numOfUsers).ToArray();
                List<double[]> PtU = new List<double[]>();
                PtU.Add(P0U);
                _t = 1;

                for (int round = 0; round < numOfRounds; round++)
                {
                    int NextCue = selectCue(PtU.LastOrDefault(), "IGmax");

                    double[] z = test.GetNextVec(user, NextCue);
                    while (z == null)
                    {
                        z = test.GetNextVec(user, rnd.Next(1, 16));
                    }

                    updateProbabilitiesTest(_recordedSamples, PtU, z, NextCue);
                    _t++;
                }

                foreach (double prob in PtU.LastOrDefault()) Console.WriteLine(prob);
                Console.WriteLine("System guess: " + test._unique_users[maxInd(PtU.LastOrDefault())]);
                Console.WriteLine("Actual User: " + user);
            }

            return;
        }

        private void TestSelectCue()
        {
            staticCue = 1;
            string home = @"C:\Users\Mark\Desktop\TestTraining\";

            /*
            readAfter(home + @"\Results\afterDiv.txt", ref matAfterDiv);
            readAfter(home + @"\Results\afterLog.txt", ref matAfterLog);
            readAfter(home + @"\Results\afterMul.txt", ref matAfterMul);
            readAfter(home + @"\Results\PxC.txt", ref matPxC);
            readAfter(home + @"\Results\cumSum.txt", ref matCumSum);
            */

            _recordedSamples = new SamplesDB(home, true);
            _numOfUsers = _recordedSamples.NumOfUsers;

            for (int x = 1; x <= _numOfUsers; x++)
            {
                List<double[]> responses = new List<double[]>();
                List<int> recordedCues = new List<int>();
                List<double[]> recordedPtU = new List<double[]>();
                readZandPtU(home + @"\Results\IGmax\" + x + @"\responses.txt", responses);
                readZandPtU(home + @"\Results\IGmax\" + x + @"\PtU.txt", recordedPtU);
                readCues(home + @"\Results\IGmax\" + x + @"\cues.txt", recordedCues);

                _currentCues = new List<int>(); // List of cue id's during this game
                _PtU = new List<double[]>(); // List of probabilities (the system's belief about the current user's identity), starting with a uniform P0U
                double[] P0U = Enumerable.Repeat<double>(((double)1 / _numOfUsers), _numOfUsers).ToArray();
                _PtU.Add(P0U);
                _t = 0;

                for (int i = 0; i < responses.Count; i++)
                {
                    int nextCue = selectCue(_PtU.LastOrDefault(), "IGmax");
                    Console.WriteLine("VS next cue: " + nextCue);
                    Console.WriteLine("Matlab cue: " + recordedCues.ElementAt(i));
                    _currentCues.Add(nextCue);

                    _PtU.Add(recordedPtU[i]);

                    _t++;
                }

                Console.WriteLine("End of user " + x);
            }
            return;
        }

        private void normalizeWRTSpineCenter(List<List<Point3D>> frames, double[][] normalizedFrames)
        {
            int yIndex, frameIndex = 0;

            foreach (List<Point3D> frame in frames)
            {
                Point3D spineBase = frame.ElementAt(0);
                yIndex = 0;

                for (int jointIndex = 2; jointIndex < 21; jointIndex++) // we need only frames 2:20, we ignore #1 which is spine base and joints 21:25 which are fingers 
                {
                    int jointIndexInFrame = jointIndex - 1;
                    Point3D currentJoint = frame.ElementAt(jointIndexInFrame);
                    normalizedFrames[yIndex++][frameIndex] = (currentJoint.X - spineBase.X);
                    normalizedFrames[yIndex++][frameIndex] = (currentJoint.Y - spineBase.Y);
                    normalizedFrames[yIndex++][frameIndex] = (currentJoint.Z - spineBase.Z);
                }

                frameIndex++;
            }
        }

        private int selectCue(double[] PtU, string selectionMethod)
        {
            int cueId = -3;
            List<double[,]> PxCU = _recordedSamples.PxCU.probabilities;
            double[,] currentPxCU;
            int Nc = _recordedSamples.PxCU.probabilities.Count;

            double[] IG = new double[Nc];
            double afterLog2, afterMul, afterDiv;
            double[] Pc = new double[3];
            int ind;

            switch (selectionMethod)
            {
                case "Static":
                    
                    if (++this.staticCue > this._recordedSamples.NumOfUniqueCues)
                    {
                        this.staticCue = 1;
                    }

                    return this.staticCue;

                case "IGmaxRand5":
                case "IGmax":

                    for (int cc = 0; cc < Nc; cc++)
                    {
                        currentPxCU = PxCU.ElementAt(cc);

                        double[] PxC = new double[currentPxCU.GetLength(1)];
                        for (int row = 0; row < currentPxCU.GetLength(0); row++) // iterate rows
                        {
                            for (int column = 0; column < currentPxCU.GetLength(1); column++) // iterate columns
                            {
                                PxC[column] += (currentPxCU[row, column] * PtU[row]);
                            }
                        }

                        // sum[i] = sum of row i in PxCU{cc}  multiplied by log2 of:[ A./B where A is PxCU{cc} and B = PxC ]
                        double[] rowSum = new double[currentPxCU.GetLength(0)];
                        
                        for (int i = 0; i < currentPxCU.GetLength(0); i++) // itereate through rows
                        {
                            for (int j = 0; j < currentPxCU.GetLength(1); j++)
                            {
                                afterDiv = (currentPxCU[i, j] / PxC[j]);
                                afterLog2 = Math.Log(afterDiv, 2.0);
                                afterMul = afterLog2 * currentPxCU[i, j];
                                rowSum[i] += afterMul;
                            }
                        }

                        for (int i = 0; i < PtU.Length; i++)
                        {
                            IG[cc] += (PtU[i] * rowSum[i]);
                        }

                    }

                    // [~,indOpt] = sort(IG,'descend');
                    int[] indOpt = Enumerable.Range(0, IG.Length).ToArray();
                    double[] IGClone = new double[IG.Length];
                    Array.Copy(IG, IGClone, IG.Length);
                    Array.Sort(IGClone, indOpt);
                    Array.Reverse(indOpt);

                    if (selectionMethod == "IGmaxRand5")
                    {
                        Random rnd = new Random();
                        int randomIndex = rnd.Next(0,5);
                        cueId = indOpt[randomIndex] + 1; // indices start from 0, cue ids start from 1, thus the +1
                        return cueId;
                    }

                    if (_t > 1)
                    {
                        double sum3 = IG[indOpt[0]] + IG[indOpt[1]] + IG[indOpt[2]];
                        for (int tmp = 0; tmp < 3; tmp++)
                        {
                            Pc[tmp] = IG[indOpt[tmp]] / sum3; //    Pc = IG(indOpt(1:3))/sum(IG(indOpt(1:3))); => this probability distribution function (PDF)
                        }
                        ind = discreternd(Pc) - 1; // indexes start from 0.. 
                        cueId = indOpt[ind] + 1; // ind = discreternd(Pc); // this computes CDF
                    }
                    else
                    {
                        cueId = indOpt[0] + 1; // indices start from 0, cue ids start from 1, thus the +1
                    }

                    break;

                case "Random":
                    cueId = this.randomGenerator.Next(1, _recordedSamples.NumOfUniqueCues + 1);
                    break;

                default:
                    cueId = -1;
                    break;
            }

            return cueId;
        }
        
        private void calculateSpline(int origin_size,int normalized_size,double[] y,ref double[] normalized) 
        {
            double[] x = new double[origin_size];
            double[] xs = new double[normalized_size];
            CubicSpline spline = new CubicSpline();
            
            // create x
            for (int i = 0; i < origin_size; i++)
            {
                x[i] = i;
            }

            // create xs
            double stepSize = (x[x.Length - 1] - x[0]) / (normalized_size - 1);
            for (int i = 0; i < normalized_size; i++)
            {
                xs[i] = x[0] + i * stepSize;
            }

            // now we have x,y and xx: now compute yy using spline
            normalized = spline.FitAndEval(x, y, xs);
        }
        
        private void updateProbabilitiesTest(SamplesDB db, List<double[]> PtUList, double[] z, int prevCue)
        {
            double[] newPtU = new double[db.NumOfUsers];
            db.updateProbabilitiesHelperFunction(z, prevCue, newPtU); 
            double[] prevProb = PtUList.LastOrDefault();


            for (int i = 0; i < db.NumOfUsers; i++)
            {
                newPtU[i] *= prevProb[i];
            }
            double sum = newPtU.Sum();
            for (int i = 0; i < db.NumOfUsers; i++)
            {
                newPtU[i] /= sum;
            }

            PtUList.Add(newPtU);
        }
        
        private void updateProbabilities(double[] z,int prevCue) {
            double[] newPtU = new double[_recordedSamples.NumOfUsers];
            _recordedSamples.updateProbabilitiesHelperFunction(z, prevCue, newPtU); // PzUA = sum(PzUA, 2);
            double[] prevProb = _PtU.LastOrDefault();
             
            for (int i = 0; i < _recordedSamples.NumOfUsers; i++)
            {
                newPtU[i] *= prevProb[i];
            }

            double sum = newPtU.Sum();
            for (int i = 0; i<_recordedSamples.NumOfUsers; i++)
            {
                newPtU[i] /= sum;
            }

            _PtU.Add(newPtU);
        }
        
        private double computeEnthropy(double[] P) 
        {
            double sum = 0;
            int i;
            double H = 0;

            //    P = P./repmat(sum(P,1),size(P,1),1);
            for (i = 0; i < P.Length; i++)
            {
                sum += P[i];
            }
            for(i = 0; i < P.Length; i++) {
                P[i]/=sum;
            }

            // H = -sum(P.*log2(P),dim);
            for (i = 0; i < P.Length; i++)
            {
                H += (P[i] * Math.Log(P[i], 2));
            }
            return -H;
        }
        
        int discreternd(double[] p) {
            // NOTICE: we set n=1 !
            // for n!=1 change the implementation

            Random rnd = new Random();
            double u = rnd.NextDouble();
            double sum = p.Sum();

            for (int i = 1; i < p.Length; i++)
            {
                p[i] += p[i - 1]; // cumsum(p)
                p[i] /= sum; // normalize cumsum(p)   
            }

           // var tmp = Array.FindAll<double>(p, i => u > i);

            var res = Array.FindAll<double>(p, i => u > i).Length + 1; // sum(u > cdf,1)+1;
            return res;
        }
        
        private void _server_NewSessionStarted()
        {
            _pointsParser.Init();
            _game = new GameLogic(_pointsParser.GetStages());
            _kinect.SetInflationRatios(_pointsParser.GetInnerBoundaryInflationRatios(), _pointsParser.GetOuterBoundaryInflationRatios());
            _game.ResetInstructionsCounter();
            if (SessionStarted != null)
            {
                SessionStarted();
            }
        }

        private void _kinect_NewJointsDataReady(DisplayData screenData, List<Point3D> kinectCoordinatesSkeleton, Point3D enemyObject)
        {
            _server.PostMessage<DisplayData>(screenData, Communication.Protocol.SKELETON_DATA);
            if (_waitingForPoint)
            {
                _recordedHitsData[_game.CurrentState].Add(new Hit() { T1 = screenData.SkeletonIndex });
                _server.PostMessage<ScreenPoint>(screenData.NextEnemyPoint, Communication.Protocol.GET_NEXT_STAR_POSITION);
                _waitingForPoint = false;
            }

            if (_game.CurrentState != "win")
            {
                if (enemyObject != null)
                {
                    _trajectoriesData[_game.CurrentState].Add(enemyObject);                        //This is a real location of a moving object
                    if (screenData.NextEnemyPoint != null)
                    {
                        _trajectoriesDataIn2D[_game.CurrentState].Add(screenData.NextEnemyPoint);
                    }
                    else
                    {
                        _trajectoriesDataIn2D[_game.CurrentState].Add(new ScreenPoint());
                    }
                }
                else
                {
                    _trajectoriesData[_game.CurrentState].Add(new Point3D(0, 0, 0));  //This is padding so indecis will match skeleton indecis
                    var sp = new ScreenPoint();
                    _trajectoriesDataIn2D[_game.CurrentState].Add(sp);
                }
            }

            if (_game.CurrentState != null && _game.CurrentState != "win")
            {
                if (!_kinectSkeletonCoordinatePerStage.ContainsKey(_game.CurrentState))
                {
                    _kinectSkeletonCoordinatePerStage.Add(_game.CurrentState, new List<List<Point3D>>());
                    _2dCoordinatePerStage.Add(_game.CurrentState, new List<List<ScreenPoint>>());
                }
                _kinectSkeletonCoordinatePerStage[_game.CurrentState].Add(kinectCoordinatesSkeleton);
                _2dCoordinatePerStage[_game.CurrentState].Add(screenData.Joints);

            }

            if (NewKinectDataReady != null)
            {
                NewKinectDataReady(kinectCoordinatesSkeleton);
            }
        }

        private void _kinect_KinectAvailabletyChanged(bool state)
        {
            _server.PostMessage<bool>(state, Communication.Protocol.KINECT_CHANGED_AVAILABILITY);
            if (KinectStateChanged != null)
            {
                if (state)
                {
                    KinectStateChanged(KinectState.READY);
                }
                else
                {
                    KinectStateChanged(KinectState.NOT_READY);
                }
            }
        }

        private void GenerateMatlabFiles()
        {
            foreach (double[] move in moves)
            {
                saveVector(move, "moves" + DateTime.Now.Day + "-" + DateTime.Now.Date.Month + DateTime.Now.Hour + "_" + DateTime.Now.Minute);
            }

            foreach (double[] probs in _PtU)
            {
                saveVector(probs, "probs" + DateTime.Now.Day + "-" + DateTime.Now.Date.Month + DateTime.Now.Hour + "_" + DateTime.Now.Minute);
            }

            _currentCues.RemoveAt(_currentCues.Count-1);
            _statesCues[_instruction].RemoveAt(_statesCues[_instruction].Count-1);
            using (System.IO.StreamWriter file =
            System.IO.File.AppendText(string.Format(@"C:\Users\Mark\Desktop\cues{0}-{1}-{2}-{3}.txt",DateTime.Now.Day,DateTime.Now.Month,DateTime.Now.Hour,DateTime.Now.Minute)))
            {
                foreach (int cue in _currentCues)
                {
                    file.WriteLine(cue);
                }
            }

            MatlabDriver.SetFolder();
            if (_kinectSkeletonCoordinatePerStage.Count != 0)
            {
                foreach (var stage in _kinectSkeletonCoordinatePerStage.Keys)
                {
                    MatlabDriver.ToMatAll(_kinectSkeletonCoordinatePerStage[stage], _recordedHitsData[stage], _trajectoriesData[stage], _2dCoordinatePerStage[stage], _trajectoriesDataIn2D[stage],_statesCues[stage], _currentPlayer);
                }
            }
        }
       
        #endregion
    }
}
