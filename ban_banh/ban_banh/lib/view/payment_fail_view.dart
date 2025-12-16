import 'package:flutter/material.dart';

class PaymentFailView extends StatelessWidget {
  final String? message; // Thông báo lỗi tùy chọn

  const PaymentFailView({super.key, this.message});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Thanh toán thất bại"),
        backgroundColor: Colors.red.shade600,
        elevation: 0,
      ),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.cancel, color: Colors.red, size: 100),
            const SizedBox(height: 20),
            const Text(
              "Thanh toán thất bại!",
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 10),
            Text(
              message ?? "Rất tiếc, giao dịch của bạn không thành công. Vui lòng thử lại.",
              style: TextStyle(color: Colors.grey[700], fontSize: 16),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 30),
            Divider(thickness: 1.5),
            const SizedBox(height: 30),

            // 🔁 Nút thử lại thanh toán
            ElevatedButton.icon(
              onPressed: () => Navigator.pop(context),
              icon: const Icon(Icons.refresh),
              label: const Text("Thử lại"),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.orangeAccent,
                padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
            ),
            const SizedBox(height: 20),

            // 🔙 Về trang chủ
            OutlinedButton.icon(
              onPressed: () => Navigator.pushNamedAndRemoveUntil(
                  context, '/home_view', (_) => false),
              icon: const Icon(Icons.home, color: Colors.red),
              label: const Text("Về trang chủ"),
              style: OutlinedButton.styleFrom(
                side: const BorderSide(color: Colors.red, width: 1.5),
                padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
