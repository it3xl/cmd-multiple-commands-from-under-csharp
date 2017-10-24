using System;
using System.Diagnostics;
using System.Linq;

namespace CmdShellProj
{
    /// <summary>
    /// it3xl/CMD-shell_from_under_csharp
    /// https://github.com/it3xl/CMD-shell_from_under_csharp/blob/master/Program.cs
    /// </summary>
    public class CmdShell
    {
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
                proc.Start();

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

                var waitSeconds = 600;
                var interrupted = !proc.WaitForExit(waitSeconds * 1000);

                if (interrupted)
                {
                    //throw new Exception(string.Format("Was interrupted after waiting for {0} seconds.", waitSeconds));
                }

                var output = proc.StandardOutput.ReadToEnd();
                var errorOutput = proc.StandardError.ReadToEnd();

                var exitCode = proc.ExitCode;
                if (exitCode != 0)
                {
                    // STUB: Remove the return.
                    return;

                    throw new Exception(string.Format(@"Error exit code {0} received.
Error Output:
{1}

Output:
{2}
",
                        exitCode,
                        errorOutput,
                        output
                        ));
                }
            }

        }
    }
}