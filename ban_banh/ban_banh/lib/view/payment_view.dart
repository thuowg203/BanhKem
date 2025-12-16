import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:url_launcher/url_launcher.dart';
import '../services/api_service.dart';
import 'payment_success_view.dart';

class PaymentView extends StatefulWidget {
  final Map<String, dynamic> orderData;

  const PaymentView({super.key, required this.orderData});

  @override
  State<PaymentView> createState() => _PaymentViewState();
}

class _PaymentViewState extends State<PaymentView> with WidgetsBindingObserver {
  bool _isProcessing = false;
  int? _pendingOrderId;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  // 🧩 Khi quay lại app sau khi thanh toán xong
  @override
  void didChangeAppLifecycleState(AppLifecycleState state) async {
    if (state == AppLifecycleState.resumed && _pendingOrderId != null) {
      print("🟢 App resumed → Kiểm tra lại thanh toán cho đơn $_pendingOrderId");
      await _checkPaymentStatus(_pendingOrderId!);
    }
  }

  String _formatVND(num? v) {
    if (v == null) return "0₫";
    final s = v.toInt().toString().replaceAllMapped(
      RegExp(r'(\d)(?=(\d{3})+(?!\d))'),
          (m) => '${m[1]}.',
    );
    return "$s₫";
  }

  List<dynamic> get _items =>
      (widget.orderData["orderDetails"] as List<dynamic>? ?? const []);

  num get _subtotal {
    num sum = 0;
    for (final it in _items) {
      final price = (it["price"] as num?) ?? 0;
      final qty = (it["quantity"] as num?) ?? 0;
      sum += price * qty;
    }
    return sum;
  }

  static const num _shipping = 30000;

  String get _deliveryText {
    final raw = widget.orderData["deliveryDateTime"]?.toString() ?? "";
    final dt = DateTime.tryParse(raw);
    if (dt == null) return "Không xác định";
    final hh = dt.hour.toString().padLeft(2, '0');
    final mm = dt.minute.toString().padLeft(2, '0');
    return "${dt.day}/${dt.month}/${dt.year} $hh:$mm";
  }

  /// Thanh toán qua VNPAY
  Future<void> _payWithVnpay() async {
    setState(() => _isProcessing = true);
    try {
      final payload = {...widget.orderData, "paymentMethod": "VNPAY"};

      // Gửi yêu cầu tạo thanh toán
      final res = await http.post(
        Uri.parse("http://10.0.2.2:5006/api/ShoppingCartApi/checkout"),
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Bearer ${ApiService.token!}",
        },
        body: jsonEncode(payload),
      );

      if (res.statusCode == 200) {
        final data = jsonDecode(res.body);
        final paymentUrl = data["paymentUrl"];
        final orderId = data["orderId"];

        if (paymentUrl != null && paymentUrl.toString().startsWith("http")) {
          // Lưu orderId để kiểm tra khi quay lại app
          _pendingOrderId = orderId;

          //Mở trang thanh toán VNPAY
          await launchUrl(Uri.parse(paymentUrl),
              mode: LaunchMode.externalApplication);

          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content:
                Text("Sau khi thanh toán xong, hãy quay lại ứng dụng."),
              ),
            );
          }
        } else {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text("Không nhận được đường dẫn thanh toán.")),
          );
        }
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Lỗi khi khởi tạo thanh toán: ${res.body}")),
        );
      }
    } catch (e, stack) {
      print("[PAYMENT-VNPAY] Exception: $e");
      print(stack);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi: $e")),
      );
    } finally {
      if (mounted) setState(() => _isProcessing = false);
    }
  }

  ///Kiểm tra trạng thái thanh toán
  Future<void> _checkPaymentStatus(int orderId) async {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircularProgressIndicator(color: Colors.green),
            SizedBox(height: 16),
            Text(
              "Đang xác nhận thanh toán...",
              style: TextStyle(color: Colors.white, fontSize: 16),
            ),
          ],
        ),
      ),
    );

    try {
      final check = await http.get(
        Uri.parse(
            "http://10.0.2.2:5006/api/ShoppingCartApi/check-payment-status/$orderId"),
        headers: {"Authorization": "Bearer ${ApiService.token!}"},
      );

      print("🟣 Check status: ${check.statusCode}");
      print("🟣 Body: ${check.body}");

      if (check.statusCode == 200) {
        final result = jsonDecode(check.body);
        final status = result["status"]?.toString() ?? "";

        Navigator.pop(context); // Đóng loading

        if (status == "Đã thanh toán") {
          if (mounted) {
            Navigator.pushReplacement(
              context,
              MaterialPageRoute(
                builder: (_) => PaymentSuccessView(
                  orderData: {
                    ...widget.orderData,
                    "orderId": orderId,
                    "paymentMethod": "VNPAY",
                    "totalPrice": widget.orderData["totalPrice"],
                    "deliveryDateTime": DateTime.now().toString(),
                  },
                ),
              ),
            );
          }
        } else if (status == "Thất bại") {
          if (mounted) {
            Navigator.pushNamedAndRemoveUntil(
                context, '/payment_fail', (_) => false);
          }
        } else {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
                content: Text("Thanh toán chưa hoàn tất. Vui lòng thử lại sau.")),
          );
        }
      } else {
        Navigator.pop(context);
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Không thể kiểm tra trạng thái thanh toán.")),
        );
      }
    } catch (e) {
      Navigator.pop(context);
      print("[CHECK STATUS ERROR] $e");
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi khi kiểm tra thanh toán: $e")),
      );
    }
  }

  /// Thanh toán COD
  Future<void> _confirmCOD() async {
    try {
      final payload = {
        ...widget.orderData,
        "paymentMethod": "COD",
      };

      final res = await http.post(
        Uri.parse("http://10.0.2.2:5006/api/ShoppingCartApi/checkout"),
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Bearer ${ApiService.token!}",
        },
        body: jsonEncode(payload),
      );

      final responseData = jsonDecode(res.body);

      if (res.statusCode == 200) {
        if (!mounted) return;
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(
            builder: (_) => PaymentSuccessView(
              orderData: {
                ...payload,
                "orderId": responseData["orderId"],
              },
            ),
          ),
        );
      } else {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text("Lỗi xác nhận: ${res.body}")));
      }
    } catch (e, stack) {
      print("[PAYMENT-COD] Exception: $e");
      print(stack);
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text("Lỗi: $e")));
    }
  }

  @override
  Widget build(BuildContext context) {
    final order = widget.orderData;

    return Scaffold(
      appBar: AppBar(
        title: const Text("Thanh toán đơn hàng"),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          const Text("Thông tin người nhận",
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          _info("Người nhận:", order["recipientName"]),
          _info("SĐT:", order["recipientPhone"]),
          _info("Địa chỉ:", order["specificAddress"]),
          _info("Phường/Xã:", order["ward"]),
          _info("Quận/Huyện:", order["district"]),
          _info("Thời gian nhận hàng:", _deliveryText),
          _info("Ghi chú đơn:", order["notes"]),

          const SizedBox(height: 12),
          const Divider(),
          const Text("Chi tiết đơn hàng",
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),

          for (final it in _items)
            Card(
              elevation: 1.5,
              margin: const EdgeInsets.only(bottom: 8),
              child: Padding(
                padding: const EdgeInsets.all(10),
                child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        "${it["name"] ?? it["productName"] ?? "Sản phẩm"} × ${it["quantity"]}",
                        style: const TextStyle(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 2),
                      Text("Giá: ${_formatVND((it["price"] as num?) ?? 0)}"),
                      if ((it["notes"] ?? "").toString().isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.only(top: 2),
                          child: Text(
                            "Ghi chú: ${it["notes"]}",
                            style: const TextStyle(fontStyle: FontStyle.italic),
                          ),
                        ),
                    ]),
              ),
            ),

          const Divider(),
          _rowAmount("Tạm tính", _formatVND(_subtotal)),
          _rowAmount("Phí vận chuyển", _formatVND(_shipping)),
          const SizedBox(height: 4),
          _rowAmount("Tổng cộng", _formatVND(_subtotal + _shipping),
              isTotal: true),
          const SizedBox(height: 24),
          Center(
            child: _isProcessing
                ? const CircularProgressIndicator()
                : ElevatedButton.icon(
              onPressed: _payWithVnpay,
              icon: const Icon(Icons.credit_card, color: Colors.white),
              label: const Text("THANH TOÁN QUA VNPAY",
                  style: TextStyle(fontWeight: FontWeight.bold)),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFFF77E6E),
                padding: const EdgeInsets.symmetric(
                    horizontal: 40, vertical: 14),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10)),
              ),
            ),
          ),
          const SizedBox(height: 12),
          Center(
            child: ElevatedButton.icon(
              onPressed: _confirmCOD,
              icon: const Icon(Icons.local_shipping, color: Colors.white),
              label: const Text("THANH TOÁN KHI NHẬN HÀNG (COD)",
                  style: TextStyle(fontWeight: FontWeight.bold)),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.green,
                padding:
                const EdgeInsets.symmetric(horizontal: 40, vertical: 14),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10)),
              ),
            ),
          ),
        ]),
      ),
    );
  }

  Widget _info(String label, dynamic value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        children: [
          Expanded(
              flex: 2,
              child: Text(label,
                  style: const TextStyle(fontWeight: FontWeight.bold))),
          Expanded(flex: 3, child: Text(value?.toString() ?? "")),
        ],
      ),
    );
  }

  Widget _rowAmount(String label, String value, {bool isTotal = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        children: [
          Expanded(
              child: Text(label,
                  style: TextStyle(
                    fontWeight: isTotal ? FontWeight.bold : FontWeight.w500,
                    fontSize: isTotal ? 16 : 14,
                  ))),
          Text(
            value,
            style: TextStyle(
              color: isTotal ? const Color(0xFFF77E6E) : Colors.black87,
              fontWeight: isTotal ? FontWeight.bold : FontWeight.w500,
              fontSize: isTotal ? 16 : 14,
            ),
          ),
        ],
      ),
    );
  }
}
