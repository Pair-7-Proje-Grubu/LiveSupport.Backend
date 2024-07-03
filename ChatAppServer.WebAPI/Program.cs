using ChatAppServer.WebAPI.Context;
using ChatAppServer.WebAPI.Hubs;
using DefaultCorsPolicyNugetPackage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder.WithOrigins("http://localhost:4200") // Angular adresi
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});


builder.Services.AddDefaultCors();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("SqlServer")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp"); //cors

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chat-hub");  //frontend baglantisi icin

app.Run();
