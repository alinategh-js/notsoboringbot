using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NotSoBoring.DataAccess;
using NotSoBoring.Domain.Models;
using NotSoBoring.Domain.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotSoBoring.Matchmaking
{
    public class ContactService
    {
        private readonly MainDbContext _mainDb;
        private readonly IMemoryCache _memoryCache;

        public ContactService(MainDbContext mainDb, IMemoryCache memoryCache)
        {
            _mainDb = mainDb;
            _memoryCache = memoryCache;
        }

        public async Task<bool> AddUserToContacts(long userId, long contactId, string contactName)
        {
            try
            {
                var contact = await _mainDb.Contacts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.ContactId == contactId);

                if (contact != null)
                    return false;
                
                var newContact = new ApplicationContact
                {
                    UserId = userId,
                    ContactId = contactId,
                    ContactName = contactName
                };

                _mainDb.Contacts.Add(newContact);
                await _mainDb.SaveChangesAsync();

                ClearUserContactsCache(userId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveUserFromContacts(long userId, long contactId)
        {
            try
            {
                var contact = await _mainDb.Contacts.FirstOrDefaultAsync(x => x.UserId == userId && x.ContactId == contactId);
                if (contact == null)
                    return false;

                _mainDb.Contacts.Remove(contact);
                await _mainDb.SaveChangesAsync();

                ClearUserContactsCache(userId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ApplicationContact>> GetUserContacts(long userId)
        {
            if (!_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserContacts(userId), out List<ApplicationContact> contacts))
            {
                contacts = await _mainDb.Contacts
                    .AsNoTracking()
                    .Where(x => x.UserId == userId)
                    .Include(x => x.ContactUser)
                    .ToListAsync();

                _memoryCache.Set(StringUtils.CacheSettings.Keys.UserContacts(userId), contacts);
            }

            return contacts;
        }

        public async Task<bool> IsUserInContacts(long userId, long contactId)
        {
            var contacts = await GetUserContacts(userId);

            return contacts.Any(x => x.UserId == userId && x.ContactId == contactId);
        }

        public bool AddUserToContactsRequest(long userId, long contactId)
        {
            if (_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserAddingContactId(userId), out long targetContactId))
                return false;

            _memoryCache.Set(StringUtils.CacheSettings.Keys.UserAddingContactId(userId), contactId);
            return true;
        }

        public bool GetContactIdFromRequests(long userId, out long targetContactId)
        {
            return _memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserAddingContactId(userId), out targetContactId);
        }

        private void ClearUserContactsCache(long userId)
        {
            if (_memoryCache.TryGetValue(StringUtils.CacheSettings.Keys.UserContacts(userId), out List<ApplicationContact> contacts))
            {
                _memoryCache.Remove(StringUtils.CacheSettings.Keys.UserContacts(userId));
            }
        }
    }
}