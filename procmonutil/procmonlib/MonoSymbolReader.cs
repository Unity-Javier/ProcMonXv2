using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProcMonUtils
{
    public class MonoJitSymbol : IAddressRange
    {
        public AddressRange Address;
        
        // mono pmip files sometimes have the symbol portion blank
        public string? AssemblyName;
        public string? Symbol;
        
        ref AddressRange IAddressRange.AddressRef => ref Address;
    }

    public class MonoSymbolReader
    {
        public uint UnityProcessId;
        public DateTime DomainCreationTime; // use for picking the correct set of jit symbols given the event time
        public MonoJitSymbol[] Symbols; // keep sorted for bsearch

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
                
                var addressBase = ulong.Parse(lmatch.Groups["start"].Value, NumberStyles.HexNumber);
                var addressSize = (uint)(ulong.Parse(lmatch.Groups["end"].Value, NumberStyles.HexNumber) - addressBase);
                
                var monoJitSymbol = new MonoJitSymbol
                {
                    Address = new AddressRange(addressBase, addressSize),
                    AssemblyName = lmatch.Groups["module"].Value,
                    Symbol = lmatch.Groups["symbol"].Value
                        .Replace(" (", "(").Replace('/', '.').Replace(':', '.').Replace(",", ", "), // remove mono-isms
                };
                
                // MISSING: handling of a blank assembly+symbol, which *probably* means it's a trampoline
                
                entries.Add(monoJitSymbol);            
            }
            
            Symbols = entries.OrderBy(e => e.Address.Base).ToArray();
        }
        
        public MonoJitSymbol? FindSymbol(ulong address) => Symbols.FindAddressIn(address);
    }
}
