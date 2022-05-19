using System;
using System.Management;
using System.Runtime.Versioning;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

string token = System.IO.File.ReadAllText(@"..\..\..\..\..\token.txt");
var botClient = new TelegramBotClient(token);

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    errorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    
    if (update.Type != UpdateType.Message)
        return;
    // Only process text messages
    if (update.Message!.Type != MessageType.Text)
        return;

    var chatId = update.Message.Chat.Id;
    var messageText = update.Message.Text;
    var username = update.Message.Chat.Username;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {username}");

    if (messageText == "/start") return;
    if (messageText == "/getdata")
    {
        await botClient.SendTextMessageAsync(chatId: chatId, text: GetData(), cancellationToken: cancellationToken);
        return;
    }

    Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: "incorrect input", cancellationToken: cancellationToken);
    Console.WriteLine($"Sent a 'incorrect input' message in chat {chatId} to {username}");
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

string GetData()
{
    string data = "";
    data += GetResultString("Процессор:\n", GetInfo("Win32_Processor", "Name"));
    return data;
}

static string GetResultString(string info, List<string> result)
{
    string data = "";
    if (info.Length > 0) data += info;

    if (result.Count > 0)
    {
        for (int i = 0; i < result.Count; ++i) data += result[i];
    }
    return data;
}
static List<string> GetInfo(string device, string property)
{
    List<string> result = new();

    ManagementObjectSearcher searcher = new("SELECT * FROM " + device);

    try
    {
        foreach (ManagementObject obj in searcher.Get())
        {
            result.Add(obj[property].ToString().Trim());
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
    return result;
}