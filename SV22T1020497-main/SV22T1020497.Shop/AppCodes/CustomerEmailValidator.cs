using System.Text.RegularExpressions;

namespace SV22T1020497.Shop.AppCodes
{
    public static class CustomerEmailValidator
    {
        private static readonly Regex GmailRegex = new(
            @"^[A-Za-z0-9](?:[A-Za-z0-9._%+-]{4,62})@gmail\.com$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryValidate(string? input, out string normalizedEmail, out string errorMessage)
        {
            normalizedEmail = (input ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                errorMessage = "Vui lòng nhập email.";
                return false;
            }

            if (!GmailRegex.IsMatch(normalizedEmail))
            {
                errorMessage = "Email phải đúng định dạng và sử dụng đuôi @gmail.com.";
                return false;
            }

            normalizedEmail = normalizedEmail.ToLowerInvariant();
            errorMessage = string.Empty;
            return true;
        }
    }
}
