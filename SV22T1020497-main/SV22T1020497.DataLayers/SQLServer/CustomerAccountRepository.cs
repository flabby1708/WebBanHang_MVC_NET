using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Security;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu tài khoản khách hàng trên SQL Server.
    /// </summary>
    public class CustomerAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu.</param>
        public CustomerAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập của khách hàng.
        /// </summary>
        /// <param name="userName">Tên đăng nhập, sử dụng email khách hàng.</param>
        /// <param name="password">Mật khẩu đăng nhập.</param>
        /// <returns>Thông tin tài khoản nếu hợp lệ; ngược lại trả về null.</returns>
        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            const string sql = @"
SELECT CAST(CustomerID AS nvarchar(50)) AS UserId,
       Email AS UserName,
       CustomerName AS DisplayName,
       ISNULL(Email, N'') AS Email,
       N'' AS Photo,
       N'Customer' AS RoleNames
FROM Customers
WHERE Email = @UserName
  AND Password = @Password
  AND ISNULL(IsLocked, 0) = 0;";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql, new { UserName = userName, Password = password });
        }

        /// <summary>
        /// Đổi mật khẩu cho tài khoản khách hàng.
        /// </summary>
        /// <param name="userName">Tên đăng nhập, sử dụng email khách hàng.</param>
        /// <param name="password">Mật khẩu mới.</param>
        /// <returns>True nếu đổi mật khẩu thành công, ngược lại là false.</returns>
        public async Task<bool> ChangePassword(string userName, string password)
        {
            const string sql = @"UPDATE Customers
                                 SET Password = @Password
                                 WHERE Email = @UserName;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { UserName = userName, Password = password }) > 0;
        }
    }
}
