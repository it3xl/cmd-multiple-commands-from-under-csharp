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
        /// This method is for evolving by you and combines main techniques at once.
        /// It is an exact anamog of <see cref="ExecAndShow"/>.
        /// Also consider to look at <see cref="ExecAndShowCatched"/>.
        /// </summary>
        /// <param name="cmdCommands">CMD commands to be executed separated. Multi or a single line.</param>
        /// <param name="executionLimit">The maximum duration limit for the entire execution. Default is 15 minutes.</param>
        /// <param name="throwExceptions">Throw an exceptions in case of a non-zero exit code or exceeding the duration limit.</param>
        public int ExecExample(string cmdCommands, TimeSpan? executionLimit = null, bool throwExceptions = false)
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
                // Allows exiting from a CMD side.
                proc.StandardInput.WriteLine("EXIT");

                var span = executionLimit ?? TimeSpan.FromMinutes(15);
                var milliseconds = span.TotalMilliseconds;
                var duration = milliseconds < int.MaxValue
                    ? (int)milliseconds
                    : int.MaxValue;

                var interrupted = !proc.WaitForExit(duration);

                if (throwExceptions)
                {
                     if (interrupted)
                        throw new Exception("Duration limit is exceeded");

                    if (proc.ExitCode != 0)
                        throw new Exception(string.Format("Error exit code {0} received.", proc.ExitCode));
                }

                return proc.ExitCode;
            }
        }

        /// <summary>
        /// Executes CMD commands and shows outputs (stdout, stderr) on a console window.
        /// </summary>
        /// <param name="cmdCommands">CMD commands to be executed separated. Multi or a single line.</param>
        /// <param name="throwExceptions">Throw an exceptions in case of a non-zero exit code or exceeding the duration limit.</param>
        /// <param name="executionLimit">The maximum duration limit for the entire execution. Default is 15 minutes.</param>
        public int ExecAndShow(string cmdCommands, TimeSpan? executionLimit = null, bool throwExceptions = false)
        {
            var exitCode = new Demonstrating(cmdCommands, executionLimit, throwExceptions)
                .Exec();

            return exitCode;
        }

        /// <summary>
        /// Executes CMD commands and shows outputs (stdout, stderr) on a console window.
        /// </summary>
        private sealed class Demonstrating : CmdShellBase
        {
            private readonly int _executionLimitMillisec;

            internal Demonstrating(string cmdCommands, TimeSpan? executionLimit, bool throwExceptions)
                : base(cmdCommands, throwExceptions)
            {
                _executionLimitMillisec = GetMilliseconds(executionLimit);
            }

            internal override int Exec()
            {
                InitProcess();
                using (Proc)
                {
                    RunCommands();

                    var interrupted = !Proc.WaitForExit(_executionLimitMillisec);

                    Throw(interrupted, Proc.ExitCode);

                    return Proc.ExitCode;
                }
            }
        }

        /// <summary>
        /// In some cases you must catch huge outputs, otherwise you shell will fail.
        /// This method executes CMD commands, captures all outputs (stdout and stderr, not only stdin)
        ///  and passes them to your shell.
        /// </summary>
        /// <param name="cmdCommands">CMD commands to be executed separated. Multi or a single line.</param>
        /// <param name="throwExceptions">Throw an exceptions in case of a non-zero exit code or exceeding the duration limit.</param>
        /// <param name="collectOutputs">Instructs to collect all console outputs.</param>
        /// <param name="outputWaitLimit">The maximum duration limit for any output waiting from a CMD-shell. Default is 15 minutes.</param>
        public int ExecAndShowCatched(string cmdCommands, bool throwExceptions = false, bool collectOutputs = false, TimeSpan? outputWaitLimit = null)
        {
            var exitCode = new OutputCatcher(cmdCommands, throwExceptions, collectOutputs, outputWaitLimit)
                .Exec();

            return exitCode;
        }

        /// <summary>
        /// In some cases you must catch huge outputs, otherwise you shell will fail.
        /// This method executes CMD commands, captures all outputs (stdout and stderr, not only stdin)
        ///  and passes them to your shell.
        /// </summary>
        /// <param name="cmdCommands">CMD commands to be executed separated. Multi or a single line.</param>
        /// <param name="outputCombined">Combined CMD outputs of stdout and stderr.</param>
        /// <param name="throwExceptions">Throw an exceptions in case of a non-zero exit code or exceeding the duration limit.</param>
        /// <param name="outputWaitLimit">The maximum duration limit for any output waiting from a CMD-shell. Default is 15 minutes.</param>
        public int ExecAndShowCatched(string cmdCommands, out StringBuilder outputCombined, bool throwExceptions = false, TimeSpan? outputWaitLimit = null)
        {
            var outputer = new OutputCatcher(cmdCommands, throwExceptions, true, outputWaitLimit);
            var exitCode = outputer.Exec();

            outputCombined = outputer.OutputCombined;

            return exitCode;
        }

        /// <summary>
        /// Executes CMD commands and catches outputs (stdout, stderr) from the CMD-console.
        /// It is mostly for debug purposes, so you prefer to use the CMD Redirection to a log-file.
        /// </summary>
        private sealed class OutputCatcher : CmdShellBase
        {
            private readonly int _outputWaitingLimit;
            private readonly bool _collectOutputs;

            internal readonly StringBuilder OutputCombined = new StringBuilder();

            internal OutputCatcher(string cmdCommands,
                bool throwExceptions = false,
                bool collectOutputs = false,
                TimeSpan? outputWaitingLimit = null)
                : base(cmdCommands, throwExceptions)
            {
                _outputWaitingLimit = GetMilliseconds(outputWaitingLimit);
                _collectOutputs = collectOutputs;
            }

            internal override int Exec()
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

                    if (Debugger.IsAttached)
                    {
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    }

                    Throw(interrupted, Proc.ExitCode);

                    return Proc.ExitCode;
                }
            }

            private void DataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == null)
                    return;

                // Passes CMD's outputs to your process' console.
                // It must be here, otherwise we'll get a mess from stdout & stderr in _outputCombined at the beginning.
                Console.WriteLine(e.Data);

                if (!_collectOutputs)
                    return;

                lock (OutputCombined)
                {
                    if (e.Data != null)
                    {
                        OutputCombined.AppendLine(e.Data);
                    }
                }
            }

            protected override void Throw(bool interrupted, int exitCode)
            {
                if (!ThrowExceptions)
                    return;

                if (!_collectOutputs)
                {
                    base.Throw(interrupted, exitCode);

                    return;
                }

                string catchedOutput;
                lock (OutputCombined)
                {
                    catchedOutput = OutputCombined.ToString();
                }

                if (interrupted)
                {
                    throw new Exception(string.Format("Duration limit is exceeded.\nOutput:\n{0}", catchedOutput));
                }

                if (exitCode != 0)
                {
                    throw new Exception(string.Format("Error exit code {0} received.\nOutput:\n{1}", exitCode, catchedOutput));
                }
            }
        }

        private abstract class CmdShellBase
        {
            private List<string> CommandsList { get; set; }
            protected bool ThrowExceptions { get; private set; }
            protected ProcessStartInfo ProcStartInfo { get; private set; }
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

            internal abstract int Exec();

            protected virtual void Throw(bool interrupted, int exitCode)
            {
                if (!ThrowExceptions)
                    return;

                if (interrupted)
                    throw new Exception("Duration limit is exceeded");

                if (exitCode != 0)
                    // ReSharper disable once UseStringInterpolation
                    throw new Exception(string.Format("Error exit code {0} received.", exitCode));
            }

            protected void InitProcess()
            {
                Proc = new Process {  StartInfo = ProcStartInfo };
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