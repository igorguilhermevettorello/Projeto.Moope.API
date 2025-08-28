using Projeto.Moope.API.Configurations;
using Projeto.Moope.API.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddConectionConfig(builder.Configuration);
builder.Services.AddDependencyInjectionConfig(builder.Configuration);
builder.Services.AddCelPayServices(builder.Configuration);
builder.Services.AddIdentityConfig(builder.Configuration);
builder.Services.AddAuthConfig(builder.Configuration);
builder.Services.AddSwaggerConfig();
builder.Services.AddApiConfig(builder.Configuration);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddLoggingConfig(builder.Configuration);

var app = builder.Build();

app.UseSeedData();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwaggerConfig();

// Configurar CORS baseado no ambiente - DEVE vir antes da autenticação
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCorsPolicy");
}
else
{
    app.UseCors("CorsPolicy");
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseLoggingConfiguration();

app.Run();
