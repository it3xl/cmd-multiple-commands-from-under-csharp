using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CmdShellProj
{
    /// <summary>
    /// it3xl/cmd-multiple-commands-from-under-csharp
    /// https://github.com/it3xl/cmd-multiple-commands-from-under-csharp
    /// </summary>
    public class CmdShell
    {
        /// <summary>
        /// Executes CMD commands and shows outputs (stdout, stderr) on a console window. A combined version to show an idea.
        /// </summary>
        /// <param name="cmdCommands">CMD commands to be executed separated. Multi or a single line.</param>
        /// <param name="throwExceptions">Throw an exceptions in case of a non-zero exit code or exceeding the duration limit.</param>
        /// <param name="executionLimit">The maximum duration limit for the entire execution.</param>
        public void Execute(string cmdCommands, bool throwExceptions = false, TimeSpan? executionLimit = null)
        {
            var commandsList = cmdCommands
                    .Replace("\r", string.Empty)
                    .Split('\n')
                    .ToList();

            var info = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                // The false allows to access IO streams.
                UseShellExecute = false,
                // Allows write commands directly to a CMD-shell.
                RedirectStandardInput = true,
            };
            var proc = new Process { StartInfo = info };
            using (proc)
            {
                proc.Start();

                commandsList.ForEach(command => proc
                        .StandardInput.WriteLine(command));

                proc.StandardInput.WriteLine("@REM Exiting by CmdShell App. The last command sent.");
                // Allows exiting from CMD side.
                proc.StandardInput.WriteLine("EXIT");

                var span = executionLimit ?? TimeSpan.FromMinutes(15);
                var milliseconds = span.TotalMilliseconds;
                var duration = milliseconds < int.MaxValue
                    ? (int)milliseconds
                    : int.MaxValue;

                var interrupted = !proc.WaitForExit(duration);

                if (!throwExceptions)
                    return;

                if (interrupted)
                    throw new Exception("Duration limit is exceeded");

                if (proc.ExitCode != 0)
                    throw new Exception($@"Error exit code {proc.ExitCode} received.");
            }
        }

        /// <summary>
        /// Executes CMD commands and shows outputs (stdout, stderr) on a console window.
        /// </summary>
        /// <param name="cmdCommands">CMD commands to be executed separated. Multi or a single line.</param>
        /// <param name="throwExceptions">Throw an exceptions in case of a non-zero exit code or exceeding the duration limit.</param>
        /// <param name="executionLimit">The maximum duration limit for the entire execution.</param>
        public void ExecAndShow(string cmdCommands, bool throwExceptions = false, TimeSpan? executionLimit = null)
        {
            new Demonstrating(cmdCommands, throwExceptions, executionLimit)
                .Exec();
        }

        /// <summary>
        /// Executes CMD commands and shows outputs (stdout, stderr) on a console window.
        /// </summary>
        private sealed class Demonstrating : CmdShellBase
        {
            private readonly int _executionLimitMillisec;

            public Demonstrating(string cmdCommands, bool throwExceptions, TimeSpan? executionLimit)
                : base(cmdCommands, throwExceptions)
            {
                _executionLimitMillisec = GetMilliseconds(executionLimit);
            }

            public override void Exec()
            {
                InitProcess();
                using (Proc)
                {
                    RunCommands();

                    var interrupted = !Proc.WaitForExit(_executionLimitMillisec);

                    Throw(interrupted, Proc.ExitCode);
                }
            }
        }

        /// <summary>
        /// Executes CMD commands and catches all outputs (stdout, stderr).
        /// </summary>
        /// <param name="cmdCommands">CMD commands to be executed separated. Multi or a single line.</param>
        /// <param name="throwExceptions">Throw an exceptions in case of a non-zero exit code or exceeding the duration limit.</param>
        /// <param name="outputWaitingLimit">The maximum duration limit for any output from a CMD-shell.</param>
        public void ExecAndLog(string cmdCommands, bool throwExceptions = false, TimeSpan? outputWaitingLimit = null)
        {
            new OutputCatcher(cmdCommands, throwExceptions, outputWaitingLimit)
                .Exec();
        }

        /// <summary>
        /// Executes CMD commands and catches outputs (stdout, stderr) from the CMD-console.
        /// It is mostly for debug purposes, so you prefer to use the CMD Redirection to a log-file.
        /// </summary>
        private sealed class OutputCatcher : CmdShellBase
        {
            private readonly int _outputWaitingLimit;

            private readonly StringBuilder _outputCombined = new StringBuilder();

            public OutputCatcher(string cmdCommands, bool throwExceptions = false, TimeSpan? outputWaitingLimit = null) : base(cmdCommands, throwExceptions)
            {
                _outputWaitingLimit = GetMilliseconds(outputWaitingLimit);
            }

            public override void Exec()
            {
                ProcStartInfo.RedirectStandardOutput = true;
                ProcStartInfo.RedirectStandardError = true;

                InitProcess();
                using (Proc)
                {
                    Proc.OutputDataReceived += DataReceived;
                    Proc.BeginOutputReadLine();
                    Proc.ErrorDataReceived += DataReceived;
                    Proc.BeginErrorReadLine();

                    RunCommands();

                    var interrupted = false;
                    while (!interrupted && !Proc.HasExited)
                    {
                        interrupted = !Proc.WaitForExit(_outputWaitingLimit);
                    }

                    Throw(interrupted, Proc.ExitCode);
                }
            }

            private void DataReceived(object sender, DataReceivedEventArgs e)
            {
                lock (_outputCombined)
                {
                    _outputCombined.AppendLine(e.Data);
                }
            }

            protected override void Throw(bool interrupted, int exitCode)
            {
                if (!ThrowExceptions)
                    return;

                string catchedOutput;
                lock (_outputCombined)
                {
                    catchedOutput = _outputCombined.ToString();
                }

                if (interrupted)
                {
                    throw new Exception($"Duration limit is exceeded.\nOutput:\n{catchedOutput}");
                }

                if (exitCode != 0)
                {
                    throw new Exception($"Error exit code {exitCode} received.\nOutput:\n{catchedOutput}");
                }
            }
        }

        private abstract class CmdShellBase
        {
            private List<string> CommandsList { get; }
            protected bool ThrowExceptions { get; }
            protected ProcessStartInfo ProcStartInfo { get; }
            protected Process Proc { get; private set; }

            protected CmdShellBase(string cmdCommands, bool throwExceptions)
            {
                CommandsList = cmdCommands
                    .Replace("\r", string.Empty)
                    .Split('\n')
                    .ToList();

                ThrowExceptions = throwExceptions;

                ProcStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    // The false allows to access IO streams.
                    UseShellExecute = false,
                    // Allows write commands directly to a CMD-shell.
                    RedirectStandardInput = true,
                };
            }

            public abstract void Exec();

            protected virtual void Throw(bool interrupted, int exitCode)
            {
                if (!ThrowExceptions)
                    return;

                if (interrupted)
                    throw new Exception("Duration limit is exceeded");

                if (exitCode != 0)
                    throw new Exception($@"Error exit code {exitCode} received.");
            }

            protected void InitProcess()
            {
                Proc = new Process
                    {
                        StartInfo = ProcStartInfo
                    };
                try
                {
                    Proc.Start();
                }
                catch (Exception)
                {
                    Proc.Dispose();

                    throw;
                }
            }

            protected void RunCommands()
            {
                CommandsList.ForEach(command => Proc.StandardInput.WriteLine(command));
                FinishCmd(Proc.StandardInput);
            }

            protected int GetMilliseconds(TimeSpan? timeSpan)
            {
                var span = timeSpan ?? TimeSpan.FromMinutes(15);
                var milliseconds = span.TotalMilliseconds;
                var duration = milliseconds < int.MaxValue
                    ? (int)milliseconds
                    : int.MaxValue;

                return duration;
            }

            /// <summary>
            /// Allows exiting from a CMD side. Required.
            /// </summary>
            /// <param name="cmdInput"></param>
            private void FinishCmd(StreamWriter cmdInput)
            {
                cmdInput.WriteLine("@REM Exiting by CmdShell App. The last command sent.");
                // Allows exiting from CMD side.
                cmdInput.WriteLine("EXIT");
            }
        }
    }
}