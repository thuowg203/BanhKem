using Microsoft.ML;
using DoAnLapTrinhWeb_QLyTiemBanh.Models;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Services
{
    public class SentimentService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private PredictionEngine<SentimentData, SentimentPrediction> _predictionEngine;

        public SentimentService()
        {
            _mlContext = new MLContext();
            TrainModel(); // Huấn luyện ngay khi khởi tạo Service
        }

        private void TrainModel()
        {
            // Dữ liệu mẫu (Càng nhiều càng chính xác) - Bạn có thể bổ sung thêm
            var data = new List<SentimentData>
            {
                // --- TÍCH CỰC (Positive - true) ---
                new SentimentData { SentimentText = "Bánh ngon quá", Sentiment = true },
                new SentimentData { SentimentText = "Tuyệt vời", Sentiment = true },
                new SentimentData { SentimentText = "Rất hài lòng", Sentiment = true },
                new SentimentData { SentimentText = "Bánh mềm thơm", Sentiment = true },
                new SentimentData { SentimentText = "Giao hàng nhanh, bánh đẹp", Sentiment = true },
                new SentimentData { SentimentText = "Sẽ ủng hộ lần sau", Sentiment = true },
                new SentimentData { SentimentText = "Kem béo ngậy, rất thích", Sentiment = true },
                new SentimentData { SentimentText = "Chất lượng tốt", Sentiment = true },
                new SentimentData { SentimentText = "Ok", Sentiment = true },
                new SentimentData { SentimentText = "Good", Sentiment = true },

                // --- TIÊU CỰC (Negative - false) ---
                new SentimentData { SentimentText = "Bánh dở tệ", Sentiment = false },
                new SentimentData { SentimentText = "Không ngon", Sentiment = false },
                new SentimentData { SentimentText = "Thất vọng", Sentiment = false },
                new SentimentData { SentimentText = "Bánh bị khô", Sentiment = false },
                new SentimentData { SentimentText = "Kem chua lèo", Sentiment = false },
                new SentimentData { SentimentText = "Phí tiền", Sentiment = false },
                new SentimentData { SentimentText = "Thái độ phục vụ kém", Sentiment = false },
                new SentimentData { SentimentText = "Đừng mua", Sentiment = false },
                new SentimentData { SentimentText = "Như hạch", Sentiment = false },
                new SentimentData { SentimentText = "Chó chết", Sentiment = false }, // Từ tục
                new SentimentData { SentimentText = "Ngu", Sentiment = false }
            };

            // Chuyển dữ liệu vào ML.NET
            IDataView trainingData = _mlContext.Data.LoadFromEnumerable(data);

            // Tạo Pipeline huấn luyện
            // FeaturizeText: Chuyển văn bản thành số để máy hiểu
            // SdcaLogisticRegression: Thuật toán phân loại
            var pipeline = _mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentData.SentimentText))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: nameof(SentimentData.Sentiment), featureColumnName: "Features"));

            // Huấn luyện mô hình
            _model = pipeline.Fit(trainingData);

            // Tạo engine dự đoán
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
        }

        public SentimentPrediction Predict(string comment)
        {
            var input = new SentimentData { SentimentText = comment };
            return _predictionEngine.Predict(input);
        }
    }
}