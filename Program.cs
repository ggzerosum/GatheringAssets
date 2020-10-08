using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GatheringAssets.Core;

namespace GatheringAssets
{
    /// <summary>
    /// 이 프로그램은 지정된 최상위 디렉토리의 모든 하위 디렉토리를 검사하여
    /// 미리 정해둔 확장자를 만족하는 모든 파일들을 이름별로 묶어 정렬해주는 기능을 수행합니다.
    /// </summary>
    class Program
    {
        // 검색을 시작할 최상위 폴더
        private static string _tempFolderName = "TempFolder";
        // 검색할 확장자를 제한
        private static string[] _extensionConstraint = new string[] { "apk", "txt", "log" };

        // 비동기 연산을 제어하는 변수들
        private static int _tickTimePerChunk_ms = 2000; // 1000ms == 1s // 계산단위 1개당 소모될 시간
        private static int _unitPerChunk = 1; // 계산단위 1개당 한번에 계산을 진행할 양

        static async Task Main(string[] args)
        {
            try
            {
#if DEBUG
                TextWriterTraceListener tracer = new TextWriterTraceListener(System.Console.Out);
                Debug.Listeners.Add(tracer);
#endif
                // Description
                Console.WriteLine("이 프로그램은 지정된 최상위 디렉토리의 모든 하위 디렉토리를 검사하여 " +
                                  "미리 정해둔 확장자를 만족하는 모든 파일들을 이름별로 묶어주는 기능을 수행합니다.");

                // Start
                Console.Write("Input Target Path:");
                string absoluteDirectoryPath = Console.ReadLine();
                bool isValidAbsoluteDirectoryPath = PathUtility.IsValidDirectoryPath(absoluteDirectoryPath);
                Debug.WriteLine($"Path:{absoluteDirectoryPath}, IsValid:{isValidAbsoluteDirectoryPath}");

                // Main Logic
                if (isValidAbsoluteDirectoryPath)
                {
                    await GatherFilesIntoTempDirectory(absoluteDirectoryPath, _tempFolderName, _unitPerChunk, _tickTimePerChunk_ms);
                }
                else
                {
                    Console.WriteLine("유효하지않은 디렉토리 경로입니다.");
                }


                // End
                Console.Write("Exit(AnyKey):");
                Console.ReadKey();
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Trace.Flush();
            }
        }

        // 클래스 분화안하고 Main에서 간단하게 함수를 만들어서 실행
        // Main에서 직접 사용하는 함수
        private static async Task GatherFilesIntoTempDirectory(string targetPath, string tempFolderName, int countPerSingleChunk, int tickTimePerChunk)
        {
            try
            {
                FileInfoCollector fileInfoCollector = new FileInfoCollector(targetPath);
                FileMover fileMover = new FileMover($"{targetPath}/{tempFolderName}");
                fileInfoCollector.Print();

                // --- 검사해야할 총 파일의 갯수를 알아낸다. --- //
                var directoryGroupContainFiles = ExtractFilesWithRepresentativeName(targetPath, fileInfoCollector);
                int totalFileCount = 0;
                foreach (var grouping in directoryGroupContainFiles)
                {
                    foreach (string element in grouping)
                    {
                        totalFileCount++;
                    }
                }
                Console.WriteLine($"Total File Count : {totalFileCount}");
                // ---------------------------------------------- //

                // -- 파일을 TempFolder로 모아주는 로직 --- //
                // 선언부
                IEnumerable<IGrouping<string, string>> GatherFiles()
                {
                    foreach (IGrouping<string, string> directoryGroup in directoryGroupContainFiles)
                    {
                        Debug.WriteLine("=============");
                        string directoryGroupName = directoryGroup.Key;
                        Debug.WriteLine("=============");
                        Debug.WriteLine($"Directory Name: {directoryGroupName}");

                        foreach (string filePath in directoryGroup)
                        {
                            Debug.WriteLine($"\t{filePath}");
                            GatherFileIntoCertainDirectory(directoryGroupName, filePath, fileMover);
                        }
                        Debug.WriteLine("=============");

                        yield return directoryGroup;
                    }
                }
                // 선언부
                int currentlyProcessedElementCount = 0;
                double GetNormalizedProgress(IGrouping<string, string> group)
                {
                    foreach (var e in @group)
                    {
                        currentlyProcessedElementCount++;
                    }

                    return (double)currentlyProcessedElementCount / (double)totalFileCount;
                }

                // 비동기 로직 실행
                ReportableCoroutine<IGrouping<string, string>>._tickTime = tickTimePerChunk; // 실행 Chunk당 Thread 대기 속도
                using (ReportableCoroutine<IGrouping<string, string>> reportableCoroutine
                        = new ReportableCoroutine<IGrouping<string, string>>(GatherFiles(), null, GetNormalizedProgress, countPerSingleChunk))
                {
                    await reportableCoroutine.Tick();
                }
                // ---------------------------------------- //
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {

            }
        }

        // 보조 함수들
        private static IEnumerable<IGrouping<string, string>> ExtractFilesWithRepresentativeName(string absolutePath, FileInfoCollector fileInfoCollector)
        {
            var filesHasExtension = fileInfoCollector.FindAllFilesHasExtension(_extensionConstraint);
            var groupByRepresentativeName = GroupByRepresentativeName(filesHasExtension, GetRepresentativeNameOfFile);
            foreach (IGrouping<string, string> singleGroup in groupByRepresentativeName)
            {
                yield return singleGroup;
            }
        }
        private static IEnumerable<IGrouping<string, string>> GroupByRepresentativeName(IEnumerable<string> files, Func<string, string> methodToGetRepresentativeName)
        {
            return files.GroupBy(methodToGetRepresentativeName);
        }
        private static string GetRepresentativeNameOfFile(string path)
        {
            string name = Path.GetFileName(path);
            while (!string.IsNullOrEmpty(name) && name.Contains("."))
            {
                name = Path.GetFileNameWithoutExtension(name);
            }

            return name;
        }

        private static void GatherFileIntoCertainDirectory(string directoryName, string filePath, FileMover fileMover)
        {
            fileMover.CreateDirectoryAndMoveFile(directoryName, filePath);
        }
    }
}