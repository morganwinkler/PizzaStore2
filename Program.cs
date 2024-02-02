using Microsoft.EntityFrameworkCore;
using PizzaStore.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// checks the configuration provider for a connection string named Pizzas. If it doesn't find one, it will use Data Source=Pizzas.db as the connection string
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";

builder.Services.AddEndpointsApiExplorer();
// replaces in-memory DB with SQLite
builder.Services.AddSqlite<PizzaDb>(connectionString);
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "PizzaStore API",
    Description = "Making the Pizzas you love",
    Version = "v1"
  });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});


// get ALL
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());
// post CREATE
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
  await db.Pizzas.AddAsync(pizza);
  await db.SaveChangesAsync();
  return Results.Created($"/pizza/{pizza.Id}", pizza);
});
// get by ID
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));
// put UPDATE
app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatepizza, int id) =>
{
  var pizza = await db.Pizzas.FindAsync(id);
  if (pizza is null) return Results.NotFound();
  pizza.Name = updatepizza.Name;
  pizza.Description = updatepizza.Description;
  await db.SaveChangesAsync();
  return Results.NoContent();
});
// DELETE
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
  var pizza = await db.Pizzas.FindAsync(id);
  if (pizza is null)
  {
    return Results.NotFound();
  }
  db.Pizzas.Remove(pizza);
  await db.SaveChangesAsync();
  return Results.Ok();
});

app.Run();
