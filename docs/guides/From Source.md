## Setting up WizBot From Source

**Note: 32-bit Windows version is experimental**  
32-bit linux is not possible because of .Net compatability.

#### Prerequisites  
- [.net core sdk 2.x][.netcore]  
- [ffmpeg][ffmpeg]. Optional, needed for music. Download the static, release version for your architecture and store.  
- [youtube-dl](http://rg3.github.io/youtube-dl/download.html). Optional, needed for music. Download and store the exe. Install Microsoft Visual C++ 2010 Redistributable Package.  
- [Git][git]  
- Redis  
  - Windows 64 bit: Download and install the [latest msi][redis].  
  - Windows 32 bit: Download [redis-server.exe](https://github.com/Wizkiller96/WizBotFiles/blob/master/x86%20Prereqs/redis-server.exe?raw=true) and store.  
  - Linux: `apt-get install redis-server`  
- In addition, for 32-bit Windows, download [libsodium](https://github.com/MaybeGoogle/WizBotFiles/blob/master/x86%20Prereqs/WizBot_Music/libsodium.dll?raw=true) and (lib)[opus](https://github.com/Wizkiller96/WizBotFiles/blob/master/x86%20Prereqs/WizBot_Music/opus.dll?raw=true).  
- [Create Discord Bot application](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/#creating-discord-bot-application) and [Invite the bot to your server](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/#inviting-your-bot-to-your-server).  

#### Getting WizBot Ready to Run  
- Download the source: `git clone -b 1.9 https://github.com/Wizkiller96/WizBot`  
- Edit the `credentials.json` in `WizBot/src/WizBot` according to this [guide](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/#setting-up-credentialsjson-file).  
- Move `youtube-dl.exe` and `ffmpeg.exe` into `WizBot/src/WizBot` (or add them to your PATH env variable, if you know how) 
- For 32-bit Windows, replace `libsodium.dll` and `opus.dll` with the ones you downloaded.


**!!! NOTE FOR WINDOWS USERS  !!!**  
If you're running from source on windows, you will have to add these 2 extra lines to your credentials, after the first open bracket:
```js
    "ShardRunCommand": "dotnet",
    "ShardRunArguments": "run -c Release --no-build -- {0} {1}",
```

#### Running WizBot  
- For 32-bit Windows, run the `redis-server.exe` that you downloaded. You must have this window open when you use WizBot.  
- Move to the correct directory. `cd WizBot/src/WizBot`  
- Build and run. `dotnet run -c Release`  
- The bot should now start up and show as online in your Discord server.

#### Updating WizBot  
- Might not work if you've made custom edits to the source, make sure you know how git works  
- Download updates. `git pull`  
- Run again. `dotnet run -c Release`

[.netcore]: https://www.microsoft.com/net/download/core#/sdk
[ffmpeg]: http://ffmpeg.zeranoe.com/builds/
[git]: https://git-scm.com/downloads
[redis]: https://github.com/MicrosoftArchive/redis/releases/latest