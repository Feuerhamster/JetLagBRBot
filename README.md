# LetLag BR Bot
This is a modular Telegram Chat Bot and Web API for a custom "JetLag The Game".
Developed for the 38c3 and future Games.

This software is written in C# with .NET Core 8

## Telegram Commands

### `/start`

### `/new_game`
Starts a new game.
This command has to be executed within the telegram chat group the game should take place.
After this command is executed, it will prompt for a template. 
You can select a template and the game will be initialized with the data and gamemode configured in the template.

### `/reload_templates`
Clears and then reloads all templates in the /Game/Templates

*Todo: complete docs for commands*

## Game Templates
Templates are the configuration for a specific game scenario.
It defines the map, the game mode and all data tied to that.

### BattleRoyaleHamburg
A "Battle Royale" game that takes place in Hamburg.
Tag players with photos and chase Landmarks to obtain useful powerups.

## Additions, Ideas, Fixes and Pull Requests
Feel free to fork this repo and make pull requests. I will review and approve them.
The repo has a CI Pipeline that automatically builds the docker container.
If it is something urgent like a hotfix and I am currently not there on the 38c3 and/or in the game,
please call me on EPVPN or DECT, my number is 5044. I will hit deploy on my server as fast as I can.