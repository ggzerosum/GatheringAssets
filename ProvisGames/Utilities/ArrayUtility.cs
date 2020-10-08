using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringAssets
{
    internal class ArrayUtility<T>
    {
        private static readonly T[] _empty = new T[0];
        public static T[] Empty => _empty;
    }
}