namespace RaceDay.Api.Auth;

public static class Roles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";

    public static bool IsKnown(string? role) =>
        string.Equals(role, Organiser, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, Participant, StringComparison.OrdinalIgnoreCase);
}
