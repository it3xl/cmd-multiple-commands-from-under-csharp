using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CmdShellProj
{
    /// <summary>
    /// it3xl/CMD-shell_from_under_csharp
    /// https://github.com/it3xl/CMD-shell_from_under_csharp/blob/master/Program.cs
    /// </summary>
    public class CmdShell
    {
        private readonly StringBuilder _outputCombined = new StringBuilder();

        public void Execute(string cmdCommands)
        {
            var commandsList = cmdCommands.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var info = new ProcessStartInfo
            {
                FileName = "cmd.exe",

                // The Process object must have the UseShellExecute property set to false in order to redirect IO streams.
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var proc = new Process();
            // The "using" is more safe alternative for "proc.Close()" to release resources.
            using (proc)
            {
                proc.StartInfo = info;
                proc.OutputDataReceived += DataReceived;
                proc.ErrorDataReceived += DataReceived;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                // This could be useful for somebody to use async reads if you wish.
                //proc.BeginOutputReadLine
                //proc.BeginErrorReadLine

                commandsList.ToList()
                    .ForEach(command => proc
                        .StandardInput.WriteLine(command));

                // Allow not-blocking use of ReadToEnd().
                // Use the CMD EXIT command vs "proc.StandardInput.Close()" to pass the exit code to .NET proc.ExitCode below;
                proc.StandardInput.WriteLine("EXIT");
                // At this point, the used CMD process does not exist anymore.

                var waitMinutes = 15;
                bool interrupted;
                while (true)
                {
                    interrupted = !proc.WaitForExit(waitMinutes * 1000 * 60);

                    if (interrupted || proc.HasExited)
                    {
                        break;
                    }
                }

                string catchedOutputAll;
                lock (_outputCombined)
                {
                    catchedOutputAll = _outputCombined.ToString();
                }

                if (interrupted)
                {
                    //throw new Exception(string.Format("Was interrupted after waiting for {0} seconds.", waitSeconds));
                }

                

                if (!proc.HasExited)
                {
                    
                }


                var exitCode = proc.ExitCode;
                if (exitCode != 0)
                {
                    // STUB: Remove the return.
                    return;

                    throw new Exception(string.Format(@"Error exit code {0} received.

Output:
{1}
",
                        exitCode,
                        catchedOutputAll
                        ));
                }
            }

        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_outputCombined)
            {
                _outputCombined.AppendLine(e.Data);
            }
        }
    }
}