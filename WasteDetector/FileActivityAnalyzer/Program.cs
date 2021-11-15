using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    class Program
    {
        static void ParseArgs(string[] args, out Config config)
        {
            config = new Config();

            for(int i = 0; i < args.Length; ++i)
            {
                if(args[i].ToLower() == "-csvfile")
                {
                    config.PathToCSV = args[i + 1];
                }
                else if(args[i].ToLower() == "-rules")
                {
                    config.RulesFile = args[i + 1];
                }
            }

        }

        static void Main(string[] args)
        {
            ParseArgs(args, out var config);
            var parser = new Parser(config);
            var loadRules = new Rules(config);
        }
    }
}
