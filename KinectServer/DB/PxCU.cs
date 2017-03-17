using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.DB
{
    class PxCU
    {
        public int N, M;
        public List<double[,]> probabilities;
        public PxCU(int n, int m)
        {
            probabilities = new List<double[,]>();
            this.N = n;
            this.M = m;
        }
    }
}
