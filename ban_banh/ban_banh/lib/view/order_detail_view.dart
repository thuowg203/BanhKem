import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../services/api_service.dart';

class OrderDetailView extends StatefulWidget {
  final int orderId;

  const OrderDetailView({super.key, required this.orderId});

  @override
  State<OrderDetailView> createState() => _OrderDetailViewState();
}

class _OrderDetailViewState extends State<OrderDetailView> {
  Map<String, dynamic>? order;
  bool isLoading = true;
  bool isError = false;

  @override
  void initState() {
    super.initState();
    _fetchOrderDetail();
  }

  Future<void> _fetchOrderDetail() async {
    try {
      final data = await ApiService.getOrderDetail(widget.orderId);
      setState(() {
        order = data;
        isLoading = false;
      });
    } catch (e) {
      debugPrint("Lỗi tải chi tiết đơn hàng: $e");
      setState(() {
        isError = true;
        isLoading = false;
      });
    }
  }

  String _formatVND(num value) =>
      "${NumberFormat("#,###", "vi_VN").format(value)} ₫";

  String _formatDate(String? dateStr) {
    if (dateStr == null || dateStr.isEmpty) return "Không rõ";
    try {
      return DateFormat("dd/MM/yyyy HH:mm").format(DateTime.parse(dateStr));
    } catch (_) {
      return "Không rõ";
    }
  }

  Color _statusColor(String? status) {
    switch (status) {
      case "0":
        return Colors.orange;
      case "1":
        return Colors.amber;
      case "2":
        return Colors.blueAccent;
      case "3":
        return Colors.green;
      case "4":
        return Colors.red;
      default:
        return Colors.grey;
    }
  }

  String _statusText(String? status) {
    switch (status) {
      case "0":
        return "🟠 Chờ xác nhận";
      case "1":
        return "📦 Chờ lấy hàng";
      case "2":
        return "🚚 Đang giao hàng";
      case "3":
        return "✅ Đã giao";
      case "4":
        return "❌ Đã hủy";
      default:
        return "⚪ Không xác định";
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text("Đơn hàng #${widget.orderId}"),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : isError
          ? const Center(
        child: Text(
          "Lỗi tải dữ liệu. Vui lòng thử lại sau.",
          style: TextStyle(color: Colors.red),
        ),
      )
          : order == null
          ? const Center(child: Text("Không tìm thấy đơn hàng."))
          : _buildOrderDetail(),
    );
  }

  Widget _buildOrderDetail() {
    final rawStatus =
        order!["orderStatus"]?.toString() ?? order!["OrderStatus"]?.toString();

    final details = order!["orderDetails"] ?? order!["details"] ?? [];
    final totalPrice = order!["totalPrice"] ?? order!["TotalPrice"] ?? 0;

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                _statusText(rawStatus),
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: _statusColor(rawStatus),
                  fontSize: 16,
                ),
              ),
              Text(
                _formatDate(order!["orderDate"] ?? order!["OrderDate"]),
                style: const TextStyle(color: Colors.grey),
              ),
            ],
          ),
          const Divider(height: 30),
          const Text(
            "📦 Thông tin người nhận",
            style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
          ),
          const SizedBox(height: 6),
          Text("👤 ${order!["recipientName"] ?? order!["RecipientName"] ?? "Không rõ"}"),
          Text("📞 ${order!["recipientPhone"] ?? order!["RecipientPhone"] ?? "Không rõ"}"),
          Text(
              "🏠 ${order!["specificAddress"] ?? order!["SpecificAddress"] ?? ""}, ${order!["ward"] ?? order!["Ward"] ?? ""}, ${order!["district"] ?? order!["District"] ?? ""}"),
          const Divider(height: 30),
          const Text(
            "🎂 Sản phẩm trong đơn",
            style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
          ),
          const SizedBox(height: 8),
          for (final item in details)
            Card(
              margin: const EdgeInsets.symmetric(vertical: 8),
              elevation: 2,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10)),
              child: ListTile(
                leading: SizedBox(
                  width: 55,
                  height: 55,
                  child: item["image"] != null
                      ? Image.network(
                    item["image"].toString().startsWith("http")
                        ? item["image"]
                        : "http://10.0.2.2:5006${item["image"]}",
                    fit: BoxFit.cover,
                  )
                      : const Icon(Icons.cake, size: 40),
                ),
                title: Text(item["productName"] ?? "Sản phẩm không rõ"),
                subtitle: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text("Số lượng: ${item["quantity"] ?? 1}"),
                    if (item["notes"] != null &&
                        item["notes"].toString().isNotEmpty)
                      Text("Ghi chú: ${item["notes"]}"),
                  ],
                ),
                trailing: Text(
                  _formatVND(
                    item["price"] is num
                        ? item["price"]
                        : num.tryParse(item["price"].toString()) ?? 0,
                  ),
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
              ),
            ),
          const Divider(height: 30),
          Text(
            "💳 Phương thức thanh toán: ${order!["paymentMethod"] ?? "Không rõ"}",
            style: const TextStyle(fontSize: 16),
          ),
          Text(
            "💰 Trạng thái thanh toán: ${order!["paymentStatus"] ?? "Không rõ"}",
            style: const TextStyle(fontSize: 16),
          ),
          const SizedBox(height: 10),
          Text(
            "Tổng tiền: ${_formatVND(totalPrice)}",
            style: const TextStyle(
                fontSize: 18, fontWeight: FontWeight.bold, color: Colors.red),
          ),
          const SizedBox(height: 30),
          Center(
            child: ElevatedButton.icon(
              onPressed: () => Navigator.pop(context),
              icon: const Icon(Icons.arrow_back),
              label: const Text("Quay lại"),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFFF77E6E),
                padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 14),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10)),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
