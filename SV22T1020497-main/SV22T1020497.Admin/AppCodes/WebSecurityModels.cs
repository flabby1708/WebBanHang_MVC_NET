using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SV22T1020497.Admin.AppCodes
{
    /// <summary>
    /// Thông tin tài khoản người dùng được lưu trong phiên đăng nhập (cookie)
    /// </summary>
    public class WebUserData
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Photo { get; set; }
        public List<string>? Roles { get; set; }

        private List<Claim> Claims
        {
            get
            {
                List<Claim> claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, UserId ?? string.Empty),
                    new Claim(ClaimTypes.Name, DisplayName ?? UserName ?? string.Empty),
                    new Claim(nameof(UserId), UserId ?? string.Empty),
                    new Claim(nameof(UserName), UserName ?? string.Empty),
                    new Claim(nameof(DisplayName), DisplayName ?? string.Empty),
                    new Claim(nameof(Email), Email ?? string.Empty),
                    new Claim(nameof(Photo), Photo ?? string.Empty)
                ];

                if (Roles != null)
                {
                    foreach (string role in Roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }

                return claims;
            }
        }

        public ClaimsPrincipal CreatePrincipal()
        {
            var claimIdentity = new ClaimsIdentity(Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimIdentity);
        }

        public bool HasRole(string role)
        {
            return Roles?.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public bool HasAnyRole(params string[] roles)
        {
            return roles.Any(HasRole);
        }
    }

    /// <summary>
    /// Định nghĩa tên của các role sử dụng trong phân quyền chức năng cho nhân viên
    /// </summary>
    public static class WebUserRoles
    {
        public const string Administrator = "admin";
        public const string DataManager = "datamanager";
        public const string Sales = "sales";

        public const string Customers = "Customers";
        public const string Products = "Products";
        public const string Orders = "Orders";
        public const string SystemAdmin = "SystemAdmin";
    }

    /// <summary>
    /// Extension các phương thức cho các đối tượng liên quan đến xác thực tài khoản người dùng
    /// </summary>
    public static class WebUserExtensions
    {
        public static WebUserData? GetUserData(this ClaimsPrincipal principal)
        {
            try
            {
                if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
                    return null;

                var userData = new WebUserData
                {
                    UserId = principal.FindFirstValue(nameof(WebUserData.UserId)),
                    UserName = principal.FindFirstValue(nameof(WebUserData.UserName)),
                    DisplayName = principal.FindFirstValue(nameof(WebUserData.DisplayName)),
                    Email = principal.FindFirstValue(nameof(WebUserData.Email)),
                    Photo = principal.FindFirstValue(nameof(WebUserData.Photo)),
                    Roles = []
                };

                foreach (Claim claim in principal.FindAll(ClaimTypes.Role))
                    userData.Roles.Add(claim.Value);

                return userData;
            }
            catch
            {
                return null;
            }
        }
    }
}
