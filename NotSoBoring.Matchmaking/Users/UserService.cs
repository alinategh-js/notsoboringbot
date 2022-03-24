using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotSoBoring.Core.Models;
using NotSoBoring.DataAccess;
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
        private readonly ILogger<UserService> _logger;

        public UserService(MainDbContext mainDb, ILogger<UserService> logger)
        {
            _mainDb = mainDb;
            _logger = logger;
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

        private async Task FetchUsers()
        {
            var users = await _mainDb.Users.ToDictionaryAsync(x => x.Id, y => y);
            _users = new ConcurrentDictionary<long, ApplicationUser>(users);
        }
    }
}
