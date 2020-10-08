using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringAssets
{
    internal class PathUtility
    {
        public static bool IsValidDirectoryPath(string absolutePath)
        {
            return Directory.Exists(absolutePath);
        }

        public static bool IsDirectory(string absolutePath)
        {
            return HasFileAttribute(absolutePath, FileAttributes.Directory);
        }
        public static bool HasFileAttribute(string absolutePath, FileAttributes attribute)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(absolutePath);

            //detect whether its a directory or file
            if ((attr & attribute) == attribute)
                return true;

            return false;
        }

        public static string AppendPath(string input, string append)
        {
            return $"{input}{Path.DirectorySeparatorChar}{append}";
        }
    }
}
