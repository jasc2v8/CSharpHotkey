# CSharpHotkey
AutoHotkey limited functions for C#

## OVERVIEW
- CSharpHotkey implements many of the AutoHotkey_L_v1 automation functions in C# .NET code
- The user's C# .NET code provides all the flow, gui, structure, and work.
- CSharpHotkey provides many of the Pinvoke Win32 native functions as AHK_L_v1 Win functions.
- CSharpHotkey uses many C# features that enhance AHK (examples: Color, Keys, Point, Rectangle).
- CSharpHotkey is a single C# class compiled with the user's exe.

CSharpHotkey is NOT:
- A complete implementation or replacement of AHK_L_v1
- Cannot load or execute .ahk scripts
- No plans to update for AHK_L_v2

## DIFFERENCES
WinTitle: Most modern desktop and web apps don't use controls anymore, therefore there is no need to match classname nor text in controls. We can still use the WinTitle for basic Hotkey automation

HotKey: Class to register hot keys

InputHook: Class to register global keyboard and mouse hooks

Unimplemented: Functions implemented with C# .NET code: File, I/O, flow, Group, Obj, Send, Sounds, String, etc. Windows Control functions not implemented because modern desktop and web apps don't use controls

## BUILD TOOLS
Visual Studio 2022 with .NET Framework 3.5 (for compatability)
Can easily be converted to other Visual Studio, .NET Core, and .NET Framework versions.
Can easily be converted a static class if preferred (remove Constructor and change methods to static).

## CREDITS
ImageSearch:        See ImageSearch()
Everything Else:    The many contributors to StackOverflow, CodeProject, MSDN, and many others.
