using NotSoBoring.DataAccess;
using Microsoft.Extensions.Caching.Memory;
using NotSoBoring.Domain.Utils;

namespace NotSoBoring.Matchmaking
{
    public class DirectMessageService
    {
        private readonly MainDbContext _mainDb;
        private readonly IMemoryCache _memoryCache;

        public DirectMessageService(MainDbContext mainDb, IMemoryCache memoryCache)
        {
            _mainDb = mainDb;
            _memoryCache = memoryCache;
        }

        public bool AddDirectMessageRequest(long userId, long targetUserId)
        {
            if (_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserSendingMessageToUserId(userId), out long targetUserId2))
                return false;

            _memoryCache.Set(StringUtils.CacheSettings.Keys.UserSendingMessageToUserId(userId), targetUserId);
            return true;
        }

        public bool GetTargetUserId(long userId, out long? targetUserId)
        {
            if (_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserSendingMessageToUserId(userId), out targetUserId))
                return true;

            return false;
        }
    }
}
