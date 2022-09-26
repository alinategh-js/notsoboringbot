# NotSoBoringBot
NotSoBoringBot is a Telegram Bot that connects random users to chat with each other. This project was once launched for free use for testing purposes of the application and gained **+1000 users** at the time. But at the moment it is no longer online anywhere and it is just an open-source reference for those who want to learn how to build these kind of Telegram bots.

## Overview
This repo contains code for a Telegram Bot that connects random users to chat with each other. If you're not sure what a Bot is in Telegram, check out [Introduction to Bots](https://core.telegram.org/bots) for more information.
<br /><br />
### Technology Highlights
Here is a list of technologies used for developing this Bot:
* [.NET 5](https://github.com/dotnet/core)
* [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot)
* [Entity Framework Core](https://github.com/dotnet/efcore)

## Local development
These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

#### Create a Telegram bot
First thing you need to do is to create a Telegram bot using [BotFather](https://core.telegram.org/bots#6-botfather). After creating one, your bot will have a **Token** which you need to keep secured because that token is used to control your bot.

#### Create a Telegram channel (optional)
There is a feature in this bot that is optional. You can create a Telegram channel if you want and require your users to be a member of that channel in order to use the bot. If you don't know what a Telegram channel is check out [Channels FAQ](https://telegram.org/faq_channels) to learn more.

#### Setup ngrok
[ngrok](https://ngrok.com/) exposes local networked services behinds NATs and firewalls to the public internet over a secure tunnel. Share local websites, build/test webhook consumers and self-host personal services.
<br />
In order to use ngrok for development of our bot, you need to download and install it first from [here](https://ngrok.com/download). After you're done with the installation you run this command on your cmd:
```bash
ngrok http [PORT_NUMBER]
```
replace [PORT_NUMBER] with the port number of your bot application after you run it locally (described below).

#### Setup SQL Server
You need to have [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) on your system and create a Database called "NotSoBoringBot". The database is used to save bot users profile data.

#### Clone the Repo
```bash
git clone https://github.com/alinategh-js/notsoboringbot.git
```

#### Run the project
Now you need to open the solution file in Visual Studio. From the Solution Explorer find NotSoBoring.WebHook project, then open **appsettings.Development.json**. You need to change BotToken and insert your own bot's Token:
```json
"BotConfiguration": {
    "BotToken": "your-telegram-bot-token",
    "HostAddress": "https://your-ngrok-path.ngrok.io",
    "CertificatePublicKeyPath": "", // ignore this
    "TelegramChannel": "@your-channel-id-if-any"
},
```
You can leave the TelegramChannel as an empty string if you don't want to have a channel.
Now build and run the project in Debug mode and try to send a text to your bot on your Telegram app to see it work.

## Documentation
If you want to learn more about the architecture and how the code works, make sure to check out the [Wiki](https://github.com/alinategh-js/notsoboringbot/wiki) page of this repo.

## Contribution
If you have any ideas for improving this project, feel free to create an issue in this repo and I will check it as soon as I can.
