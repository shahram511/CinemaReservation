using CinemaReservation.API.Extension;
using CinemaReservation.API.Middleware;
using CinemaReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplicationServices(builder.Configuration);

// Register the Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExeptionHandler>();
builder.Services.AddProblemDetails();


var app = builder.Build(); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


// Create a temporary scope to resolve the database context safely on startup
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    var context = service.GetRequiredService<ApplicationDbContext>();

    // it automatically applies the migrations to create the tab
    if (context.Database.IsRelational())
    {
        context.Database.Migrate();
    }
    else
    {
        context.Database.EnsureCreated();
    }   

    // Excute the seed logic
    DbSeeder.SeedSeats(context);
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles(); // this automatically expose a folder named wwwroot in our API project

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

