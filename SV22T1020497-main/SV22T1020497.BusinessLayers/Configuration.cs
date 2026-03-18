using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020497.BusinessLayers
{
    /// <summary>
    /// Lớp lưu giữ các thông tin cấu hình cần sử dụng cho BusinessLayer
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString = "";
        /// <summary>
        /// Khởi tạo cấu hình cho BusinessLayer
        /// (hàm này phải được gọi trước khi chạy ứng dụng)
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// thuộc tính trả về chuỗi tham số kết nối đến cơ sở dữ liệu
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}
