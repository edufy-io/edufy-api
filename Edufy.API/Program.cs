using Edufy.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPresentation();

var app = builder.Build();

app.UsePresentation();

app.Run();
