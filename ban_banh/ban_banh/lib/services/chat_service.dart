import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:signalr_core/signalr_core.dart';

class ChatService {
  late HubConnection connection;
  static const String baseUrl = "http://10.0.2.2:5006";

  // 🔌 Kết nối tới ChatHub
  Future<void> connect(String userId) async {
    connection = HubConnectionBuilder()
        .withUrl(
      "$baseUrl/chathub?userId=$userId",
      HttpConnectionOptions(
        logging: (level, message) => print(message),
      ),
    )
        .build();

    await connection.start();
    print("✅ Connected to chat hub");
  }

  // 💬 Gửi tin nhắn đến admin
  Future<void> sendMessageToAdmin(String userId, String message) async {
    await connection.invoke("SendMessageToAdmin", args: [userId, message]);
  }

  // 🧾 Lấy lịch sử tin nhắn
  Future<List<Map<String, dynamic>>> getChatHistory(String userId) async {
    final url = Uri.parse("$baseUrl/api/ChatApi/history?userId=$userId");
    final response = await http.get(url);

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body) as List;
      return data.map((m) => Map<String, dynamic>.from(m)).toList();
    } else {
      throw Exception("Không thể tải lịch sử chat: ${response.body}");
    }
  }

  Future<void> disconnect() async {
    await connection.stop();
    print("🔌 Disconnected");
  }
}
