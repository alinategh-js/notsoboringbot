using System;
using System.Collections.Generic;
using System.Text;

namespace NotSoBoring.Domain.Utils
{
    public static class StringUtils
    {
        public static class Keyboard
        {
            public const string ConnectMeToAnAnonymous = "به یه ناشناس وصلم کن! 🎲";
            public const string CancelRequest = "لغو درخواست ❌";
            public const string CancelSession = "اتمام چت ❌";
            public const string Profile = "مشاهده پروفایل 👨";
        }

        public static class InlineKeyboard
        {
            public const string EditProfile = "ویرایش اطلاعات پروفایل 📝";
            public const string EditNickname = "نام مستعار 📝";
            public const string EditGender = "جنسیت 📝";
            public const string EditAge = "سن 📝";
            public const string EditProfilePhoto = "عکس 📝";

            public const string Male = "پسر 👨";
            public const string Female = "دختر 👩";
        }

        public static class Errors
        {
            public const string InvalidInput = "پیامی که فرستادی نامعتبره، دوباره تلاش کن ❌";
            public const string CantEditProfileInSession = "امکان ویرایش پروفایل هنگام چت وجود ندارد ❌";
        }
    }
}
