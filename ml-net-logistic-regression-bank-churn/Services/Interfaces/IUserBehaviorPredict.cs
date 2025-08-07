using ml_net_logistic_regression_bank_churn.Models;

namespace ml_net_logistic_regression_bank_churn.Services.Interfaces;

public interface IUserBehaviorPredict
{
    UserBehaviorPrediction Predict(UserBehaviorData userBehaviorData);
}
