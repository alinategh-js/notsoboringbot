using System;

namespace NotSoBoring.Domain.Utils
{
    public static class StringUtils
    {
        private static TimeSpan OnlineTimeSpan = TimeSpan.FromMinutes(2);
        public static class Keyboard
        {
            public const string ConnectMeToAnAnonymous = "به یه ناشناس وصلم کن! 🎲";
            public const string CancelRequest = "لغو درخواست ❌";
            public const string SeeContactProfile = "مشاهده پروفایل مخاطب 📖";
            public const string CancelSession = "اتمام چت ❌";
            public const string Profile = "مشاهده پروفایل 👨";
            public const string SendMyLocation = "فرستادن لوکیشن من 📌";
            public const string CancelOperation = "لغو عملیات ❌";
        }

        public static class InlineKeyboard
        {
            public const string EditProfile = "ویرایش اطلاعات پروفایل 📝";
            public const string EditNickname = "نام مستعار 📝";
            public const string EditGender = "جنسیت 📝";
            public const string EditAge = "سن 📝";
            public const string EditProfilePhoto = "عکس 📝";
            public const string EditLocation = "لوکیشن 📌";

            public const string Male = "پسر 👨";
            public const string Female = "دختر 👩";

            public const string EndChat = "اتمام چت ❌";
            public const string ContinueChat = "ادامه چت ✔️";

            public const string OnlyMales = "فقط پسر 👨";
            public const string OnlyFemales = "فقط دختر 👩";
            public const string DontCareGender = "فرقی نداره 🎲";

            public const string AddToContacts = "اضافه کردن به مخاطبین ➕";
            public const string RemoveFromContacts = "حذف کردن از مخاطبین ➖";
            public const string MyContacts = "لیست مخاطبین من 👪";

            public const string NextPage = "صفحه بعد ➡️";
            public const string PreviousPage = "صفحه قبل ⬅️";

            public const string SendDirectMessage = "ارسال پیام دایرکت ✉️";
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
            public const string InvalidInput = "پیامی که فرستادی نامعتبره، دوباره تلاش کن ❌";
            public const string CantEditProfileInSession = "امکان ویرایش پروفایل هنگام چت وجود ندارد ❌";
            public const string ProfileIsNotComplete = "اطلاعات پروفایل شما کامل نیست. لطفا پس از تکمیل پروفایل خود دوباره تلاش کنید.";
            public const string CantAddContactsInSession = "امکان افزودن مخاطب در هنگام چت وجود ندارد.";
            public const string NoMoreResultToShow = "نتیجه دیگری برای نمایش وجود ندارد.";
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
                return $"پیام دایرکت از طرف /user_{uniqueId} :\n" + 
                        text;
            }
        }

        public static string GetUserOnlineStatus(DateTimeOffset? lastActivity, bool isInSession)
        {
            var currentTime = DateTimeOffset.Now;
            string status = "";
            var timeSpan = currentTime - lastActivity;
            if (timeSpan < OnlineTimeSpan && lastActivity.HasValue)
                status = "آنلاین ✔️";
            else
            {
                if (timeSpan == null)
                {
                    status = "آفلاین ❌";
                }
                else
                {
                    string timeSpanString = timeSpan.Value.ToReadableString();
                    status = $"({timeSpanString} پیش آنلاین بوده)";
                }
            }

            if (isInSession)
            {
                status = status + " (در حال چت کردن 🗣)";
            }

            return status;
        }
    }
}
