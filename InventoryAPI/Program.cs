// File: Program.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: this is the main entry point for the app
//              it sets up all the services and middleware
//              https is enforced here for data in transit (Week 10)

using InventoryAPI.Models;

// create the web app builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// add controllers to the app
builder.Services.AddControllers();

// add swagger for testing our api endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add the database helper as a service so all controllers can use it
builder.Services.AddSingleton<DatabaseHelper>();

// add cors so our frontend can talk to the backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // allow the frontend to make requests to the backend
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// build the app
WebApplication app = builder.Build();

// use swagger only in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// enforce https for all requests (Week 10 - data in transit)
app.UseStaticFiles();
app.UseHttpsRedirection();

// use the cors policy we set up above
app.UseCors("AllowFrontend");

// use authorization middleware
app.UseAuthorization();

// map all controllers to routes
app.MapControllers();

// start the app
app.Run();