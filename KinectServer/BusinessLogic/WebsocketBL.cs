using KinectServer.Communication;
using KinectServer.DTO;
using Newtonsoft.Json;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.BusinessLogic
{
    public class WebsocketBL
    {
        private WebSocketServer _server = null;
        public WebsocketBL(int port)
        {
            _server = new WebSocketServer();
            _server.Setup(port);
            _server.NewSessionConnected += server_NewSessionConnected;
            _server.NewMessageReceived += server_NewMessageReceived;
            _server.SessionClosed += server_SessionClosed;

        }




        void server_NewMessageReceived(WebSocketSession session, string value)
        {

            if (NewMessageRecived != null)
            {
                var message = JsonConvert.DeserializeObject<ClientMessage>(value);
                NewMessageRecived(message);
            }

        }

        public void PostMessage<T>(T data, Protocol type)
        {
            ServerMessage<T> message = new ServerMessage<T>();
            message.Type = type;
            message.Data = data;
            _session.Send(JsonConvert.SerializeObject(message));
        }

        private void server_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            if (SessionClosed != null)
            {
                SessionClosed();
            }

        }

        private void server_NewSessionConnected(WebSocketSession session)
        {
            _session = session;
            if (NewSessionStarted != null)
            {
                NewSessionStarted();
            }
        }

        public WebSocketServer Server
        {
            get { return _server; }
        }

        private WebSocketSession _session = null;
        public WebSocketSession Session
        {
            get { return _session; }
            set
            {
                if (_session != value)
                {
                    _session = value;
                }
            }
        }



        public void StartServer()
        {
            _server.Start();
            if (ServerStateChanged != null)
            {
                ServerStateChanged(true);
            }
        }

        public void StopServer()
        {
            _server.Stop();
            if (ServerStateChanged != null)
            {
                ServerStateChanged(false);
            }
        }

        public event Action NewSessionStarted;
        public event Action SessionClosed;
        public event Action<ClientMessage> NewMessageRecived;
        public event Action<bool> ServerStateChanged;


    }
}
