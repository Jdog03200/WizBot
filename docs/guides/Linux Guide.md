## Setting up WizBot on Linux

#### Setting up WizBot on Linux Digital Ocean Droplet
If you want WizBot to play music for you 24/7 without having to hosting it on your PC and want to keep it cheap, reliable and convenient as possible, you can try Wiz on Linux Digital Ocean Droplet using the link [DigitalOcean](https://m.do.co/c/7290047d0c84/) (and using this link will be supporting Wiz and will give you **$10 credit**)

#### Setting up WizBot
Assuming you have followed the link above to setup an account and Droplet with 64bit OS in Digital Ocean and got the `IP address and root password (in email)` to login, its time to get started.

**Go through this whole guide before setting up WizBot**

#### Prerequisites
- Download [PuTTY](http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html)
- Download [WinSCP](https://winscp.net/eng/download.php) *(optional)*

#### Starting up

- **Open PuTTY.exe** that you downloaded before, and paste or enter your `IP address` and then click **Open**.
If you entered your Droplets IP address correctly, it should show **login as:** in a newly opened window.
- Now for **login as:**, type `root` and hit enter.
- It should then, ask for password, type the `root password` you have received in your **email address registered with Digital Ocean**, then hit Enter.

*as you are running it for the first time, it will most likely to ask you to change your root password, for that, type the "password you received through email", hit Enter, enter a "new password", hit Enter and confirm that "new password" again.*
**SAVE that new password somewhere safe, not just in your mind**. After you've done that, you are ready to write commands.

**NOTE:** Copy the commands, and just paste them using **mouse single right-click.**

#### Creating and Inviting bot

- Read here how to [create a DiscordBot application](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#creating-discordbot-application)
- [Visual Invite Guide](http://discord.kongslien.net/guide.html) **(Note: Client ID is your Bot ID)**
- Copy your `Client ID` from your [applications page](https://discordapp.com/developers/applications/me).
- Replace the **12345678** in this link: 	
`https://discordapp.com/oauth2/authorize?client_id=`12345678`&scope=bot&permissions=66186303`		
 with your `Client ID`
- The link should now look like this: 	
`https://discordapp.com/oauth2/authorize?client_id=`**YOUR_CLENT_ID_HERE**`&scope=bot&permissions=66186303`
- Go to the newly created link and pick the server we created, and click `Authorize`
- The bot should have been added to your server.

#### Getting WizBot
##### Part I - Downloading the installer
Use the following command to get and run `linuxAIO.sh`		
(Remember **Do Not** rename the file **linuxAIO.sh**)

`cd ~ && wget -N https://github.com/Wizkiller96/WizBot-BashScript/raw/1.4/linuxAIO.sh && bash linuxAIO.sh`

You should see these following options after using the above command:

```
1. Download WizBot
2. Run WizBot (Normally)
3. Run WizBot with Auto Restart (Run WizBot normally before using this.)
4. Auto-Install Prerequisites (for Ubuntu, Debian and CentOS)
5. Set up credentials.json (if you have downloaded the bot already)
6. To exit
```
##### Part II - Downloading WizBot prerequisites

**If** you are running WizBot for the first time on your system and never had any *prerequisites* installed and have Ubuntu, Debian or CentOS, Press `4` and `enter` key, then `y` when you see the following:
```
Welcome to WizBot Auto Prerequisites Installer.
Would you like to continue?
```
That will install all the prerequisites your system need to run WizBot.

(Optional) **If** you want to install it manually, you can try finding it [here](https://github.com/Wizkiller96/WizBot-BashScript/blob/1.4/wizbotautoinstaller.sh)

Once *prerequisites* finish installing,

##### Part III - Installing WizBot
Choose `1` to get the **most updated build of WizBot** 

and then press `enter` key.	

Once installation is completed you should see the options again.

Next, check out:
##### Part IV - Setting up credentials

- [1. Setting up credentials.json](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#setting-up-credentialsjson)
- [2. To Get the Google API](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-wizbot-for-music)
- [3. JSON Explanations for other APIs](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/)

You will need the following for the next step:
![botimg](https://cdn.discordapp.com/attachments/251504306010849280/276455844223123457/Capture.PNG)

- **Bot's Client ID** and **Bot's ID** (both are same) [(*required)](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-credentialsjson-file)
- **Bot's Token** (not client secret) [(*required)](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-credentialsjson-file)
- Your **Discord userID** [(*required)](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-credentialsjson-file)
- **Google Api Key** [(optional)](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-wizbot-for-music)
- **LoL Api Key** [(optional)](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/)
- **Mashape Key** [(optional)](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/)
- **Osu Api Key** [(optional)](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/)
- **Sound Cloud Client Id** [(optional)](http://wizbot.readthedocs.io/en/latest/JSON%20Explanations/)

Once you have acquired them, press `5` to **Set up credentials.json**

You will be asked to enter the required informations, just follow the on-screen instructions and enter the required information.		
*i.e* If you are asked **Bot's Token**, then just copy and paste or type the **Bot's Token** and press `enter` key.

(If you want to skip any optional infos, just press `enter` key without typing/pasting anything.)		
Once done,		
##### Part V - Checking if WizBot is working
You should see the options again.	
Next, press `2` to **Run WizBot (Normally)**.
Check in your discord server if your new bot is working properly.	
##### Part VI - Running WizBot on tmux
If your bot is working properly in your server, type `.die` to **shut down the bot**, then press `6` on the console to **exit**.
Next, [Run your bot again with **tmux**.](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#running-wizbot)	

[Check this when you need to **restart** your **WizBot** anytime later along with tmux session.](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#restarting-wizbot)

#### Running WizBot

**Create a new Session:**

- `tmux new -s wizbot`  
  
The above command will create a new session named **wizbot** *(you can replace “wizbot” with anything you prefer and remember its your session name)* so you can run the bot in background without having to keep the PuTTY running.

**Next, we need to run `linuxAIO.sh` in order to get the latest running scripts with patches:**

- `cd ~ && wget -N https://github.com/Wizkiller96/WizBot-BashScript/raw/1.4/linuxAIO.sh && bash linuxAIO.sh`

**From the options,**

Choose `2` to **Run WizBot normally.**		
**NOTE:** With option `2` (Running normally), if you use `.die` [command](http://wizbot.readthedocs.io/en/latest/Commands%20List/#administration) in discord. The bot will shut down and will stay offline until you manually run it again. (best if you want to check the bot.)

Choose `3` to **Run WizBot with Auto Restart.**	
**NOTE:** With option `3` (Running with Auto Restart), bot will auto run if you use `.die` [command](http://wizbot.readthedocs.io/en/latest/Commands%20List/#administration) making the command `.die` to function as restart.	

It will show you the following options: 
```
1. Run Auto Restart normally without Updating.
2. Run Auto Restart and update WizBot.
3. Exit
```

- With option `1. Run Auto Restart normally without Updating.` Bot will restart on `die` command and will not be downloading the latest build available.
- With option `2. Run Auto Restart and update WizBot.` Bot will restart and download the latest build of bot available everytime `die` command is used.

**Remember** that, while running with Auto Restart, you will need to [close the tmux session](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#restarting-wizbot) to stop the bot completely.

**Now check your Discord, the bot should be online**

Next to **move the bot to background** and to do that, press **CTRL+B, release, D** (that will detach the wizbot session using TMUX) and you can finally close **PuTTY**.

#### Restarting WizBot

**Restarting WizBot:**

**If** you have chosen option `2` to **Run WizBot with Auto Restart** from WizBot's `linuxAIO.sh` *[(you got it from this step)](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#getting-wizbot)*	
You can simply type `.die` in the server you have your WizBot to make her restart.

**Restarting WizBot with the Server:**

Open **PuTTY** and login as you have before, type `reboot` and hit Enter.

**Restarting Manually:**

- Kill your previous session, check with `tmux ls`
- `tmux kill-session -t wizbot` (don't forget to replace "wizbot" to what ever you named your bot's session)
- [Run the bot again.](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#running-wizbot)

#### Updating WizBot

- Connect to the terminal through **PuTTY**.
- `tmux kill-session -t wizbot` (don't forget to replace **wizbot** in the command with the name of your bot's session)
- Make sure the bot is **not** running.
- `tmux new -s wizbot` (**wizbot** is the name of the session)
- `cd ~ && wget -N https://github.com/Wizkiller96/WizBot-BashScript/raw/1.4/linuxAIO.sh && bash linuxAIO.sh`
- Choose `1` to update the bot with **latest build** available.
- Next, choose either `2` or `3` to run the bot again with **normally** or **auto restart** respectively.
- Done.

#### Setting up Music

To set up WizBot for music and Google API Keys, follow [Setting up WizBot for Music](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-wizbot-for-music)

Once done, go back to **PuTTY**

#### Some more Info

##### Info about tmux

- If you want to **see the sessions** after logging back again, type `tmux ls`, and that will give you the list of sessions running.
- If you want to **switch to/ see that session**, type `tmux a -t wizbot` (**wizbot** is the name of the session we created before so, replace **“wizbot”** with the session name you created.)
- If you want to **kill** WizBot **session**, type `tmux kill-session -t wizbot`

#### Guide for Advance Users (Optional)

**Skip this step if you are a Regular User or New to Linux.**

[![img7][img7]](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#getting-wizbot)

- Right after [Getting WizBot](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#getting-wizbot)
- `cd WizBot/src/WizBot/` (go to this folder)
- `pico credentials.json` (open credentials.json to edit)
- Insert your bot **Client ID, Bot ID** (should be same as your Client ID) **and Token** if you got it following [Creating and Inviting bot](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#creating-and-inviting-bot).
- Insert your own ID in Owners ID follow: [Setting up credentials.json](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-credentialsjson-file)
- And Google API from [Setting up WizBot for Music](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-wizbot-for-music)
- Once done, press `CTRL+X`
- It will ask for "Save Modified Buffer?", press `Y` for yes
- It will then ask "File Name to Write" (rename), just hit `Enter` and Done.
- You can now move to [Running WizBot](http://wizbot.readthedocs.io/en/latest/guides/Linux%20Guide/#running-wizbot)

#### Setting up SFTP

- Open **WinSCP**
- Click on **New Site** (top-left corner).
- On the right-hand side, you should see **File Protocol** above a drop-down selection menu.
- Select **SFTP** *(SSH File Transfer Protocol)* if its not already selected.
- Now, in **Host name:** paste or type in your `Digital Ocean Droplets IP address` and leave `Port: 22` (no need to change it).
- In **Username:** type `root`
- In **Password:** type `the new root password (you changed at the start)`
- Click on **Login**, it should connect.
- It should show you the WizBot folder which was created by git earlier on the right-hand side window.
- Open that folder, then open the `src` folder, followed by another `WizBot` folder and you should see `credentials.json` there.

#### Setting up credentials.json

- Copy the `credentials.json` to desktop
- EDIT it as it is guided here: [Setting up credentials.json](http://wizbot.readthedocs.io/en/latest/guides/Windows%20Guide/#setting-up-credentialsjson-file)
- Paste/put it back in the folder once done. `(Using WinSCP)`
- **If** you already have WizBot 1.3.x setup and have `credentials.json` and `WizBot.db`, you can just copy and paste the `credentials.json` to `WizBot/src/WizBot` and `WizBot.db` to `WizBot/src/WizBot/bin/Release/netcoreapp1.1/data` using WinSCP.
- **If** you have WizBot 0.9x follow the [Upgrading Guide](http://wizbot.readthedocs.io/en/latest/guides/Upgrading%20Guide/)


[img7]: https://cdn.discordapp.com/attachments/251504306010849280/251505766370902016/setting_up_credentials.gif