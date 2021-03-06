![GitHub](https://img.shields.io/github/license/jasc2v8/CSharpHotkey)
![Environment](https://img.shields.io/badge/Windows-XP,%20Vista,%207,%208,%2010-brightgreen.svg)
![GitHub forks](https://img.shields.io/github/forks/jasc2v8/CSharpHotkey)
[![Donate](https://img.shields.io/badge/Donate-PayPal-red.svg)](https://www.paypal.me/JimDreherHome)

<img src="resources/CSharpHotkey-64.png"> 

# CSharpHotkey
AutoHotkey limited functions for C#

## Overview

CSharpHotkey is:
- Implements many of the AutoHotkey_L_v1 automation functions in C# .NET code.
- The user's C# .NET code provides all the flow, gui, structure, and work.
- Provides many of the Pinvoke Win32 native functions as AHK_L_v1 Win functions.
- Uses many C# features that enhance AHK (examples: Color, Keys, Point, Rectangle).
- A single C# class compiled with the user's exe.

CSharpHotkey is NOT:
- A complete implementation or replacement of AHK_L_v1.
- Cannot load or execute .ahk scripts.
- No plans to update for AHK_L_v2.

## Differences
**WinTitle:** Most modern desktop and web apps don't use controls anymore, therefore there is no need to match class name nor text in controls. We can still use the WinTitle and ImageSearch find and activate button images and links for basic Hotkey automation.

**HotKey:** Class to register hot keys.

**InputHook:** Class to register global keyboard and mouse hooks.

**Unimplemented:** Windows Control functions not implemented because modern desktop and web apps don't use controls. Use C# .NET code to implement: File, I/O, flow, Group, Obj, Send, Sounds, String, etc. 

## Demo

<img src="resources/DEMO_CSharpHotkey_640.png"> 

## Build Tools
- Visual Studio 2022 with .NET Framework 3.5 (for compatibility).
- Windows 10 Home 64bit with i7 CPU @3.3GHZ (6 cores) and 16GB memory.
- Can easily be converted to other Visual Studio, .NET Core, and .NET Framework versions.

## Credits
- ImageSearch: See ImageSearch() in CSharpHotkey.cs
- Everything else: The many contributors to StackOverflow, CodeProject, MSDN, and many others.

## Objectives
This is a continuation of my COVID-19 Pandemic projects to pass the time until the world gets back to work and it's safe to socialize once again.

My hobbyist programming background is Visual Basic, AutoIT, Free Pascal, and AutoHotkey as well as Microsoft Excel macros.  I was always curious about C programming but never made the plunge to learn it.

For my first Pandemic project, I started with [AHKEZ](https://github.com/jasc2v8/AHKEZ). It was fun and I learned a lot, then needed another project.

Since my work is fully on Microsoft Windows, and all my PCs and server are on Microsoft Windows, I decided it was time to learn C programming.

I downloaded Visual Studio 2022 and was amazed how mature this product it. There was a bit of a learning curve but I found my "dream" IDE. It's best suited for a higher power CPU and ample memory. I'm very happy with the performance on my machine (see my Build Tools above).

I've always wanted to learn C so I started with it and learned that it's best suited for low-level applications and not my area of interest.

Then I switched to C++, which was great but annoying when calling Win32 native functions.  I can't believe how random and non-standard the Microsoft Win32 calling conventions are! How many data and pointer types are enough?  Again not my area of interest.

Finally, I switched to C# .NET programming, and found my "dream" language.  I never realized that this is a high level language and relatively easy to understand. I'm very happy that I chose C# .NET to create this CSharpHotkey library!

I HIGHLY recommend C# .NET on VS2022 for any hobbyist programming on Windows (it's also cross-platform now if you are interested).

