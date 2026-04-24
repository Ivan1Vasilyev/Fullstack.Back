using Backend.Databases;
using Backend.FileLoaders;
using Backend.Middleware;
using Backend.Repositories;
using Backend.Services;
using Backend.Utils;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#if DEBUG
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "NextJsClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});
#endif

builder.Services.AddControllers();

builder.Services
    .AddDatabases()
    .AddUtils()
    .AddRepositories()
    .AddServices()
    .AddFileLoaders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#if DEBUG
app.UseCors("NextJsClient");
#endif

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

enum InternetTehnologyEnum
{
    val1, val2, val3, val4
};