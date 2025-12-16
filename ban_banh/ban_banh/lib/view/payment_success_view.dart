import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import 'order_detail_view.dart'; // dùng để format tiền và ngày giờ

class PaymentSuccessView extends StatelessWidget {
  final Map<String, dynamic>? orderData;

  const PaymentSuccessView({super.key, this.orderData});

  String _formatVND(num? value) {
    if (value == null) return "0 ₫";
    final formatter = NumberFormat("#,###", "vi_VN");
    return "${formatter.format(value)} ₫";
  }

  @override
  Widget build(BuildContext context) {
    final data = orderData ?? {};

    // ✅ Lấy thông tin đơn hàng an toàn
    final orderId = data["orderId"]?.toString() ?? "N/A";
    final recipient = data["recipientName"] ?? "Không rõ";
    final address =
        "${data["specificAddress"] ?? ""}, ${data["ward"] ?? ""}, ${data["district"] ?? ""}";
    final paymentMethod = data["paymentMethod"] ?? "VNPAY";

    final total = (data["totalPrice"] is num)
        ? (data["totalPrice"] as num)
        : 0;

    // ✅ Dùng ngày thật của đơn hàng nếu có
    DateTime payTime = DateTime.now();
    if (data["deliveryDateTime"] != null) {
      try {
        payTime = DateTime.parse(data["deliveryDateTime"]);
      } catch (_) {}
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text("Thanh toán thành công"),
        backgroundColor: Colors.green.shade600,
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            const Icon(Icons.check_circle, color: Colors.green, size: 100),
            const SizedBox(height: 20),
            const Text(
              "🎉 Giao dịch thành công!",
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 10),
            Text(
              "Cảm ơn bạn đã mua hàng tại Cake Me!",
              style: TextStyle(color: Colors.grey[700], fontSize: 16),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 30),
            Divider(thickness: 1.5),
            const SizedBox(height: 10),

            // 🧾 Thông tin đơn hàng
            _infoRow("Mã đơn hàng:", orderId),
            _infoRow("Người nhận:", recipient),
            _infoRow("Địa chỉ:", address.isEmpty ? "Không rõ" : address),
            _infoRow("Phương thức:", paymentMethod),
            _infoRow("Tổng tiền:", _formatVND(total), highlight: true),

            const SizedBox(height: 10),
            Divider(thickness: 1.5),
            const SizedBox(height: 20),

            // 🕓 Thời gian thanh toán
            _infoRow(
              "Thời gian thanh toán:",
              DateFormat("dd/MM/yyyy HH:mm").format(payTime),
            ),

            const SizedBox(height: 40),

            // 🔙 Nút về trang chủ
            ElevatedButton.icon(
              onPressed: () => Navigator.pushNamedAndRemoveUntil(
                context,
                '/home_view',
                    (_) => false,
              ),
              icon: const Icon(Icons.home),
              label: const Text("Về trang chủ"),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.green,
                padding:
                const EdgeInsets.symmetric(horizontal: 40, vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
            ),
            const SizedBox(height: 20),

            // 🔁 Xem chi tiết đơn hàng
            OutlinedButton.icon(
              onPressed: () {
                final orderId = orderData?["orderId"];
                if (orderId != null) {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => OrderDetailView(orderId: int.parse(orderId.toString())),
                    ),
                  );
                } else {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text("Không tìm thấy mã đơn hàng.")),
                  );
                }
              },
              icon: const Icon(Icons.receipt_long, color: Colors.green),
              label: const Text("Xem chi tiết đơn hàng"),
            )

          ],
        ),
      ),
    );
  }

  /// 🔹 Widget hiển thị từng dòng thông tin
  Widget _infoRow(String label, String value, {bool highlight = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            flex: 2,
            child: Text(label,
                style:
                const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
          ),
          Expanded(
            flex: 3,
            child: Text(
              value,
              style: TextStyle(
                fontSize: 16,
                fontWeight: highlight ? FontWeight.bold : FontWeight.normal,
                color: highlight ? Colors.redAccent : Colors.black87,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
