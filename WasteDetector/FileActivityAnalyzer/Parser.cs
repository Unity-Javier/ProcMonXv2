using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class Parser
    {
        public enum ProcMonEntry
        {
            Time_of_Day = 0,
            Duration = 1,
            Process_Name = 2,
            PID = 3,
            Parent_PID = 4,
            TID = 5,
            Operation = 6,
            Category = 7,
            Path = 8,
            Result = 9,
            Detail = 10
        };

        public Parser(Config config)
        {
            var allLines = GetAllLines(config);

            //Tokenize the lines
            var extractedLines = ExtractLineContents(allLines);

            //Put lines together
            GroupLines(extractedLines);
        }

        private void GroupLines(ProcMonLine[] extractedLines)
        {
            var fileToInfo = new Dictionary<string, List<ProcMonOperationInfo>>();
            for (int i = 0; i < extractedLines.Length; ++i)
            {
                var curLine = extractedLines[i];
                fileToInfo.TryGetValue(curLine.contents[(int) ProcMonEntry.Path], out var operation);

                if (operation == null)
                {
                    operation = new List<ProcMonOperationInfo>();
                    fileToInfo.Add(curLine.contents[(int)ProcMonEntry.Path], operation);
                }

                operation.Add(new ProcMonOperationInfo()
                {
                    operation = curLine.contents[(int)ProcMonEntry.Operation],
                    details = new ProcMonOperationDetails()
                    {
                        Time_of_Day = curLine.contents[(int)ProcMonEntry.Time_of_Day],
                        Duration = curLine.contents[(int)ProcMonEntry.Duration],
                        Process_Name = curLine.contents[(int)ProcMonEntry.Process_Name],
                        PID = curLine.contents[(int)ProcMonEntry.PID],
                        Parent_PID = curLine.contents[(int)ProcMonEntry.Parent_PID],
                        TID = curLine.contents[(int)ProcMonEntry.TID],
                        Category = curLine.contents[(int)ProcMonEntry.Category],
                        Result = curLine.contents[(int)ProcMonEntry.Result]
                    }
                });
            }

            Console.WriteLine("Complete");
        }

        private ProcMonLine[] ExtractLineContents(string[] allLines)
        {
            var extractedLines = new ProcMonLine[allLines.Length];

            Regex matcher = new Regex("(?:,|\n|^)(\"(?:(?: \"\") *[^\"]*)*\" |[^\",\n]*|(?:\n|$))", RegexOptions.Compiled);

            int kMaxSize = 10;
            for (int i = 0; i < allLines.Length; ++i)
            {
                var curLine = allLines[i];
                MatchCollection matches = matcher.Matches(curLine);

                extractedLines[i] = new ProcMonLine()
                {
                    contents = new string[kMaxSize]
                };

                int counter = 0;
                foreach(Match match in matches)
                {
                    var group = match.Groups;
                    var entry = group[1].Value;
                    
                    extractedLines[i].contents[counter] = entry;
                    counter++;

                    if (counter >= kMaxSize)
                        break;
                }

            }

            return extractedLines;
        }

        private string[] GetAllLines(Config config)
        {
            return File.ReadAllLines(config.PathToCSV);
        }
    }

    public class ProcMonLine
    {
        public string[] contents;
    }

    [DebuggerDisplay("Operation = {operation}")]
    public class ProcMonOperationInfo
    {
        public string operation;
        public ProcMonOperationDetails details;
    }

    public class ProcMonOperationDetails
    {
        public string Time_of_Day;
        public string Duration;
        public string Process_Name;
        public string PID;
        public string Parent_PID;
        public string TID;
        public string Category;
        public string Result;
        public string Detail;
    }



}
