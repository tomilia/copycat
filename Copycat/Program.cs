using Copycat;
using Microsoft.AspNetCore.Hosting;
using StackExchange.Redis;
using System;

var builder = WebApplication.CreateBuilder(args);
string _filePath = "/FileUpload";
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<FileService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis"));
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!Directory.Exists(_filePath))
{
    Directory.CreateDirectory(_filePath);
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("corsapp");
app.MapControllers();

app.Run();

