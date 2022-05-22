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

    if (messageText == "/start")
    {
        await botClient.SendTextMessageAsync(chatId: chatId, text: "Привет, я бот Олега для курсачa", cancellationToken: cancellationToken);
        return;
    }

    if (chatId != 428846415)
    {
        await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы не мой хозяин :)", cancellationToken: cancellationToken);
        return;
    }

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

string GetData()
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

string GetVideoData()
{
    string info = "Информация о видеоадаптерах\n";
    ManagementObjectSearcher searcher = new("SELECT * FROM Win32_VideoController");

    info += "Текущее разрешение:" + GetInfo(searcher, "CurrentHorizontalResolution")[0];
    info += "x" + GetInfo(searcher, "CurrentVerticalResolution")[0] + '\n';
    info += GetResultString("Битов на пиксель: ", GetInfo(searcher, "CurrentBitsPerPixel"));
    info += "Количество цветов: " + Math.Round(Convert.ToDouble(GetInfo(searcher, "CurrentNumberOfColors")[0]) / 1_000_000_000, 1) + " млрд\n";
    int count = GetInfo(searcher, "Name").Count;
    string[] data = new string[count];

    for(int i = 0; i < count; i++)
    {
        data[i] = "\nИнформация о видеоадаптере " + (i+1) + ":\n";

        data[i] += "Имя процессора:\n" + GetInfo(searcher, "Name")[i] + '\n';
        data[i] += "Версия драйвера:\n" + GetInfo(searcher, "DriverVersion")[i] + '\n';
        data[i] += "Размер памяти видеоадаптера: " + Math.Round(Convert.ToDouble(GetInfo(searcher, "AdapterRAM")[i])/1024/1024/1024) +"Gb" + '\n';
        data[i] += "Тип архитектуры видео: ";
        switch(Convert.ToInt16(GetInfo(searcher, "VideoArchitecture")[i]))
        {
            case 1: data[i] += "Другое"; break;
            case 2: data[i] += "Неизвестно"; break;
            case 3: data[i] += "CGA"; break;
            case 4: data[i] += "EGA"; break;
            case 5: data[i] += "VGA"; break;
            case 6: data[i] += "SVGA"; break;
            case 7: data[i] += "MDA"; break;
            case 8: data[i] += "HGC"; break;
            case 9: data[i] += "MCGA"; break;
            case 10: data[i] += "8514A"; break;
            case 11: data[i] += "XGA"; break;
            case 12: data[i] += "Линейный буфер кадров"; break;
            case 160: data[i] += "PC-98"; break;
        }
        data[i] += "\n";
    }

    for (int i = 0; i < count; i++)
    {
        info += data[i];
    }
    return info;
}

string GetMemoryData()
{
    string info = "Информация о памяти:\n\n";
    ManagementObjectSearcher searcher = new("SELECT * FROM Win32_DiskDrive");
    info += GetResultString("Имя модуля памяти:\n", GetInfo(searcher, "Caption"));
    info += GetResultString("Описание:\n", GetInfo(searcher, "Description"));
    info += "Размер накопителя: "+ Math.Round(Convert.ToDouble(GetInfo(searcher, "Size")[0])/1024/1024/1024)+"Gb\n";
    info += GetResultString("Тип интерфейса:", GetInfo(searcher, "InterfaceType"));
    info += GetResultString("Тип носителя:\n", GetInfo(searcher, "MediaType"));

    info += "\nИнформация о разделах:";
    DriveInfo[] drives = DriveInfo.GetDrives();

    foreach (DriveInfo d in drives)
    {
        if (d.IsReady == true)
        {
            info += "\n\nРаздел " + d.Name;
            info += "\nTип диска: " + d.DriveType;
            info += "\nОбъем памяти: " + Math.Round(Convert.ToDouble(d.TotalSize) / 1024 / 1024 / 1024, 3) + "Gb";
            info += "\nOбъем свободного места: " + Math.Round(Convert.ToDouble(d.TotalFreeSpace) / 1024 / 1024 / 1024, 3) + "Gb";
        }
    }
    return info;
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