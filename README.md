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

* With a small change the solution is ready to intercept CMD outputs in C#.

## Known issues

* Prefer to use the CALL CMD-command to invoke batch files. Otherwise, you can hang up your CMD execution in some rare cases.
