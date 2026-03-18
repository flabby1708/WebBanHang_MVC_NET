using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.DataDictionary;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho danh mục tỉnh thành trên SQL Server.
    /// </summary>
    public class ProvinceRepository : IDataDictionaryRepository<Province>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu.</param>
        public ProvinceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách tỉnh thành.
        /// </summary>
        /// <returns>Danh sách tỉnh thành được sắp xếp theo tên.</returns>
        public async Task<List<Province>> ListAsync()
        {
            const string sql = @"SELECT ProvinceName FROM Provinces ORDER BY ProvinceName;";
            await using var connection = new SqlConnection(_connectionString);
            var items = await connection.QueryAsync<Province>(sql);
            return items.ToList();
        }
    }
}
