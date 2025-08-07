using Microsoft.ML;
using ml_net_logistic_regression_bank_churn.Models;
using ml_net_logistic_regression_bank_churn.Services;
using ml_net_logistic_regression_bank_churn.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTransient<MLContext>()
    .AddTransient<IUserBehaviorModelTrainer, UserBehaviorModelTrainer>()
    .AddTransient<IUserBehaviorPredict, UserBehaviorPredict>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/train", (IUserBehaviorModelTrainer trainer) =>
{
    trainer.TrainModel();
    return Results.Ok("Model trained successfully");
})
    .WithName("Train");

app.MapPost("/predict", (UserBehaviorData userBehaviorData, IUserBehaviorPredict predictor) =>
    predictor.Predict(userBehaviorData)
)
    .WithName("Predict");

app.Run();

