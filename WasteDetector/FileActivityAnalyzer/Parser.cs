using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class Parser
    {

        public Parser(Config config)
        {
            var allLines = GetAllLines(config);

            //Tokenize the lines
            ExtractLineContents(allLines);

            //Put lines together
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
}
