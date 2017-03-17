using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Common
{
    class Cue
    {
        private string id;

        public Cue(string id)
        {
            this.id = id;
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }
    }
}
