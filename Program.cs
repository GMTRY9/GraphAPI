using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GraphAPI.Data;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<GraphAPIContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GraphAPIContext") ?? throw new InvalidOperationException("Connection string 'GraphAPIContext' not found."))
    );

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://192.168.0.1:5201");
                      });
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


