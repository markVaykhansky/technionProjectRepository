using HttpServer;
using KinectServer.BusinessLogic;
using KinectServer.DTO;
using KinectServer.Enum;
using Microsoft.Kinect;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer
{
    class KinectServerViewModel : INotifyPropertyChanged
    {
        #region Members
        private KinectServerBL _businessLogics;
        private IKinectBusinessLogic _kinect;
        private int _serverPort = 2012;
        private Server _server;
        #endregion

        #region Ctor
        public KinectServerViewModel()
        {
            _kinect = new KinectBL();
            _businessLogics = new KinectServerBL(_kinect, _serverPort);
            _businessLogics.KinectStateChanged += _businessLogics_KinectStateChanged;
            Player = "default";
        }

        private void _businessLogics_KinectStateChanged(KinectState state)
        {
            KinectState = state;
        }
        #endregion

        #region Private Methods
        
        #endregion

        #region Properties
        private double _scaleFactorX;
        public double ScaleFactorX { 
            get { return _scaleFactorX; }
            set
            {
                if (_scaleFactorX != value)
                {
                    _scaleFactorX = value;
                    OnPropertyChanged("ScaleFactorX");
                }
            }
        }
        

        private double _scaleFactorY;
        public double ScaleFactorY
        {
            get { return _scaleFactorY; }
            set
            {
                if (_scaleFactorY != value)
                {
                    _scaleFactorY = value;
                    OnPropertyChanged("ScaleFactorY");
                }
            }
        }


        private string _player;
        public string Player
        {
            get { return _player; }
            set
            {
                if (_player != value)
                {
                    _player = value;
                    _businessLogics.SetPlayerName(value);
                    OnPropertyChanged("Player");
                }
            }
        }

        private KinectState _kinectState;
        public KinectState KinectState
        {
            get
            {
                return _kinectState;
            }
            set
            {
                _kinectState = value;
                OnPropertyChanged("KinectState");
            }
        }
        #endregion

        #region Public Methods

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        #endregion

    }
}
