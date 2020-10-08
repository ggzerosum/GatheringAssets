using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GatheringAssets.Core
{
    class FileInfoCollector
    {
        // 간단하게 경로를 수집하기 위함. 최적화하고싶으면 트리구조로 만들어야함.
        private Dictionary<int, List<int>> _relationShip; // Key: Directory Index, Value: File Index
        private string[] _allDirectories;
        private string[] _allFiles;

        public FileInfoCollector(string rootPath)
        {
            Initialize(rootPath);
        }
        private void Initialize(string rootPath)
        {
            _relationShip = new Dictionary<int, List<int>>();
            _allDirectories = null;
            _allFiles = null;

            // 총 디렉토리의 갯수와 파일의 갯수를 기록하고 컨테이너의 크기를 결정함.
            int directoryCount = 0;
            int fileCount = 0;
            TraverseDirectoryRecursive(rootPath, 
                (directoryPath) =>
                {
                    directoryCount++;
                }, 
                (parentDirectory, filePath) =>
                {
                    fileCount++;
                });

            _allDirectories = new string[directoryCount];
            _allFiles = new string[fileCount];

            // 하위 디렉토리를 모두 순회하면서 데이터를 쌓음.
            int directoryIndex = 0;
            int fileIndex = 0;
            void OnContactDirectory(string path)
            {
                _allDirectories[directoryIndex] = path;

                if (!_relationShip.ContainsKey(directoryIndex))
                    _relationShip.Add(directoryIndex, new List<int>());

                directoryIndex++;
            }
            void OnContactFile(string parent, string path)
            {
                _allFiles[fileIndex] = path;

                int parentDirectoryIndex = FindIndex(FileType.Directory, parent);
                if (parentDirectoryIndex >= 0)
                {
                    _relationShip[parentDirectoryIndex].Add(fileIndex);
                }
                else
                {
                    throw new ApplicationException("Cannot Find Parent in directoriesList. Initialize Failed");
                }

                fileIndex++;
            }
            TraverseDirectoryRecursive(rootPath, OnContactDirectory, OnContactFile);
        }

        public IEnumerable<string> FindAllFilesHasExtension(string[] extensions)
        {
            return FindAllFilesHasExtension(this._allFiles, extensions);
        }
        private IEnumerable<string> FindAllFilesHasExtension(string[] candidates, string[] allowedExtensions)
        {
            return candidates.Where(e => Utilities.HasExtension(e, allowedExtensions));
        }


        private enum FileType
        {
            Directory,
            File
        }
        private int FindIndex(FileType fileType, string path)
        {
            string[] targetPaths = ArrayUtility<string>.Empty;
            if (fileType == FileType.Directory)
            {
                if (_allDirectories != null)
                    targetPaths = _allDirectories;
            }
            else
            {
                if (_allFiles != null)
                    targetPaths = _allFiles;
            }

            int index = 0;
            foreach (string targetPath in targetPaths)
            {
                if (targetPath.Equals(path))
                    return index;
                index++;
            }

            return -1;
        }


        private void TraverseDirectoryRecursive(string absoluteDirectoryPath, Action<string> contactDirectory, Action<string, string> contactFile)
        {
            if (!PathUtility.IsDirectory(absoluteDirectoryPath))
                return;

            contactDirectory?.Invoke(absoluteDirectoryPath);

            foreach (string directory in Directory.GetDirectories(absoluteDirectoryPath))
            {
                if (PathUtility.IsDirectory(directory))
                    TraverseDirectoryRecursive(directory, contactDirectory, contactFile);
            }

            foreach (string file in Directory.GetFiles(absoluteDirectoryPath))
            {
                contactFile?.Invoke(absoluteDirectoryPath, file);
            }
        }

        [Conditional("DEBUG")]
        public void Print()
        {
            Console.WriteLine("<<< Start Print >>>");

            for (int parentIndex = 0; parentIndex < _allDirectories.Length; parentIndex++)
            {
                Console.WriteLine("======================");
                Console.WriteLine($"Directory:{_allDirectories[parentIndex]}");
                Console.WriteLine($"\tFiles");
                foreach (int childIndex in _relationShip[parentIndex])
                {
                    Console.Write("\t\t");
                    Console.WriteLine($"{_allFiles[childIndex]}");
                }
                Console.WriteLine("======================");
            }

            Console.WriteLine("<<< End Print >>>");
        }
    }
}
