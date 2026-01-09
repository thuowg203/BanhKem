import 'package:flutter/material.dart';
import '../services/chat_service.dart';
import '../services/moderation_service.dart'; // Import service vừa tạo

class ChatView extends StatefulWidget {
  final String userId;
  final String fullName;

  const ChatView({super.key, required this.userId, required this.fullName});

  @override
  State<ChatView> createState() => _ChatViewState();
}

class _ChatViewState extends State<ChatView> {
  // Controllers
  final TextEditingController _textController = TextEditingController();
  final ScrollController _scrollController = ScrollController();

  // Services
  final ChatService _chatService = ChatService();
  final ModerationService _moderationService = ModerationService();

  // State Variables
  final List<Map<String, String>> _messages = [];
  bool _isCheckingContent = false; // Trạng thái đang check AI
  bool _isLoadingHistory = true;

  @override
  void initState() {
    super.initState();
    _initializeChat();
  }

  @override
  void dispose() {
    _chatService.disconnect();
    _textController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _initializeChat() async {
    try {
      // 1. Load lịch sử tin nhắn
      final history = await _chatService.getChatHistory(widget.userId);

      if (mounted) {
        setState(() {
          _messages.addAll(history.map((m) => {
            "from": m["isFromAdmin"] == true ? "Quản trị viên" : "Bạn",
            "text": m["text"].toString(),
          }));
          _isLoadingHistory = false;
        });
        _scrollToBottom();
      }

      // 2. Kết nối SignalR
      await _chatService.connect(widget.userId);

      // 3. Lắng nghe tin nhắn mới
      _chatService.connection.on("ReceiveMessage", (args) {
        if (!mounted) return;
        final sender = args?[0] ?? "Hệ thống";
        final text = args?[1] ?? "";

        setState(() {
          _messages.add({"from": sender.toString(), "text": text.toString()});
        });
        _scrollToBottom();
      });

    } catch (e) {
      if (mounted) {
        setState(() => _isLoadingHistory = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Lỗi kết nối: $e")),
        );
      }
    }
  }

  // Hàm cuộn xuống cuối danh sách
  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          _scrollController.position.maxScrollExtent,
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
        );
      }
    });
  }

  Future<void> _sendMessage() async {
    final msg = _textController.text.trim();
    if (msg.isEmpty) return;

    // 1. BẮT ĐẦU LOADING
    setState(() {
      _isCheckingContent = true;
    });

    try {
      // --- KIỂM TRA TỪ CẤM CỤC BỘ (Ưu tiên cái này trước vì miễn phí và nhanh) ---
      // Bạn nên định nghĩa list này ở ngoài hoặc trong Service
      final List<String> localBadWords = ["fuck", "con cac", "đm", "vkl", "chó"];
      for (var word in localBadWords) {
        if (msg.toLowerCase().contains(word)) {
          throw "Nội dung chứa từ ngữ không phù hợp (Local Filter).";
        }
      }

      // --- GỌI API OPENAI ---
      // Nếu API lỗi 429, service sẽ trả về true (cho qua) hoặc false tùy cách bạn viết.
      // Nhưng ở đây ta bọc try-catch để an toàn tuyệt đối.
      bool isSafe = true;
      try {
        isSafe = await _moderationService.isContentSafe(msg);
      } catch (apiError) {
        print("Lỗi API Moderation: $apiError");
        // QUAN TRỌNG: Nếu API lỗi (hết tiền, mất mạng), bạn muốn CHẶN hay CHO QUA?
        // Ở đây tôi để true (cho qua) để người dùng không bị chặn oan khi hệ thống lỗi.
        isSafe = true;
      }

      if (!isSafe) {
        throw "Nội dung vi phạm tiêu chuẩn cộng đồng (AI Filter).";
      }

      // --- NẾU MỌI THỨ OK THÌ GỬI ---
      _chatService.sendMessageToAdmin(widget.userId, msg);
      _textController.clear();

    } catch (e) {
      // Xử lý lỗi (Hiển thị thông báo đỏ)
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString().replaceAll("Exception: ", "")),
            backgroundColor: Colors.red,
            duration: const Duration(seconds: 3),
          ),
        );
      }
    } finally {
      // 2. TẮT LOADING (BẮT BUỘC)
      // Dòng này nằm trong finally nên nó SẼ LUÔN CHẠY dù có lỗi hay không
      if (mounted) {
        setState(() {
          _isCheckingContent = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text("Hỗ trợ - ${widget.fullName}"),
        backgroundColor: const Color(0xFFF77E6E),
        elevation: 0,
      ),
      body: Column(
        children: [
          // Hiển thị thanh loading khi đang check AI hoặc đang load lịch sử
          if (_isCheckingContent || _isLoadingHistory)
            const LinearProgressIndicator(
              backgroundColor: Color(0xFFFFE5E0),
              valueColor: AlwaysStoppedAnimation<Color>(Color(0xFFF77E6E)),
              minHeight: 2,
            ),

          // Danh sách tin nhắn
          Expanded(
            child: _messages.isEmpty && !_isLoadingHistory
                ? const Center(
                child: Text("Bắt đầu cuộc trò chuyện...",
                    style: TextStyle(color: Colors.grey)))
                : ListView.builder(
              controller: _scrollController,
              padding: const EdgeInsets.all(12),
              itemCount: _messages.length,
              itemBuilder: (_, i) {
                final msg = _messages[i];
                final isUser = msg["from"] == "Bạn";
                return Align(
                  alignment: isUser
                      ? Alignment.centerRight
                      : Alignment.centerLeft,
                  child: Container(
                    constraints: BoxConstraints(
                      maxWidth: MediaQuery.of(context).size.width * 0.75,
                    ),
                    margin: const EdgeInsets.symmetric(vertical: 4),
                    padding: const EdgeInsets.symmetric(
                        vertical: 10, horizontal: 16),
                    decoration: BoxDecoration(
                      color: isUser
                          ? const Color(0xFFF77E6E)
                          : Colors.grey.shade200,
                      borderRadius: BorderRadius.only(
                        topLeft: const Radius.circular(12),
                        topRight: const Radius.circular(12),
                        bottomLeft: isUser
                            ? const Radius.circular(12)
                            : Radius.zero,
                        bottomRight: isUser
                            ? Radius.zero
                            : const Radius.circular(12),
                      ),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          msg["text"]!,
                          style: TextStyle(
                            color: isUser ? Colors.white : Colors.black87,
                            fontSize: 15,
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),

          // Khu vực nhập tin nhắn
          SafeArea(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
              decoration: BoxDecoration(
                color: Colors.white,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    offset: const Offset(0, -2),
                    blurRadius: 5,
                  )
                ],
              ),
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _textController,
                      enabled: !_isCheckingContent, // Khóa khi đang check
                      decoration: InputDecoration(
                        hintText: _isCheckingContent
                            ? "Đang kiểm tra nội dung..."
                            : "Nhập tin nhắn...",
                        filled: true,
                        fillColor: Colors.grey.shade100,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(24),
                          borderSide: BorderSide.none,
                        ),
                        contentPadding: const EdgeInsets.symmetric(
                            vertical: 10, horizontal: 20),
                      ),
                      textInputAction: TextInputAction.send,
                      onSubmitted: (_) => _sendMessage(),
                    ),
                  ),
                  const SizedBox(width: 8),

                  // Nút gửi
                  CircleAvatar(
                    backgroundColor: _isCheckingContent
                        ? Colors.grey
                        : const Color(0xFFF77E6E),
                    child: IconButton(
                      icon: _isCheckingContent
                          ? const SizedBox(
                          width: 16, height: 16,
                          child: CircularProgressIndicator(
                              strokeWidth: 2, color: Colors.white)
                      )
                          : const Icon(Icons.send, color: Colors.white, size: 20),
                      onPressed: _isCheckingContent ? null : _sendMessage,
                    ),
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