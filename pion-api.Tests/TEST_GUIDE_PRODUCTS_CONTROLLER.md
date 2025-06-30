# HƯỚNG DẪN TEST ĐƠN VỊ CHO PRODUCTS CONTROLLER (RÚT GỌN, ĐẠT COVERAGE CAO)

## 1. Mục tiêu
- Đảm bảo tất cả các nhánh logic chính của ProductsController được kiểm thử.
- Tối ưu số lượng test, tránh trùng lặp, nhưng vẫn đạt coverage cao nhất.

## 2. Cấu trúc test
- **Khởi tạo InMemoryDbContext**: Sử dụng `UseInMemoryDatabase` để tạo context giả lập cho mỗi test.
- **Mock IStorageService**: Dùng Moq để kiểm soát hành vi lưu/xóa file.
- **Khởi tạo controller**: Truyền context và mock service vào controller.

## 3. Các nhóm test chính
### a. CRUD cơ bản
- **GetProducts**: Đảm bảo trả về đúng số lượng sản phẩm.
- **GetProduct**: Test cả trường hợp tìm thấy và không tìm thấy (NotFound).
- **PostProduct**: Test tạo mới sản phẩm, cả khi có và không có file upload.
- **PutProduct**: Test cập nhật sản phẩm thành công và cập nhật không tìm thấy (NotFound).
- **DeleteProduct**: Test xóa sản phẩm thành công, xóa sản phẩm không tồn tại, xóa sản phẩm có ảnh (kiểm tra gọi xóa file).

### b. Search
- Test search có kết quả, không có kết quả, và search rỗng (trả về tất cả).

### c. Cập nhật/xóa với file
- Test cập nhật sản phẩm có file mới, đảm bảo file cũ bị xóa và file mới được lưu.
- Test xóa sản phẩm có ảnh, đảm bảo file được xóa.

### d. Concurrency Exception
- Test khi cập nhật sản phẩm gặp lỗi đồng bộ (DbUpdateConcurrencyException):
  - Nếu sản phẩm vẫn còn: controller ném lại exception.
  - Nếu sản phẩm không còn: controller trả về NotFound.

### e. **Test với các bảng có quan hệ khóa ngoại (Foreign Key)**
- **Seed dữ liệu liên quan**: Khi Product có quan hệ với bảng khác (ví dụ: Category, OrderDetail...), cần seed dữ liệu liên quan trước khi test.
- **Test xóa cascade**: Nếu cấu hình cascade delete, test xóa Product sẽ tự động xóa các entity liên quan.
- **Test lỗi khi xóa entity đang được tham chiếu**: Nếu không cascade, test xóa Product đang được tham chiếu sẽ ném exception (DbUpdateException).
- **Test truy vấn bao gồm navigation property**: Test lấy Product kèm dữ liệu liên quan (Include, ThenInclude).

**Ví dụ:**
```csharp
// Giả sử Product có FK đến Category
_context.Categories.Add(new Category { Id = 1, Name = "Cat1" });
_context.Products.Add(new Product { Id = 1, Name = "P1", CategoryId = 1 });
_context.SaveChanges();

// Test xóa category khi có product tham chiếu
await Assert.ThrowsAsync<DbUpdateException>(() => _context.Categories.Remove(category); _context.SaveChangesAsync());
```

## 4. Kỹ thuật tối ưu coverage
- Mỗi test đại diện cho một nhánh logic (thành công, thất bại, exception, tương tác với service).
- Sử dụng context phụ (TestDbContext) để ép ném exception.
- Dùng Moq để kiểm tra các hàm SaveFileAsync, DeleteFileAsync được gọi đúng.
- Không cần lặp lại các test tương tự, chỉ giữ lại test đại diện cho từng nhánh.

## 5. Ví dụ test tiêu biểu
```csharp
[Fact]
public async Task GetProduct_ReturnsProduct_AndNotFound()
{
    var ok = await _controller.GetProduct(1);
    Assert.Equal("Product 1", ok.Value!.Name);
    var notfound = await _controller.GetProduct(999);
    Assert.IsType<NotFoundResult>(notfound.Result);
}
```

```csharp
[Fact]
public async Task PutProduct_Concurrency_ThrowsOrNotFound()
{
    // Throw khi product còn
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
        .Options;
    var context = new TestDbContext(options);
    context.Products.Add(new Product { Id = 30, Name = "Old3", Tags = new List<string> { "t" } });
    context.SaveChanges();
    var controller = new ProductsController(context, _storageServiceMock.Object);
    var dto = new ProductDto { Name = "Update", Tags = "[\"t4\"]" };
    await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => controller.PutProduct(30, dto));
    // NotFound khi product không còn
    var context2 = new TestDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()).Options);
    var controller2 = new ProductsController(context2, _storageServiceMock.Object);
    var result = await controller2.PutProduct(12345, dto);
    Assert.IsType<NotFoundResult>(result);
}
```

## 6. Kết luận
- Bộ test này đã tối ưu, đảm bảo coverage cao nhất cho ProductsController.
- Khi thêm logic mới, chỉ cần bổ sung test cho nhánh mới phát sinh.
- Có thể áp dụng cách tổ chức này cho các controller/service khác để tiết kiệm thời gian mà vẫn đảm bảo chất lượng.

---
**Lưu ý:**  
- Coverage cao là điều kiện cần, nhưng vẫn nên review logic và test thủ công với các case đặc biệt nếu cần.
- Có thể mở rộng test cho các trường hợp validate, phân quyền, hoặc các logic nghiệp vụ đặc thù khác nếu muốn kiểm soát sâu hơn.
