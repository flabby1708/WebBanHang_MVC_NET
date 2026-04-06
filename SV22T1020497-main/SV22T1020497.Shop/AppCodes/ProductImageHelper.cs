using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SV22T1020497.Shop.AppCodes
{
    public static class ProductImageHelper
    {
        private static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp"];
        private static readonly object SyncLock = new();
        private static Dictionary<string, string> _imageIndex = new(StringComparer.Ordinal);
        private static DateTime _lastIndexedUtc = DateTime.MinValue;

        public static string? ResolveProductImageUrl(string? productName, string? photo)
        {
            EnsureImageIndex();

            string? uploadedImageUrl = FindUploadedImageUrl(productName);
            if (!string.IsNullOrWhiteSpace(uploadedImageUrl))
                return uploadedImageUrl;

            string? photoImageUrl = FindUploadedImageUrl(photo);
            if (!string.IsNullOrWhiteSpace(photoImageUrl))
                return photoImageUrl;

            if (!string.IsNullOrWhiteSpace(photo))
            {
                if (photo.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    photo.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return photo;
                }

                if (photo.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    return EncodeUrlPath(photo);

                return EncodeUrlPath("/images/products/" + photo);
            }

            return null;
        }

        private static string? FindUploadedImageUrl(string? productNameOrPhoto)
        {
            string normalizedName = Normalize(Path.GetFileNameWithoutExtension(productNameOrPhoto ?? string.Empty));
            if (string.IsNullOrWhiteSpace(normalizedName))
                return null;

            if (_imageIndex.TryGetValue(normalizedName, out string? exactMatch))
                return exactMatch;

            string productNoCode = RemoveLeadingProductCode(normalizedName);
            if (!string.IsNullOrWhiteSpace(productNoCode) &&
                _imageIndex.TryGetValue(productNoCode, out string? noCodeMatch))
            {
                return noCodeMatch;
            }

            string productCode = ExtractLeadingProductCode(normalizedName);
            if (!string.IsNullOrWhiteSpace(productCode))
            {
                var codeMatch = _imageIndex.FirstOrDefault(x => ExtractLeadingProductCode(x.Key) == productCode);
                if (!string.IsNullOrWhiteSpace(codeMatch.Value))
                    return codeMatch.Value;
            }

            var bestMatch = _imageIndex
                .Select(x => new { x.Value, Score = MatchScore(normalizedName, x.Key) })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault(x => x.Score > 0);

            return bestMatch?.Value;
        }

        private static void EnsureImageIndex()
        {
            if (_imageIndex.Count > 0 && DateTime.UtcNow - _lastIndexedUtc < TimeSpan.FromSeconds(30))
                return;

            lock (SyncLock)
            {
                if (_imageIndex.Count > 0 && DateTime.UtcNow - _lastIndexedUtc < TimeSpan.FromSeconds(30))
                    return;

                string productImagePath = Path.Combine(ApplicationContext.WwwRootPath, "images", "products");
                var index = new Dictionary<string, string>(StringComparer.Ordinal);

                if (Directory.Exists(productImagePath))
                {
                    var imageFiles = Directory.EnumerateFiles(productImagePath, "*.*", SearchOption.AllDirectories)
                        .OrderBy(path => Path.GetRelativePath(productImagePath, path).Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar))
                        .ThenBy(path => path, StringComparer.OrdinalIgnoreCase);

                    foreach (string filePath in imageFiles)
                    {
                        string extension = Path.GetExtension(filePath);
                        if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                            continue;

                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        string key = Normalize(fileName);
                        if (string.IsNullOrWhiteSpace(key))
                            continue;

                        string relativePath = Path.GetRelativePath(ApplicationContext.WwwRootPath, filePath)
                            .Replace("\\", "/");

                        string publicUrl = EncodeUrlPath("/" + relativePath);
                        if (!index.ContainsKey(key))
                            index[key] = publicUrl;

                        string keyWithoutCode = RemoveLeadingProductCode(key);
                        if (!string.IsNullOrWhiteSpace(keyWithoutCode) && !index.ContainsKey(keyWithoutCode))
                            index[keyWithoutCode] = publicUrl;
                    }
                }

                _imageIndex = index;
                _lastIndexedUtc = DateTime.UtcNow;
            }
        }

        private static int MatchScore(string productName, string fileName)
        {
            if (productName == fileName)
                return int.MaxValue;

            string productNoCode = RemoveLeadingProductCode(productName);
            string fileNoCode = RemoveLeadingProductCode(fileName);

            if (!string.IsNullOrWhiteSpace(productNoCode) && productNoCode == fileNoCode)
                return int.MaxValue - 1;

            string productCode = ExtractLeadingProductCode(productName);
            string fileCode = ExtractLeadingProductCode(fileName);
            if (!string.IsNullOrWhiteSpace(productCode) && productCode == fileCode)
                return int.MaxValue - 2;

            if (productName.Contains(fileName, StringComparison.Ordinal) || fileName.Contains(productName, StringComparison.Ordinal))
                return Math.Min(productName.Length, fileName.Length) + 1000;

            if (!string.IsNullOrWhiteSpace(productNoCode) &&
                !string.IsNullOrWhiteSpace(fileNoCode) &&
                (productNoCode.Contains(fileNoCode, StringComparison.Ordinal) || fileNoCode.Contains(productNoCode, StringComparison.Ordinal)))
            {
                return Math.Min(productNoCode.Length, fileNoCode.Length) + 500;
            }

            string[] productTokens = productNoCode.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string[] fileTokens = fileNoCode.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            int overlap = productTokens.Intersect(fileTokens).Count();
            int threshold = Math.Max(2, Math.Min(productTokens.Length, fileTokens.Length) / 2);
            return overlap >= threshold ? overlap : 0;
        }

        private static string ExtractLeadingProductCode(string value)
        {
            Match match = Regex.Match(value, @"^\d+");
            return match.Success ? match.Value : string.Empty;
        }

        private static string RemoveLeadingProductCode(string value)
        {
            return Regex.Replace(value, @"^\d+\s*", string.Empty).Trim();
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }

            string result = builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('\u0111', 'd');

            return Regex.Replace(result, @"[^a-z0-9]+", " ").Trim();
        }

        private static string EncodeUrlPath(string path)
        {
            string[] segments = path.Split('/', StringSplitOptions.None);
            for (int i = 0; i < segments.Length; i++)
            {
                if (string.IsNullOrEmpty(segments[i]))
                    continue;

                segments[i] = Uri.EscapeDataString(segments[i]);
            }

            return string.Join("/", segments);
        }
    }
}
