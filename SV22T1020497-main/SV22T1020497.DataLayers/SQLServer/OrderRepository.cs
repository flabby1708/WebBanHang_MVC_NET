using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Sales;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho đơn hàng trên SQL Server.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu.</param>
        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">Thông tin đầu vào tìm kiếm đơn hàng.</param>
        /// <returns>Kết quả dữ liệu phân trang.</returns>
        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            input ??= new OrderSearchInput();
            const string fromClause = @"
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID";

            const string whereClause = @"
WHERE (@SearchValue = N'' OR c.CustomerName LIKE @Keyword OR c.ContactName LIKE @Keyword OR c.Phone LIKE @Keyword)
  AND (@Status = 0 OR o.Status = @Status)
  AND (@DateFrom IS NULL OR CAST(o.OrderTime AS date) >= @DateFrom)
  AND (@DateTo IS NULL OR CAST(o.OrderTime AS date) <= @DateTo)";

            string countSql = $@"SELECT COUNT(*) {fromClause} {whereClause};";
            string dataSql = input.PageSize > 0
                ? $@"
SELECT o.OrderID, o.CustomerID, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
       o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime,
       CAST(o.Status AS int) AS Status,
       ISNULL(e.FullName, N'') AS EmployeeName,
       ISNULL(c.CustomerName, N'') AS CustomerName,
       ISNULL(c.ContactName, N'') AS CustomerContactName,
       ISNULL(c.Email, N'') AS CustomerEmail,
       ISNULL(c.Phone, N'') AS CustomerPhone,
       ISNULL(c.Address, N'') AS CustomerAddress,
       ISNULL(s.ShipperName, N'') AS ShipperName,
       ISNULL(s.Phone, N'') AS ShipperPhone
{fromClause}
{whereClause}
ORDER BY o.OrderTime DESC, o.OrderID DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"
                : $@"
SELECT o.OrderID, o.CustomerID, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
       o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime,
       CAST(o.Status AS int) AS Status,
       ISNULL(e.FullName, N'') AS EmployeeName,
       ISNULL(c.CustomerName, N'') AS CustomerName,
       ISNULL(c.ContactName, N'') AS CustomerContactName,
       ISNULL(c.Email, N'') AS CustomerEmail,
       ISNULL(c.Phone, N'') AS CustomerPhone,
       ISNULL(c.Address, N'') AS CustomerAddress,
       ISNULL(s.ShipperName, N'') AS ShipperName,
       ISNULL(s.Phone, N'') AS ShipperPhone
{fromClause}
{whereClause}
ORDER BY o.OrderTime DESC, o.OrderID DESC;";

            var parameters = new
            {
                input.SearchValue,
                Keyword = $"%{input.SearchValue}%",
                Status = (int)input.Status,
                input.DateFrom,
                input.DateTo,
                input.Offset,
                input.PageSize
            };

            await using var connection = new SqlConnection(_connectionString);
            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<OrderViewInfo>(dataSql, parameters);

            return new PagedResult<OrderViewInfo>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một đơn hàng theo mã.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <returns>Thông tin đơn hàng hoặc null nếu không tồn tại.</returns>
        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            const string sql = @"
SELECT o.OrderID, o.CustomerID, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
       o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime,
       CAST(o.Status AS int) AS Status,
       ISNULL(e.FullName, N'') AS EmployeeName,
       ISNULL(c.CustomerName, N'') AS CustomerName,
       ISNULL(c.ContactName, N'') AS CustomerContactName,
       ISNULL(c.Email, N'') AS CustomerEmail,
       ISNULL(c.Phone, N'') AS CustomerPhone,
       ISNULL(c.Address, N'') AS CustomerAddress,
       ISNULL(s.ShipperName, N'') AS ShipperName,
       ISNULL(s.Phone, N'') AS ShipperPhone
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
WHERE o.OrderID = @OrderID;";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { OrderID = orderID });
        }

        /// <summary>
        /// Lấy danh sách đơn hàng thuộc về một khách hàng cụ thể.
        /// </summary>
        /// <param name="customerID">Mã khách hàng.</param>
        /// <returns>Danh sách đơn hàng của khách hàng.</returns>
        public async Task<List<OrderViewInfo>> ListByCustomerAsync(int customerID)
        {
            const string sql = @"
SELECT o.OrderID, o.CustomerID, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
       o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime,
       CAST(o.Status AS int) AS Status,
       ISNULL(e.FullName, N'') AS EmployeeName,
       ISNULL(c.CustomerName, N'') AS CustomerName,
       ISNULL(c.ContactName, N'') AS CustomerContactName,
       ISNULL(c.Email, N'') AS CustomerEmail,
       ISNULL(c.Phone, N'') AS CustomerPhone,
       ISNULL(c.Address, N'') AS CustomerAddress,
       ISNULL(s.ShipperName, N'') AS ShipperName,
       ISNULL(s.Phone, N'') AS ShipperPhone
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
WHERE o.CustomerID = @CustomerID
ORDER BY o.OrderTime DESC, o.OrderID DESC;";

            await using var connection = new SqlConnection(_connectionString);
            var items = await connection.QueryAsync<OrderViewInfo>(sql, new { CustomerID = customerID });
            return items.ToList();
        }

        /// <summary>
        /// Bổ sung một đơn hàng mới.
        /// </summary>
        /// <param name="data">Dữ liệu đơn hàng cần bổ sung.</param>
        /// <returns>Mã đơn hàng vừa được tạo.</returns>
        public async Task<int> AddAsync(Order data)
        {
            const string sql = @"
INSERT INTO Orders(CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, AcceptTime, ShipperID, ShippedTime, FinishedTime, Status)
VALUES (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @EmployeeID, @AcceptTime, @ShipperID, @ShippedTime, @FinishedTime, @Status);
SELECT CAST(SCOPE_IDENTITY() AS int);";

            var parameters = new
            {
                data.CustomerID,
                data.OrderTime,
                data.DeliveryProvince,
                data.DeliveryAddress,
                data.EmployeeID,
                data.AcceptTime,
                data.ShipperID,
                data.ShippedTime,
                data.FinishedTime,
                Status = (int)data.Status
            };

            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng.
        /// </summary>
        /// <param name="data">Dữ liệu đơn hàng sau chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateAsync(Order data)
        {
            const string sql = @"
UPDATE Orders
SET CustomerID = @CustomerID,
    OrderTime = @OrderTime,
    DeliveryProvince = @DeliveryProvince,
    DeliveryAddress = @DeliveryAddress,
    EmployeeID = @EmployeeID,
    AcceptTime = @AcceptTime,
    ShipperID = @ShipperID,
    ShippedTime = @ShippedTime,
    FinishedTime = @FinishedTime,
    Status = @Status
WHERE OrderID = @OrderID;";

            var parameters = new
            {
                data.OrderID,
                data.CustomerID,
                data.OrderTime,
                data.DeliveryProvince,
                data.DeliveryAddress,
                data.EmployeeID,
                data.AcceptTime,
                data.ShipperID,
                data.ShippedTime,
                data.FinishedTime,
                Status = (int)data.Status
            };

            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, parameters) > 0;
        }

        /// <summary>
        /// Xóa đơn hàng theo mã.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteAsync(int orderID)
        {
            const string sql = @"DELETE FROM Orders WHERE OrderID = @OrderID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { OrderID = orderID }) > 0;
        }

        /// <summary>
        /// Lấy danh sách mặt hàng thuộc một đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <returns>Danh sách chi tiết mặt hàng trong đơn hàng.</returns>
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            const string sql = @"
SELECT d.OrderID, d.ProductID, d.Quantity, d.SalePrice,
       ISNULL(p.ProductName, N'') AS ProductName,
       ISNULL(p.Unit, N'') AS Unit,
       ISNULL(p.Photo, N'') AS Photo
FROM OrderDetails d
LEFT JOIN Products p ON d.ProductID = p.ProductID
WHERE d.OrderID = @OrderID
ORDER BY p.ProductName, d.ProductID;";

            await using var connection = new SqlConnection(_connectionString);
            var items = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { OrderID = orderID });
            return items.ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng trong đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>Thông tin chi tiết hoặc null nếu không tồn tại.</returns>
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            const string sql = @"
SELECT d.OrderID, d.ProductID, d.Quantity, d.SalePrice,
       ISNULL(p.ProductName, N'') AS ProductName,
       ISNULL(p.Unit, N'') AS Unit,
       ISNULL(p.Photo, N'') AS Photo
FROM OrderDetails d
LEFT JOIN Products p ON d.ProductID = p.ProductID
WHERE d.OrderID = @OrderID AND d.ProductID = @ProductID;";

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql, new { OrderID = orderID, ProductID = productID });
        }

        /// <summary>
        /// Bổ sung một mặt hàng vào đơn hàng.
        /// </summary>
        /// <param name="data">Dữ liệu chi tiết mặt hàng cần bổ sung.</param>
        /// <returns>True nếu bổ sung thành công, ngược lại là false.</returns>
        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            const string sql = @"INSERT INTO OrderDetails(OrderID, ProductID, Quantity, SalePrice)
                                 VALUES (@OrderID, @ProductID, @Quantity, @SalePrice);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong đơn hàng.
        /// </summary>
        /// <param name="data">Dữ liệu chi tiết mặt hàng sau chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            const string sql = @"UPDATE OrderDetails
                                 SET Quantity = @Quantity,
                                     SalePrice = @SalePrice
                                 WHERE OrderID = @OrderID AND ProductID = @ProductID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            const string sql = @"DELETE FROM OrderDetails
                                 WHERE OrderID = @OrderID AND ProductID = @ProductID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { OrderID = orderID, ProductID = productID }) > 0;
        }
    }
}
