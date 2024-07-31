using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.Logics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ServerConnection")));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<UserLogic>();
builder.Services.AddScoped<PaginationLogic>();
builder.Services.AddScoped<BranchLogic>();
builder.Services.AddScoped<AttendanceLogic>();

var app = builder.Build();                                                                                                                                                                               

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
