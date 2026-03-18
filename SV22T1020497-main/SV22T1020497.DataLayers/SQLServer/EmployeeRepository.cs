using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.HR;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho nhân viên trên SQL Server.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu.</param>
        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách nhân viên dưới dạng phân trang.
        /// </summary>
        /// <param name="input">Thông tin đầu vào tìm kiếm và phân trang.</param>
        /// <returns>Kết quả dữ liệu phân trang.</returns>
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            const string whereClause = @"WHERE (@SearchValue = N'' OR FullName LIKE @Keyword OR Phone LIKE @Keyword OR Email LIKE @Keyword)";

            string countSql = $@"SELECT COUNT(*) FROM Employees {whereClause};";
            string dataSql = input.PageSize > 0
                ? $@"SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames
                     FROM Employees
                     {whereClause}
                     ORDER BY FullName
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"
                : $@"SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames
                     FROM Employees
                     {whereClause}
                     ORDER BY FullName;";

            var parameters = new
            {
                input.SearchValue,
                Keyword = $"%{input.SearchValue}%",
                input.Offset,
                input.PageSize
            };

            await using var connection = new SqlConnection(_connectionString);
            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<Employee>(dataSql, parameters);

            return new PagedResult<Employee>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một nhân viên theo mã.
        /// </summary>
        /// <param name="id">Mã nhân viên.</param>
        /// <returns>Thông tin nhân viên hoặc null nếu không tồn tại.</returns>
        public async Task<Employee?> GetAsync(int id)
        {
            const string sql = @"SELECT EmployeeID, FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames
                                 FROM Employees
                                 WHERE EmployeeID = @EmployeeID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { EmployeeID = id });
        }

        /// <summary>
        /// Bổ sung một nhân viên mới.
        /// </summary>
        /// <param name="data">Dữ liệu nhân viên cần bổ sung.</param>
        /// <returns>Mã nhân viên vừa được tạo.</returns>
        public async Task<int> AddAsync(Employee data)
        {
            const string sql = @"INSERT INTO Employees(FullName, BirthDate, Address, Phone, Email, Password, Photo, IsWorking, RoleNames)
                                 VALUES (@FullName, @BirthDate, @Address, @Phone, @Email, @Password, @Photo, @IsWorking, @RoleNames);
                                 SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên.
        /// </summary>
        /// <param name="data">Dữ liệu nhân viên sau chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateAsync(Employee data)
        {
            const string sql = @"UPDATE Employees
                                 SET FullName = @FullName,
                                     BirthDate = @BirthDate,
                                     Address = @Address,
                                     Phone = @Phone,
                                     Email = @Email,
                                     Password = @Password,
                                     Photo = @Photo,
                                     IsWorking = @IsWorking,
                                     RoleNames = @RoleNames
                                 WHERE EmployeeID = @EmployeeID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa nhân viên theo mã.
        /// </summary>
        /// <param name="id">Mã nhân viên cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM Employees WHERE EmployeeID = @EmployeeID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { EmployeeID = id }) > 0;
        }

        /// <summary>
        /// Kiểm tra nhân viên có đang được sử dụng trong đơn hàng hay không.
        /// </summary>
        /// <param name="id">Mã nhân viên cần kiểm tra.</param>
        /// <returns>True nếu có dữ liệu liên quan, ngược lại là false.</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = @"SELECT CAST(CASE WHEN EXISTS
                                    (SELECT 1 FROM Orders WHERE EmployeeID = @EmployeeID)
                                THEN 1 ELSE 0 END AS bit);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { EmployeeID = id });
        }

        /// <summary>
        /// Kiểm tra email của nhân viên có hợp lệ và không bị trùng hay không.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <param name="id">Mã nhân viên đang chỉnh sửa; bằng 0 nếu là thêm mới.</param>
        /// <returns>True nếu email có thể sử dụng, ngược lại là false.</returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            const string sql = @"SELECT COUNT(*)
                                 FROM Employees
                                 WHERE Email = @Email AND EmployeeID <> @EmployeeID;";
            await using var connection = new SqlConnection(_connectionString);
            int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, EmployeeID = id });
            return count == 0;
        }
    }
}
