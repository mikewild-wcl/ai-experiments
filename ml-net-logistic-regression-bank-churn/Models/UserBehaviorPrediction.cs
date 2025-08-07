using Microsoft.ML.Data;

namespace ml_net_logistic_regression_bank_churn.Models;

public class UserBehaviorPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Probability { get; set; }
}
