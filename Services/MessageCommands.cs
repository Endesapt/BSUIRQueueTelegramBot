using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using BSUIRQueueTelegramBot.Data;
using BSUIRQueueTelegramBot.Repository;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Threading;
using System.Runtime.CompilerServices;

namespace BSUIRQueueTelegramBot.Services
{
    public class MessageCommands
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandlers> _logger;
        private readonly RecordRepository _recordRepository;

        public MessageCommands(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger,RecordRepository recordRepository)
        {
            _botClient = botClient;
            _logger = logger;
            _recordRepository = recordRepository;
        }

        public async Task<Message> Start( Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Занять очередь", "enterQueue"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Отменить очередь", "deleteQueue"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Посмотреть очередь", "showQueue"),
                    },
                });
            return await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                replyMarkup : inlineKeyboard,
                text: "Выберете опцию",
                cancellationToken: cancellationToken);


        }
        public async Task<Message> StartInline(Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Занять очередь", "enterQueue"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Отменить очередь", "deleteQueue"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Посмотреть очередь", "showQueue"),
                    },
                });
            return await _botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: "Выберете опцию",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);


        }

        public async Task<Message> enterQueueMenu(Message message, CancellationToken cancellationToken)
        {

            List<InlineKeyboardButton[]> buttons=new();
            foreach(var subject in Enum.GetNames(typeof(Subject)))
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(subject,$"enter:{subject}") });
            }
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("<< Меню", "menu") });
            InlineKeyboardMarkup inlineKeyboard = new(buttons);
            await _botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: "Выберете предмет",
                cancellationToken: cancellationToken);
            return await _botClient.EditMessageReplyMarkupAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public async Task<Message> deleteQueueMenu(Message message, CancellationToken cancellationToken)
        {
            List<InlineKeyboardButton[]> buttons = new();
            foreach (var subject in Enum.GetNames(typeof(Subject)))
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(subject, $"delete:{subject}") });
            }
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("<< Меню", "menu") });
            InlineKeyboardMarkup inlineKeyboard = new(buttons);
            await _botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: "Выберете предмет",
                cancellationToken: cancellationToken);
            return await _botClient.EditMessageReplyMarkupAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public async Task<Message> showQueueMenu(Message message, CancellationToken cancellationToken)
        {
            List<InlineKeyboardButton[]> buttons = new();
            foreach (var subject in Enum.GetNames(typeof(Subject)))
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(subject, $"show:{subject}") });
            }
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("<< Меню", "menu") });
            InlineKeyboardMarkup inlineKeyboard = new(buttons);
            await _botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: "Выберете предмет",
                cancellationToken: cancellationToken);
            return await _botClient.EditMessageReplyMarkupAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }



        public async Task<Message> EnterQueueManu(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string userName = callbackQuery.From.Username;
            Subject subject=Subject.ОАиП; 
            Enum.TryParse(callbackQuery.Data.Split(':')[1],out subject);
            var records = _recordRepository.GetRecordList(subject);
            var record = records.FirstOrDefault(r => r.UserName == userName);
            if (record != null)
            {
                return await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"Вы уже стоите в очереди на этот предмет на месте {record.Place+1}"
                );
            }
            var isQueued = Enumerable.Repeat(false, 30).ToList();
            foreach (var r in records)
            {
                isQueued[r.Place] = true;
            }
            List<InlineKeyboardButton[]> buttons = new();
            int i = 0;
            List<InlineKeyboardButton> tempButtons = new();
            for (int j = 0; j < isQueued.Count; j++)
            {
                if (!isQueued[j])
                {
                    if (i == 3)
                    {
                        buttons.Add(tempButtons.ToArray());
                        tempButtons.Clear();
                        i = 0;
                    }
                    tempButtons.Add(InlineKeyboardButton.WithCallbackData($"{j+1}", $"enter:{subject}:{j}"));
                    i++;
                }
            }
            buttons.Add(tempButtons.ToArray());
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("<< Меню", "menu") });

            InlineKeyboardMarkup inlineKeyboard = new(buttons);

            await _botClient.EditMessageTextAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: "Выберете одно из свободных мест",
                cancellationToken: cancellationToken);
            return await _botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        public async Task<Message> EnterQueue(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string userName = callbackQuery.From.Username;
            var Info =callbackQuery.Data.Split(":");
            Subject subject = Subject.ОАиП;
            int place;
            if(!Enum.TryParse(Info[1], out subject)||
            !int.TryParse(Info[2], out place))
            {
                SendError(callbackQuery.Message, cancellationToken);
                return callbackQuery.Message;
            }
            Record? record=_recordRepository.getInLine(userName,subject, place);
            if(record == null)
            {
               await _botClient.SendTextMessageAsync(
                   chatId: callbackQuery.Message.Chat.Id,
                   text: $"Это место заняли пока вы выбирали его",

                   cancellationToken: cancellationToken);
            }
            else
            {
               await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Вы заняли {record.Place+1} место на предмете {subject}",
                cancellationToken: cancellationToken);
            }
            return await StartInline(callbackQuery.Message, cancellationToken);
            
        }
        

        public async  Task<Message> ShowSubjectInline(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            Subject subject = Subject.ОАиП;
            Enum.TryParse(callbackQuery.Data.Split(":")[1], out subject);
            string answer = getQueryString(subject);
            InlineKeyboardMarkup inline = new(new[] { InlineKeyboardButton.WithCallbackData("<< Меню", "menu") });
            return await _botClient.EditMessageTextAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: answer,
                replyMarkup: inline,
                cancellationToken: cancellationToken);

        }
        
        public async Task<Message> SendError(Message? message, CancellationToken cancellationToken)
        {
            return await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Такой функции или ответа не существует",
                cancellationToken: cancellationToken);
        }
        public string getQueryString(Subject subject)
        {
            var records = _recordRepository.GetRecordList(subject).ToList();
            string answer = "";
            if (records.Count == 0)
            {
                answer += $"Очередь на {subject} пуста\n";
            }
            else
            {
                answer += $"Очередь на предмет {subject}:\n";
                foreach (var record in records)
                {
                    answer += $"{record.Place + 1}. @{record.UserName}\n";
                }
            }
            return answer;
        }

        public async  Task<Message> DeleteConfirmation(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string subject = callbackQuery.Data.Split(":")[1];
            InlineKeyboardMarkup inline = new(new[] {
                InlineKeyboardButton.WithCallbackData("Да", $"deleteSure:{subject}"),
                InlineKeyboardButton.WithCallbackData("Нет", "menu"),

            });
            return await _botClient.EditMessageTextAsync(
               chatId: callbackQuery.Message.Chat.Id,
               messageId: callbackQuery.Message.MessageId,
               text: $"Вы уверены, что хотите покинуть очередь на предмете {subject}",
               replyMarkup: inline,
               cancellationToken: cancellationToken);


        }

        public async Task<Message> DeleteFromQueue(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            Subject subject = Subject.ОАиП;
            string userName=callbackQuery.From.Username;
            Enum.TryParse(callbackQuery.Data.Split(":")[1], out subject);
            Record deletedRecord = _recordRepository.getOutOfLine(userName, subject);
            if(deletedRecord != null)
            {
              await _botClient.SendTextMessageAsync(
              chatId: callbackQuery.Message.Chat.Id,
              text: $"Вы покинули очередь на предмете {subject}",
              cancellationToken: cancellationToken);
            }
            else
            {
              await _botClient.SendTextMessageAsync(
              chatId: callbackQuery.Message.Chat.Id,
              text: $"Вы не занимали очередь на предмете {subject}",
              cancellationToken: cancellationToken);
            }
            return await StartInline(callbackQuery.Message, cancellationToken);
        }
    }
}
