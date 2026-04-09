using Backend.Services;
using Backend.Repositories;
using Backend.Databases;
using Backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
    .AddServices()
    .AddRepositories()
    .AddDatabases();

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

