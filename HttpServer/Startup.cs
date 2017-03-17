using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public class Startup
    {
        private FileServerOptions _options;
        private PhysicalFileSystem _fileSystem;


        public void Configuration(IAppBuilder builder)
        {
            _fileSystem = new PhysicalFileSystem(@".\Public");
             _options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = _fileSystem
            };
             builder.UseFileServer(_options);
        }
    }
}
