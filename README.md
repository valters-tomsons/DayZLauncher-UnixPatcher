# DayZLauncher-UnixPatcher

![Screenshot from 2023-04-19 11-19-32](https://user-images.githubusercontent.com/4209639/233074371-563ca89b-2dda-4d90-b2fe-ef7045ea653b.png)

Provides unofficial fixes for the DayZ launcher when running on Linux/Proton & SteamOS.

# Features

* Fixes being unable to install mods from workshop

## Limitations

* Anything related to workshop mods will be slow
* Might not work if game is installed on non-primary drive (e.g. SD card), see [this fork](https://github.com/djedu/DayZLauncher-UnixPatcher).
* Any number of fixes might break after game updates

## Upgrading

If you are upgrading from a previous version, make sure to (**in order**):
1. delete `steamapps/compatdata/221100` folder
1. verify game files
1. start the DayZ launcher once
1. follow [installation](#usage)

## Notices

1. The author of the software is not affiliated, associated, authorized, endorsed by, or in any way officially connected with Bohemia Interactive, a.s., or any of its subsidiaries or affiliates. The use of any trademarks, logos, or brand names is for identification purposes only and does not imply endorsement or affiliation.

2. By utilizing the provided software, the user acknowledges and accepts all potential risks and consequences, including but not limited to **spontaneous combustion, implosion, or vaporization of their computer system**. The author of the software shall not be held liable for any damages or losses incurred as a result of the software's operation.

*TLDR:* Use at your own risk.

## Usage:

1. Start DayZ launcher at least once
1. Download and extract the latest release
2. Go to extracted directory and run `chmod +x DayZLauncher.UnixPatcher` in terminal
1. Run `./DayZLauncher.UnixPatcher` in terminal and follow the instructions
1. If you see a success message, boot up DayZ and enjoy!

! **Patch will need to be re-applied after game updates.**

![image](https://user-images.githubusercontent.com/4209639/233074283-b42db574-c6cd-42a8-8371-0a632b6c349d.png)
