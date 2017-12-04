using CmdShellProj;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var msBuildCommandPrompt = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsMSBuildCmd.bat";
            var somePath = @"C:\temp";

            new CmdShell()
                .Execute(string.Format(@"
none_existing_command /oops
ping example.com -n 5
none_existing_command /oops

CALL ""{0}""

CD ""{1}""

CALL MsBuild SomeProject.csproj^
 /target:Build^
 /p:Configuration=Release^
 /verbosity:normal^
 /maxCpuCount

ECHO ErrorLever = %ERRORLEVEL%

",
                msBuildCommandPrompt,
                somePath
                ));
        }
    }
}
