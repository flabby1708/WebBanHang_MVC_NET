Tạo solution và project 
Tạo blank solution có tên SV<MaSV> ( vd: SV22T1020587)
- AdminLTE4, Bootstrap5

Bổ sung cho solution các project 
-	<SolutionName>.Admin : project dạng ASP.NET Core MVC (vd : SV22T1020587.Admin)
-	<SolutionName>.Shop : project dạng ASP.NET Core MVC
-	<SolutionName>.Models : project dưới dạng class library
-	<SolutionName>.BusinessLayers : project dưới dạng class library
-	<SolutionName>.DataLayers : project dưới dạng class library

Thiết kế layout cho admin 
-	Sử dụng theme adminTE4 , bootstrap5
-	Copy nội dung của file layout.html sang file _layout.cshtml
Các controller và các action dự kiến 
home 

-	Home / index: Trang chủ của admin, hiển thị dashboard tổng quan (ví dụ: thống kê đơn hàng, sản phẩm, khách hàng).
account  : các chức năng liên quan đến tài khoản 
-	Account/login: Trang đăng nhập cho nhân viên/admin.
-	account/logout: Đăng xuất tài khoản hiện tại.
-	account/changepassword: Thay đổi mật khẩu của tài khoản đang đăng nhập.
Supplier
-	supplier/index: Hiển thị danh sách nhà cung cấp (có thể phân trang và tìm kiếm).
-	supplier/create: Thêm mới nhà cung cấp.
-	supplier/edit/{id}: Chỉnh sửa thông tin nhà cung cấp theo ID.
-	supplier/delete/{id}: Xóa nhà cung cấp theo ID.
customer : 
-	customer/index: Hiển thị danh sách khách hàng dưới dạng phân trang, hỗ trợ tìm kiếm theo tên, và điều hướng đến các chức năng liên quan (như chỉnh sửa, xóa).
-	customer/create: Thêm mới khách hàng.
-	customer/edit/{id}: Chỉnh sửa thông tin khách hàng theo ID.
-	customer/delete/{id}: Xóa khách hàng theo ID.
-	customer/changepassword/{id}: Thay đổi mật khẩu cho khách hàng theo ID.
shipper :
-	shipper/index: Hiển thị danh sách người giao hàng (shipper).
-	shipper/create: Thêm mới shipper.
-	shipper/edit/{id}: Chỉnh sửa thông tin shipper theo ID.
-	shipper/delete/{id}: Xóa shipper theo ID.
employee :
-	employee/index: Hiển thị danh sách nhân viên.
-	employee/create: Thêm mới nhân viên.
-	employee/edit/{id}: Chỉnh sửa thông tin nhân viên theo ID.
-	employee/delete/{id}: Xóa nhân viên theo ID.
-	employee/changepassword/{id}: Thay đổi mật khẩu cho nhân viên theo ID.
-	employee/changrole/{id}: Thay đổi vai trò (role) của nhân viên theo ID.
category :
-	category/index: Hiển thị danh sách danh mục sản phẩm.
-	category/create: Thêm mới danh mục.
-	category/edit/{id}: Chỉnh sửa danh mục theo ID.
-	category/delete/{id}: Xóa danh mục theo ID.
product :
-	product/index: Hiển thị danh sách sản phẩm.
-	product/create: Thêm mới sản phẩm.
-	product/edit/{id}: Chỉnh sửa sản phẩm theo ID.
-	product/delete/{id}: Xóa sản phẩm theo ID.
-	product/listattributes/{id}: Liệt kê các thuộc tính (attributes) của sản phẩm theo ID.
-	product/createattributes/{id}: Thêm thuộc tính mới cho sản phẩm theo ID.
-	product/editattributes/{id}/attributeid = { attributeId }: Chỉnh sửa thuộc tính cụ thể của sản phẩm.
-	product/deleteattribute/{id}/attributeid = { attributeId }: Xóa thuộc tính của sản phẩm.
-	product/listPhotos/{id}: Liệt kê các ảnh của sản phẩm theo ID.
-	product/createPhoto/{id}: Thêm ảnh mới cho sản phẩm.
-	product/editPhotos/{id}?Photoid = { Photoid }: Chỉnh sửa ảnh của sản phẩm.
-	product/deletePhoto/{id}? Photoid = { Photoid }: Xóa ảnh của sản phẩm.
order :
-	order/index: Hiển thị danh sách đơn hàng.
-	order/create: Tạo đơn hàng mới.
-	order/edit/{id}: Chỉnh sửa đơn hàng theo ID.
-	order/delete/{id}: Xóa đơn hàng theo ID.
-	order/detail/{id}: Xem chi tiết đơn hàng theo ID.
-	order/updateStatus/{id}: Cập nhật trạng thái của đơn hàng (ví dụ: đang xử lý, đã giao, hủy).


<!-- Lưu ý -->
- input tìm kiếm/lọc mặt hàng: Loại hàng (select), NCC (select), Khoảng giá, Tên hàng
- input tìm kiếm đơn hàng: Trạng thái (select), Thời gian (từ ngày-đến ngày), tên KH
- 


# Thiết lập và Yêu cầu
- .NET 8.0 SDK (hoặc phiên bản mới hơn, như được chỉ định trong các file dự án [SV22T1020497.Admin.csproj](SV22T1020497.Admin/SV22T1020497.Admin.csproj) và [SV22T1020497.Shop.csproj](SV22T1020497.Shop/SV22T1020497.Shop.csproj)).
- Visual Studio 2022 hoặc VS Code với các tiện ích mở rộng .NET.
- CSDL: Đảm bảo thiết lập CSDL SQL tương thích (tham khảo schema.sql nếu có trong các mục giải pháp).
- Gói NuGet: Khôi phục qua `dotnet restore` hoặc Visual Studio.

# Xây dựng Dự án
1. Mở giải pháp [SV22T1020497.sln](SV22T1020497.sln) trong Visual Studio.
2. Xây dựng toàn bộ giải pháp (Ctrl+Shift+B) hoặc chạy `dotnet build` trong terminal.
3. Phụ thuộc dự án: [SV22T1020497.Admin](SV22T1020497.Admin/) và [SV22T1020497.Shop](SV22T1020497.Shop/) tham chiếu đến [SV22T1020497.Models](SV22T1020497.Models/), [SV22T1020497.BusinessLayers](SV22T1020497.BusinessLayers/), và [SV22T1020497.DataLayers](SV22T1020497.DataLayers/).

# Chạy Ứng dụng
- Cho Admin: Đặt [SV22T1020497.Admin](SV22T1020497.Admin/) làm dự án khởi động và chạy (F5 hoặc `dotnet run`).
- Cho Shop: Đặt [SV22T1020497.Shop](SV22T1020497.Shop/) làm dự án khởi động và chạy.
- Cổng mặc định: Admin thường chạy trên http://localhost:5114 (dựa trên nhật ký lỗi), Shop trên cổng riêng biệt.

# Layout và Giao diện
- Admin sử dụng theme AdminLTE4 với Bootstrap5.
- Sao chép nội dung của file layout.html sang [Views/Shared/_Layout.cshtml](SV22T1020497.Admin/Views/Shared/_Layout.cshtml) trong dự án Admin.
- Đảm bảo gọi `@RenderBody()` trong _Layout.cshtml để tránh lỗi "RenderBody has not been called" (vấn đề phổ biến trong nhật ký workspace).

# Vấn đề Thường gặp và Khắc phục
- **View Không Tìm Thấy (ví dụ: ChangePassword)**: Đảm bảo các view như [Views/Account/ChangePassword.cshtml](SV22T1020497.Admin/Views/Account/ChangePassword.cshtml) tồn tại ở vị trí đúng. Đường dẫn tìm kiếm: /Views/Account/ và /Views/Shared/.
- **Lỗi RenderBody**: Thêm `@RenderBody()` trong [Views/Shared/_Layout.cshtml](SV22T1020497.Admin/Views/Shared/_Layout.cshtml) cho cả dự án Admin và Shop.
- **Lỗi Xây dựng**: Kiểm tra tham chiếu dự án trong các file [project.assets.json](SV22T1020497.Admin/obj/project.assets.json). Chạy `dotnet clean` và xây dựng lại.
- **Tài sản Tĩnh**: Bootstrap và các file khác nằm trong [wwwroot](SV22T1020497.Admin/wwwroot/) (đã xác minh trong staticwebassets.upToDateCheck.txt).

# Tổng quan Cấu trúc Dự án
- [SV22T1020497.Admin](SV22T1020497.Admin/): Ứng dụng MVC cho các chức năng admin (controller, view, v.v.).
- [SV22T1020497.Shop](SV22T1020497.Shop/): Ứng dụng MVC cho giao diện shop.
- [SV22T1020497.Models](SV22T1020497.Models/): Các mô hình dữ liệu.
- [SV22T1020497.BusinessLayers](SV22T1020497.BusinessLayers/): Logic nghiệp vụ.
- [SV22T1020497.DataLayers](SV22T1020497.DataLayers/): Lớp truy cập dữ liệu.

