<<<<<<< HEAD
﻿using FoodDelivery.API.Common;
=======
﻿using FoodDelivery.API.Common; // 👈 Nhớ using cái này để dùng AppRoles
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
using FoodDelivery.API.Hubs;
using FoodDelivery.API.Services;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using FoodDelivery.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
<<<<<<< HEAD
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

// ---------------------------------------------------------
// 🛠️ 0. CẤU HÌNH CỐT LÕI
// ---------------------------------------------------------
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

=======
using System.Text.Json.Serialization; // 👈 Dùng cho cấu hình JSON

var builder = WebApplication.CreateBuilder(args);

>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
// ==========================================
// 1. CẤU HÌNH DATABASE
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString,
        b => b.MigrationsAssembly("FoodDelivery.Infrastructure")));

// ==========================================
<<<<<<< HEAD
// 2. CẤU HÌNH IDENTITY
// ==========================================
=======
// 2. CẤU HÌNH IDENTITY (USER/ROLE)
// ==========================================

// 👇 SỬA LẠI ĐOẠN NÀY
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ==========================================
<<<<<<< HEAD
// 3. CẤU HÌNH JWT AUTHENTICATION
// ==========================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new Exception("❌ LỖI: Chưa cấu hình đầy đủ JWT trong appsettings.json (Key, Issuer, Audience)");
}

=======
// 3. CẤU HÌNH JWT AUTHENTICATION (+ SIGNALR FIX)
// ==========================================
var secretKey = builder.Configuration["JwtSettings:Secret"];
var issuer = builder.Configuration["JwtSettings:Issuer"];
var audience = builder.Configuration["JwtSettings:Audience"];

// Tìm đoạn này trong Program.cs
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
<<<<<<< HEAD
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        //RoleClaimType = "role",
        NameClaimType = "name"
=======
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Key 'Jwt:Key' not found in appsettings.json")
        )),

        // 👇👇👇 THÊM DÒNG NÀY ĐỂ FIX LỖI 403 (QUAN TRỌNG NHẤT) 👇👇👇
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
               (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/orderHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ==========================================
<<<<<<< HEAD
// 4. SERVICES
// ==========================================
=======
// 4. CẤU HÌNH SERVICES (Controller, Swagger, SignalR, CORS)
// ==========================================

// 👇 CẤU HÌNH JSON: Fix lỗi "Cycle detected" khi trả về dữ liệu có quan hệ vòng tròn
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
<<<<<<< HEAD
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "FoodDelivery API", Version = "v1" });
=======

// --- CẤU HÌNH SWAGGER ---
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "FoodDelivery API", Version = "v1" });

>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập Token: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
<<<<<<< HEAD
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type=ReferenceType.SecurityScheme, Id="Bearer" } },
=======

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type=ReferenceType.SecurityScheme, Id="Bearer" }
            },
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
<<<<<<< HEAD
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPayOSService, PayOSService>();
builder.Services.AddMemoryCache();
=======

// --- CẤU HÌNH SIGNALR ---
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
<<<<<<< HEAD
    options.AddPolicy("AllowClient", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ==========================================
// 5. MIDDLEWARE
// ==========================================
=======
    options.AddPolicy("AllowClient",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true) // 👈 Quan trọng: Chấp nhận mọi nguồn (kể cả file:// hay localhost khác)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Bắt buộc cho SignalR
        });
});

//đăng ký dịch vụ và thêm dòng này vào để kích hoạt Cache:
builder.Services.AddMemoryCache();

//Đăng ký cho email
builder.Services.AddScoped<IEmailService, EmailService>();

// Đăng ký Service cho VNPAY
builder.Services.AddScoped<IPayOSService, PayOSService>();
var app = builder.Build();

// ==========================================
// 5. PIPELINE & MIDDLEWARE
// ==========================================

// Middleware xử lý lỗi toàn cục (Tránh hiện lỗi 500 xấu xí)
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = "Lỗi hệ thống! Vui lòng thử lại sau." });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

<<<<<<< HEAD
var uploadPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(Path.Combine(uploadPath, "chats")))
{
    Directory.CreateDirectory(Path.Combine(uploadPath, "chats"));
}

app.UseStaticFiles();
=======
//app.UseStaticFiles();
// Cho phép truy cập thư mục uploads bên ngoài wwwroot
// --- ĐOẠN NÀY ĐỂ TRƯỚC app.UseCors ---

var uploadPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
var chatPath = Path.Combine(uploadPath, "chats");

// Tạo thư mục bằng lệnh này sẽ tự tạo cả cha lẫn con nếu thiếu
if (!Directory.Exists(chatPath))
{
    Directory.CreateDirectory(chatPath);
    Console.WriteLine($"---> Đã tạo thư mục tại: {chatPath}");
}

app.UseStaticFiles(); // Cho wwwroot

// Cấu hình Static Files cho uploads - Sử dụng app.Environment thay vì builder
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

<<<<<<< HEAD
app.UseCors("AllowClient");

app.Use(async (context, next) =>
{
    var accessToken = context.Request.Query["access_token"];
    if (!string.IsNullOrEmpty(accessToken))
    {
        context.Request.Headers.Authorization = "Bearer " + accessToken;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/orderHub");
app.MapHub<ChatHub>("/chatHub");

// ==========================================
// 6. AUTO SEED DATA (GỘP CHUNG VÀ TỐI ƯU)
=======
app.UseCors("AllowClient"); // CORS phải đứng trước Auth

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/orderHub");

app.MapHub<ChatHub>("/chatHub");

// ==========================================
// 6. AUTO SEED DATA (Tự động tạo Role)
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
// ==========================================
using (var scope = app.Services.CreateScope())
{
    try
    {
<<<<<<< HEAD
        var services = scope.ServiceProvider;
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        // 1. Tạo các Roles nếu chưa có
        string[] roles = [AppRoles.SuperAdmin, AppRoles.Merchant, AppRoles.Staff, AppRoles.Shipper, AppRoles.Customer];
=======
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 👇 Dùng hằng số từ AppRoles cho chuẩn, không gõ tay
        string[] roles = [
            AppRoles.SuperAdmin,
            AppRoles.Merchant,
            AppRoles.Staff,
            AppRoles.Shipper,
            AppRoles.Customer
        ];

>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
<<<<<<< HEAD

        // 2. Tạo tài khoản Admin mặc định
        var adminEmail = "Admin@gmail.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var newAdmin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Super Admin",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(newAdmin, "Admin@123");

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, AppRoles.SuperAdmin);
                Console.WriteLine("✅ TẠO ADMIN THÀNH CÔNG: Admin@gmail.com / Admin@123");
            }
            else
            {
                Console.WriteLine("❌ Lỗi tạo Admin: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Lỗi Seeding Data: " + ex.Message);
    }
}

app.Run();
=======
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi khởi tạo Role: " + ex.Message);
    }
}

app.Run();
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
