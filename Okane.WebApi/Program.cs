using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Okane.Application;
using Okane.Application.Auth.SignIn;
using Okane.Application.Auth.Signup;
using Okane.Application.Categories;
using Okane.Application.Categories.ById;
using Okane.Application.Categories.Create;
using Okane.Application.Categories.Delete;
using Okane.Application.Expenses;
using Okane.Application.Expenses.ById;
using Okane.Application.Expenses.Create;
using Okane.Application.Expenses.Delete;
using Okane.Application.Expenses.Retrieve;
using Okane.Application.Expenses.Update;
using Okane.Application.Responses;
using Okane.Storage.EF;
using Okane.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOkane()
    .AddOkaneEFStorage()
    .AddOkaneWebApi();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

const string idPath = "/{id}";

var auth = app.MapGroup("/auth");
auth.MapPost("/signup", (IRequestHandler<SignUpRequest, ISignUpResponse> handler, SignUpRequest request) =>
        handler.Handle(request).ToResult())
    .WithOpenApi();

auth.MapPost("/token", (IRequestHandler<SignInRequest, ISignInResponse> handler, SignInRequest request) =>
        handler.Handle(request).ToResult())
    .WithOpenApi();


var categories = app.MapGroup("/categories").RequireAuthorization();
categories.MapPost("/", (CreateCategoryHandler handler, CreateCategoryRequest request) =>
        handler.Handle(request).ToResult())
    .Produces<CategoryResponse>()
    .Produces<ConflictResponse>(StatusCodes.Status409Conflict)
    .WithOpenApi();

categories.MapGet(idPath, (GetCategoryByIdHandler handler, int id) =>
        handler.Handle(id).ToResult())
    .Produces<CategoryResponse>()
    .Produces<NotFoundResponse>(StatusCodes.Status404NotFound)
    .WithOpenApi();

categories.MapDelete(idPath, (DeleteCategoryHandler handler, int id) =>
        handler.Handle(id).ToResult())
    .Produces<CategoryResponse>()
    .Produces<NotFoundResponse>(StatusCodes.Status404NotFound)
    .WithOpenApi();

var expenses = app.MapGroup("/expenses").RequireAuthorization();
expenses.MapPost("/", (CreateExpenseHandler handler, CreateExpenseRequest request) =>
        handler.Handle(request).ToResult())
    .Produces<ExpenseResponse>()
    .Produces<ValidationErrorsResponse>(StatusCodes.Status400BadRequest)
    .WithOpenApi();

expenses.MapPut(idPath, (UpdateExpenseHandler handler, int id, UpdateExpenseRequest request) =>
        handler.Handle(id, request).ToResult())
    .Produces<ExpenseResponse>()
    .Produces<NotFoundResponse>(StatusCodes.Status404NotFound)
    .WithOpenApi();

expenses.MapDelete(idPath, (DeleteExpenseHandler handler, int id) =>
        handler.Handle(id).ToResult())
    .Produces<ExpenseResponse>()
    .Produces<NotFoundResponse>(StatusCodes.Status404NotFound)
    .WithOpenApi();

expenses.MapGet("/", (RetrieveExpensesHandler handler) =>
        handler.Handle())
    .WithOpenApi();

expenses.MapGet(idPath, (GetExpenseByIdHandler handler, int id) => 
        handler.Handle(id).ToResult())
    .Produces<ExpenseResponse>()
    .Produces<NotFoundResponse>(StatusCodes.Status404NotFound)
    .WithOpenApi();

app.Run();