using KinectServer.DTO;
using KinectServer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KinectServer.BusinessLogic
{
    public class CustomPointsListParser
    {
        GameInputFile _gameInputDetails;
        bool _shuffle = false;
        List<int> _currentCuesIndecis;
        public CustomPointsListParser()
        {
            _gameInputDetails = null;
            _currentCuesIndecis = new List<int>();

        }
        public bool Init()
        {
            var enemiesFilePath = ConfigurationManager.AppSettings["enemiesFile"];
            try
            {
                var str = File.ReadAllText(enemiesFilePath);
                _gameInputDetails = JsonConvert.DeserializeObject<GameInputFile>(str);
                _shuffle = bool.Parse(ConfigurationManager.AppSettings["shuffleCues"]);
                _currentCuesIndecis = new List<int>();
                return true;
            }
            catch
            {
                MessageBox.Show("Json file is malformed");
                return false;
            }
        }

        public Dictionary<string,float> GetInnerBoundaryInflationRatios()
        {
            return _gameInputDetails.InnerBoundaryInflRatio;
        }

        public Dictionary<string, float> GetOuterBoundaryInflationRatios()
        {
            return _gameInputDetails.OuterBoundaryInflRatio;
        }
   

        public List<UserDefinedPoint> GetPointsForStage(string username, string stage)
        {
            if (_gameInputDetails.Players.ContainsKey(username))
            {
                if (_gameInputDetails.Players[username].ContainsKey(stage))
                {

                    var requiredActionsIds =  _gameInputDetails.Players[username][stage];
                    if (_shuffle)
                    {
                        requiredActionsIds = FisherYatesShuffle(requiredActionsIds.ToArray<int>());
                    }
                    _currentCuesIndecis = requiredActionsIds;
                    var requiredPoints = new List<UserDefinedPoint>();
                    for (int i = 0; i < requiredActionsIds.Count; i++)
                    {
                        requiredPoints.Add(_gameInputDetails.Actions[requiredActionsIds[i]]);
                    }
                    return requiredPoints;
                }
            }
            return null;
        }

        public UserDefinedPoint GetCue(int cueID)
        {
            return _gameInputDetails.Actions[cueID];
        }

        public List<int> GetCurrentCuesIndecis()
        {
            return _currentCuesIndecis;
        }

        public List<Instruction> GetStages(string playerName = "")
        {
            var stages = new List<Instruction>();
            var firstPlayerStages = _gameInputDetails.Players.FirstOrDefault().Value;
            if (playerName != string.Empty && _gameInputDetails.Players.ContainsKey(playerName))
            {
                firstPlayerStages = _gameInputDetails.Players[playerName];
            }
            
            if (firstPlayerStages != null)
            {
                foreach(var s in firstPlayerStages.Keys)
                {
                    var inst = new Instruction();
                    inst.State = s;
                    inst.Text = s;
                    inst.EnemyCount = firstPlayerStages[s].Count;
                    stages.Add(inst);
                }               
            }
            return stages;
        }

        private List<int> FisherYatesShuffle(int[] array)
        {
            Random rng = new Random();   // i.e., java.util.Random.
            int n = array.Length;        // The number of items left to shuffle (loop invariant).
            while (n > 1)
            {
                int k = rng.Next(n);  // 0 <= k < n.
                n--;                     // n is now the last pertinent index;
                int temp = array[n];     // swap array[n] with array[k] (does nothing if k == n).
                array[n] = array[k];
                array[k] = temp;
            }
            return array.ToList<int>();
        }

    }

}
