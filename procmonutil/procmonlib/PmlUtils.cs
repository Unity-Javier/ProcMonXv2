using System;
using System.IO;
using System.Text;

namespace ProcMonUtils
{
    public static class PmlUtils
    {
        // MISSING: support for domain reloads. pass in a timestamp to use (which would in non-test scenarios just come from
        // a stat for create-time on the pmip file itself) and the symbolicator can use the event create time to figure out
        // which pmip set to use.
        // ALSO: probably want to support an automatic `dir $env:temp\pmip*.txt` -> MonoSymbolReader[] 
        
        // the purpose of this function is to bake text symbols (for native and mono jit) so the data can be transferred
        // to another machine without needing the exact same binaries and pdb's.
        public static void Symbolicate(string inPmlPath, string inMonoPmipPath, string outFramesPath) 
        {
            using var pmlReader = new PmlReader(inPmlPath);
            var monoSymbolReader = new MonoSymbolReader(inMonoPmipPath);
            
            var unityProcess = pmlReader.FindProcessByProcessId(monoSymbolReader.UnityProcessId)
                ?? throw new FileLoadException($"Unity PID {monoSymbolReader.UnityProcessId} not found in PML processes", inMonoPmipPath);
            
            using var framesFile = File.CreateText(outFramesPath);
            framesFile.Write("# EVENTS BEGIN\n");
            framesFile.Write("#\n");
            framesFile.Write($"# EventCount = {pmlReader.EventCount}\n");
            framesFile.Write("# EventDoc = Sequence;Time of Day;PID;Frame[0];Frame[1];Frame[..n]\n");
            framesFile.Write("# FrameDoc = $type [$module] $symbol + $offset (type: K=kernel, U=user, M=mono)\n");
            framesFile.Write("#\n");
            
            using var dbghelp = DebugHelp.SymbolHandler.Create();
            
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
                        sb.Append(';');
                    
                    var address = eventStack.Frames[i];
                    
                    var type = (address & 1UL << 63) != 0 ? 'K' : 'U';
                    
                    var module = eventStack.Process.FindModule(address);
                    if (module != null)
                    {
                        dbghelp.LoadSymbolsForModule(module.ImagePath, module.Address.Base);
                        var symbolInfo = dbghelp.GetSymbolFromAddress(address, out var offset);
                        
                        sb.AppendFormat("{0} [{1}] {2} + 0x{3:x}", type, Path.GetFileName(module.ImagePath), symbolInfo.Name, offset);
                    }
                    else if (eventStack.Process == unityProcess)
                    {
                        var monoJit = monoSymbolReader.FindSymbol(address);
                        if (monoJit == null)
                            throw new FileLoadException($"Unable to find mono frame {address} in any mono jit symbols of process {eventStack.Process.ProcessName}", inPmlPath);
                        
                        sb.AppendFormat("M [{0}] {1} + 0x{2:x}", monoJit.AssemblyName, monoJit.Symbol, address - monoJit.Address.Base);
                    }
                    else
                    {
                        sb.AppendFormat("{0} 0x{1:x}", type, address);
                    }
                }
                
                sb.Append('\n');
                framesFile.Write(sb.ToString());
                sb.Clear();
            }
            
            framesFile.Write("#\n");
            framesFile.Write("# EVENTS END\n");
            framesFile.Write("#\n");
        }
    }
}
