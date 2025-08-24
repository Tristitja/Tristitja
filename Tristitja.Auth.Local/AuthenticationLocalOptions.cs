namespace Tristitja.Auth.Local;

public class AuthenticationLocalOptions
{
    public HashSet<Role> Roles { get; } =
    [
        AuthenticationConstants.AdminRole,
        AuthenticationConstants.UserRole,
        AuthenticationConstants.AnonymousRole
    ];

    // Current OWASP recommendation [Aug 2025]
    // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
    public int Pbkdf2Iterations { get; set; } = 600_000;
}
