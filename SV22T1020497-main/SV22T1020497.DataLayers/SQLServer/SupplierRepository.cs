using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Partner;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho nhà cung cấp trên hệ quản trị cơ sở dữ liệu SQL Server.
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng repository sử dụng chuỗi kết nối đến cơ sở dữ liệu.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server.</param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Truy vấn danh sách nhà cung cấp theo điều kiện tìm kiếm và trả về kết quả phân trang.
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang.</param>
        /// <returns>Kết quả truy vấn dưới dạng phân trang.</returns>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();

            string whereClause = @"WHERE (@SearchValue = N'' OR SupplierName LIKE @Keyword OR ContactName LIKE @Keyword)";
            string countSql = $@"
SELECT COUNT(*)
FROM Suppliers
{whereClause};";

            string dataSql = input.PageSize > 0
                ? $@"
SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email
FROM Suppliers
{whereClause}
ORDER BY SupplierName
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"
                : $@"
SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email
FROM Suppliers
{whereClause}
ORDER BY SupplierName;";

            var parameters = new
            {
                input.SearchValue,
                Keyword = $"%{input.SearchValue}%",
                input.Offset,
                input.PageSize
            };

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            IEnumerable<Supplier> items = await connection.QueryAsync<Supplier>(dataSql, parameters);

            return new PagedResult<Supplier>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin của một nhà cung cấp theo mã.
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần lấy.</param>
        /// <returns>Thông tin nhà cung cấp hoặc <c>null</c> nếu không tìm thấy.</returns>
        public async Task<Supplier?> GetAsync(int id)
        {
            const string sql = @"
SELECT SupplierID, SupplierName, ContactName, Province, Address, Phone, Email
FROM Suppliers
WHERE SupplierID = @SupplierID;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { SupplierID = id });
        }

        /// <summary>
        /// Bổ sung một nhà cung cấp mới vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="data">Dữ liệu nhà cung cấp cần bổ sung.</param>
        /// <returns>Mã nhà cung cấp vừa được tạo.</returns>
        public async Task<int> AddAsync(Supplier data)
        {
            const string sql = @"
INSERT INTO Suppliers(SupplierName, ContactName, Province, Address, Phone, Email)
VALUES (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
SELECT CAST(SCOPE_IDENTITY() AS int);";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp.
        /// </summary>
        /// <param name="data">Dữ liệu nhà cung cấp sau khi chỉnh sửa.</param>
        /// <returns><c>true</c> nếu cập nhật thành công; ngược lại là <c>false</c>.</returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            const string sql = @"
UPDATE Suppliers
SET SupplierName = @SupplierName,
    ContactName = @ContactName,
    Province = @Province,
    Address = @Address,
    Phone = @Phone,
    Email = @Email
WHERE SupplierID = @SupplierID;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            int affectedRows = await connection.ExecuteAsync(sql, data);
            return affectedRows > 0;
        }

        /// <summary>
        /// Xóa một nhà cung cấp theo mã.
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa.</param>
        /// <returns><c>true</c> nếu xóa thành công; ngược lại là <c>false</c>.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"
DELETE FROM Suppliers
WHERE SupplierID = @SupplierID;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            int affectedRows = await connection.ExecuteAsync(sql, new { SupplierID = id });
            return affectedRows > 0;
        }

        /// <summary>
        /// Kiểm tra nhà cung cấp có đang được sử dụng bởi dữ liệu liên quan hay không.
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần kiểm tra.</param>
        /// <returns><c>true</c> nếu nhà cung cấp đã được sử dụng; ngược lại là <c>false</c>.</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = @"
SELECT CASE
           WHEN EXISTS (SELECT 1 FROM Products WHERE SupplierID = @SupplierID) THEN CAST(1 AS bit)
           ELSE CAST(0 AS bit)
       END;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.ExecuteScalarAsync<bool>(sql, new { SupplierID = id });
        }
    }
}
