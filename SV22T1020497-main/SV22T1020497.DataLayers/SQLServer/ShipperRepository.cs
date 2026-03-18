using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Partner;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho người giao hàng trên SQL Server.
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối đến cơ sở dữ liệu.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối SQL Server.</param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách người giao hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang.</param>
        /// <returns>Kết quả dữ liệu phân trang.</returns>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            input ??= new PaginationSearchInput();
            const string whereClause = "WHERE (@SearchValue = N'' OR ShipperName LIKE @Keyword OR Phone LIKE @Keyword)";

            string countSql = $@"SELECT COUNT(*) FROM Shippers {whereClause};";
            string dataSql = input.PageSize > 0
                ? $@"SELECT ShipperID, ShipperName, Phone
                     FROM Shippers
                     {whereClause}
                     ORDER BY ShipperName
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"
                : $@"SELECT ShipperID, ShipperName, Phone
                     FROM Shippers
                     {whereClause}
                     ORDER BY ShipperName;";

            var parameters = new
            {
                input.SearchValue,
                Keyword = $"%{input.SearchValue}%",
                input.Offset,
                input.PageSize
            };

            await using var connection = new SqlConnection(_connectionString);
            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<Shipper>(dataSql, parameters);

            return new PagedResult<Shipper>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một người giao hàng theo mã.
        /// </summary>
        /// <param name="id">Mã người giao hàng.</param>
        /// <returns>Thông tin người giao hàng hoặc null nếu không tồn tại.</returns>
        public async Task<Shipper?> GetAsync(int id)
        {
            const string sql = @"SELECT ShipperID, ShipperName, Phone
                                 FROM Shippers
                                 WHERE ShipperID = @ShipperID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { ShipperID = id });
        }

        /// <summary>
        /// Bổ sung một người giao hàng.
        /// </summary>
        /// <param name="data">Dữ liệu người giao hàng cần bổ sung.</param>
        /// <returns>Mã người giao hàng được tạo.</returns>
        public async Task<int> AddAsync(Shipper data)
        {
            const string sql = @"INSERT INTO Shippers(ShipperName, Phone)
                                 VALUES (@ShipperName, @Phone);
                                 SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng.
        /// </summary>
        /// <param name="data">Dữ liệu sau khi chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            const string sql = @"UPDATE Shippers
                                 SET ShipperName = @ShipperName,
                                     Phone = @Phone
                                 WHERE ShipperID = @ShipperID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa một người giao hàng theo mã.
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"DELETE FROM Shippers WHERE ShipperID = @ShipperID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { ShipperID = id }) > 0;
        }

        /// <summary>
        /// Kiểm tra người giao hàng có đang được sử dụng trong đơn hàng hay không.
        /// </summary>
        /// <param name="id">Mã người giao hàng cần kiểm tra.</param>
        /// <returns>True nếu có dữ liệu liên quan, ngược lại là false.</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            const string sql = @"SELECT CAST(CASE WHEN EXISTS
                                    (SELECT 1 FROM Orders WHERE ShipperID = @ShipperID)
                                THEN 1 ELSE 0 END AS bit);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { ShipperID = id });
        }
    }
}
