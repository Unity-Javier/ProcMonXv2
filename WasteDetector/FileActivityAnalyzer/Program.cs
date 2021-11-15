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
                else if (args[i].ToLower() == "-opcodes")
                {
                    config.OpCodesFile = args[i + 1];
                }
                else if (args[i].ToLower() == "-generate_opcodes")
                {
                    config.GenerateOpCodes = true;
                }
                else if (args[i].ToLower() == "-opcodes_output_path")
                {
                    config.OpCodesOutputPath = args[i + 1];
                }
            }

        }

        static void Main(string[] args)
        {
            ParseArgs(args, out var config);
            
            var opCodes = new OpCodes(config);
            var parser = new Parser(config, opCodes);
            var loadRules = new Rules(config, opCodes);
            var rulesTrie = new RulesTrie(loadRules);

            var rulesMatcher = new RulesMatcher(parser, rulesTrie);
        }
    }
}
