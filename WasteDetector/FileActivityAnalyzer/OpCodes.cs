using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileActivityAnalyzer
{
    public class OpCodes
    {
        private Dictionary<string, int> m_OpCodes;

        public OpCodes(Config config)
        {
            m_OpCodes = new Dictionary<string, int>();

            var allLines = File.ReadAllLines(config.OpCodesFile);
            for(int i = 0; i < allLines.Length; ++i)
            {
                var definition = allLines[i].Split(':');
                var name = definition[0];
                int.TryParse(definition[1], out var opCode);
                m_OpCodes.Add(name, opCode);
            }
        }

        public int GetOpCode(string name)
        {
            if (m_OpCodes.ContainsKey(name))
                return m_OpCodes[name];
            
            return int.MaxValue;
        }
    }
}
