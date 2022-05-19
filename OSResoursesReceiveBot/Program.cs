using System.Management;
using System.Diagnostics;
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
        Stopwatch stopwatch = Stopwatch.StartNew();
        await botClient.SendTextMessageAsync(chatId: chatId, text: GetCPUData(), cancellationToken: cancellationToken);
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

string GetCPUData()
{
    ManagementObjectSearcher searcher = new("SELECT * FROM Win32_Processor");
    string data = "Информация о процессоре:\n";
    data += GetResultString("Имя процессора:\n", GetInfo(searcher, "Name"));
    data += GetResultString("Описание:\n", GetInfo(searcher, "Description"));
    data += GetResultString("Имя устройства:\n", GetInfo(searcher, "SystemName"));
    data += GetResultString("Разрядность системы:", GetInfo(searcher, "AddressWidth"));
    
    data += "Архитектура процессора:";
    /*Определение архитектуры процессора*/{
        string str = GetResultString("", GetInfo(searcher, "Architecture"));
        switch (str.Replace("\n", ""))
        {
            case "0": data += "x86"; break;
            case "1": data += "MIPS"; break;
            case "2": data += "Alpha"; break;
            case "3": data += "PowerPC"; break;
            case "5": data += "ARM"; break;
            case "6": data += "ia64"; break;
            case "9": data += "x64"; break;
            case "12": data += "ARM64"; break;
        }
    }/*Определение архитектуры процессора*/
    data += "\n";
    data += GetResultString("Количество ядер:", GetInfo(searcher, "NumberOfCores"));
    data += GetResultString("Количество потоков:", GetInfo(searcher, "ThreadCount"));

    
    List<string> list = new();
    list = GetInfo(searcher, "CurrentClockSpeed");
    data += "Текущая частота процессора:" + Convert.ToDouble(list[0]) / 1000 + "GHz\n";
    list = GetInfo(searcher, "MaxClockSpeed");
    data += "Макс. частота процессора:" + Convert.ToDouble(list[0]) / 1000 + "GHz\n";
    
    return data;
}

static string GetResultString(string info, List<string> result)
{
    string data = "";
    if (info.Length > 0) data += info;

    if (result.Count > 0)
    {
        for (int i = 0; i < result.Count; ++i) data += result[i] + '\n';
    }
    return data;
}
static List<string> GetInfo(ManagementObjectSearcher searcher, string property)
{
    List<string> result = new();
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