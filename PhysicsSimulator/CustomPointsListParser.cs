
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testint.CustomParser;

namespace Testing.CustomParser
{
    public class CustomPointsListParser
    {
        string path = @"C:\Users\everysight\Downloads\igor.json";
        GameInputFile _file;
        public CustomPointsListParser()
        {
            var str = File.ReadAllText(path);
            var file = JsonConvert.DeserializeObject(str);
            Console.Write("");
        }
    }
}
