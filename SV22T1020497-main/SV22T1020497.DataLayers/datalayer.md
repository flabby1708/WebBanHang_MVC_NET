# IGenericRepository:
Cung cấp các chức năng để làm việc với các bảng trong DB
## - SupplierRepository
## - ShipperRepository
## - CategoryRepository


# IProductRepository:
## ProductRepository

# ICustomerRepository:
## - CustomerRepository

# IEmployeeRepository:
## - EmployeeRepository

# IOrderRepository:
## - OrderRepository

# IDataDictionaryRepository:
## - ProvinceRepository

# IUserAccountRepository:
## - EmployeeAccountRepository
## - CustomerAccountRepository


cho 1 csdl được cài đặt như sau:
-- 1. Bảng Provinces: Lưu danh sách các tỉnh/thành phố
CREATE TABLE [dbo].[Provinces]
(
	[ProvinceName] [nvarchar](255) NOT NULL PRIMARY KEY
) 
GO
-- 2. Bảng Suppliers: Lưu danh sách nhà cung cấp
CREATE TABLE [dbo].[Suppliers]
(
	[SupplierID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[SupplierName] [nvarchar](255) NOT NULL,
	[ContactName] [nvarchar](255) NOT NULL,
	[Province] [nvarchar](255) NULL,
	[Address] [nvarchar](255) NULL,
	[Phone] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL
)
GO
-- 3. Bảng Customers: Lưu danh sách khách hàng
CREATE TABLE [dbo].[Customers]
(
	[CustomerID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[CustomerName] [nvarchar](255) NOT NULL,
	[ContactName] [nvarchar](255) NOT NULL,
	[Province] [nvarchar](255) NULL,
	[Address] [nvarchar](255) NULL,
	[Phone] [nvarchar](255) NULL,
	[Email] [nvarchar](50) NULL,
	[Password] [nvarchar](50) NULL,
	[IsLocked] [bit] NULL
)
GO

-- 4. Bảng Employees: Lưu dữ liệu nhân viên
CREATE TABLE [dbo].[Employees]
(
	[EmployeeID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[FullName] [nvarchar](255) NOT NULL,
	[BirthDate] [date] NULL,
	[Address] [nvarchar](255) NULL,
	[Phone] [nvarchar](255) NULL,
	[Email] [nvarchar](50) NULL UNIQUE,
	[Password] [nvarchar](50) NULL,
	[Photo] [nvarchar](255) NULL,
	[IsWorking] [bit] NULL,
	[RoleNames] [nvarchar](500) NULL
)
GO

-- 5. Bảng Shippers: Lưu dữ liệu người giao hàng
CREATE TABLE [dbo].[Shippers]
(
	[ShipperID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ShipperName] [nvarchar](255) NOT NULL,
	[Phone] [nvarchar](255) NULL
)
GO

-- 6. Bảng Categories: Lưu danh mục loại hàng
CREATE TABLE [dbo].[Categories]
(
	[CategoryID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[CategoryName] [nvarchar](255) NOT NULL,
    [Description] [nvarchar](255) NULL,
    [Photo] [nvarchar](255) NULL
)
GO

-- 7. Bảng Products: Lưu dữ liệu mặt hàng
CREATE TABLE [dbo].[Products]
(
	[ProductID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ProductName] [nvarchar](255) NOT NULL,
	[ProductDescription] [nvarchar](2000) NULL,
	[SupplierID] [int] NULL,
	[CategoryID] [int] NULL,
	[Unit] [nvarchar](255) NOT NULL,
	[Price] [money] NOT NULL,
	[Photo] [nvarchar](255) NULL,
	[IsSelling] [bit] NULL
)
GO

-- 8. Bảng ProductAttributes: Lưu danh sách các thuộc tính của mặt hàng
CREATE TABLE [dbo].[ProductAttributes]
(
	[AttributeID] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ProductID] [int] NOT NULL,
	[AttributeName] [nvarchar](255) NOT NULL,
	[AttributeValue] [nvarchar](500) NOT NULL,
	[DisplayOrder] [int] NOT NULL
)
GO

-- 9. Bảng ProductPhotos: Lưu danh sách ảnh của mặt hàng
CREATE TABLE [dbo].[ProductPhotos]
(
	[PhotoID] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ProductID] [int] NOT NULL,
	[Photo] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[IsHidden] [bit] NOT NULL
)
GO

-- 10. Bảng OrderStatus: Lưu dữ liệu định nghĩa các trạng thái của đơn hàng
CREATE TABLE [dbo].[OrderStatus]
(
	[Status] [int] NOT NULL PRIMARY KEY,
	[Description] [nvarchar](50) NOT NULL
)
GO

-- 11. Bảng Orders: Lưu dữ liệu đơn hàng
CREATE TABLE [dbo].[Orders]
(
	[OrderID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[CustomerID] [int] NULL,
	[OrderTime] [datetime] NOT NULL,
	[DeliveryProvince] [nvarchar](255) NULL,
	[DeliveryAddress] [nvarchar](255) NULL,
	[EmployeeID] [int] NULL,
	[AcceptTime] [datetime] NULL,
	[ShipperID] [int] NULL,
	[ShippedTime] [datetime] NULL,
	[FinishedTime] [datetime] NULL,
	[Status] [int] NOT NULL	
)
GO

-- 12. Bảng OrderDetails: Lưu thông tin chi tiết các mặt hàng được bán trong đơn hàng
CREATE TABLE [dbo].[OrderDetails]
(
	[OrderID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[SalePrice] [money] NOT NULL,
	PRIMARY KEY ([OrderID], [ProductID])
)
GO

-- Thiết lập mối quan hệ giữa các bảng
ALTER TABLE [dbo].[Suppliers]  
ADD FOREIGN KEY([Province])
	REFERENCES [dbo].[Provinces] ([ProvinceName])
GO

ALTER TABLE [dbo].[Customers]  
ADD	FOREIGN KEY([Province])
	REFERENCES [dbo].[Provinces] ([ProvinceName])
GO

ALTER TABLE [dbo].[Products]
ADD	FOREIGN KEY([CategoryID])
	REFERENCES [dbo].[Categories] ([CategoryID])
GO

ALTER TABLE [dbo].[Products]  
ADD	FOREIGN KEY([SupplierID])
	REFERENCES [dbo].[Suppliers] ([SupplierID])
GO

ALTER TABLE [dbo].[ProductAttributes] 
ADD	FOREIGN KEY([ProductID])
	REFERENCES [dbo].[Products] ([ProductID])
GO

ALTER TABLE [dbo].[ProductPhotos]
ADD	FOREIGN KEY([ProductID])
	REFERENCES [dbo].[Products] ([ProductID])
GO

ALTER TABLE [dbo].[Orders]  
ADD	FOREIGN KEY([CustomerID])
	REFERENCES [dbo].[Customers] ([CustomerID])
GO

ALTER TABLE [dbo].[Orders]  
ADD FOREIGN KEY([EmployeeID])
	REFERENCES [dbo].[Employees] ([EmployeeID])
GO

ALTER TABLE [dbo].[Orders]
ADD	FOREIGN KEY([ShipperID])
	REFERENCES [dbo].[Shippers] ([ShipperID])
GO

ALTER TABLE [dbo].[Orders]
ADD	FOREIGN KEY([Status])
	REFERENCES [dbo].[OrderStatus] ([Status])
GO

ALTER TABLE [dbo].[OrderDetails]  
ADD	FOREIGN KEY([OrderID])
	REFERENCES [dbo].[Orders] ([OrderID])
GO

ALTER TABLE [dbo].[OrderDetails]  
ADD FOREIGN KEY([ProductID])
	REFERENCES [dbo].[Products] ([ProductID])
GO


cho các lớp sau:
namespace SV22T1020218.Models.Common
{
    /// <summary>
    /// Lớp dùng để biểu diễn thông tin đầu vào của một truy vấn/tìm kiếm 
    /// dữ liệu đơn giản dưới dạng phân trang
    /// </summary>
    public class PaginationSearchInput
    {
        private const int MaxPageSize = 100; //Giới hạn tối đa 100 dòng mỗi trang
        private int _page = 1;
        private int _pageSize = 20;
        private string _searchValue = "";
        
        /// <summary>
        /// Trang cần được hiển thị (bắt đầu từ 1)
        /// </summary>
        public int Page 
        { 
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }
        /// <summary>
        /// Số dòng được hiển thị trên mỗi trang
        /// (0 có nghĩa là hiển thị tất cả các dòng trên một trang, tức là không phân trang)
        /// </summary>
        public int PageSize 
        { 
            get => _pageSize; 
            set
            {
                if (value < 0)
                    _pageSize = 0;
                else if (value > MaxPageSize)
                    _pageSize = MaxPageSize;
                else
                    _pageSize = value;
            }
        }
        /// <summary>
        /// Giá trị tìm kiếm (nếu có) được sử dụng để lọc dữ liệu 
        /// (Nếu không có giá trị tìm kiếm, thì để rỗng)
        /// </summary>
        public string SearchValue
        { 
            get => _searchValue; 
            set => _searchValue = value?.Trim() ?? ""; 
        }        
        /// <summary>
        /// Số dòng cần bỏ qua (tính từ dòng đầu tiên của tập dữ liệu) 
        /// để lấy dữ liệu cho trang hiện tại
        /// </summary>
        public int Offset => PageSize > 0 ? (Page - 1) * PageSize : 0;
    }
}

namespace SV22T1020218.Models.Common
{
    /// <summary>
    /// Phần tử trên thanh phân trang, có thể là một số trang hoặc dấu "..." để phân cách các nhóm trang
    /// </summary>
    public class PageItem
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pageNumber">0 nếu là phần tử dùng để thể hiện dấu "..." phân cách</param>
        /// <param name="isCurrent"></param>
        public PageItem(int pageNumber, bool isCurrent = false)
        {
            Page = pageNumber;
            IsCurrent = isCurrent;
        }
        /// <summary>
        /// Số trang (có giá trị là 0 nếu là dấu "..." để phân cách các nhóm trang)
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Có phải là trang hiện tại hay không?
        /// </summary>
        public bool IsCurrent { get; set; }
        /// <summary>
        /// Có phải là vị trí hiển thị dấu "..." để phân cách các nhóm trang hay không?
        /// </summary>
        public bool IsEllipsis => Page == 0;
    }
}


namespace SV22T1020218.Models.Common
{
    /// <summary>
    /// Lớp dùng để biểu diễn kết quả truy vấn/tìm kiếm dữ liệu dưới dạng phân trang
    /// </summary>
    /// <typeparam name="T">Kiểu của dữ liệu truy vấn được</typeparam>
    public class PagedResult<T> where T : class
    {
        /// <summary>
        /// Trang đang được hiển thị
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Số dòng được hiển thị trên mỗi trang (0 có nghĩa là hiển thị tất cả các dòng trên một trang/không phân trang)
        /// </summary>
        public int PageSize { get; set; }        
        /// <summary>
        /// Tổng số dòng dữ liệu được tìm thấy
        /// </summary>
        public int RowCount { get; set; }
        /// <summary>
        /// Danh sách các dòng dữ liệu được hiển thị trên trang hiện tại
        /// </summary>
        public List<T> DataItems { get; set; } = new List<T>();

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int PageCount
        {
            get
            {
                if (PageSize == 0)
                    return 1;
                return (int)Math.Ceiling((decimal)RowCount / PageSize);
            }
        }
        /// <summary>
        /// Có trang trước không?
        /// </summary>
        public bool HasPreviousPage => Page > 1;
        /// <summary>
        /// Có trang sau không?
        /// </summary>
        public bool HasNextPage => Page < PageCount;             
        /// <summary>
        /// Lấy danh sách các trang được hiển thị trên thanh phân trang
        /// </summary>
        /// <param name="n">Số lượng trang lân cận trang hiện tại cần được hiển thị</param>
        /// <returns></returns>
        public List<PageItem> GetDisplayPages(int n = 5)
        {
            var result = new List<PageItem>();

            if (PageCount == 0)
                return result;

            n = n > 0 ? n : 5; //Giá trị n không hợp lệ, đặt lại về mặc định            

            int currentPage = Page;
            if (currentPage < 1) 
                currentPage = 1;
            else if (currentPage > PageCount)
                currentPage = PageCount;

            int displayedPages = 2 * n + 1;     //Số lượng trang tối đa hiển thị trên thanh phân trang (bao gồm cả trang hiện tại)
            int startPage = currentPage - n;    //Trang bắt đầu hiển thị
            int endPage = currentPage + n;      //Trang kết thúc hiển thị

            //Nếu thiếu bên trái
            if (startPage < 1)
            {
                endPage += (1 - startPage);
                startPage = 1;
            }

            //Nếu thiếu bên phải
            if (endPage > PageCount)
            {
                startPage -= (endPage - PageCount);
                endPage = PageCount;
            }

            //Gán lại bằng 1 nếu startPage bị âm sau khi trừ
            if (startPage < 1)
                startPage = 1;

            //Đảm bảo không vượt quá displayedPages
            if (endPage - startPage + 1 > displayedPages)
                endPage = startPage + displayedPages - 1;

            //Trang đầu
            if (startPage > 1)
            {
                result.Add(new PageItem(1, currentPage == 1));
                //Thêm dấu "..." để phân cách nếu có nhiều trang ở giữa
                if (startPage > 2)
                    result.Add(new PageItem(0));
            }

            //Trang hiện tại và các trang lân cận
            for (int i = startPage; i <= endPage; i++)
            {
                result.Add(new PageItem(i, i == currentPage));
            }

            //Trang cuối
            if (endPage < PageCount)
            {
                //Thêm dấu "..." để phân cách nếu có nhiều trang ở giữa
                if (endPage < PageCount - 1)
                    result.Add(new PageItem(0));
                result.Add(new PageItem(PageCount, currentPage == PageCount));
            }

            return result;
        }
    }
}

Cho interface như sau:
using SV22T1020218.Models.Common;

namespace SV22T1020218.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu đơn giản trên một
    /// kiểu dữ liệu T nào đó (T là một Entity/DomainModel nào đó)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Truy vấn, tìm kiếm dữ liệu và trả về kết quả dưới dạng được phân trang
        /// </summary>
        /// <param name="input">Đầu vào tìm kiếm, phân trang</param>
        /// <returns></returns>
        Task<PagedResult<T>> ListAsync(PaginationSearchInput input);
        /// <summary>
        /// Lấy dữ liệu của một bản ghi có mã là id (trả về null nếu không có dữ liệu)
        /// </summary>
        /// <param name="id">Mã của dữ liệu cần lấy</param>
        /// <returns></returns>
        Task<T?> GetAsync(int id);
        /// <summary>
        /// Bổ sung một bản ghi vào bảng trong CSDL
        /// </summary>
        /// <param name="data">Dữ liệu cần bổ sung</param>
        /// <returns>Mã của dòng dữ liệu được bổ sung (thường là IDENTITY)</returns>
        Task<int> AddAsync(T data);
        /// <summary>
        /// Cập nhật một bản ghi trong bảng của CSDL
        /// </summary>
        /// <param name="data">Dữ liệu cần cập nhật</param>
        /// <returns></returns>
        Task<bool> UpdateAsync(T data);
        /// <summary>
        /// Xóa bản ghi có mã là id
        /// </summary>
        /// <param name="id">Mã của bản ghi cần xóa</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(int id);
        /// <summary>
        /// Kiểm tra xem một bản ghi có mã là id có dữ liệu liên quan hay không?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> IsUsed(int id);
    }
}

Viết lớp SupplierRepository cho entity Supplier sau, cài đặt interface trên.
namespace SV22T1020218.Models.Partner
{
    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int SupplierID { get; set; }
        /// <summary>
        /// Tên nhà cung cấp
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;
        /// <summary>
        /// Tên giao dịch
        /// </summary>
        public string ContactName { get; set; } = string.Empty;
        /// <summary>
        /// Tỉnh thành
        /// </summary>
        public string? Province { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Điện thoại
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }
    }
}

Yêu cầu: 
- Constructor của lớp có tham số đầu vào là connectionString.
- Sử dụng Dapper, Microsoft.Data.SqlClient để làm việc với CSDL SQL Server
- Lớp thuộc namespace SV22T1020218.DataLayers.SQLServer
- viết đầy đủ summary cho lớp và hàm
