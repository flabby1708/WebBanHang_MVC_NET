using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SV22T1020497.Admin.AppCodes
{
    public static class CategoryImageHelper
    {
        private static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp"];
        private static readonly object SyncLock = new();
        private static Dictionary<string, string> _imageIndex = new(StringComparer.Ordinal);
        private static DateTime _lastIndexedUtc = DateTime.MinValue;

        public static string? ResolveCategoryImageUrl(string? categoryName, string? photo)
        {
            EnsureImageIndex();

            string? directPhotoUrl = FindDirectImageUrl(photo);
            if (!string.IsNullOrWhiteSpace(directPhotoUrl))
                return directPhotoUrl;

            string? uploadedImageUrl = FindUploadedImageUrl(photo);
            if (!string.IsNullOrWhiteSpace(uploadedImageUrl))
                return uploadedImageUrl;

            directPhotoUrl = FindDirectImageUrl(categoryName);
            if (!string.IsNullOrWhiteSpace(directPhotoUrl))
                return directPhotoUrl;

            uploadedImageUrl = FindUploadedImageUrl(categoryName);
            if (!string.IsNullOrWhiteSpace(uploadedImageUrl))
                return uploadedImageUrl;

            RebuildImageIndex();

            directPhotoUrl = FindDirectImageUrl(photo);
            if (!string.IsNullOrWhiteSpace(directPhotoUrl))
                return directPhotoUrl;

            uploadedImageUrl = FindUploadedImageUrl(photo);
            if (!string.IsNullOrWhiteSpace(uploadedImageUrl))
                return uploadedImageUrl;

            directPhotoUrl = FindDirectImageUrl(categoryName);
            if (!string.IsNullOrWhiteSpace(directPhotoUrl))
                return directPhotoUrl;

            uploadedImageUrl = FindUploadedImageUrl(categoryName);
            if (!string.IsNullOrWhiteSpace(uploadedImageUrl))
                return uploadedImageUrl;

            if (!string.IsNullOrWhiteSpace(photo))
            {
                if (photo.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    photo.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return photo;
                }

                if (photo.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    return EncodeUrlPath(photo);

                return EncodeUrlPath("/images/categories/" + photo);
            }

            return null;
        }

        private static string? FindDirectImageUrl(string? categoryNameOrPhoto)
        {
            if (string.IsNullOrWhiteSpace(categoryNameOrPhoto))
                return null;

            string categoryImagePath = Path.Combine(ApplicationContext.WWWRootPath, "images", "categories");
            if (!Directory.Exists(categoryImagePath))
                return null;

            string rawFileName = Path.GetFileName(categoryNameOrPhoto);
            if (!string.IsNullOrWhiteSpace(rawFileName))
            {
                string directPath = Path.Combine(categoryImagePath, rawFileName);
                if (File.Exists(directPath))
                    return EncodeUrlPath("/images/categories/" + rawFileName);
            }

            string slug = ToSlug(Path.GetFileNameWithoutExtension(categoryNameOrPhoto));
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            foreach (string extension in SupportedExtensions)
            {
                string fileName = slug + extension;
                string directPath = Path.Combine(categoryImagePath, fileName);
                if (File.Exists(directPath))
                    return EncodeUrlPath("/images/categories/" + fileName);
            }

            return null;
        }

        private static string? FindUploadedImageUrl(string? categoryNameOrPhoto)
        {
            string normalizedName = Normalize(Path.GetFileNameWithoutExtension(categoryNameOrPhoto ?? string.Empty));
            if (string.IsNullOrWhiteSpace(normalizedName))
                return null;

            if (_imageIndex.TryGetValue(normalizedName, out string? exactMatch))
                return exactMatch;

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

            RebuildImageIndex();
        }

        private static void RebuildImageIndex()
        {
            lock (SyncLock)
            {
                string categoryImagePath = Path.Combine(ApplicationContext.WWWRootPath, "images", "categories");
                var index = new Dictionary<string, string>(StringComparer.Ordinal);

                if (Directory.Exists(categoryImagePath))
                {
                    var imageFiles = Directory.EnumerateFiles(categoryImagePath, "*.*", SearchOption.AllDirectories)
                        .OrderBy(path => Path.GetRelativePath(categoryImagePath, path).Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar))
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

                        string relativePath = Path.GetRelativePath(ApplicationContext.WWWRootPath, filePath)
                            .Replace("\\", "/");

                        if (!index.ContainsKey(key))
                            index[key] = EncodeUrlPath("/" + relativePath);
                    }
                }

                _imageIndex = index;
                _lastIndexedUtc = DateTime.UtcNow;
            }
        }

        private static int MatchScore(string categoryName, string fileName)
        {
            if (categoryName == fileName)
                return int.MaxValue;

            if (categoryName.Contains(fileName, StringComparison.Ordinal) || fileName.Contains(categoryName, StringComparison.Ordinal))
                return Math.Min(categoryName.Length, fileName.Length) + 1000;

            string[] categoryTokens = categoryName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string[] fileTokens = fileName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            int overlap = categoryTokens.Intersect(fileTokens).Count();
            int threshold = Math.Max(2, Math.Min(categoryTokens.Length, fileTokens.Length));
            return overlap >= threshold ? overlap : 0;
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

        private static string ToSlug(string? value)
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

            return Regex.Replace(result, @"[^a-z0-9]+", "-").Trim('-');
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
