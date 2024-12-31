using Personal.Data;
using Personal.LibraryAPI.Endpoints;
using Personal.LibraryAPI.Helper;
using Personal.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (hostingContext, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", _ =>
    {
        _.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddConfiguration();
builder.Services.AddDataDependencies(builder.Configuration);
builder.Services.AddServiceDependencies(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOrigin");
app.ConfigureCountriesEndpoints();
app.ConfigureAuthorEndpoints();
app.ConfigureBookEndpoints();
app.ConfigurePageReadingEndpoints();
app.ConfigureLayoutEndpoints();
app.ConfigureBookPartitionEndpoints();
app.UseHttpsRedirection();


app.Run();
