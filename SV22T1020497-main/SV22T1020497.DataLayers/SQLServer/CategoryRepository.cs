using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho loại hàng trên SQL Server.
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu.</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách loại hàng có phân trang.
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang.</param>
        /// <returns>Kết quả dữ liệu phân trang.</returns>
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            const string whereClause = "WHERE (@SearchValue = N'' OR CategoryName LIKE @Keyword OR Description LIKE @Keyword)";

            await using var connection = new SqlConnection(_connectionString);
            bool hasPhotoColumn = await HasPhotoColumnAsync(connection);

            string photoSelect = hasPhotoColumn
                ? "ISNULL(Photo, N'') AS Photo"
                : "N'' AS Photo";

            string countSql = $@"SELECT COUNT(*) FROM Categories {whereClause};";
            string dataSql = input.PageSize > 0
                ? $@"SELECT CategoryID, CategoryName, Description, {photoSelect}
                     FROM Categories
                     {whereClause}
                     ORDER BY CategoryName
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"
                : $@"SELECT CategoryID, CategoryName, Description, {photoSelect}
                     FROM Categories
                     {whereClause}
                     ORDER BY CategoryName;";

            var parameters = new
            {
                input.SearchValue,
                Keyword = $"%{input.SearchValue}%",
                input.Offset,
                input.PageSize
            };

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<Category>(dataSql, parameters);

            return new PagedResult<Category>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một loại hàng theo mã.
        /// </summary>
        /// <param name="id">Mã loại hàng.</param>
        /// <returns>Thông tin loại hàng hoặc null nếu không có dữ liệu.</returns>
        public async Task<Category?> GetAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString);
            bool hasPhotoColumn = await HasPhotoColumnAsync(connection);

            string photoSelect = hasPhotoColumn
                ? "ISNULL(Photo, N'') AS Photo"
                : "N'' AS Photo";

            string sql = $@"SELECT CategoryID, CategoryName, Description, {photoSelect}
                            FROM Categories
                            WHERE CategoryID = @CategoryID;";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { CategoryID = id });
        }

        /// <summary>
        /// Bổ sung loại hàng mới.
        /// </summary>
        /// <param name="data">Dữ liệu cần bổ sung.</param>
        /// <returns>Mã loại hàng vừa được tạo.</returns>
        public async Task<int> AddAsync(Category data)
        {
            await using var connection = new SqlConnection(_connectionString);
            bool hasPhotoColumn = await HasPhotoColumnAsync(connection);

            string sql = hasPhotoColumn
                ? @"INSERT INTO Categories(CategoryName, Description, Photo)
                    VALUES (@CategoryName, @Description, @Photo);
                    SELECT CAST(SCOPE_IDENTITY() AS int);"
                : @"INSERT INTO Categories(CategoryName, Description)
                    VALUES (@CategoryName, @Description);
                    SELECT CAST(SCOPE_IDENTITY() AS int);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng.
        /// </summary>
        /// <param name="data">Dữ liệu sau cập nhật.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            await using var connection = new SqlConnection(_connectionString);
            bool hasPhotoColumn = await HasPhotoColumnAsync(connection);

            string sql = hasPhotoColumn
                ? @"UPDATE Categories
                    SET CategoryName = @CategoryName,
                        Description = @Description,
                        Photo = @Photo
                    WHERE CategoryID = @CategoryID;"
                : @"UPDATE Categories
                    SET CategoryName = @CategoryName,
                        Description = @Description
                    WHERE CategoryID = @CategoryID;";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        private static async Task<bool> HasPhotoColumnAsync(SqlConnection connection)
        {
            const string sql = @"SELECT COUNT(*)
                                 FROM INFORMATION_SCHEMA.COLUMNS
                                 WHERE TABLE_NAME = 'Categories' AND COLUMN_NAME = 'Photo';";

            int count = await connection.ExecuteScalarAsync<int>(sql);
            return count > 0;
        }

        /// <summary>
        /// Xóa loại hàng theo mã.
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM Categories WHERE CategoryID = @CategoryID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { CategoryID = id }) > 0;
        }

        /// <summary>
        /// Kiểm tra loại hàng có đang được dùng bởi mặt hàng nào hay không.
        /// </summary>
        /// <param name="id">Mã loại hàng cần kiểm tra.</param>
        /// <returns>True nếu có dữ liệu liên quan, ngược lại là false.</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = @"SELECT CAST(CASE WHEN EXISTS
                                    (SELECT 1 FROM Products WHERE CategoryID = @CategoryID)
                                THEN 1 ELSE 0 END AS bit);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { CategoryID = id });
        }
    }
}
