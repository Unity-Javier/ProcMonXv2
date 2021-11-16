using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PmlConversion
{
    [DebuggerDisplay("{ProcessName}")]
    public class PmlProcess
    {
        public uint ProcessId;
        public string ProcessName;
        public PmlModule[] Modules;
    }

    [DebuggerDisplay("{ImagePath}")]
    public class PmlModule
    {
        public string ImagePath;
        public ulong BaseAddress;
        public uint Size; 
    }

    [DebuggerDisplay("#{EventIndex} ({FrameCount} frames)")]
    public class PmlEventStack
    {
        public int EventIndex;
        public long CaptureTime; // FILETIME
        public PmlProcess Process;
        public ulong[] Frames = new ulong[200];
        public int FrameCount;
    }

    public class PmlReader : IDisposable
    {
        BinaryReader m_Reader;
        uint m_EventCount;
        ulong m_EventOffsetsOffset;
        Dictionary<uint, PmlProcess> m_ProcessesByPmlIndex = new();
        string[] m_Strings;
        
        // see https://github.com/eronnen/procmon-parser/blob/master/docs/PML%20Format.md for reverse-engineered format

        public void Dispose() => m_Reader.Dispose();

        public PmlReader(string pmlPath)
        {
            m_Reader = new BinaryReader(new FileStream(pmlPath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.Unicode);
            
            if (m_Reader.ReadByte() != 'P' || // Signature - "PML_"
                m_Reader.ReadByte() != 'M' ||
                m_Reader.ReadByte() != 'L' ||
                m_Reader.ReadByte() != '_')
            {
                throw new FileLoadException("Not a PML file", pmlPath);
            }
            
            const int expectedVersion = 9;
            var version = m_Reader.ReadUInt32();
            if (version != expectedVersion) // The version of the PML file. I assume its 9
                throw new FileLoadException($"PML has version {version}, expected {expectedVersion}", pmlPath);
            
            var is64Bit = m_Reader.ReadUInt32(); // 1 if the system is 64 bit, 0 otherwise
            if (is64Bit != 1)
                throw new FileLoadException($"PML must be 64-bit", pmlPath);
            
            SeekCurrent(
                0x20 +  // The computer name
                0x208); // The system root path (like "C:\Windows")

            m_EventCount = m_Reader.ReadUInt32();
            SeekCurrent(8); // Unknown
            var eventsDataOffset = m_Reader.ReadUInt64();
            m_EventOffsetsOffset = m_Reader.ReadUInt64();
            var processesOffset = m_Reader.ReadUInt64();
            var stringsOffset = m_Reader.ReadUInt64();

            // minor sanity check
            SeekBegin(m_EventOffsetsOffset);
            var eventOffset0 = m_Reader.ReadUInt32();
            if (eventOffset0 != eventsDataOffset)
                throw new FileLoadException($"PML has mismatched first event offset ({eventOffset0} and {eventsDataOffset})", pmlPath);

            ReadStringData(stringsOffset);
            ReadProcessData(processesOffset);
        }

        void ReadStringData(ulong stringsOffset)
        {
            SeekBegin(stringsOffset);

            var stringDataOffsets = new uint[m_Reader.ReadUInt32()];
            for (var istring = 0; istring < stringDataOffsets.Length; ++istring)
                stringDataOffsets[istring] = m_Reader.ReadUInt32();
            m_Strings = new string[stringDataOffsets.Length];
            for (var istring = 0; istring < stringDataOffsets.Length; ++istring)
            {
                SeekBegin(stringsOffset + stringDataOffsets[istring]);
                m_Strings[istring] = new string(m_Reader.ReadChars((int)m_Reader.ReadUInt32() / 2));
            }
        }
        
        void ReadProcessData(ulong  processesOffset)
        {
            SeekBegin(processesOffset);

            var processCount = (int)m_Reader.ReadUInt32();
            SeekCurrent(processCount * 4); // jump over the process indexes array
            var processDataOffsets = new uint[processCount];
            for (var iprocess = 0; iprocess < processDataOffsets.Length; ++iprocess)
                processDataOffsets[iprocess] = m_Reader.ReadUInt32();
            for (var iprocess = 0; iprocess < processDataOffsets.Length; ++iprocess)
            {
                var process = new PmlProcess();
                m_ProcessesByPmlIndex.Add(m_Reader.ReadUInt32(), process); // The process index (for events to use as a reference to the process)
                
                process.ProcessId = m_Reader.ReadUInt32();
                
                SeekCurrent(
                    4 + // Parent process id
                    4 + // Unknown
                    8 + // Authentication id
                    4 + // Session number
                    4 + // Unknown
                    8 + // The starting time of the process.
                    8 + // The ending time of the process.
                    4 + // 1 if the process is virtualized, 0 otherwise.
                    4 + // 1 if this process is 64 bit, 0 if WOW64.
                    4 + // Integrity - as a string index
                    4); // the user - as a string index
                
                process.ProcessName = m_Strings[m_Reader.ReadUInt32()];
                
                SeekCurrent(
                    4 + // the image path - as a string index
                    4 + // the command line - as a string index
                    4 + // company of the executable - as a string index
                    4 + // version of the executable - as a string index
                    4 + // description of the executable - as a string index
                    4 + // Icon index small (0x10 pixels)
                    4 + // Icon index big (0x20 pixels)
                    8); // Unknown

                var moduleCount = m_Reader.ReadUInt32();
                process.Modules = new PmlModule[moduleCount];
                for (var imodule = 0; imodule < moduleCount; ++imodule)
                {
                    SeekCurrent(8); // Unknown

                    process.Modules[imodule] = new PmlModule
                    {
                        BaseAddress = m_Reader.ReadUInt64(), // Base address of the module.
                        Size = m_Reader.ReadUInt32(), // Size of the module. 
                        ImagePath = m_Strings[m_Reader.ReadUInt32()] // image path - as a string index
                    };
                    
                    SeekCurrent(
                        4 + // version of the executable - as a string index
                        4 + // company of the executable - as a string index
                        4 + // description of the executable - as a string index
                        4 + // timestamp of the executable
                        8 * 3); // Unknown
                    
                }
            }
        }
        
        // one instance is created per call, then updated and yielded on each iteration  
        public IEnumerable<PmlEventStack> SelectEventStacks()
        {
            var offsets = new UInt64[m_EventCount];
            SeekBegin(m_EventOffsetsOffset);
            for (var ievent = 0; ievent < m_EventCount; ++ievent)
            {
                offsets[ievent] = m_Reader.ReadUInt32();
                SeekCurrent(1); // Unknown flags
            }
            
            var eventStack = new PmlEventStack();
            
            for (; eventStack.EventIndex < m_EventCount; ++eventStack.EventIndex)
            {
                SeekBegin(offsets[eventStack.EventIndex]);
             
                // below: consts.py is in https://github.com/eronnen/procmon-parser/blob/master/procmon_parser/consts.py 

                eventStack.Process = m_ProcessesByPmlIndex[m_Reader.ReadUInt32()];
                
                SeekCurrent(
                    4 + // Thread Id.
                    4 + // Event class - see class PmlEventClass(enum.IntEnum) in consts.py
                    2 + // see ProcessOperation, RegistryOperation, NetworkOperation, FilesystemOperation in consts.py
                    6 + // Unknown.
                    8); // Duration of the operation in 100 nanoseconds interval.

                eventStack.CaptureTime = (long)m_Reader.ReadUInt64(); // (FILETIME) The time when the event was captured.
                
                SeekCurrent(4); // The value of the event result.
                
                eventStack.FrameCount = m_Reader.ReadUInt16(); // The depth of the captured stack trace.
                
                SeekCurrent(
                    2 + // Unknown
                    4 + // The size of the specific detail structure (contains path and other details)
                    4); // The offset from the start of the event to extra detail structure (not necessarily continuous with this structure).

                for (var iframe = 0; iframe < eventStack.FrameCount; ++iframe)
                    eventStack.Frames[iframe] = m_Reader.ReadUInt64();
                
                yield return eventStack;
            }
        }

        public PmlProcess? FindProcessByProcessId(uint processId) =>
            m_ProcessesByPmlIndex.Values.FirstOrDefault(p => p.ProcessId == processId);            
        
        void SeekBegin(ulong offset) => m_Reader.BaseStream.Seek((long)offset, SeekOrigin.Begin); 
        void SeekCurrent(int offset) => m_Reader.BaseStream.Seek(offset, SeekOrigin.Current); 
    }
}
