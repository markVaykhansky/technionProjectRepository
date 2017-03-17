using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public class Server
    {
        private string _path;
        private string _url = @"https://localhost:2402";
        
      

        public void Start()
        {
            WebApp.Start<Startup>(_url);
        }

    }
}
