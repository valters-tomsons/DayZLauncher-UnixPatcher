# DayZLauncher-UnixPatcher

## Fork details
This is a fork of Valters-Tomsons [DayZLauncher-UnixPatcher](https://github.com/valters-tomsons/DayZLauncher-UnixPatcher).

This fork provides a few changes / fixes to the patcher.

##

### Changes
1. Added a patch.sh script to simplify installation (not completed, but usable).
2. Renamed the temp doriectory from `!Linux` to `linux-temp` as some systems wouldn't allow the shell to access the `!Linux` directory (File not found errors).
3. Added a function to locate the absolute path of the DayZ installation.
4. Changed the UnixJunctions function accept any drive letter and use absolute path, this accomodates for cases where the installation is on an external drive / SD card.

### Known issues
1. On some occasions, closing the launcher during a 'preventative sync' causes the launcher to delete the workshop mods and force Steam to redownload them.
2. Verifying the mod signatures sometimes fails. This is due to the Launcher using 'Hash algorithm name' whereas Mono (the wine .Net equivalent) expects a 'Hash algorithm OID' instead. I am not completely sure of the implicarions of this yet.
3. On very rare ocassions the launcher fails to load the patch and displays an error message on launch. Relaunching seems to solve this.
4. Although this does enable workshop functionality it doesn't fix the poor performance of the launcher. If like me you are running a stupid amount of mods it can be very slow to initialize. DO NOT interrupt it or things will get messed up!

### Installation
1. Make sure that you have deleted your compatdata directory and validated game files first!: \
   ``` (Example) rm -rf /$HOME/.steam/steam/steamapps/compatdata/221100 ``` \
   ``` (Manually verify game files in steam) ```
2. Download the latest release from here and unzip: \
   ``` wget https://github.com/djedu/DayZLauncher-UnixPatcher/releases/latest/download/unixpatcher.tar.xz```
   ``` tar -xvf ./unixpatcher.tar.xz```
3. Change the patch.sh file to be executable: \
   ``` chmod +x ./unixpatcher/patch.sh ```
4. Run the patch: \
   ``` ./unixpatcher/patch.sh ```

### Disclaimer
As with the original project, use this at your own risk! \
This patch may cause your mods to vanish and believe me I know the pain, running with over 1300 workshop mods...\
Other issues may happen to the stability of the launcher and game! \
It is not known for this patch to catch battleye's attention as it doesn't modify the game in any way but that could change without notice! \
Again, USE THIS AT YOUR OWN RISK!

### Issues
If you have any issues with any of the modifications I have made please get in touch, but for any other issues please check out the [original repository](https://github.com/valters-tomsons/DayZLauncher-UnixPatcher) first.

##

## Original README:

Provides unofficial fixes for the DayZ launcher when running in Linux/Proton.

Features:
* Fixes being unable to install mods from workshop
* Fixes settings always being reset
* Fixes settings not being saved

## Upgrading

If you are upgrading from a previous version, make sure to (in order):
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
