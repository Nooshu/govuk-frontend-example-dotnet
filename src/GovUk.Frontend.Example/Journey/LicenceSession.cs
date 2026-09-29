using System.Text.Json;
using GovUk.Frontend.Example.Journey;

namespace GovUk.Frontend.Example.Journey;

public static class LicenceSession
{
    public static LicenceDraft Load(ISession session)
    {
        var json = session.GetString(LicenceJourney.SessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new LicenceDraft();
        }

        return JsonSerializer.Deserialize<LicenceDraft>(json) ?? new LicenceDraft();
    }

    public static void Save(ISession session, LicenceDraft draft) =>
        session.SetString(LicenceJourney.SessionKey, JsonSerializer.Serialize(draft));
}
