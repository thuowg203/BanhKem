using Microsoft.ML.Data;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    // Dữ liệu đầu vào (Comment của khách)
    public class SentimentData
    {
        [LoadColumn(0)]
        public string SentimentText { get; set; }

        [LoadColumn(1)]
        public bool Sentiment { get; set; } // true = Tích cực, false = Tiêu cực
    }

    // Kết quả dự đoán
    public class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; } // Kết quả AI đoán

        public float Probability { get; set; } // Độ tin cậy (0.0 - 1.0)

        public float Score { get; set; }
    }
}