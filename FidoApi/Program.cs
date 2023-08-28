using Fido2NetLib;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFido2(options =>
{
    var fido2Config = new Fido2Configuration();
    builder.Configuration.GetSection("fido2").Bind(fido2Config);

    options.ServerDomain = fido2Config.ServerDomain;
    options.ServerName = "FIDO2 Test";
    options.Origins = fido2Config.Origins;
    options.TimestampDriftTolerance = fido2Config.TimestampDriftTolerance;
    options.MDSCacheDirPath = fido2Config.MDSCacheDirPath;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
