using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.DB
{
    public static class CollectionsExtensions
    {
        public static List<int> FindAllIndexNEW(List<int> container, Predicate<int> match)
        {
            List<int> result = new List<int>();
            for (int index = 0; index < container.Count; index++)
            {
                int elem = container.ElementAt(index);
                if (match(elem))
                {
                    result.Add(index);
                }
            }
            return result;
        }
        /*
        public static List<int> FindAllIndex<T>(this List<T> container, Predicate<T> match)
        {
            var items = container.FindAll(match);
            List<int> indexes = new List<int>();
            foreach (var item in items)
            {
                indexes.Add(container.IndexOf(item));
            }

            return indexes;
        }
        */
    }
        
}
