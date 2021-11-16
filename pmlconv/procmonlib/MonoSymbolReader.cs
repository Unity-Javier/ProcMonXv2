using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PmlConversion
{
    public class MonoJitSymbol
    {
        public ulong BaseAddress;
        public uint Size;
        public ulong EndAddress => BaseAddress + Size;
        
        // mono pmip files sometimes have the symbol portion blank
        public string? AssemblyName;
        public string? Symbol;
    }

    public class MonoSymbolReader
    {
        public uint UnityProcessId;
        public DateTime DomainCreationTime; // use for picking the correct set of jit symbols given the event time
        public MonoJitSymbol[] Symbols; // sorted by address for easy bsearch

        public MonoSymbolReader(string monoPmipPath, DateTime? domainCreationTime = null)
        {
            // default to creation time of the pmip as a way to detect domain creation
            
            DomainCreationTime = domainCreationTime ?? File.GetCreationTime(monoPmipPath);
            
            // find matching unity process
            
            var fmatch = Regex.Match(Path.GetFileName(monoPmipPath), @"^pmip_(?<pid>\d+)_\d+\.txt$", RegexOptions.IgnoreCase);
            if (!fmatch.Success)
                throw new FileLoadException("Unable to extract unity PID from mono pmip filename", monoPmipPath);
            UnityProcessId = uint.Parse(fmatch.Groups["pid"].Value);

            // parse pmip
            
            var lines = File.ReadAllLines(monoPmipPath);
            if (lines[0] != "UnityMixedCallstacks:1.0")
                throw new FileLoadException("Mono pmip file has unexpected header or version", monoPmipPath);

            var rx = new Regex(
                @"(?<start>[0-9A-F]{16});"+
                @"(?<end>[0-9A-F]{16});"+
                @"(\[(?<module>([^\]]+))\] (?<symbol>.*))?");
            
            var entries = new List<MonoJitSymbol>();
            
            for (var iline = 1; iline != lines.Length; ++iline)
            {
                var lmatch = rx.Match(lines[iline]);
                if (!lmatch.Success)
                    throw new FileLoadException($"Mono pmip file has unexpected format line {iline}", monoPmipPath);
                
                var monoJitSymbol = new MonoJitSymbol
                {
                    BaseAddress = ulong.Parse(lmatch.Groups["start"].Value, NumberStyles.HexNumber),
                    AssemblyName = lmatch.Groups["module"].Value,
                    Symbol = lmatch.Groups["symbol"].Value.Replace(" (", "("), // remove mono-ism
                };

                monoJitSymbol.Size = (uint)(ulong.Parse(lmatch.Groups["end"].Value, NumberStyles.HexNumber) - monoJitSymbol.BaseAddress);
                
                entries.Add(monoJitSymbol);            
            }
            
            Symbols = entries.OrderBy(e => e.BaseAddress).ToArray();
        }
        
        public MonoJitSymbol? FindSymbol(ulong address)
        {
            if (address < Symbols[0].BaseAddress || address >= Symbols[^1].EndAddress)
                return null;

            for (var (l, h) = (0, Symbols.Length - 1); l <= h; )
            {
                var i = l + (h - l) / 2;
                var test = Symbols[i];

                if (test.EndAddress <= address)
                    l = i + 1;
                else if (test.BaseAddress > address)
                    h = i - 1;
                else
                    return test;
            }
            return null;
        }
    }
}
