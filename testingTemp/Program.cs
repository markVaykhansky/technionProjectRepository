using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using KinectServer.BusinessLogic;
using KinectServer.DB;
using KinectServer.Common;
namespace testingTemp
{
    class Program
    {
       
   /*     
        private int selectCue(float[] PtU, string selMethod)
        {
            // Hu = computeEnthropy(PtU); ( not used in original code, maybe use later )

            int cueId = -3; // initial value
            switch (selMethod)
            {
                case "IGmax":
                    List<float[,]> PxCU = _recordedSamples.PxCU.probabilities;
                    float[,] currentPxCU;
                    int Nc = _recordedSamples.PxCU.probabilities.Count; // Nc = length(PxCU);
                    // [M,K] = ... 
                    float[] IG = new float[Nc];
                    // ==================
                    // helping variables:
                    //  float[,] calcTemp;// tempporary matrix for calculations 
                    float afterLog2;
                    float[] Pc = new float[3];
                    int ind;
                    // ==================
                    for (int cc = 0; cc < Nc; cc++)
                    {
                        //PxC = sum(PxCU{cc}.*repmat(PtU,1,K),1); this multiplies the i'th row in PxCU{cc} by PtU[i]
                        currentPxCU = PxCU.ElementAt(cc);
                        //calcTemp = new float[currentPxCU.GetLength(0), currentPxCU.GetLength(1)];

                        //  float[] PxC = Enumerable.Repeat<float>((float)0, currentPxCU.GetLength(1)).ToArray(); //
                        float[] PxC = new float[currentPxCU.GetLength(1)];
                        for (int row = 0; row < currentPxCU.GetLength(0); row++) // iterate rows
                        {
                            for (int column = 0; column < currentPxCU.GetLength(1); column++) // iterate columns
                            {
                                PxC[column] += (currentPxCU[row, column] * PtU[cc]);
                            }
                        }
                        //IG(cc) = sum(PtU.*sum(PxCU{cc}.*log2(PxCU{cc}./repmat(PxC,M,1)),2),1);
                        // #1: sum(PxCU{cc}.*log2(PxCU{cc}./repmat(PxC,M,1)),2)
                        // sum[i] = sum of row i in PxCU{cc}  multiplied by log2 of:[ A./B where A is PxCU{cc} and B = PxC ]

                        float[] rowSum = Enumerable.Repeat<float>((float)0, currentPxCU.GetLength(0)).ToArray();
                        for (int i = 0; i < currentPxCU.GetLength(0); i++) // itereate through rows
                        {
                            for (int j = 0; j < currentPxCU.GetLength(1); j++)
                            {
                                afterLog2 = (float)Math.Log((double)(currentPxCU[i, j] / PxC[j]), 2.0);
                                rowSum[i] += afterLog2;
                            }
                            // done with i'th row
                        }

                        // sum(PtU.* prev_sum, 1)
                        IG[cc] = 0;
                        for (int i = 0; i < PtU.Length; i++)
                        {
                            IG[cc] += (PtU[i] * rowSum[i]);
                        }
                        // now IG array is created

                        // [~,indOpt] = sort(IG,'descend');
                        int[] indOpt = Enumerable.Range(0, IG.Length).ToArray(); // 0, ... , IG.Length-1
                        float[] IGClone = new float[IG.Length];
                        Array.Copy(IG, IGClone, IG.Length);
                        Array.Sort(IGClone, indOpt); //  [~,indOpt] = sort(IG);
                        Array.Reverse(indOpt); // 'descend'


                        if (_t > 1)
                        {
                            float sum3 = IG[indOpt[0]] + IG[indOpt[1]] + IG[indOpt[2]];
                            for (int tmp = 0; tmp < 3; tmp++)
                            {
                                Pc[tmp] = IG[indOpt[tmp]] / sum3; //    Pc = IG(indOpt(1:3))/sum(IG(indOpt(1:3))); => this probability distribution function (PDF)
                            }
                            ind = discreternd(Pc) - 1; // indexes start from 0.. 
                            cueId = indOpt[ind]; // ind = discreternd(Pc); // this computes CDF

                        }
                        else
                        {
                            cueId = indOpt[0] + 1; // indices start from 0, cue ids start from 1, thus the +1
                        }

                    }
                    break;

                case "Random":
                    Random rand = new Random();
                    cueId = rand.Next(1, _recordedSamples.NumOfCues);
                    break;

                default:
                    cueId = -1;
                    break;
            }
            return cueId;
        }

        int discreternd(float[] p)
        {
            // NOTICE: we set n=1 !
            // for n!=1 change the implementation

            Random rnd = new Random();
            float u = (float)rnd.NextDouble();
            float sum = p.Sum();
            for (int i = 1; i < p.Length; i++)
            {
                p[i] += p[i - 1]; // cumsum(p)
                p[i] /= sum; // normalize cumsum(p)   
            }
            return Array.FindAll<float>(p, i => u > i).Length + 1; // sum(u > cdf,1)+1;
        }



        private float computeEnthropy(float[] P) // I assumed dim=1 
        {
            float sum = 0;
            int i;
            float H = 0;

            //    P = P./repmat(sum(P,1),size(P,1),1);
            for (i = 0; i < P.Length; i++)
            {
                sum += P[i];
            }
            for (i = 0; i < P.Length; i++)
            {
                P[i] /= sum;
            }

            // H = -sum(P.*log2(P),dim);
            for (i = 0; i < P.Length; i++)
            {
                H += (P[i] * (float)Math.Log((float)P[i], 2));
            }
            return -H;
        }

        private void updateProbabilities(float[] z, int prevCue)
        {
            //
            float[] newPtU = new float[_recordedSamples.NumOfUsers]; // PzUA[i] = sum of element in row i in PzUA (matrix in helper function, in that matrix the are 'num of users' rows)
            _recordedSamples.updateProbabilitiesHelperFunction(z, _currentCues.LastOrDefault(), newPtU); // PzUA = sum(PzUA, 2);
            float[] prevProb = _PtU.LastOrDefault();

            // assert start
            if (prevProb.Length != newPtU.Length) { Console.WriteLine("Panic!!!"); }
            // assert end 

            for (int i = 0; i < _recordedSamples.NumOfUsers; i++)
            {
                // multiply the prev_prob vector with temporary newPtU
                newPtU[i] *= prevProb[i];
            }
            float sum = newPtU.Sum();
            for (int i = 0; i < _recordedSamples.NumOfUsers; i++)
            {
                // normalize the new vector 
                newPtU[i] /= sum;
            }
            _PtU.Add(newPtU);
        }

        // private PlayersDB _players;
       
        public SamplesDB _recordedSamples;
        private List<int> _currentCues;
        private int _average_response_size = 40;
        private int _sampleDim = 57; // 19 joints (25 total - 21:25 and minus the first one which is the center) * 3 dimensions for each joint = 57
        private int _t;
        private int _numOfUsers;
        private List<float[]> _PtU;
        private float _H;

        */

        static void Main(string[] args)
        {
            
            string home = @"C:\Users\Mark\Desktop\TestTraining\";
           // _recordedSamples = new SamplesDB(home + @"\MatlabPxCU\PxCU_", home + "Data.txt", home + "U.txt", home + "C.txt", home + "UU.txt", home + "CC.txt", home + "Dict.txt", _sampleDim);
            
            Console.Read();
        }
    }
}
