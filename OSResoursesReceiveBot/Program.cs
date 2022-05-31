using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using OSResoursesReceiveBot;

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

    if (messageText == "/start")
    {
        await botClient.SendTextMessageAsync(chatId: chatId, text: "Привет, я бот Олега для курсовой работы", cancellationToken: cancellationToken);
        return;
    }

    /*
    if (chatId != 428846415)
    {
        await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы не мой хозяин :)", cancellationToken: cancellationToken);
        return;
    }
    */

    if (messageText == "/alldata")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetOSData(), cancellationToken: cancellationToken);        
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetCPUData(), cancellationToken: cancellationToken);        
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetVideoData(), cancellationToken: cancellationToken);    
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetMemoryData(), cancellationToken: cancellationToken);        
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetRAMData(), cancellationToken: cancellationToken);
        
        stopwatch.Stop();
        Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
        return;
    }
    if (messageText == "/osdata")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetOSData(), cancellationToken: cancellationToken);
        stopwatch.Stop();
        Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
        return;
    }
    if (messageText == "/cpudata")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetCPUData(), cancellationToken: cancellationToken);
        stopwatch.Stop();
        Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
        return;
    }
    if (messageText == "/gpudata")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetVideoData(), cancellationToken: cancellationToken);
        stopwatch.Stop();
        Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
        return;
    }
    if (messageText == "/memorydata")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetMemoryData(), cancellationToken: cancellationToken);
        stopwatch.Stop();
        Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
        return;
    }
    if (messageText == "/ramdata")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await botClient.SendTextMessageAsync(chatId: chatId, text: ResoursesOS.GetRAMData(), cancellationToken: cancellationToken);
        stopwatch.Stop();
        Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
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