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
    "TelegramChannel": "@your-channel-id"
},
```


## Documentation
If you want to learn more about the architecture and how the code works, make sure to check out the [Wiki](https://github.com/alinategh-js/notsoboringbot/wiki) page of this repo.

## Contribution
If you have any ideas for improving this project, feel free to create an issue in this repo and I will check it as soon as I can.
