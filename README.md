## CMD multiple commands from under C#
 
This emulates work with CMD console. Just like you typing in the console. But now you invoke your commands from C#.

I use this approach to execute a bunch of multiple CMD commands and batch files from under .NET FrameWork C# in a single CMD console.

Implementation [CmdShell.cs](https://github.com/it3xl/cmd-multiple-commands-from-under-csharp/blob/master/CmdShellProj/CmdShell.cs)

Sample usage [Program.cs](https://github.com/it3xl/cmd-multiple-commands-from-under-csharp/blob/master/ConsoleRunner/Program.cs).

```csharp
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
```

## Known issues

* Always use the CALL CMD-command to invoke batch files. Otherwise, you can hang up your CMD execution.
