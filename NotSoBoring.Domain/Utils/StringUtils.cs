using System;

namespace NotSoBoring.Domain.Utils
{
    public static class StringUtils
    {
        private static TimeSpan OnlineTimeSpan = TimeSpan.FromMinutes(2);
        public static class Keyboard
        {
            public const string ConnectMeToAnAnonymous = "Connect to random person! 🎲";
            public const string CancelRequest = "Cancel request ❌";
            public const string SeeContactProfile = "See contact profile 📖";
            public const string CancelSession = "End chat ❌";
            public const string Profile = "My profile 👨";
            public const string SendMyLocation = "Send my location 📌";
            public const string CancelOperation = "Cancel operation ❌";
        }

        public static class InlineKeyboard
        {
            public const string EditProfile = "Edit profile 📝";
            public const string EditNickname = "Nickname 📝";
            public const string EditGender = "Gender 📝";
            public const string EditAge = "Age 📝";
            public const string EditProfilePhoto = "Photo 📝";
            public const string EditLocation = "Location 📌";

            public const string Male = "Male 👨";
            public const string Female = "Female 👩";

            public const string EndChat = "End chat ❌";
            public const string ContinueChat = "Continue chat ✔️";

            public const string OnlyMales = "Only males 👨";
            public const string OnlyFemales = "Only females 👩";
            public const string DontCareGender = "Either one 🎲";

            public const string AddToContacts = "Add to contacts ➕";
            public const string RemoveFromContacts = "Remove from contacts ➖";
            public const string MyContacts = "My contacts 👪";

            public const string NextPage = "Next page ➡️";
            public const string PreviousPage = "Previous page ⬅️";

            public const string SendDirectMessage = "Send direct message ✉️";
        }

        public static class InlineKeyboardCallbackData
        {
            // add to contacts
            public const string AddToContactsPrefix = "AddToContacts_";
            public static string AddToContacts(long targetUserId) => $"{AddToContactsPrefix}{targetUserId}";
            public static string GetAddToContactsContactId(string text) => text.Split("_")?[1];

            // remove from contacts
            public const string RemoveFromContactsPrefix = "RemoveFromContacts_";
            public static string RemoveFromContacts(long targetUserId) => $"{RemoveFromContactsPrefix}{targetUserId}";
            public static string GetRemoveFromContactsContactId(string text) => text.Split("_")?[1];

            // contacts list
            public const string ContactsListNextPagePrefix = "ContactsListNextPage_";
            public static string ContactsListNextPage(int pageNumber) => $"{ContactsListNextPagePrefix}{pageNumber}";
            public static string GetContactsListNextPageNumber(string text) 
            {
                try
                {
                    return text.Split("_")?[1];
                }
                catch
                {
                    return null;
                }
            }

            // send direct message
            public const string SendDirectMessagePrefix = "SendDirectMessage_";
            public static string SendDirectMessage(long targetUserId) => $"{SendDirectMessagePrefix}{targetUserId}";
            public static string GetSendDirectMessageTargetUserId(string text) => text.Split("_")?[1];
        }

        public static class Errors
        {
            public const string InvalidInput = "Invalid input, try again ❌";
            public const string CantEditProfileInSession = "You can't edit your profile while in chat ❌";
            public const string ProfileIsNotComplete = "You need to complete your profile info before doing that.";
            public const string CantAddContactsInSession = "You can't add contacts while in chat.";
            public const string NoMoreResultToShow = "No more results to show.";
        }

        public static class CacheSettings
        {
            public static class Keys
            {
                public static string UserInfo(long userId) => $"UserInfo_{userId}";
                public static string UserState(long userId) => $"UserState_{userId}";
                public static string UserContacts(long userId) => $"UserContacts_{userId}";
                public static string UserAddingContactId(long userId) => $"UserAddingContactId_{userId}";
                public static string UserSendingMessageToUserId(long userId) => $"UserSendingMessageToUserId_{userId}";
            }
        }

        public static class DirectMessage
        {
            public static string DirectMessageText(string text, string uniqueId)
            {
                return $"Direct message from: /user_{uniqueId} :\n" + 
                        text;
            }
        }

        public static string GetUserOnlineStatus(DateTimeOffset? lastActivity, bool isInSession)
        {
            var currentTime = DateTimeOffset.Now;
            string status = "";
            var timeSpan = currentTime - lastActivity;
            if (timeSpan < OnlineTimeSpan && lastActivity.HasValue)
                status = "Online ✔️";
            else
            {
                if (timeSpan == null)
                {
                    status = "Offline ❌";
                }
                else
                {
                    string timeSpanString = timeSpan.Value.ToReadableString();
                    status = $"(Last seen {timeSpanString} ago)";
                }
            }

            if (isInSession)
            {
                status = status + " (In Chat 🗣)";
            }

            return status;
        }
    }
}
