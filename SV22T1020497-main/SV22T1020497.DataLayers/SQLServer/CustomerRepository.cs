using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Partner;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho khách hàng trên SQL Server.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu.</param>
        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">Thông tin đầu vào tìm kiếm và phân trang.</param>
        /// <returns>Kết quả dữ liệu phân trang.</returns>
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            const string whereClause = @"WHERE (@SearchValue = N'' OR CustomerName LIKE @Keyword OR ContactName LIKE @Keyword OR Phone LIKE @Keyword OR Email LIKE @Keyword)";

            string countSql = $@"SELECT COUNT(*) FROM Customers {whereClause};";
            string dataSql = input.PageSize > 0
                ? $@"SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked
                     FROM Customers
                     {whereClause}
                     ORDER BY CustomerName
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"
                : $@"SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked
                     FROM Customers
                     {whereClause}
                     ORDER BY CustomerName;";

            var parameters = new
            {
                input.SearchValue,
                Keyword = $"%{input.SearchValue}%",
                input.Offset,
                input.PageSize
            };

            await using var connection = new SqlConnection(_connectionString);
            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<Customer>(dataSql, parameters);

            return new PagedResult<Customer>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một khách hàng theo mã.
        /// </summary>
        /// <param name="id">Mã khách hàng.</param>
        /// <returns>Thông tin khách hàng hoặc null nếu không tồn tại.</returns>
        public async Task<Customer?> GetAsync(int id)
        {
            const string sql = @"SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked
                                 FROM Customers
                                 WHERE CustomerID = @CustomerID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerID = id });
        }

        /// <summary>
        /// Bổ sung một khách hàng mới.
        /// </summary>
        /// <param name="data">Dữ liệu khách hàng cần bổ sung.</param>
        /// <returns>Mã khách hàng vừa được tạo.</returns>
        public async Task<int> AddAsync(Customer data)
        {
            const string sql = @"INSERT INTO Customers(CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                                 VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                                 SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng.
        /// </summary>
        /// <param name="data">Dữ liệu khách hàng sau chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            const string sql = @"UPDATE Customers
                                 SET CustomerName = @CustomerName,
                                     ContactName = @ContactName,
                                     Province = @Province,
                                     Address = @Address,
                                     Phone = @Phone,
                                     Email = @Email,
                                     Password = @Password,
                                     IsLocked = @IsLocked
                                 WHERE CustomerID = @CustomerID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa khách hàng theo mã.
        /// </summary>
        /// <param name="id">Mã khách hàng cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM Customers WHERE CustomerID = @CustomerID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { CustomerID = id }) > 0;
        }

        /// <summary>
        /// Kiểm tra khách hàng có đang được sử dụng trong đơn hàng hay không.
        /// </summary>
        /// <param name="id">Mã khách hàng cần kiểm tra.</param>
        /// <returns>True nếu có dữ liệu liên quan, ngược lại là false.</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = @"SELECT CAST(CASE WHEN EXISTS
                                    (SELECT 1 FROM Orders WHERE CustomerID = @CustomerID)
                                THEN 1 ELSE 0 END AS bit);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { CustomerID = id });
        }

        /// <summary>
        /// Kiểm tra địa chỉ email của khách hàng có hợp lệ và không bị trùng hay không.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <param name="id">Mã khách hàng đang chỉnh sửa; bằng 0 nếu là thêm mới.</param>
        /// <returns>True nếu email hợp lệ để sử dụng, ngược lại là false.</returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            const string sql = @"SELECT COUNT(*)
                                 FROM Customers
                                 WHERE Email = @Email AND CustomerID <> @CustomerID;";
            await using var connection = new SqlConnection(_connectionString);
            int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, CustomerID = id });
            return count == 0;
        }
    }
}
