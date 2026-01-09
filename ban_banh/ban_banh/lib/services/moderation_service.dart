import 'package:google_generative_ai/google_generative_ai.dart';

class ModerationService {
  // Nhớ dán API Key của bạn vào đây
  static const String _apiKey = 'AIzaSyAUumUf5oO5WGsXiOI1t3XoM0EjH68B0lk';

  late final GenerativeModel _model;

  ModerationService() {
    _model = GenerativeModel(
      // Thử dùng tên chính xác này sau khi đã update thư viện
      model: 'gemini-pro',
      apiKey: _apiKey,
      safetySettings: [
        SafetySetting(HarmCategory.harassment, HarmBlockThreshold.low),
        SafetySetting(HarmCategory.hateSpeech, HarmBlockThreshold.low),
        SafetySetting(HarmCategory.sexuallyExplicit, HarmBlockThreshold.low),
        SafetySetting(HarmCategory.dangerousContent, HarmBlockThreshold.low),
      ],
    );
  }

  // Danh sách từ cấm cứng (Local)
  final List<String> _badWords = [
    "đm", "đéo", "đù", "đụ", "cặc", "lồn", "buồi", "đĩ", "phò", "chó chết", "ngu", "óc chó", "điên",
    "dm", "dkm", "vkl", "vcl", "vl", "deo", "du ma", "chet", "chết"
    "cac", "lon", "buoi", "cut", "cho chet",
    "cc", "cl", "dmm", "cmm", "fuck", "shit", "bitch"
  ];

  Future<bool> isContentSafe(String text) async {
    if (text.trim().isEmpty) return true;
    final String lowerText = text.toLowerCase();

    // LỚP 1: KIỂM TRA LOCAL (Regex)
    for (var badWord in _badWords) {
      final regExp = RegExp(r'\b' + RegExp.escape(badWord) + r'\b', caseSensitive: false, unicode: true);
      if (regExp.hasMatch(text)) {
        print("🚫 Chặn bởi bộ lọc Regex: '$badWord'");
        return false;
      }
    }

    // LỚP 2: KIỂM TRA BẰNG GEMINI AI
    try {
      final prompt = '''
      Bạn là hệ thống kiểm duyệt.
      Kiểm tra câu: "$text"
      Nếu chứa chửi thề, xúc phạm, thô tục (kể cả viết tắt, không dấu như 'vcl', 'dkm', 'ngu') -> Trả về "UNSAFE".
      Nếu an toàn -> Trả về "SAFE".
      Chỉ trả lời 1 từ.
      ''';

      final content = [Content.text(prompt)];
      final response = await _model.generateContent(content);

      final resultText = response.text?.trim().toUpperCase() ?? "SAFE";
      print("🤖 Gemini ('gemini-pro') đánh giá: $resultText");

      if (resultText.contains("UNSAFE")) return false;
      if (response.promptFeedback?.blockReason != null) return false;

      return true;
    } catch (e) {
      // In lỗi ra để biết tại sao
      print("⚠️ Lỗi gọi Gemini: $e");

      // MẸO DEBUG:
      // - Khi đang test: Để 'return false' để biết chắc chắn API có chạy không.
      // - Khi chạy thật: Để 'return true' để không chặn nhầm người dùng nếu mạng lag.
      return true;
    }
  }
}