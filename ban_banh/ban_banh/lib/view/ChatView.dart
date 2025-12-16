import 'package:flutter/material.dart';
import '../services/chat_service.dart';

class ChatView extends StatefulWidget {
  final String userId;
  final String fullName;

  const ChatView({super.key, required this.userId, required this.fullName});

  @override
  State<ChatView> createState() => _ChatViewState();
}

class _ChatViewState extends State<ChatView> {
  final _controller = TextEditingController();
  final ChatService _chatService = ChatService();
  final List<Map<String, String>> _messages = [];

  @override
  void initState() {
    super.initState();
    _initializeChat();
  }

  Future<void> _initializeChat() async {
    try {
      // Load tin nhắn cũ
      final history = await _chatService.getChatHistory(widget.userId);
      setState(() {
        _messages.addAll(history.map((m) => {
          "from": m["isFromAdmin"] == true ? "Quản trị viên" : "Bạn",
          "text": m["text"].toString(),
        }));
      });

      // Kết nối tới SignalR
      await _chatService.connect(widget.userId);

      // Lắng nghe tin nhắn mới realtime
      _chatService.connection.on("ReceiveMessage", (args) {
        final sender = args?[0] ?? "Hệ thống";
        final text = args?[1] ?? "";
        setState(() {
          _messages.add({"from": sender.toString(), "text": text.toString()});
        });
      });
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi tải chat: $e")),
      );
    }
  }

  void _sendMessage() {
    final msg = _controller.text.trim();
    if (msg.isEmpty) return;

    _chatService.sendMessageToAdmin(widget.userId, msg);
    _controller.clear();
  }


  @override
  void dispose() {
    _chatService.disconnect();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text("Hỗ trợ khách hàng - ${widget.fullName}"),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: Column(
        children: [
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.all(8),
              itemCount: _messages.length,
              itemBuilder: (_, i) {
                final msg = _messages[i];
                final isUser = msg["from"] == "Bạn";
                return Align(
                  alignment:
                  isUser ? Alignment.centerRight : Alignment.centerLeft,
                  child: Container(
                    margin: const EdgeInsets.symmetric(vertical: 4),
                    padding: const EdgeInsets.symmetric(
                        vertical: 10, horizontal: 14),
                    decoration: BoxDecoration(
                      color: isUser
                          ? const Color(0xFFF77E6E)
                          : Colors.grey.shade300,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      msg["text"]!,
                      style: TextStyle(
                        color: isUser ? Colors.white : Colors.black87,
                      ),
                    ),
                  ),
                );
              },
            ),
          ),
          SafeArea(
            child: Padding(
              padding: const EdgeInsets.all(8),
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _controller,
                      decoration: InputDecoration(
                        hintText: "Nhập tin nhắn...",
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        contentPadding: const EdgeInsets.symmetric(
                            vertical: 10, horizontal: 14),
                      ),
                    ),
                  ),
                  const SizedBox(width: 6),
                  IconButton(
                    icon: const Icon(Icons.send, color: Color(0xFFF77E6E)),
                    onPressed: _sendMessage,
                  )
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
