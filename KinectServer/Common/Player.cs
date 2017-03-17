using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Common
{
    class Player
    {
        private string name;
        private int id;
        private int uniqueUserNum;

        public Player(int uniqueUserNum, int userId, string name)
        {
            this.uniqueUserNum = uniqueUserNum;
            this.id = userId;
            this.name = name;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public int Num { 
            get 
            { 
                return this.uniqueUserNum; 
            } 
        }

        public int Id
        {
            get
            {
                return this.id;
            }
        }
    }
}
