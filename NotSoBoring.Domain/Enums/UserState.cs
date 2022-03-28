

namespace NotSoBoring.Domain.Enums
{
    public enum UserState
    {
        InMenu = 0,
        WaitingForMatch = 1,
        InSession = 2,

        Edit_Profile_Start = 10000,
        EditingNickname = 10001,
        EditingGender = 10002,
        EditingPhoto = 10003,
        EditingAge = 10004,
        EditingLocation = 10005,
        Edit_Profile_End = 10020
    }
}
