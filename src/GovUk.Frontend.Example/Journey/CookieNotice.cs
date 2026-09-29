using Microsoft.AspNetCore.Http;

namespace GovUk.Frontend.Example.Journey;

public static class CookieNotice
{
    public const string Key = "cookie-choice";

    public static string? Read(ISession session) => session.GetString(Key);

    public static bool TrySave(ISession session, string? choice)
    {
        switch (choice)
        {
            case "accept":
            case "reject":
            case "hide":
                session.SetString(Key, choice);
                return true;
            default:
                return false;
        }
    }
}
