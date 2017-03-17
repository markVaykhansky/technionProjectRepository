using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = "moves11_33";
            using (System.IO.StreamWriter file =
  System.IO.File.AppendText(string.Format(@"C:\Users\Mark\Desktop\{0}.txt", str)))
            {
                file.WriteLine("");
            }
            Console.Read();
        }
    }
}
