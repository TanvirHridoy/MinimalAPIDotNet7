using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalAPI.Auth;
using MinimalAPI.DBModels;
using MinimalAPI.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services
ApiConst apiConst = new ApiConst();
builder.Configuration.GetSection("ApiConst").Bind(apiConst);

builder.Services.AddDbContext<APIDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("MinimalDb")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
}).AddJwtBearer("JwtBearer", jwtoptions =>
{
    jwtoptions.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = apiConst.Issuer,
        ValidAudience = apiConst.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiConst.key)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

#endregion

var app = builder.Build();

#region Configure 

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

#endregion



#region Endpoints
#region JWT
app.MapPost("/api/Auth", (IConfiguration config, AuthRequest model) =>
{
    AuthResp authResp = new AuthResp();
    try
    {
        string Username = "shellncore";
        string Password = "2022";

        if (model.Password == Password && model.userName == Username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiConst.key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                   apiConst.Issuer,
                   apiConst.Audience,
                   expires: DateTime.UtcNow.AddMinutes(apiConst.expiresIn),
                   signingCredentials: creds

                );
            authResp.Token = new JwtSecurityTokenHandler().WriteToken(token);
            authResp.Expiration = token.ValidTo;
            authResp.UserName = Username;
            return Results.Ok(authResp);
        }
        else
        {
            return Results.BadRequest("Invalid User");
        }

    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
}

).AllowAnonymous();

#endregion

#region People endpoints
app.MapGet("api/people", async (APIDbContext _Db) => await _Db.People.ToListAsync()).RequireAuthorization();
app.MapGet("api/people/{Id}", async (APIDbContext _Db,int Id) => await _Db.People.SingleOrDefaultAsync(e=>e.Id==Id)).RequireAuthorization();

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
}).RequireAuthorization();

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
}).RequireAuthorization();


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
}).RequireAuthorization();


#endregion


#endregion



app.Run();
