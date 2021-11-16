using System;
using System.IO;
using System.Text;

namespace PmlConversion
{
    public static class PmlSymbolicator
    {
        // MISSING: support for domain reloads. pass in a timestamp to use (which would in non-test scenarios just come from
        // a stat for create-time on the pmip file itself) and the symbolicator can use the event create time to figure out
        // which pmip set to use.
        // ALSO: probably want to support an automatic `dir $env:temp\pmip*.txt` -> MonoSymbolReader[] 
        
        // write out frames file
        //   format: eventid;capturetime;pid;frame0,frame1,frame2..
        //   ^ capturetime should be same format the csv gets so a string compare will work, and the frames are just hex addresses (no 0x)
        // write out map file
        //   format: pid;start;end;[module.dll] symbol(args)
        //   ^ start/end are hex addresses (no 0x)
        // integration tests:
        //   1. run unity with right env flags and command line args (incl editor log to custom location)

        public static void Symbolicate(string inPmlPath, string inMonoPmipPath, string outFramesPath, string outMapPath) 
        {
            using var pmlReader = new PmlReader(inPmlPath);
            var monoSymbolReader = new MonoSymbolReader(inMonoPmipPath);
            
            var unityProcess = pmlReader.FindProcessByProcessId(monoSymbolReader.UnityProcessId)
                ?? throw new FileLoadException($"Unity PID {monoSymbolReader.UnityProcessId} not found in PML processes", inMonoPmipPath);
            
            using var framesFile = File.CreateText(outFramesPath);
            framesFile.Write("Sequence;Time of Day;PID;Frame\n");
            
            var sb = new StringBuilder();
            foreach (var eventStack in pmlReader.SelectEventStacks())
            {
                sb.Append(eventStack.EventIndex);
                sb.Append(';');
                sb.AppendFormat("{0:hh:mm:ss.fffffff tt}", DateTime.FromFileTime(eventStack.CaptureTime));
                sb.Append(';');
                sb.Append(eventStack.Process.ProcessId);
                sb.Append(';');
                
                for (var i = 0; i < eventStack.FrameCount; ++i)
                {
                    if (i != 0)
                        sb.Append(',');
                    sb.AppendFormat("{0:X}", eventStack.Frames[i]);
                }
                
                sb.Append('\n');
                framesFile.Write(sb.ToString());
                sb.Clear();
            }
        }
    }
}
