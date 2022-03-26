using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NotSoBoring.Domain.DTOs;
using System.Collections.Concurrent;
using NotSoBoring.Matchmaking.Users;
using Telegram.Bot;
using NotSoBoring.Domain.Utils;
using NotSoBoring.Domain.Enums;

namespace NotSoBoring.Matchmaking
{
    public class MatchingEngine
    {
        private readonly ITelegramBotClient _botClient;
        private readonly CancellationToken _cancellationToken;
        private ConcurrentQueue<MatchRequest> _matchRequests;
        private ConcurrentDictionary<long, long> _matchedSessions;
        private readonly UserService _userService;

        public MatchingEngine(CancellationTokenSource cancellationTokenSource, UserService userService, ITelegramBotClient botClient)
        {
            _botClient = botClient;
            _matchRequests = new ConcurrentQueue<MatchRequest>();
            _matchedSessions = new ConcurrentDictionary<long, long>();
            _userService = userService;
            _cancellationToken = cancellationTokenSource.Token;
            Task.Factory.StartNew(async () => await Processor(), _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public bool TryAddRequest(MatchRequest request)
        {
            if (_matchRequests.Any(x => x.UserId == request.UserId && !x.IsCancelled))
                return false;

            _matchRequests.Enqueue(request);
            _userService.ChangeUserState(request.UserId, UserState.WaitingForMatch);
            return true;
        }

        public bool TryCancelRequest(long userId)
        {
            var request = _matchRequests.FirstOrDefault(x => x.UserId == userId && !x.IsCancelled);
            if (request != null)
            {
                request.IsCancelled = true;
                _userService.ChangeUserState(userId, UserState.InMenu);
                return true;
            }

            return false;
        }

        public bool TryCancelSession(long userId, out long secondUserId)
        {
            if(_matchedSessions.TryRemove(userId, out secondUserId) && _matchedSessions.TryRemove(secondUserId, out long firstUserId))
            {
                _userService.ChangeUserState(userId, UserState.InSession);
                return true;
            }

            return false;
        }

        public bool IsUserInSession(long userId)
        {
            long secondUserId;
            return _matchedSessions.TryGetValue(userId, out secondUserId)
                    && _matchedSessions.TryGetValue(secondUserId, out long firstUserId);
        }

        public bool IsUserInSession(long userId, out long secondUserId)
        {
            return _matchedSessions.TryGetValue(userId, out secondUserId)
                    && _matchedSessions.TryGetValue(secondUserId, out long firstUserId);
        }

        public bool IsUserWaitingForMatch(long userId)
        {
            return _matchRequests.Any(x => x.UserId == userId && !x.IsCancelled);
        }

        private async Task Processor()
        {
            MatchRequest[] requestsArray;
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_matchRequests.Count >= 2 && _matchRequests.TryDequeue(out var firstRequest) && !firstRequest.IsCancelled)
                {
                    // make an array from the requests queue so that it doesn't change until we are done processing firstRequest
                    requestsArray = _matchRequests.ToArray();
                    foreach (var request in requestsArray)
                    {
                        if (await IsMatched(request, firstRequest))
                        {
                            _matchedSessions.TryAdd(firstRequest.UserId, request.UserId);
                            _matchedSessions.TryAdd(request.UserId, firstRequest.UserId);
                            request.IsCancelled = true;
                            _userService.ChangeUserState(firstRequest.UserId, UserState.InSession);
                            _userService.ChangeUserState(request.UserId, UserState.InSession);
                            _ = Task.Run(() => NotifyUsers(firstRequest.UserId, request.UserId));
                            break;
                        }
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private async Task NotifyUsers(long firstUserId, long secondUserId)
        {
            string text = "به یک ناشناس وصل شدی. سلام کن!";
            var replyMarkup = ReplyMarkupFactory.GetInSessionKeyboardReplyMarkup();

            await _botClient.SendTextMessageAsync(chatId: firstUserId,
                                                      text: text,
                                                      replyMarkup: replyMarkup);

            await _botClient.SendTextMessageAsync(chatId: secondUserId,
                                                      text: text,
                                                      replyMarkup: replyMarkup);
        }

        private async Task<bool> IsMatched(MatchRequest first, MatchRequest second)
        {
            // checking cancellation status
            if (first.IsCancelled || second.IsCancelled)
                return false;

            var firstUser = await _userService.GetUser(first.UserId);
            var secondUser = await _userService.GetUser(second.UserId);

            // checking gender preferrence
            if ((first.PreferredGender != null && secondUser.Gender != first.PreferredGender) ||
                (second.PreferredGender != null && firstUser.Gender != second.PreferredGender))
                return false;

            return true;
        }
    }
}
