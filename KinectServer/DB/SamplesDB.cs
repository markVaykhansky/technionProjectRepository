using KinectServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.DB
{
    class SamplesDB
    {
        #region Fields

        private List<Sample> _samples = new List<Sample>();
        private List<double[]> _samplesArrayLst = new List<double[]>();
        private Player[] _users;
        private int[] usersArray;
        private int[] _cues;

        private List<int> usedIndexes = new List<int>();
        public int[] _unique_users;
        private int[] _unique_cues;
        private Dictionary<int, string> _playersDict = new Dictionary<int, string>();

        private int _num_of_samples;

        public double[,,] PrCU; // dim1 = users, dim2 = responses, dim3 = cues
        public double PxRU = double.NaN; // do we need this? 
        public PxCU PxCU;

        #endregion 

        public SamplesDB(string dataFolderPath, bool usePrecalculated)
        {
            readNumbersStream(constructFullFilePath(dataFolderPath,"C"), ref _cues);
            readNumbersStream(constructFullFilePath(dataFolderPath,"CC"), ref _unique_cues);
            readNumbersStream(constructFullFilePath(dataFolderPath,"UU"), ref _unique_users);
            
            createUserDict(constructFullFilePath(dataFolderPath,"Dict"));
            readUsers(constructFullFilePath(dataFolderPath, "U"));
            readSamples(constructFullFilePath(dataFolderPath, "Samples"));
            estimatePrCU();
            
            if(usePrecalculated)
            {
                this.PxCU = readPrecalculatedProbabilities(dataFolderPath + "Precalculated\\");
            }
            else
            {
                this.PxCU = estimatePxCU();
              /*
                PxCU precalculatedPxCU = this.readPrecalculatedProbabilities(dataFolderPath + "Precalculated\\");
                double maxDiff = calculateDiff(this.PxCU, precalculatedPxCU);
                Console.WriteLine("===========================================");
                Console.WriteLine("Max Diff between PxCU created in Matlab and VS: " + maxDiff);
                Console.WriteLine("===========================================");
              */
            }
            //testMatchingData();
        }

        #region Properties 

        public int NumOfUniqueCues
        {
            get
            {
                return _unique_cues.Length;
            }
        }

        #endregion 

        #region Public Methods

        public int NumOfUsers
        {
            get
            {
                return _unique_users.Length;
            }
        }

        public string getName(int index)
        {
            int userId = _unique_users[index];
            return _playersDict[userId];
        }

        private void createUserDict(string usersDictionaryPath)
        {
            string[] usersNames = System.IO.File.ReadAllLines(usersDictionaryPath);
            int counter = 0;

            foreach (string user in usersNames)
            {
                _playersDict.Add(_unique_users[counter++], user);
            }

            return;
        }

        public double[] GetNextVec(int user, int cue)
        {
            List<int> playerIndexes = new List<int>();
            int counter = 0;
            foreach (int curUser in usersArray)
            {
                if (curUser == user)
                {
                    playerIndexes.Add(counter);
                }
                counter++;
            }

            foreach (int index in playerIndexes)
            {
                if (_cues[index] == cue)
                {
                    if (!usedIndexes.Contains(index))
                    {
                        usedIndexes.Add(index);
                        return _samplesArrayLst[index];
                    }
                }
            }
            return null;
        }

        private void testMatchingData() {
            
            for (int uu = 1; uu <= _unique_users.Length; uu++)
            {
                for (int aa = 1; aa <= _unique_cues.Length; aa++)
                {
                    //List<int> indexes = getMatchingIndexes(uu,aa);
                    //List<int> secondOption = CollectionsExtensions.FindAllIndexNEW(Enumerable.Range(0, _num_of_samples).ToList(), i => ((((_users[i]).Num + 1) == uu) && (_cues[i] == aa)));
                    List<int> secondOption = getIndexesWhere(i => ((((_users[i]).Num + 1) == uu) && (_cues[i] == aa)));

                    if (secondOption.Count == 0)
                    {
                        Console.WriteLine("User: " + uu + "user id: "+ _unique_users[uu-1] + " missing cue: "+ aa);
                        continue; 
                    }
                }
            }
        }

        public void updateProbabilitiesHelperFunction(double[] z, int cue, double[] result)
        {
            int M = _unique_users.Length;
            int N = _unique_cues.Length;

            double[,] PzUA = new double[M, N]; 
            for (int uu = 1; uu <= M; uu++)
            {
                for (int aa = 1; aa <= N; aa++)
                {
                    //List<int> indexes = CollectionsExtensions.FindAllIndexNEW(Enumerable.Range(0, _num_of_samples).ToList(), i => ((((_users[i]).Num+1) == uu) && (_cues[i] == aa)));
                    //List<int> indexes = getMatchingIndexes(uu, aa);
                    List<int> indexes = getIndexesWhere(i => ((((_users[i]).Num + 1) == uu) && (_cues[i] == aa)));
                    if (indexes.Count == 0)
                    {
                        PzUA[uu - 1, aa - 1] = 0;
                        continue; // missing data 
                    }
                    double searchResult = KDEstimatePxRU(z, _samplesArrayLst, indexes);
                    var tmp = PrCU[uu - 1, aa - 1, cue - 1];
                    PzUA[uu - 1, aa - 1] = PrCU[uu - 1, aa - 1, cue - 1] * searchResult;
                    if (double.IsNaN(PzUA[uu - 1, aa - 1]))
                    {
                        Console.WriteLine("NaN");
                    }
                }
            }

            // now we need to sum each row
            // result[i] = sum of the element in row i in PzUA
            for (int row = 0; row < M; row++)
            {
                result[row] = 0;
                for (int column = 0; column < N; column++)
                {
                    result[row] += PzUA[row, column];
                }
            }

            return;
        }

        #endregion

        #region Private Methods

        private void readSamples(string samplesPath)
        {
            char[] delimiter = { ' ' };
            string[] lines = System.IO.File.ReadAllLines(samplesPath);
            _num_of_samples = lines.Length;
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
                _samples.Add(new Sample(row, vals.Length));
                _samplesArrayLst.Add(row);
            }
        }

        private void readUsers(string usersPath)
        {
            string[] vals = System.IO.File.ReadAllLines(usersPath);
            _users = new Player[vals.Length];
            this.usersArray = new int[vals.Length];
            
            int counter = 0;
            //int userCounter = 0;
            int curUserID;
            Dictionary<int, int> discoveredUsers = new Dictionary<int, int>(); // 

            foreach (string val in vals)
            {
                curUserID = Convert.ToInt32(val);
                
                /*
                if (!discoveredUsers.ContainsKey(curUserID))
                {
                    userCounter++;
                    discoveredUsers.Add(curUserID, userCounter);
                }
                */

                int indexInUniqueUsers = this._unique_users.ToList().IndexOf(curUserID);
                _users[counter] = new Player(indexInUniqueUsers, curUserID, _playersDict[curUserID]);
                this.usersArray[counter] = curUserID;
                //_users[counter] = new Player(discoveredUsers[curUser], curUser, _playersDict[curUser]);
                counter++;

            }

            if (this._users.Any((Player player) => player.Id != this._unique_users[player.Num]))
            {
                Console.WriteLine("Something is wrong !!!");
            }

            return;
        }

        private PxCU readPrecalculatedProbabilities (string precalculatedDataPath)
        {
            PxCU precalculatedPxCU = new PxCU(-1,-1);

            for (int numOfPxCU = 1; numOfPxCU <= _unique_cues.Length; numOfPxCU++)
            {
                string currentPath = precalculatedDataPath + "PxCU_" + numOfPxCU.ToString() + ".txt";

                char[] delimiter = { ' ' };
                string[] lines = System.IO.File.ReadAllLines(currentPath);
                _num_of_samples = lines.Length;
                string[] vals;
                int counter = 0;
                double[,] curTable = new double[lines.Length, lines[0].Split(delimiter).Length];

                int row = 0;
                foreach (string line in lines)
                {
                    counter = 0;
                    vals = line.Split(delimiter);
                    foreach (string val in vals)
                    {
                        curTable[row,counter] = double.Parse(val);
                        counter++;
                    }
                    row++;
                }
                precalculatedPxCU.probabilities.Add(curTable);
            }

            return precalculatedPxCU;
        }

        private void readNumbersStream(string streamPath, ref int[] container)
        {
            string[] vals = System.IO.File.ReadAllLines(streamPath);
            container = new int[vals.Length];

            int counter = 0;
            foreach (string val in vals)
            {
                container[counter++] = Convert.ToInt32(val);
            }

            return;
        }

        private void estimatePrCU()
        {
            PrCU = new double[_unique_users.Length, _unique_cues.Length, _unique_cues.Length]; 
            int curUser, curAction, curCue;

            bool test = isNAN(PrCU);

            for (int i = 0; i < _num_of_samples; i++)
            {
                curUser = (_users[i]).Num; // users enumerations starts from 1 but indexes start from 0 Update : Not anymore!
                curAction = _cues[i] - 1; // in the future we will use actions instead of cues
                                         // this is a basic scenario where the is only ONE RESPONSE to each cue
                curCue = _cues[i] - 1;

                PrCU[curUser, curAction, curCue]++;
            }

            test = isNAN(PrCU);

            // repmat(sum(PaCU,2),[1 N 1]);
            double[,] sumMatrix = new double[_unique_users.Length, _unique_cues.Length];
            for (int x = 0; x < _unique_users.Length; x++)
            {   // for each cut z 
                for (int z = 0; z < _unique_cues.Length; z++)
                {
                    sumMatrix[x, z] = 0; // sum of row x in [x,-,z]
                    for (int y = 0; y < _unique_cues.Length; y++)
                    {
                        sumMatrix[x, z] += PrCU[x, y, z];
                    }
                }
            }

            test = isNAN(PrCU);

            for (int z = 0; z < _unique_cues.Length; z++)
            {
                for (int x = 0; x < _unique_users.Length; x++)
                {
                    for (int y = 0; y < _unique_cues.Length; y++)
                    {
                        if (sumMatrix[x, z] == 0) // missing
                        {
                            PrCU[x, y, z] = 0;
                        }
                        else
                        {
                            PrCU[x, y, z] /= sumMatrix[x, z]; // normalize 
                        }
                    }
                }
            }

        }

        private PxCU estimatePxCU()
        { 
            int N = _unique_cues.Length; 
            int M = _unique_users.Length; 
            PxCU calculatedPxCU = new PxCU(N, M); 

            for (int cc = 0; cc < N; cc++)
            {
                //List<int> cuesList = new List<int>(_cues);
                // List<int> currentIndicies = CollectionsExtensions.FindAllIndex<int>(cuesList, i => (i == (cc + 1))); // indexes of cues == cc 
                //List<int> currentIndicies = CollectionsExtensions.FindAllIndexNEW(cuesList, i => (i == (cc + 1))); // indexes of cues == cc 
                List<int> currentIndicies = getIndexesWhere(i => (_cues[i] == (cc + 1)));
                int NxC = currentIndicies.Count; // NxC = size(Dc,2);
                calculatedPxCU.probabilities.Add(new double[M, NxC]); // PxCU{cc} = nan(M,NxC);

                for (int ii = 0; ii < NxC; ii++)
                {
                    double[] x = _samplesArrayLst[currentIndicies[ii]]; //  x = Dc(:,ii);
                    for (int uu = 0; uu < M; uu++) // for uu=1:M
                    {
                        //List<int> userIndsTmp = CollectionsExtensions.FindAllIndexNEW(currentIndicies, i => ((_users[i]).Num == uu)); //  userIndsTmp contains the indixes in currentIndicies which meet our needs :  find(U==uniqueUsers(uu));
                        //List<int> userIndsTmp = getIndexesWhere(i => ((_users[i]).Num == uu)); //  userIndsTmp contains the indixes in currentIndicies which meet our needs :  find(U==uniqueUsers(uu));
                        List<int> userInds = currentIndicies.Where(index => (_users[index]).Num == uu).ToList();
                        
                        if (userInds.Count == 0)
                        {
                            (calculatedPxCU.probabilities[cc])[uu, ii] = 0; 
                            continue;
                        }
                     /*
                        List<int> userInds = new List<int>();
                        foreach (int index in userIndsTmp)
                        {
                            userInds.Add(currentIndicies.ElementAt(index));
                        }
                    */ 
                        userInds.Remove(currentIndicies[ii]); 
                        double p = KDEstimatePxRU(x, _samplesArrayLst, userInds); 
                        (calculatedPxCU.probabilities[cc])[uu, ii] = p; 
                    }
                }

                for (int i = 0; i < M; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < NxC; j++)
                    {
                        sum += (calculatedPxCU.probabilities[cc])[i, j];
                    } 

                    for (int j = 0; j < NxC; j++)
                    {
                        (calculatedPxCU.probabilities[cc])[i, j] /= sum;
                    } 
                }
            }

            return calculatedPxCU;
        }

        private double KDEstimatePxRU(double[] query, List<double[]> galery, List<int> indiciesToSearch = null)
        {
            double K = 1;
            double M = (indiciesToSearch != null) ? indiciesToSearch.Count : galery.Count;
            double minDist = double.MaxValue;
            foreach (int index in ((indiciesToSearch != null) ? indiciesToSearch : Enumerable.Range(0, galery.Count)))
            {
                double curDist = dist(galery[index], query);
                if (curDist == 0)
                {
                    int randVal = 0;
                    randVal++;
                }
                if (curDist < minDist)
                {
                    minDist = curDist;
                }
            }
            double result = K / (M * minDist);

            if (double.IsInfinity(result))
            {
                int randVal = 0;
                randVal++;
            }

            return result;
        }

        private bool isNAN(double[,,] matrix) {
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    for (int z = 0; z < matrix.GetLength(2); z++)
                    {
                        if (double.IsNaN(matrix[x, y, z]))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private string constructFullFilePath(string homeDir, string fileName) 
        {
            return homeDir + fileName + ".txt";
        }

        private double dist(double[] v1, double[] v2)
        {
            // suppose v1 & v2 are from Rn result is: (v1[1]-v2[1])^2 + ... + (v1[n]-v2[n])^2; 
            double result = 0;
            double diff;

            for (int i = 0; i < v1.Length; i++)
            {
                diff = (v1[i]-v2[i]);
                result += (double)Math.Pow((double)diff, (double)2);
            }

            return result;
        }

        private List<int> getIndexesWhere(Predicate<int> pred)
        {
            var indexes = new List<int>();

            for (int i = 0; i < this._users.Length; i++)
            {
                if (pred(i))
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }

        #endregion 

        #region Deprecated Methods

        private double calculateDiff(PxCU prob1, PxCU prob2)
        {
            double maxDiff = -1;
            for (int cc = 0; cc < prob1.probabilities.Count; cc++)
            {
                for (int i = 0; i < (prob1.probabilities[cc]).GetLength(0); i++)
                {
                    for (int j = 0; j < (PxCU.probabilities[cc]).GetLength(1); j++)
                    {
                        double curDiff = Math.Abs((prob1.probabilities[cc])[i, j] - (prob2.probabilities[cc])[i, j]);
                        if (curDiff > maxDiff)
                        {
                            maxDiff = curDiff;
                        }
                    }
                }
            }

            return maxDiff;
        }

        #endregion

    }
}
