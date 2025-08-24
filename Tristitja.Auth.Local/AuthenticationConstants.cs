namespace Tristitja.Auth.Local;

public static class AuthenticationConstants
{
    public static string AuthenticationScheme => "IWannaDie";
    public static string SessionClaimType => "IWannaRot";
    public static string SessionItemKey => "IWannaWitherAway";

    public static readonly Role UserRole = new Role
    {
        Name = "User",
        Description = "Normal user",
    };
    
    public static readonly Role AdminRole = new Role
    {
        Name = "Admin",
        Description = "Administrator with unlimited power",
    };
    
    public static readonly Role AnonymousRole = new Role
    {
        Name = "Anonymous",
        Description = "Anonymous user",
    };
}
