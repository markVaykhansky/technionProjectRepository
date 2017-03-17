using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int>() { 1, 2, 3, };
            foreach (int i in list)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine("=======");
            list = list.Select(i => i=1);
            foreach(int i in list) {
                Console.WriteLine(i);
            }
            Console.Read();
        }

    }
}
