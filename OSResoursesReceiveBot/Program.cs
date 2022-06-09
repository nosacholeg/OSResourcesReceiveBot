using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OSResourcesReceiveBot
{
    class Programm
    {
        static async Task Main()
        {
            string token = System.IO.File.ReadAllText(@"..\..\..\..\..\token.txt");
            var botClient = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            /// <summary>
            /// Инициализация комманд бота
            /// </summary>
            List<BotCommand> commands = new();
            commands.Add(new BotCommand { Command = "alldata", Description = "receive all host OS resources info" });
            commands.Add(new BotCommand { Command = "osdata", Description = "receive OS info" });
            commands.Add(new BotCommand { Command = "cpudata", Description = "receive processor info" });
            commands.Add(new BotCommand { Command = "gpudata", Description = "receive video processor info" });
            commands.Add(new BotCommand { Command = "memorydata", Description = "receive memory info" });
            commands.Add(new BotCommand { Command = "ramdata", Description = "receive physical memory info" });
            commands.Add(new BotCommand { Command = "help", Description = "bot commands info" });
            await botClient.SetMyCommandsAsync(commands);

            /// <summary>
            /// Инициализация клавиатуры бота
            /// </summary>
            List<KeyboardButton>[] keyboard = new List<KeyboardButton>[4]
            {
                new List<KeyboardButton>{ new KeyboardButton("All data"), new KeyboardButton("About OS") },
                new List<KeyboardButton>{ new KeyboardButton("About CPU"), new KeyboardButton("About GPU") },
                new List<KeyboardButton>{ new KeyboardButton("About memory"), new KeyboardButton("About RAM") },
                new List<KeyboardButton>{ new KeyboardButton("help") }
            };
            ReplyKeyboardMarkup ReplyKeyboard = new(keyboard);
            ReplyKeyboard.ResizeKeyboard = true;
            ReplyKeyboard.OneTimeKeyboard = true;
            ReplyKeyboard.InputFieldPlaceholder = null;


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

                if (messageText == "/start" || messageText == "start")
                {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Привет, я бот Олега для курсовой работы", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    return;
                }

                if (chatId != 428846415)
                {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы не мой хозяин :)", cancellationToken: cancellationToken);
                    return;
                }

                if (messageText == "/alldata" || messageText == "All data")
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetOSData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetCPUData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetVideoData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetMemoryData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetRAMData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    stopwatch.Stop();
                    Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
                    return;
                }
                if (messageText == "/osdata" || messageText == "About OS")
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetOSData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    stopwatch.Stop();
                    Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
                    return;
                }
                if (messageText == "/cpudata" || messageText == "About CPU")
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetCPUData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    stopwatch.Stop();
                    Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
                    return;
                }
                if (messageText == "/gpudata" || messageText == "About GPU")
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetVideoData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    stopwatch.Stop();
                    Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
                    return;
                }
                if (messageText == "/memorydata" || messageText == "About memory")
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetMemoryData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    stopwatch.Stop();
                    Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
                    return;
                }
                if (messageText == "/ramdata" || messageText == "About RAM")
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ResourcesOS.GetRAMData(), cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    stopwatch.Stop();
                    Console.WriteLine("sent info in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 3).ToString() + " seconds");
                    return;
                }
                if (messageText == "/help" || messageText == "help")
                {
                    string help = "ggg";
                    await botClient.SendTextMessageAsync(chatId: chatId, text: help, cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
                    return;
                }

                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: "incorrect input", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard);
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
        }
    }
}
