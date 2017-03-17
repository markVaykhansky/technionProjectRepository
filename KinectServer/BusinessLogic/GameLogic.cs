using KinectServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.BusinessLogic
{
    public class GameLogic
    {
        //private Dictionary<byte, Instruction> _instructionsSet;
        private List<Instruction> _instructionsSet;
        private int _currentInstruction;

        public GameLogic(List<Instruction> instructions)
        {
            //_instructionsSet = new Dictionary<byte, Instruction>();
            _instructionsSet = new List<Instruction>();
            _instructionsSet.AddRange(instructions);
            //_instructionsSet.Add( new Instruction() { Text = "Move your arms and legs in order to touch the astroid\n", State = "playground" });
            //_instructionsSet.Add( new Instruction() { Text = "Try to touch the moving objects on screen", State = "playground" });
            _instructionsSet.Add( new Instruction() { Text = "Thank you for participating!", State = "win" });
            _currentInstruction = 0;
        }

        public Instruction NextInstruction
        {
            get
            {
                if (_currentInstruction < _instructionsSet.Count)
                {
                    return _instructionsSet[++_currentInstruction];
                }
                return null;
            }
        }

        public void ResetInstructionsCounter()
        {
            _currentInstruction = -1;
        }

        public string CurrentState
        {
            get
            {
                if (_currentInstruction < _instructionsSet.Count)
                {
                    return _instructionsSet[_currentInstruction].State;

                }
                return null;
            }
        }
    }
}
