using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Common
{
    class Sample
    {
        private int dim;
        private double[] data;

        public Sample(double[] data,int dim)
        {
            this.data = data;
            this.dim = dim;
        }
/*
        public int Dim
        {
            get
            {
                return this.dim;
            }
        }
*/
        public double[] Data
        {
            get
            {
                return this.data;
            }
        }


    }
}
