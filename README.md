## CMD multiple commands from under C#
 
The solution emulates working with the CMD console.<br/>
Gather all your CMD-commands you want to type on the console and invoke them from you C# code.

I use this approach to execute a bunch of multiple CMD commands and batch files.

Implementation [CmdShell.cs](https://github.com/it3xl/cmd-multiple-commands-from-under-csharp/blob/master/CmdShellProj/CmdShell.cs). Sample usage [Program.cs](https://github.com/it3xl/cmd-multiple-commands-from-under-csharp/blob/master/ConsoleRunner/Program.cs).

### Warning

For any invocations without an iteractive session (build machines, windows services)!<br/>
Use the **ExecAndShowCatched** method instead of **ExecAndShow**. ExecAndShow doesn't work in this case.

### C# Example

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

ECHO ErrorLever = %ERRORLEVEL%
";

    //new CmdShell().ExecAndShowCatched(cmdCommands);
    new CmdShell().ExecAndShow(cmdCommands);
```
## Useful to know

* Use **ExecAndShowCatched** method if you need to intercept CMD outputs in your C#. You can store or analyze it.
* **ExecAndShow** method allows to debug in an interactive session, i.e. where you can see a console window.
* I put all logic and methods to a single file for an usefulness of automation professionals. Feel free to decompose it.


