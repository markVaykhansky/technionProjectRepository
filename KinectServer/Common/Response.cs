using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectServer.Common;

namespace KinectServer.Common
{
    class Response
    {
        private Sample response; // sample from size frame_size * average_number_of_frames = (19 * 3) * 35
        private string playerID;
        private string cueID;
        private string responseID;

        public Response(Sample response, string id, string playerID, string cueID)
        {
            this.response = response;
            responseID = id;
            this.playerID = playerID;
            this.cueID = cueID;
        }

        public Sample Data { get { return this.response; } }
        public string PlayerID { get { return this.PlayerID; } }
        public string CueID { get { return this.CueID; } }
        public string ID { get { return this.responseID; } }
    }
}
