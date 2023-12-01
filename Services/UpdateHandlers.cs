using BSUIRQueueTelegramBot.Data;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BSUIRQueueTelegramBot.Services
{
    public class UpdateHandlers
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandlers> _logger;
        private readonly MessageCommands _messageCommands;

        public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger, MessageCommands messageCommands)
        {
            _botClient = botClient;
            _logger = logger;
            _messageCommands = messageCommands;
        }
        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
                { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
                { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
                { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };
            await handler;
        }
        internal async Task HandleServiceRestriction(Update update, CancellationToken cancellationToken)
        {
            var chatId = update switch
            {
                { Message: { } message } => message.Chat.Id,
                { EditedMessage: { } message } => message.Chat.Id,
                { CallbackQuery: { } callbackQuery } => callbackQuery.Message.Chat.Id,
                _ => -1,
            };
            if (chatId < 0)
            {
                await UnknownUpdateHandlerAsync(update, cancellationToken);
                return;
            }
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "По пятницам бот недоступен для записи в очередь в связи с обновлением очереди\n"
                );
            
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            cancellationToken: cancellationToken);
            var handler = callbackQuery.Data switch
            {
                var str when new Regex(@"deleteSure:[a-zA-Zа-яА-Я]+$").IsMatch(str) => _messageCommands.DeleteFromQueue(callbackQuery, cancellationToken),
                var str when new Regex(@"delete:[a-zA-Zа-яА-Я]+$").IsMatch(str) => _messageCommands.DeleteConfirmation(callbackQuery, cancellationToken),
                var str when new Regex(@"show:[a-zA-Zа-яА-Я]+$").IsMatch(str) => _messageCommands.ShowSubjectInline(callbackQuery, cancellationToken),
                var str when new Regex(@"enter:[a-zA-Zа-яА-Я]+:\d+$").IsMatch(str) => _messageCommands.EnterQueue(callbackQuery, cancellationToken),
                var str when new Regex(@"enter:[a-zA-Zа-яА-Я]+$").IsMatch(str) => _messageCommands.EnterQueueManu(callbackQuery, cancellationToken),
                "menu" => _messageCommands.StartInline(callbackQuery.Message, cancellationToken),
                "enterQueue" => _messageCommands.enterQueueMenu(callbackQuery.Message, cancellationToken),
                "deleteQueue" => _messageCommands.deleteQueueMenu(callbackQuery.Message, cancellationToken),
                "showQueue" => _messageCommands.showQueueMenu(callbackQuery.Message, cancellationToken),
                _ => _messageCommands.SendError(callbackQuery.Message,cancellationToken)
            };
            await handler;

        }

        private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Receive message type: {MessageType}", message.Type);
            if (message.Text is not { } messageText)
                return;

            var action = messageText.Split(' ')[0] switch
            {
               
                "/start" => _messageCommands.Start(message, cancellationToken),
                _ => _messageCommands.Start(message, cancellationToken)
            }; 
        }
   
        private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
        private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

            List<InlineQueryResultArticle> results = new();
            foreach (var subject in Enum.GetNames(typeof(Subject)))
            {
                if (subject.Contains(inlineQuery.Query))
                {
                    Subject subjectEnum;
                    Enum.TryParse(subject, out subjectEnum);
                    string answer = _messageCommands.getQueryString(subjectEnum);
                    results.Add(new(
                    id: $"{subject}",
                    title: $"Очередь для {subject}",
                    inputMessageContent: new InputTextMessageContent(answer)
                    ));

                }
            }

            await _botClient.AnswerInlineQueryAsync(
                inlineQueryId: inlineQuery.Id,
                results: results,
                cacheTime: 0,
                isPersonal: true,
                cancellationToken: cancellationToken);
        }
        private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
        {
           

        }

        
    }
}
