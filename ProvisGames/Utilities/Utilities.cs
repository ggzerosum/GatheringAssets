using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GatheringAssets
{
    static class Utilities
    {
        // Support Multiple extension
        public static bool HasExtension(string path, string[] extensions)
        {
            if (extensions == null || extensions.Length == 0)
                return false;

            foreach (string extension in extensions)
            {
                if (Regex.IsMatch(path, $@".{extension}"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
