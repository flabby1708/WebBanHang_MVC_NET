using System.Threading.Tasks;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Security;

namespace SV22T1020497.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp các chức năng tác nghiệp/nghiệp vụ liên quan
    /// đến xác thực và bảo mật (Authentication, Authorization)
    /// </summary>
    public class SecurityDataService
    {
        private static readonly IUserAccountRepository employeeAccountDB;
        private static readonly IUserAccountRepository customerAccountDB;

        /// <summary>
        /// Constructor - Khởi tạo các repository
        /// </summary>
        static SecurityDataService()
        {
            employeeAccountDB = new EmployeeAccountRepository(Configuration.ConnectionString);
            customerAccountDB = new CustomerAccountRepository(Configuration.ConnectionString);
        }

        //== các chức năng liên quan đến tài khoản nhân viên
        
        /// <summary>
        /// Xác thực tài khoản nhân viên
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email)</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>Thông tin tài khoản nếu xác thực thành công, null ngược lại</returns>
        public static async Task<UserAccount?> AuthorizeEmployeeAsync(string userName, string password)
        {
            return await employeeAccountDB.Authorize(userName, password);
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản nhân viên
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email)</param>
        /// <param name="password">Mật khẩu mới</param>
        /// <returns>True nếu đổi mật khẩu thành công, false ngược lại</returns>
        public static async Task<bool> ChangeEmployeePasswordAsync(string userName, string password)
        {
            return await employeeAccountDB.ChangePassword(userName, password);
        }

        //== các chức năng liên quan đến tài khoản khách hàng
        
        /// <summary>
        /// Xác thực tài khoản khách hàng
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email)</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>Thông tin tài khoản nếu xác thực thành công, null ngược lại</returns>
        public static async Task<UserAccount?> AuthorizeCustomerAsync(string userName, string password)
        {
            return await customerAccountDB.Authorize(userName, password);
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản khách hàng
        /// </summary>
        /// <param name="userName">Tên đăng nhập (email)</param>
        /// <param name="password">Mật khẩu mới</param>
        /// <returns>True nếu đổi mật khẩu thành công, false ngược lại</returns>
        public static async Task<bool> ChangeCustomerPasswordAsync(string userName, string password)
        {
            return await customerAccountDB.ChangePassword(userName, password);
        }
    }
}
