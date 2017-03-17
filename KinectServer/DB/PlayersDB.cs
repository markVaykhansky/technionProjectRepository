using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectServer.Common;

namespace KinectServer.DB
{

    class PlayersDB
    {
        private List<Player> players;
        private int numOfPlayers;

        public PlayersDB(string path)
        {
            this.players = new List<Player>();
            readPlayers(path);
            // TO DO: create players from path
        }

        void readPlayers(string path)
        {
            int i = 0;
            foreach (string line in System.IO.File.ReadAllLines(path))
            {
                this.players.Add(new Player(line, line));
                i++;
            }
            this.numOfPlayers = i;
        }

        public int NumOfPlayers
        {
            get
            {
                return this.numOfPlayers;
            }
        }

    //    public double[] probabilities { get; set; }



            
    }
}
