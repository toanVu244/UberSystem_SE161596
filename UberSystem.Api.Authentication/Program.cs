using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;
using System.Reflection;
using UberSystem.Api.Authentication.Extensions;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Repository;
using UberSystem.Service;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using UberSystem.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IEdmModel GetEdmModel()
{
    var odataBuilder = new ODataConventionModelBuilder();

    // Định nghĩa các entity set cho OData
    odataBuilder.EntitySet<User>("Users");  // Users là tập thực thể

    return odataBuilder.GetEdmModel();
}

builder.Services.AddControllers().AddOData(options =>
        options.Select().Filter().Expand().OrderBy().SetMaxTop(100).Count()
        .AddRouteComponents("odata", GetEdmModel()));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // Giới hạn kích thước file (100 MB)
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UberSystemAPI", Version = "v1" });

    // Đường dẫn đến tệp XML chứa chú thích
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Định nghĩa bảo mật Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT", // sửa lại tên định dạng cho chính xác
        Scheme = "Bearer"
    });

    // Yêu cầu bảo mật
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var connectionString = builder.Configuration.GetConnectionString("Default");
var configuration = builder.Configuration;
//DI services
builder.Services.AddDatabase(configuration).AddServices();

builder.Services.AddCors(options => options.AddPolicy("MyCor", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerPolicy", policy =>
        policy.RequireRole("Customer"));
    options.AddPolicy("DriverPolicy", policy =>
        policy.RequireRole("Driver"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("MyCor");
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
