using Microsoft.ML;
using ml_net_logistic_regression_bank_churn.Models;
using ml_net_logistic_regression_bank_churn.Services.Interfaces;

namespace ml_net_logistic_regression_bank_churn.Services;

public class UserBehaviorModelTrainer(
    MLContext mlContext) : IUserBehaviorModelTrainer
{
    private readonly MLContext _mlContext = mlContext;

    public void TrainModel()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "UserBehaviourData.csv");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Data file not found");
        }

        var dataView = _mlContext.Data.LoadFromTextFile<UserBehaviorData>(
            path, separatorChar: ',', hasHeader: true);

        var pipeline = _mlContext.Transforms.Concatenate("Features", "Age", "PageViews", "TimeSpent") // Concatenate features
            .Append(_mlContext.Transforms.NormalizeMinMax("Features")) // Normalize features
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "ClickedAd"));

        var model = pipeline.Fit(dataView);

        CreateDirectoryIfNotExists("MLModel");

        _mlContext.Model.Save(model, dataView.Schema, "MLModel/UserBehaviorModel.zip");
    }

    private static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
