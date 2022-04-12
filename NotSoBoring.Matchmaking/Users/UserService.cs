using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotSoBoring.Core.Enums;
using NotSoBoring.Core.Models;
using NotSoBoring.DataAccess;
using NotSoBoring.Domain.Enums;
using NotSoBoring.Domain.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotSoBoring.Matchmaking.Users
{
    public class UserService
    {
        private readonly MainDbContext _mainDb;
        private ConcurrentDictionary<long, UserState> _userStates;
        private ConcurrentDictionary<long, DateTimeOffset> _usersRecentActivity;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<UserService> _logger;

        public UserService(MainDbContext mainDb, ILogger<UserService> logger, IMemoryCache memoryCache)
        {
            _mainDb = mainDb;
            _logger = logger;
            _memoryCache = memoryCache;
            _userStates = new ConcurrentDictionary<long, UserState>();
            _usersRecentActivity = new ConcurrentDictionary<long, DateTimeOffset>();
        }

        public async Task AddUser(long userId)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                Enumerable
                   .Range(65, 26)
                    .Select(e => ((char)e).ToString())
                    .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                    .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                    .OrderBy(e => Guid.NewGuid())
                    .Take(11)
                    .ToList().ForEach(e => builder.Append(e));

                var newUser = new ApplicationUser
                {
                    Id = userId,
                    UniqueId = builder.ToString()
                };

                _mainDb.Users.Add(newUser);
                await _mainDb.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        public async Task EditUserNickname(long userId, string nickname)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Nickname = nickname;
            await _mainDb.SaveChangesAsync();
            RemoveUserInfoCache(userId);
        }

        public async Task EditAge(long userId, int age)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Age = age;
            await _mainDb.SaveChangesAsync();
            RemoveUserInfoCache(userId);
        }

        public async Task EditGender(long userId, GenderTypes gender)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Gender = gender;
            await _mainDb.SaveChangesAsync();
            RemoveUserInfoCache(userId);
        }

        public async Task EditPhoto(long userId, string fileId)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Photo = fileId;
            await _mainDb.SaveChangesAsync();
            RemoveUserInfoCache(userId);
        }

        public async Task EditLocation(long userId, double latitude, double longitude)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Latitude = latitude;
            user.Longitude = longitude;
            await _mainDb.SaveChangesAsync();
            RemoveUserInfoCache(userId);
        }

        public async Task<ApplicationUser> GetUser(long userId)
        {
            if(!_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserInfo(userId), out ApplicationUser user))
            {
                user = await _mainDb.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (user != null)
                    _memoryCache.Set(StringUtils.CacheSettings.Keys.UserInfo(userId), user);
            }

            return user;
        }

        public async Task<ApplicationUser> GetUser(string uniqueId)
        {
            var user = await _mainDb.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UniqueId == uniqueId);

            return user;
        }

        public UserState GetUserState(long userId)
        {
            UserState userState = UserState.InMenu;
            if (_userStates.TryGetValue(userId, out userState))
                return userState;

            _userStates.TryAdd(userId, userState);
            return userState;
        }

        public void ChangeUserState(long userId, UserState userState)
        {
            if (_userStates.TryGetValue(userId, out var currentState))
                _userStates[userId] = userState;

            else _userStates.TryAdd(userId, userState);
        }

        public void UpdateUserRecentActivity(long userId)
        {
            if (_usersRecentActivity.TryGetValue(userId, out var dateTime))
                _usersRecentActivity[userId] = DateTimeOffset.Now;

            else _usersRecentActivity.TryAdd(userId, DateTimeOffset.Now);
        }

        public DateTimeOffset? GetUserRecentActivity(long userId)
        {
            if (_usersRecentActivity.TryGetValue(userId, out var dateTime))
                return dateTime;

            else return null;
        }

        public async Task<bool> IsProfileCompleted(long userId)
        {
            if(!_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserInfo(userId), out ApplicationUser user))
                user = await GetUser(userId);

            if (user.Gender == null) return false;
            if (user.Nickname == null) return false;
            if (user.Age == null) return false;

            return true;
        }

        private void RemoveUserInfoCache(long userId)
        {
            if(_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserInfo(userId), out var user))
            {
                _memoryCache.Remove(StringUtils.CacheSettings.Keys.UserInfo(userId));
            }
        }
    }
}
