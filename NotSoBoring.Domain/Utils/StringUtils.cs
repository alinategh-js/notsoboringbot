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
            public const string CancelEdit = "لغو ویرایش ❌";
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
        }

        public static class Errors
        {
            public const string InvalidInput = "پیامی که فرستادی نامعتبره، دوباره تلاش کن ❌";
            public const string CantEditProfileInSession = "امکان ویرایش پروفایل هنگام چت وجود ندارد ❌";
            public const string ProfileIsNotComplete = "اطلاعات پروفایل شما کامل نیست. لطفا پس از تکمیل پروفایل خود دوباره تلاش کنید.";
        }

        public static class CacheSettings
        {
            public static class Keys
            {
                public static string UserInfo(long userId) => $"UserInfo_{userId}";
                public static string UserState(long userId) => $"UserState_{userId}";
                public static string UserContacts(long userId) => $"UserContacts_{userId}";
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
