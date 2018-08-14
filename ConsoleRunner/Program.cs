using CmdShellProj;
using System;
using System.Text;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var msBuildCommandPrompt = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsMSBuildCmd.bat";
            var somePath = @"C:\temp";

            var cmdCommands = $@"
CD /

none_existing_command /oops
ping example.com -n 5
none_existing_command /oops

CALL ""{msBuildCommandPrompt}""

CD ""{somePath}""

CALL MsBuild SomeProject.csproj^
 /target:Build^
 /p:Configuration=Release^
 /verbosity:normal^
 /maxCpuCount

ECHO ErrorLever = %ERRORLEVEL%";

            var exitCode1 = new CmdShell()
                .ExecAndShowCatched(cmdCommands);

            StringBuilder output;
            var exitCode1_2 = new CmdShell()
                .ExecAndShowCatched(cmdCommands, out output);

            var exitCode2 = new CmdShell()
                .ExecAndShow(cmdCommands);

            var exitCode3 = new CmdShell()
                .ExecExample(cmdCommands);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("exit code is {0} for ExecAndShowCatched", exitCode1);
            Console.WriteLine("exit code is {0} for ExecAndShowCatched with outputs", exitCode1_2);
            Console.WriteLine("exit code is {0} for ExecAndShow", exitCode2);
            Console.WriteLine("exit code is {0} for ExecExample", exitCode3);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("========================================================");
            Console.WriteLine("Click to show intercepted outputs for ExecAndShowCatched");
            Console.WriteLine("========================================================");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Console.ReadKey();
            Console.WriteLine(output);

        }
    }
}
