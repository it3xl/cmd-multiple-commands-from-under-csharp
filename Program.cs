using System.Diagnostics;

namespace PrototypingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "cmd.exe",

                // The Process object must have the UseShellExecute property set to false in order to redirect IO streams.
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var proc = new Process();
            // The "using" is more safe alternative for "proc.Close()" to release resources in the process' wrapper.
            using (proc)
            {
                proc.StartInfo = info;
                proc.Start();

                proc.StandardInput.WriteLine("none_existing_command /oops");
                proc.StandardInput.WriteLine();
                proc.StandardInput.WriteLine(@"""C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsMSBuildCmd.bat""");
                proc.StandardInput.WriteLine();
                proc.StandardInput.WriteLine(@"ping example.com -n 5");
                proc.StandardInput.WriteLine();
                proc.StandardInput.WriteLine("none_existing_command /oops");
                proc.StandardInput.WriteLine();
                proc.StandardInput.WriteLine(@"ECHO /?");
                proc.StandardInput.WriteLine();
                proc.StandardInput.WriteLine("ECHO ErrorLever = %ERRORLEVEL%");

                // Allow not-blocking use of ReadToEnd().
                // Use the CMD EXIT command vs "proc.StandardInput.Close()" to pass the exit code to .NET proc.ExitCode below;
                proc.StandardInput.WriteLine("EXIT");
                // No more the used CMD process\ here.

                //var waitSeconds = 1;
                //var interrupted = !proc.WaitForExit(waitSeconds * 1000);

                // Remember to use async reads if you wish.
                //proc.BeginOutputReadLine
                //proc.BeginErrorReadLine

                var output = proc.StandardOutput.ReadToEnd();
                var errorOutput = proc.StandardError.ReadToEnd();

                var exitCode = proc.ExitCode;
                if(exitCode != 0)
                {
                    // Your actions.
                }
            }

        }

　
    }
}
