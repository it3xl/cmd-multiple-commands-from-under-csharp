## CMD multiple commands from under C#
 
This emulates work with CMD console. Just like you typing in the console. But now you invoke your commands from C#.

I use this approach to execute a bunch of multiple CMD commands and batch files.

Implementation [CmdShell.cs](https://github.com/it3xl/cmd-multiple-commands-from-under-csharp/blob/master/CmdShellProj/CmdShell.cs). Sample usage [Program.cs](https://github.com/it3xl/cmd-multiple-commands-from-under-csharp/blob/master/ConsoleRunner/Program.cs).

```csharp
    var cmdCommands = $@"
CD /

CALL none_existing_command /oops

ping example.com -n 5

CALL MsBuild SomeProject.csproj^
 /target:Build^
 /p:Configuration=Release^
 /verbosity:normal^
 /maxCpuCount

ECHO ErrorLever = %ERRORLEVEL%";

    new CmdShell()
        .ExecAndShow(cmdCommands);
```
## Useful to know

* Use ExecAndShowCatched method if you need to intercept CMD outputs in your C#. You can store or analyze it.

## Known issues

* ! Warning. For any invocations without an iteractive session!<br/>
Use **ExecAndShowCatched** instead of **ExecAndShow**. ExecAndShow doesn't work in this case.
