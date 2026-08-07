using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Domain.Features.Card;
using YbsSmartCardSystem.Domain.Features.TopUp;
using YbsSmartCardSystem.Domain.Features.Bus;
using YbsSmartCardSystem.Domain.Features.Terminal;
using YbsSmartCardSystem.Domain.Features.Transaction;
using YbsSmartCardSystem.Domain.Features.Package;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
});

builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<TopUpService>();
builder.Services.AddScoped<BusService>();
builder.Services.AddScoped<TerminalService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<PackageService>();

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
