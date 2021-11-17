using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProcMonUtils
{
    public struct EventRecord
    {
        public int Sequence;
        public DateTime CaptureTime;
        public int ProcessId;
        public FrameRecord[] Frames;
    }
    
    public enum FrameType
    {
        Kernel,
        User,
        Mono,
    }
    
    public struct FrameRecord
    {
        public FrameType Type;
        public string Module;
        public string Symbol;
        public int Offset;
    }
    
    public class PmlQuery
    {
        List<EventRecord> m_Events = new();
        Dictionary<DateTime, int> m_EventsByTime = new();
        Dictionary<string, List<int>> m_SymbolsToEvents = new();
        Dictionary<string, List<int>> m_ModulesToEvents = new();

        public PmlQuery(string framesPath)
        {
            var lines = File
                .ReadLines(framesPath)
                .Select((line, index) => (text: line.Trim(), index))
                .Where(line => !line.text.StartsWith("#")); // filter out comments
            
            foreach (var line in lines)
            {
                var fields = line.text.Split(';');
                const int k_FrameOffset = 3;
                
                var eventRecord = new EventRecord
                {
                    Sequence = int.Parse(fields[0]),
                    CaptureTime = DateTime.Parse(fields[1]),
                    ProcessId = int.Parse(fields[2]),
                    Frames = new FrameRecord[fields.Length - k_FrameOffset],
                };
                
                var eventIndex = m_Events.Count; 
                m_Events.Add(eventRecord);

                Debug.Assert(eventRecord.Sequence == eventIndex);

                string Add(Dictionary<string, List<int>> dict, string value)
                {
                    if (!dict.TryGetValue(value, out var list))
                        dict.Add(value, list = new());
                    list.Add(eventIndex);
                    return value;
                }
                
                string AddSymbol(string symbol) => Add(m_SymbolsToEvents, symbol);
                string AddModule(string module) => Add(m_ModulesToEvents, module);

                for (var i = 0; i < eventRecord.Frames.Length; ++i)
                {
                    var match = Regex.Match(fields[i + k_FrameOffset], @"(?nx-)
                        ^(?<type>[A-Z])\ (
                            (?<addr>0x[0-9a-f]+) |
                            \[(?<module>[^\]]+)\]\ (?<symbol>.*?)\ \+\ 0x(?<offset>[0-9a-f]+)
                        $)");
                    if (!match.Success)
                        throw new FileLoadException($"Line {eventIndex+1} in '{framesPath}' fails pattern match");

                    var type = match.Groups["type"].Value[0] switch
                    {
                        'K' => FrameType.Kernel,
                        'U' => FrameType.User,
                        'M' => FrameType.Mono,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    eventRecord.Frames[i] = match.Groups["addr"].Success
                        ? new FrameRecord
                        {
                            Type = type,
                            Symbol = AddSymbol(match.Groups["addr"].Value),
                        }
                        : new FrameRecord
                        {
                            Type = type,
                            Module = AddModule(match.Groups["module"].Value),
                            Symbol = AddSymbol(match.Groups["symbol"].Value),
                            Offset = int.Parse(match.Groups["offset"].Value, NumberStyles.HexNumber),
                        };
                }
            }
        }
        
        public IReadOnlyList<EventRecord> AllRecords => m_Events;
        
        public EventRecord GetRecordBySequence(int sequence) => m_Events[sequence];
        public EventRecord? FindRecordByCaptureTime(DateTime dateTime) => m_EventsByTime.TryGetValue(dateTime, out var foundIndex) ? m_Events[foundIndex] : null;

        IEnumerable<int> MatchRecordsByText(IEnumerable<KeyValuePair<string, List<int>>> items, Regex regex) =>
            items.Where(kv => regex.IsMatch(kv.Key)).SelectMany(kv => kv.Value).Distinct();
        
        public IEnumerable<int> MatchRecordsBySymbol(Regex regex) => MatchRecordsByText(m_SymbolsToEvents, regex);
        public IEnumerable<int> MatchRecordsByModule(Regex regex) => MatchRecordsByText(m_ModulesToEvents, regex);
    }
}
