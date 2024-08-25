using Microsoft.EntityFrameworkCore;
using SalCentral.Api.DbContext;
using SalCentral.Api.Logics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<UserLogic>();
builder.Services.AddScoped<PaginationLogic>();
builder.Services.AddScoped<BranchLogic>();
builder.Services.AddScoped<BranchAssignmentLogic>();
builder.Services.AddScoped<AttendanceLogic>();
builder.Services.AddScoped<DeductionLogic>();
builder.Services.AddScoped<ScheduleLogic>(); 

var app = builder.Build();                                                                                                                                                                               

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
