using System.Text.RegularExpressions;

namespace SV22T1020497.Shop.AppCodes
{
    public static class VietnameseNameValidator
    {
        private const string AllowedCharacters =
            "AĂÂBCDĐEÊGHIKLMNOÔƠPQRSTUƯVXY" +
            "ÁÀẢÃẠẮẰẲẴẶẤẦẨẪẬÉÈẺẼẸẾỀỂỄỆÍÌỈĨỊ" +
            "ÓÒỎÕỌỐỒỔỖỘỚỜỞỠỢÚÙỦŨỤỨỪỬỮỰÝỲỶỸỴ" +
            "aăâbcdđeêghiklmnoôơpqrstuưvxy" +
            "áàảãạắằẳẵặấầẩẫậéèẻẽẹếềểễệíìỉĩị" +
            "óòỏõọốồổỗộớờởỡợúùủũụứừửữựýỳỷỹỵ ";

        private const string AccentedCharacters =
            "ĂÂĐÊÔƠƯăâđêôơư" +
            "ÁÀẢÃẠẮẰẲẴẶẤẦẨẪẬÉÈẺẼẸẾỀỂỄỆÍÌỈĨỊ" +
            "ÓÒỎÕỌỐỒỔỖỘỚỜỞỠỢÚÙỦŨỤỨỪỬỮỰÝỲỶỸỴ" +
            "áàảãạắằẳẵặấầẩẫậéèẻẽẹếềểễệíìỉĩị" +
            "óòỏõọốồổỗộớờởỡợúùủũụứừửữựýỳỷỹỵ";

        private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex InvalidNguyenLikeRegex = new(@"^ngu[êềếểễệ]n$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryValidateCustomerName(string? input, out string normalizedName, out string errorMessage)
        {
            normalizedName = NormalizeSpacing(input);

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                errorMessage = "Vui lòng nhập tên khách hàng.";
                return false;
            }

            foreach (char character in normalizedName)
            {
                if (!AllowedCharacters.Contains(character))
                {
                    errorMessage = "Tên khách hàng phải là tiếng Việt có dấu và không chứa ký tự lạ hoặc sai bảng mã.";
                    return false;
                }
            }

            if (!normalizedName.Any(character => AccentedCharacters.Contains(character)))
            {
                errorMessage = "Tên khách hàng phải được nhập bằng tiếng Việt có dấu.";
                return false;
            }

            foreach (var part in normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (InvalidNguyenLikeRegex.IsMatch(part))
                {
                    errorMessage = "Tên khách hàng chưa đúng chính tả tiếng Việt. Vui lòng kiểm tra lại dấu và cách gõ tên.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        public static string NormalizeSpacing(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return MultiSpaceRegex.Replace(input.Trim(), " ").Normalize();
        }
    }
}
