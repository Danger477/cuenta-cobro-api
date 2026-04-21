var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Permite que cualquier dispositivo (celular, navegador) pueda llamar la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("PermitirTodo");
app.MapControllers();
app.Run();