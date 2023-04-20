# DayZLauncher-UnixPatcher

Fixes workshop mods not installing in DayZ launcher when running under in Mono runtime (Wine/Proton).

## Screenshots

![image](https://user-images.githubusercontent.com/4209639/233074283-b42db574-c6cd-42a8-8371-0a632b6c349d.png)

![Screenshot from 2023-04-19 11-19-32](https://user-images.githubusercontent.com/4209639/233074371-563ca89b-2dda-4d90-b2fe-ef7045ea653b.png)

## Notice

This patch generates a shell script for each affected UNIX file operation and other stupid things, nothing was throughly tested. If this destroys your computer, I am not to be held accountable for you running random software on your computer. 

## Usage:

1. Download or build the patcher

2. Launch patcher via wine, add your DayZ installation folder as argument 

3. If you see a success message, boot up DayZ and enjoy!

(eg. `wine ./DayZLauncher.UnixPatcher.exe "/primary/SteamLibrary/steamapps/common/DayZ/"`)

**Patch may need to be re-applied after game updates.**

## Why?

1. So you can play on community servers with the original launcher under Proton
2. So I don't have to re-distribute Bohemia's source code

##

**I am not affiliated, associated, authorized, endorsed by, or in any way officially connected with Bohemia Interactive, a.s., or any of its subsidiaries or its affiliates.**
