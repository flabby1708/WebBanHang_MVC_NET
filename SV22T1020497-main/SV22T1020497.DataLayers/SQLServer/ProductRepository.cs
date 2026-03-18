using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020497.DataLayers.Interfaces;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;

namespace SV22T1020497.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho mặt hàng trên SQL Server.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối SQL Server.
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu.</param>
        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và lọc mặt hàng.</param>
        /// <returns>Kết quả dữ liệu phân trang.</returns>
        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            input ??= new ProductSearchInput();
            const string whereClause = @"
WHERE (@SearchValue = N'' OR ProductName LIKE @Keyword)
  AND (@CategoryID = 0 OR CategoryID = @CategoryID)
  AND (@SupplierID = 0 OR SupplierID = @SupplierID)
  AND (@MinPrice <= 0 OR Price >= @MinPrice)
  AND (@MaxPrice <= 0 OR Price <= @MaxPrice)";

            string countSql = $@"SELECT COUNT(*) FROM Products {whereClause};";
            string dataSql = input.PageSize > 0
                ? $@"SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling
                     FROM Products
                     {whereClause}
                     ORDER BY ProductName
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"
                : $@"SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling
                     FROM Products
                     {whereClause}
                     ORDER BY ProductName;";

            var parameters = new
            {
                input.SearchValue,
                Keyword = $"%{input.SearchValue}%",
                input.CategoryID,
                input.SupplierID,
                input.MinPrice,
                input.MaxPrice,
                input.Offset,
                input.PageSize
            };

            await using var connection = new SqlConnection(_connectionString);
            int rowCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<Product>(dataSql, parameters);

            return new PagedResult<Product>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin một mặt hàng theo mã.
        /// </summary>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>Thông tin mặt hàng hoặc null nếu không tồn tại.</returns>
        public async Task<Product?> GetAsync(int productID)
        {
            const string sql = @"SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling
                                 FROM Products
                                 WHERE ProductID = @ProductID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { ProductID = productID });
        }

        /// <summary>
        /// Bổ sung một mặt hàng mới.
        /// </summary>
        /// <param name="data">Dữ liệu mặt hàng cần bổ sung.</param>
        /// <returns>Mã mặt hàng vừa được tạo.</returns>
        public async Task<int> AddAsync(Product data)
        {
            const string sql = @"INSERT INTO Products(ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                                 VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                                 SELECT CAST(SCOPE_IDENTITY() AS int);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin một mặt hàng.
        /// </summary>
        /// <param name="data">Dữ liệu mặt hàng sau chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateAsync(Product data)
        {
            const string sql = @"UPDATE Products
                                 SET ProductName = @ProductName,
                                     ProductDescription = @ProductDescription,
                                     SupplierID = @SupplierID,
                                     CategoryID = @CategoryID,
                                     Unit = @Unit,
                                     Price = @Price,
                                     Photo = @Photo,
                                     IsSelling = @IsSelling
                                 WHERE ProductID = @ProductID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa một mặt hàng theo mã.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteAsync(int productID)
        {
            const string sql = @"DELETE FROM Products WHERE ProductID = @ProductID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { ProductID = productID }) > 0;
        }

        /// <summary>
        /// Kiểm tra mặt hàng có đang được sử dụng trong dữ liệu đơn hàng hay không.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần kiểm tra.</param>
        /// <returns>True nếu có dữ liệu liên quan, ngược lại là false.</returns>
        public async Task<bool> IsUsedAsync(int productID)
        {
            const string sql = @"SELECT CAST(CASE WHEN EXISTS
                                    (SELECT 1 FROM OrderDetails WHERE ProductID = @ProductID)
                                THEN 1 ELSE 0 END AS bit);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { ProductID = productID });
        }

        /// <summary>
        /// Lấy danh sách thuộc tính của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>Danh sách thuộc tính của mặt hàng.</returns>
        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            const string sql = @"SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder
                                 FROM ProductAttributes
                                 WHERE ProductID = @ProductID
                                 ORDER BY DisplayOrder, AttributeName;";
            await using var connection = new SqlConnection(_connectionString);
            var items = await connection.QueryAsync<ProductAttribute>(sql, new { ProductID = productID });
            return items.ToList();
        }

        /// <summary>
        /// Lấy thông tin của một thuộc tính mặt hàng.
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính.</param>
        /// <returns>Thông tin thuộc tính hoặc null nếu không tồn tại.</returns>
        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            const string sql = @"SELECT AttributeID, ProductID, AttributeName, AttributeValue, DisplayOrder
                                 FROM ProductAttributes
                                 WHERE AttributeID = @AttributeID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { AttributeID = attributeID });
        }

        /// <summary>
        /// Bổ sung thuộc tính cho mặt hàng.
        /// </summary>
        /// <param name="data">Dữ liệu thuộc tính cần bổ sung.</param>
        /// <returns>Mã thuộc tính vừa được tạo.</returns>
        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            const string sql = @"INSERT INTO ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                                 VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                                 SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="data">Dữ liệu thuộc tính sau chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            const string sql = @"UPDATE ProductAttributes
                                 SET AttributeName = @AttributeName,
                                     AttributeValue = @AttributeValue,
                                     DisplayOrder = @DisplayOrder
                                 WHERE AttributeID = @AttributeID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa một thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            const string sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { AttributeID = attributeID }) > 0;
        }

        /// <summary>
        /// Lấy danh sách ảnh của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>Danh sách ảnh của mặt hàng.</returns>
        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            const string sql = @"SELECT PhotoID, ProductID, Photo, Description, DisplayOrder, IsHidden
                                 FROM ProductPhotos
                                 WHERE ProductID = @ProductID
                                 ORDER BY DisplayOrder, PhotoID;";
            await using var connection = new SqlConnection(_connectionString);
            var items = await connection.QueryAsync<ProductPhoto>(sql, new { ProductID = productID });
            return items.ToList();
        }

        /// <summary>
        /// Lấy thông tin một ảnh của mặt hàng.
        /// </summary>
        /// <param name="photoID">Mã ảnh.</param>
        /// <returns>Thông tin ảnh hoặc null nếu không tồn tại.</returns>
        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            const string sql = @"SELECT PhotoID, ProductID, Photo, Description, DisplayOrder, IsHidden
                                 FROM ProductPhotos
                                 WHERE PhotoID = @PhotoID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { PhotoID = photoID });
        }

        /// <summary>
        /// Bổ sung ảnh cho mặt hàng.
        /// </summary>
        /// <param name="data">Dữ liệu ảnh cần bổ sung.</param>
        /// <returns>Mã ảnh vừa được tạo.</returns>
        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            const string sql = @"INSERT INTO ProductPhotos(ProductID, Photo, Description, DisplayOrder, IsHidden)
                                 VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                                 SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin ảnh của mặt hàng.
        /// </summary>
        /// <param name="data">Dữ liệu ảnh sau chỉnh sửa.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            const string sql = @"UPDATE ProductPhotos
                                 SET Photo = @Photo,
                                     Description = @Description,
                                     DisplayOrder = @DisplayOrder,
                                     IsHidden = @IsHidden
                                 WHERE PhotoID = @PhotoID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        /// <summary>
        /// Xóa một ảnh của mặt hàng.
        /// </summary>
        /// <param name="photoID">Mã ảnh cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại là false.</returns>
        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            const string sql = @"DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID;";
            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, new { PhotoID = photoID }) > 0;
        }
    }
}
