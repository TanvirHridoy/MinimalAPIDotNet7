using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MinimalAPI.DBModels;
using MinimalAPI.ViewModels;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services

builder.Services.AddDbContext<APIDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("MinimalDb")));
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

#endregion





var app = builder.Build();

#region Configure 

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

#endregion

#region Endpoints
app.MapGet("/", () => "Hello World!");
app.MapGet("/ShellNCore", () => "Welcome to ShellNCore");

#region People endpoints
app.MapGet("api/people", async (APIDbContext _Db) => await _Db.People.ToListAsync()).AllowAnonymous();
app.MapGet("api/people/{Id}", async (APIDbContext _Db,int Id) => await _Db.People.SingleOrDefaultAsync(e=>e.Id==Id)).AllowAnonymous();

app.MapPost("api/people", async (APIDbContext _Db, Person model) =>
{
    try
    {
        await _Db.People.AddAsync(model);
        await _Db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
}).AllowAnonymous();

app.MapPut("api/people/{Id}", async (APIDbContext _Db, int Id, PersonVM model) =>
{
    var omodel = await _Db.People.FindAsync(Id);
    if (omodel == null) { return Results.BadRequest("Not Found"); }
    else
    {
        try
        {
            omodel.DoB = model.DoB;
            omodel.Age = model.Age;
            _Db.Update(omodel);
            await _Db.SaveChangesAsync();
            return Results.Ok();
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }
}).AllowAnonymous();


app.MapDelete("api/people/{Id}", async (APIDbContext _Db, int Id) =>
{
    var OModel = await _Db.People.FindAsync(Id);
    if (OModel != null)
    {
        try
        {
            _Db.People.Remove(OModel);
            await _Db.SaveChangesAsync();
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
    else
    {
        return Results.BadRequest("That person doesn't exists");
    }
   // return Results.Ok();
}).AllowAnonymous();


#endregion


#endregion



app.Run();
