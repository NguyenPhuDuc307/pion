# Hướng dẫn tạo mới dự án Pion Fullstack từ đầu (chi tiết từng file + code mẫu)

## 1. Yêu cầu hệ thống

- Node.js >= 18
- .NET SDK >= 9.0
- Docker & Docker Compose (nếu muốn chạy bằng container)
- Git

## 2. Tạo mới Backend (pion-api)

### a. Khởi tạo project ASP.NET Core Web API
```bash
dotnet new webapi -n pion-api
cd pion-api
```

### b. Cài đặt các package cần thiết
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Swashbuckle.AspNetCore
```

### c. Tạo các thư mục cấu trúc
```bash
mkdir Controllers Models Data Services Dtos wwwroot wwwroot/uploads
```

### d. Tạo các file cần thiết (có code mẫu)

#### 1. Data/ApplicationDbContext.cs
```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using pion_api.Models;

namespace pion_api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; }
}
```

#### 2. Models/ApplicationUser.cs
```csharp
using Microsoft.AspNetCore.Identity;

namespace pion_api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
```

#### 3. Models/Product.cs
```csharp
namespace pion_api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
}
```

#### 4. Dtos/ProductDto.cs
```csharp
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace pion_api.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tags { get; set; } = "[]";
    public IFormFile? File { get; set; }
    public List<string> GetTagList() => JsonSerializer.Deserialize<List<string>>(Tags) ?? new();
}
```

#### 5. Services/IStorageService.cs, FileStorageService.cs
```csharp
// IStorageService.cs
public interface IStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
}

// FileStorageService.cs
public class FileStorageService : IStorageService
{
    private readonly string _uploadPath = Path.Combine("wwwroot", "uploads");
    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(_uploadPath, fileName);
        Directory.CreateDirectory(_uploadPath);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return $"/uploads/{fileName}";
    }
}
```

#### 6. Services/IJwtService.cs, JwtService.cs
```csharp
// IJwtService.cs
public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
}

// JwtService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config) => _config = config;
    public string GenerateToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("FullName", user.FullName ?? "")
        };
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### 7. Controllers/AuthController.cs
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pion_api.Models;
using pion_api.Dtos;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        var token = _jwtService.GenerateToken(user);
        return Ok(new { user.FullName, user.Email, Token = token });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized();
        var token = _jwtService.GenerateToken(user);
        return Ok(new { user.FullName, user.Email, Token = token });
    }
}
```

#### 8. Controllers/ProductsController.cs
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pion_api.Data;
using pion_api.Models;
using pion_api.Dtos;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    public ProductsController(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    [HttpGet]
    public IActionResult GetAll() => Ok(_context.Products.ToList());

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromForm] ProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Tags = dto.GetTagList(),
            ImageUrl = dto.File != null ? await _storageService.SaveFileAsync(dto.File) : string.Empty
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
    }
}
```

#### 9. Controllers/WeatherForecastController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using pion_api.Models;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = "Sample"
        });
    }
}
```

#### 10. Data/Migrations/
Tạo migration và update database:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 11. appsettings.json, appsettings.Development.json
- Cấu hình chuỗi kết nối SQLite, JWT, ...
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Jwt": {
    "Key": "your_secret_key",
    "Issuer": "your_issuer",
    "Audience": "your_audience"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### 12. Dockerfile, docker-compose.yml
- Docker hóa backend nếu cần

### e. Cấu hình Program.cs để sử dụng Identity, JWT, Swagger, Static Files, ...
```csharp
// ...existing code...
var builder = WebApplication.CreateBuilder(args);
// Add services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IStorageService, FileStorageService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ... JWT config ...
var app = builder.Build();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
// ...existing code...
```

### f. Chạy thử backend
```bash
dotnet run
```

## 3. Tạo mới Frontend (pion-fe)

### a. Khởi tạo project Angular
```bash
cd ..
npx @angular/cli new pion-fe --routing --style=css
cd pion-fe
```

### b. Cài đặt các package UI cần thiết
```bash
npm install bootstrap @ng-select/ng-select
```

### c. Tạo các module, component, service bằng Angular CLI
```bash
ng generate module app-routing --flat --module=app
ng generate component home
ng generate module products
ng generate component products/product-list
ng generate service api-authorization/auth
ng generate component api-authorization/login
ng generate component api-authorization/register
ng generate component nav-menu
ng generate service app/services/product
ng generate component weather
ng generate interface models/product
```

### d. Tạo các file cấu hình và thư mục còn lại (nếu cần)
- `src/app/app.routes.ts`:
```typescript
import { Routes } from '@angular/router';
export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'products', loadChildren: () => import('./products/products.module').then(m => m.ProductsModule) },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent }
];
```
- `src/app/models/product.model.ts`:
```typescript
export interface Product {
  id: number;
  name: string;
  tags: string[];
  imageUrl?: string;
}
```
- `src/app/services/product.service.ts`:
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../models/product.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = environment.apiUrl + '/products';
  constructor(private http: HttpClient) {}
  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }
  create(product: FormData): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }
}
```
- `src/app/api-authorization/auth.ts`:
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiUrl + '/auth';
  constructor(private http: HttpClient) {}
  login(data: any) {
    return this.http.post(this.apiUrl + '/login', data);
  }
  register(data: any) {
    return this.http.post(this.apiUrl + '/register', data);
  }
  setToken(token: string) {
    localStorage.setItem('token', token);
  }
  getToken() {
    return localStorage.getItem('token');
  }
}
```
- `src/app/api-authorization/auth-interceptor.ts`:
```typescript
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';
import { AuthService } from './auth';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService) {}
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = this.auth.getToken();
    if (token) {
      req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }
    return next.handle(req);
  }
}
```
- `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8000/api'
};
```

### e. Cấu hình styles và import Bootstrap
- Thêm vào `angular.json`:
```json
"styles": [
  "src/styles.css",
  "node_modules/bootstrap/dist/css/bootstrap.min.css",
  "node_modules/@ng-select/ng-select/themes/default.theme.css"
],
"scripts": [
  "node_modules/bootstrap/dist/js/bootstrap.bundle.min.js"
]
```

### f. Chạy thử frontend
```bash
npm start
```

## 4. Kết nối backend và frontend
- Đảm bảo backend chạy trước, frontend gọi đúng endpoint API.
- Sửa lại các file cấu hình nếu cần.

## 5. Chạy test
- Backend: tạo project test bằng `dotnet new xunit -n pion-api.Tests` và viết test cho controller, service.
- Frontend: dùng `ng test` để chạy unit test cho các component/service.

---

Nếu cần chi tiết code mẫu cho file nào khác, hãy yêu cầu cụ thể tên file hoặc chức năng bạn muốn!
