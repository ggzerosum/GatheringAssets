using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringAssets.Core
{
    class FileMover
    {
        private string _baseAbsoluteRootPath;

        public FileMover(string baseAbsolutePath)
        {
            _baseAbsoluteRootPath = baseAbsolutePath;
        }

        public void CreateDirectoryAndMoveFile(string directoryName, string fileAbsolutePath)
        {
            string directoryPath = GetDirectoryPath(directoryName);
            string destinationPath = PathUtility.AppendPath(directoryPath, Path.GetFileName(fileAbsolutePath));

            if (File.Exists(destinationPath))
                return;

            MoveFileIntoDirectory(fileAbsolutePath, destinationPath);
        }

        private string GetDirectoryPath(string directoryName)
        {
            string groupPath = $"{_baseAbsoluteRootPath}{Path.DirectorySeparatorChar}{directoryName}";

            if (!Directory.Exists(groupPath))
                Directory.CreateDirectory(groupPath);

            return groupPath;
        }

        private bool MoveFileIntoDirectory(string fileAbsolutePath, string destinationAbsolutePath, bool ignoreExistFile = false)
        {
            if (ignoreExistFile || !File.Exists(destinationAbsolutePath))
            {
                File.Move(fileAbsolutePath, destinationAbsolutePath);
                return true;
            }

            return false;
        }
    }
}