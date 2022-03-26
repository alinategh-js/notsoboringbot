using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotSoBoring.Core.Enums;
using NotSoBoring.Core.Models;
using NotSoBoring.DataAccess;
using NotSoBoring.Domain.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotSoBoring.Matchmaking.Users
{
    public class UserService
    {
        private readonly MainDbContext _mainDb;
        private ConcurrentDictionary<long, ApplicationUser> _users;
        private ConcurrentDictionary<long, UserState> _userStates;
        private readonly ILogger<UserService> _logger;

        public UserService(MainDbContext mainDb, ILogger<UserService> logger)
        {
            _mainDb = mainDb;
            _logger = logger;
            _userStates = new ConcurrentDictionary<long, UserState>();
        }

        public async Task AddUser(long userId)
        {
            try
            {
                var newUser = new ApplicationUser
                {
                    Id = userId,
                };

                _mainDb.Users.Add(newUser);
                await _mainDb.SaveChangesAsync();
                await FetchUsers();
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
            await FetchUsers();
        }

        public async Task EditAge(long userId, int age)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Age = age;
            await _mainDb.SaveChangesAsync();
            await FetchUsers();
        }

        public async Task EditGender(long userId, GenderTypes gender)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Gender = gender;
            await _mainDb.SaveChangesAsync();
            await FetchUsers();
        }

        public async Task EditPhoto(long userId, string fileId)
        {
            var user = await _mainDb.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Photo = fileId;
            await _mainDb.SaveChangesAsync();
            await FetchUsers();
        }

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            if (_users == null)
                await FetchUsers();

            return _users.Values.ToList();
        }

        public async Task<ApplicationUser> GetUser(long userId)
        {
            if (_users == null)
                await FetchUsers();

            if (_users.TryGetValue(userId, out var user))
                return user;
            else return null; // user does not exist
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

        private async Task FetchUsers()
        {
            var users = await _mainDb.Users.ToDictionaryAsync(x => x.Id, y => y);
            _users = new ConcurrentDictionary<long, ApplicationUser>(users);
        }
    }
}
