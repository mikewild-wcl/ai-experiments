using Microsoft.ML;
using ml_net_logistic_regression_bank_churn.Models;
using ml_net_logistic_regression_bank_churn.Services.Interfaces;

namespace ml_net_logistic_regression_bank_churn.Services;

public class UserBehaviorPredict : IUserBehaviorPredict
{
    private readonly PredictionEngine<UserBehaviorData, UserBehaviorPrediction> _predictionEngine;

    public UserBehaviorPredict(MLContext mlContext)
    {
        var model = mlContext.Model.Load("MLModel/UserBehaviorModel.zip", out _);
        _predictionEngine = mlContext.Model.CreatePredictionEngine<UserBehaviorData, UserBehaviorPrediction>(model);
    }

    public UserBehaviorPrediction Predict(UserBehaviorData userBehaviorData)
    {
        return _predictionEngine.Predict(userBehaviorData);
    }
}
