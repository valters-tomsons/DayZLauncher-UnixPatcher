# DayZLauncher-UnixPatcher

Provides unofficial fixes for the DayZ launcher when running in Linux/Proton.

Features:
* Fixes being unable to install mods from workshop
* Fixes settings always being reset
* Fixes settings not being saved

## Upgrading

If you are upgrading from a previous version, make sure to (**in order**):
* delete `steamapps/compatdata/221100` folder
* verify game files
* start the game launcher once
* apply the patch using `./bin/DayZLauncher.UnixPatcher`

## Notice

**I am not affiliated, associated, authorized, endorsed by, or in any way officially connected with Bohemia Interactive, a.s., or any of its subsidiaries or its affiliates.**

If the patcher utility or patch itself destroys your computer, I am not to be held accountable for you running random software on your computer. 

While this patch *should* be safe from detection by Battleye because it doesn't modify anything from the game itself. That being said, use at your own risk and it *could* change.

## Usage:

1. Download or build the patcher

2. Launch `DayZLauncher.UnixPatcher` with full DayZ installation path as argument

(eg. `./DayZLauncher.UnixPatcher "/primary/SteamLibrary/steamapps/common/DayZ/"`)

3. If you see a success message, boot up DayZ and enjoy!

! **Patch may need to be re-applied after game updates.**

## Screenshots

![image](https://user-images.githubusercontent.com/4209639/233074283-b42db574-c6cd-42a8-8371-0a632b6c349d.png)

![Screenshot from 2023-04-19 11-19-32](https://user-images.githubusercontent.com/4209639/233074371-563ca89b-2dda-4d90-b2fe-ef7045ea653b.png)
